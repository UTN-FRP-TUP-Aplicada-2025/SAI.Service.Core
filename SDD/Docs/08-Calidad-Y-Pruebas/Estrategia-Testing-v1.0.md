# Estrategia de testing — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Estrategia-Testing-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Define la pirámide de testing, la cobertura mínima por capa, el tooling por capacidad, la política de BDD, los mocks y fixtures, los datos de prueba y el ambiente. Los umbrales que declara son piso; los quality gates que los hacen cumplir viven en `Estrategia-Calidad-v1.0.md §3` y en 09.

---

## 1. Pirámide de testing deseada

Distribución objetivo del esfuerzo de automatización: **70 % unitarias / 25 % integración / 5 % end-to-end**.

| Nivel | Qué cubre | Capacidad / realización | Porcentaje objetivo |
|---|---|---|---|
| Unitarias | Lógica de dominio y de aplicación aislada: los 21 invariantes I-1..I-21, `ResolutorTemporal`, `Costos.cuadra()`, `Intervencion.aplicar()` y sus `Efectos`, máquina de estados de `UnidadFisica`, cálculo de derivados de prueba de batería, degradación de modalidad | framework de pruebas unitarias (xUnit) + aserciones fluidas (FluentAssertions) | 70 % |
| Integración | `Infrastructure` y contratos: EF Core + migraciones contra SQLite físico, adaptador de conexión contra el adaptador simulado, API de ingesta con sus cuatro caminos (201/200/409/422). Los criterios Given-When-Then de cada CU se materializan acá. | integración EF Core contra SQLite físico + host de pruebas web (WebApplicationFactory) | 25 % |
| End-to-end | Flujos de usuario críticos sobre el panel, contra el adaptador simulado: UF-1 (alta de equipos), UF-3 (monitoreo en vivo) y el camino de apagado de UF-8 | arnés de componentes Blazor (bUnit) + driver e2e de navegador (Playwright) | 5 % |

No se declara un nivel de snapshot separado: la superficie estable a fijar (payloads de la API, render de componentes) se cubre dentro de integración y e2e con aserciones estructurales, no con un baseline de snapshot con su propia política de regeneración.

### 1.1 Reconciliación con la pirámide por defecto del tipo D8

La regla §2.2 de 08-Rules fija para `web-monolith` una pirámide de referencia **70 / 20 / 10** (unit / integración / e2e). El Intake §17.P.6 declara **70 / 25 / 5**. Hay una diferencia deliberada y esta estrategia **reconcilia a favor del intake (70 / 25 / 5)**, por dos razones que el propio intake fundamenta: (a) el núcleo de valor es lógica de dominio pura que se prueba sin infraestructura, lo que ensancha la base unitaria y, en el margen, empuja peso de e2e hacia integración; (b) con un solo desarrollador, la suite e2e —la más lenta y frágil de mantener— se acota a los tres recorridos que no se pueden cubrir de otro modo, y el resto de la interacción entre componentes se valida como integración. La base unitaria del 70 % coincide con la referencia; el desplazamiento es de 5 puntos de e2e hacia integración. No se baja ningún piso de cobertura, así que la reconciliación no requiere ADR.

### 1.2 Justificación contra las pirámides degeneradas

- **Contra la pirámide invertida (e2e pesado).** Una suite dominada por end-to-end sobre el panel sería lenta, frágil y de diagnóstico difícil, y no podría ejercitar los 21 invariantes ni el `ResolutorTemporal`, que son lógica pura. Peor aún: el flujo F-3 (apagado y reencendido físico) es imposible de automatizar (§4), así que apoyar la garantía central en e2e sería apoyarla en un test que no existe. La decisión irreversible se prueba en unitarias sobre el dominio, con el adaptador simulado.
- **Contra la pirámide aplanada (cobertura cuantitativa sin distinguir capas).** Reportar un único "80 % de cobertura" escondería 100 % en getters y 40 % en la lógica de apagado. Por eso la cobertura se declara y se evalúa por capa (§2), con un piso más alto y separado para el dominio.

## 2. Cobertura mínima por capa

Esta tabla es la **fuente única de cobertura por capa** de la categoría 08: los demás documentos (`Criterios-Validacion`, `Matriz-Cobertura-Pruebas`) la referencian, no la repiten con números propios. Son pisos bloqueantes en el pipeline. El dominio lleva el umbral del intake (N-22), más alto que el de la referencia del tipo D8, por ser donde viven las decisiones irreversibles. El conjunto de la solución respeta el piso global 80/70 (N-21).

| Capa | Líneas (piso) | Ramas (piso) | Mutation score | Origen del umbral |
|---|---|---|---|---|
| `SAI.Service.Core.Domain` | 90 % | 85 % | No adoptado en v1 | Intake P.6 (N-22) |
| `SAI.Service.Core.Application` | 80 % | 70 % | — | Regla §2.2 web-monolith |
| `SAI.Service.Core.Infrastructure` | 70 % | 60 % | — | Regla §2.2 web-monolith |
| `SAI.Service.Core.Api` | 80 % | 70 % | — | Capa de frontera con el contrato crítico de ingesta (F-20 Must, 100 % de los caminos 201/200/409/422); piso elevado respecto de la presentación |
| `SAI.Service.Core.Web` (presentación) | 60 % | 50 % | — | Regla §2.2 web-monolith |
| Conjunto de la solución (gate global) | 80 % | 70 % | — | Intake P.6 (N-21) |

Decisión explícita sobre la capa `Api`: los controllers REST y el endpoint de ingesta son **capa de frontera con contrato crítico**, así que la `Api` lleva su propia fila en 80/70 y **no** se agrupa en la presentación (60/50); el `Web` (panel Blazor) sí queda en el piso de presentación.

Mutation testing no se adopta en v1: el tipo `web-monolith` no lo exige (la regla lo pide solo para `library`) y el intake no lo declara. Queda como posible refuerzo futuro del dominio, sin umbral comprometido para no inventar cifras. Los pisos son piso, no techo: se pueden subir sin ADR; bajar cualquiera exige ADR.

## 3. Tooling por capacidad y realización

Se describe la estrategia por capacidad de testing; el framework concreto es la realización elegida, no el centro de la estrategia (regla §4.3.3).

| Capacidad de testing | Realización elegida |
|---|---|
| Pruebas unitarias de dominio y aplicación | framework de pruebas unitarias (xUnit) |
| Aserciones expresivas y legibles | biblioteca de aserciones fluidas (FluentAssertions) |
| Integración de persistencia | EF Core con proveedor SQLite contra archivo físico temporal, migraciones incluidas |
| Integración de API y del host web | host de pruebas en memoria (WebApplicationFactory) |
| Aislamiento del acceso al equipo | adaptador de conexión simulado (F-24 / BT-29), pieza central del aislamiento (§5) |
| Pruebas de componentes Blazor | arnés de componentes Blazor (bUnit) |
| Recorridos end-to-end del panel | driver e2e de navegador (Playwright) |
| Cobertura por capa | recolector de cobertura de .NET, reportado por assembly |
| Análisis de composición y vulnerabilidades | herramienta de listado de paquetes vulnerables del SDK (gate 6) |

## 4. BDD / ATDD (Given-When-Then como pruebas de integración)

Los criterios de aceptación de cada CU y de cada US se redactan en **Given / When / Then** y se materializan como **pruebas de integración**, no como especificaciones en un lenguaje de features separado. No se adopta un framework de BDD con archivos `.feature` en gherkin: con un solo desarrollador, el costo de mantener una capa de features y su pegamento de step definitions supera el beneficio, y la trazabilidad CU↔test se resuelve nombrando el CU en el propio test y en la matriz de cobertura. Cada test de integración lleva en su nombre y en su documentación el CU o RN que cubre y su cláusula Given-When-Then, de modo que el criterio de aceptación y su verificación viven juntos. Los escenarios E-1..E-8 del intake (§20) son la fuente de los datos que atraviesan estos Given-When-Then.

## 5. Mocks y fixtures

- **Pieza central de aislamiento: el adaptador de conexión simulado.** El contrato del adaptador (puerto en `Application`, ADR-22) tiene tres implementaciones: NUT (real, primera entrega), directo (diseñado, no implementado) y **simulado**. El simulado es lo que hace testeable el camino de apagado sin hardware ni riesgo: permite guionar estados del equipo (`OL`, `OB`, caídas de tensión, muestras `perdida`, comandos que no producen efecto) y ejercitar la lógica de decisión, la degradación de modalidad y la validación por efecto observado. Es también lo que cubre el flujo imposible F-3 en pruebas automatizadas.
- **Fixtures a partir de los escenarios.** Los datos de las fixtures se derivan de los ocho escenarios del Anexo A (§20 E-1..E-8), con su JSON completo, no de datos inventados por cada test. La tabla de cobertura de campos por escenario (§21.B.1) sirve para elegir la fixture mínima de cada prueba.
- **Política de reuso y de duplicación.** Las fixtures se centralizan en el proyecto de pruebas y se reutilizan entre niveles; no se duplica la construcción de un mismo agregado en varios tests. La procedencia y la lista de derivación se declaran una vez por `SesionSondeo` (como en producción, §17.P.4), no por muestra.
- **Sin catch silenciosos.** Los tests fallan rápido cuando se viola un invariante; no se enmascaran errores. Cada test tiene al menos una aserción explícita.

## 6. Datos de prueba

- **Origen: sintéticos, versionados con el código.** Todos los datos de prueba son sintéticos y provienen de los escenarios E-1..E-8 del intake, cuyos importes y seriales están marcados como ficticios o reconstruidos en la fuente. No hay datos de producción ni anonimizados: el sistema no maneja datos personales más allá del contacto de un proveedor.
- **Series temporales guionadas.** Para las reglas de derivación y el disparo del apagado se usan series de `Muestra` construidas a mano que ejercitan los casos límite: microcorte de una sola muestra (CL-11), muestra `parcial` y `perdida` con nulos (CL-12), serie que nunca enciende el flag `LB` y aun así dispara (R-04, BT-20), pérdida de muestras durante la prueba a 1 Hz (CL-13).
- **Regeneración.** Los datasets se versionan en el repositorio junto a las pruebas; su modificación pasa por PR con justificación, igual que el código. No hay regeneración automática que pueda "arreglar" un test cambiando su expectativa.

## 7. Ambiente de testing

- **Aislamiento entre tests.** Cada prueba de integración usa una base SQLite en **archivo temporal efímero**, creado y destruido por test o por clase, con las migraciones aplicadas al inicio. Ningún test depende del orden de ejecución ni comparte estado con otro.
- **Contenedor de desarrollo.** La suite corre dentro del Dev Container (spec containers.dev); el único requisito del host es Docker, el SDK vive en el contenedor. En CI corre sobre `ubuntu-latest` con .NET 10, contenedor `linux/amd64` (matriz de eje único, §17.P.8).
- **Contraseñas y configuración no productivas.** Las credenciales de desarrollo (cadena de conexión, credenciales de `upsd` si NUT corre en el host) se inyectan por variables de entorno del contenedor y por el almacén de secretos del SDK dentro del Dev Container; nunca contraseñas productivas ni valores en el repositorio.
- **Sin ambiente de staging.** No hay staging porque no habría a qué SAI conectarlo; el ambiente de producción es el contenedor en el host `i7infra` con el dispositivo USB anclado por ruta física de puerto. El comportamiento real del firmware y de la BIOS no se prueba en ambiente: se verifica en la ventana de mantenimiento y se registra como evidencia de una `Verificacion`, no como test.

## 8. Prueba imposible de automatizar (declarada)

El flujo **F-3** (ciclo completo de apagado y reencendido físico) corresponde a **CU-05 (ejecución del apagado ordenado ante corte)** y no se puede probar solo con software. El adaptador simulado cubre la lógica de decisión de CU-05; el comportamiento real del firmware (`ver-shutdown-return`) y de la BIOS (`ver-bios-autoencendido`) se verifica físicamente en la **ventana de mantenimiento de UF-8 / CU-10**, y su resultado se registra como evidencia de una `Verificacion` con su vigencia, no como un test verde. Hasta que esa ventana física ocurra, el servicio permanece forzado en `SoloAlerta`. Esta declaración es parte de la estrategia, no una omisión.

---

## 9. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Versión inicial: pirámide 70/25/5 reconciliada a favor del intake, cobertura por capa, tooling por capacidad, BDD como integración, mocks/fixtures, datos, ambiente y prueba imposible F-3. |
| 1.1 | 2026-07-21 | Corrección de conformidad: unificación de la tabla de cobertura por capa como fuente única tras audit de Fase E. La capa `Api` pasa a fila propia 80/70 (capa de frontera con contrato crítico de ingesta); `Infrastructure` en 70/60. F-3 atribuido a CU-05 con verificación física en CU-10. |
