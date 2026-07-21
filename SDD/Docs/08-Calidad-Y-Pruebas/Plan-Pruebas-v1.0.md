# Plan de pruebas — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Plan-Pruebas-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Plan operativo de pruebas mapeado al Mini-Plan de 07 (`07-Plan-Sprint/Mini-Plan-v1.0.md`). Define alcance por etapa, criterios de entrada y salida, riesgos de calidad y recursos. No redefine la Definition of Done (vive en `Definition-Of-Done-v1.0.md`) ni los umbrales de cobertura (viven en `Estrategia-Testing-v1.0.md`): los referencia.

---

## 1. Alcance del plan

Cubre las **24 historias Must y Should (153 SP)** comprometidas en v1, organizadas en un Sprint 0 de arranque más cinco etapas de valor (Mini-Plan §2). Cada etapa entrega una rebanada vertical que atraviesa las cinco capas hasta una pantalla usable, y cierra con validación humana en el navegador.

- **Incluido:** los 12 CU (CU-01..CU-12), las 13 RN (RN-01..RN-13), los 21 invariantes (I-1..I-21), los 25 NFR con objetivo numérico (N-01..N-25, con los PENDIENTE marcados como tales), la API de ingesta y el adaptador simulado.
- **Excluido de v1 (diseñado, no implementado):** US-25 (adaptador de conexión directo) y US-26 (capa de add-ons de dialecto), 8 SP, diferidas a v2 (Mini-Plan §1). No se planifican pruebas de esas capacidades más allá de verificar que su ausencia no rompe el núcleo.
- **No automatizable, declarado:** el flujo F-3 (apagado y reencendido físico); su parte de software se cubre con el adaptador simulado, la ejecución real es la ventana de mantenimiento (CU-10) y su resultado es evidencia, no test.

## 2. Criterios de entrada

Una etapa entra a ejecución de pruebas cuando:

1. Todos sus ítems cumplen la Definition of Ready de 06 (`Definition-Of-Ready-v1.0.md`); ningún ítem huérfano ni con dependencia bloqueante abierta.
2. Las dependencias técnicas de la etapa (BT previos, ADR de Sprint 0) están cerradas o su bloqueo está registrado.
3. El build compila en Release con cero warnings y el pipeline base (build, test, lint) corre (acordado en Sprint 0, BT-05).
4. Existen los datos de prueba sintéticos de los escenarios que la etapa ejercita (§20 E-1..E-8) y el adaptador simulado está disponible para los flujos que lo requieran.
5. El ambiente de pruebas (SQLite en archivo temporal, Dev Container) está operativo.

## 3. Criterios de salida

Una etapa se declara ejecutada con éxito cuando:

1. Los 10 quality gates del pipeline pasan sobre la rama de la etapa (Estrategia-Calidad §3).
2. La cobertura por capa cumple los pisos: Domain 90/85, global 80/70, y los pisos por capa de la estrategia de testing; reportada por capa, nunca como número único.
3. Cada CU que avanza en la etapa tiene al menos un test por criterio Given-When-Then, verde, trazado en la `Matriz-Cobertura-Pruebas`.
4. Cada NFR con objetivo numérico que entra en alcance de la etapa tiene su test o mecanismo de medición asociado; los PENDIENTE (N-03, N-20, N-25) quedan explícitamente marcados, no ocultos.
5. Cero defectos blocker abiertos; todo defecto cerrado en la etapa generó su test de regresión.
6. La suite de regresión de las etapas anteriores sigue verde (ningún test verde pasó a rojo sin justificación por ADR).
7. La validación humana en el navegador del criterio de cierre de la etapa (Mini-Plan) pasa.

## 4. Riesgos de calidad

Alineados con los riesgos de negocio del Intake §11 (R-01..R-14) y los riesgos arquitectónicos de 05 §9. Se listan los que impactan directamente la estrategia de pruebas.

| ID | Riesgo de calidad | Prob. | Impacto | Mitigación en la estrategia de pruebas |
|---|---|---|---|---|
| R-10 | Los invariantes son hipótesis de diseño hasta que corran como pruebas | Cierta | Medio | Escribir I-1..I-21 como pruebas unitarias **antes** de codificar el dominio (Etapa 1); son la primera línea del proyecto de tests |
| R-12 | El servicio decide apagar un host sin backups; si falla, falla de noche y sin testigos | Media | Crítico | Cubrir el arranque forzado en `SoloAlerta` y el bloqueo por verificación (I-11) con unitarias sobre el dominio y el adaptador simulado; ningún camino de apagado sin test que lo bloquee cuando falta un supuesto |
| R-01 | El ciclo apagado-reencendido no está verificado; trampa de firmware | Media | Crítico | El adaptador simulado cubre la lógica; la verificación real es CU-10 (evidencia, no test). El plan declara F-3 como no automatizable y no lo da por cubierto con software |
| R-13 | Guardar `battery.charge` sin marcar su procedencia produce una conclusión falsa | Alta | Alto | Test de invariante I-7 (procedencia) en el pipeline con cero excepciones (N-24); test de que `aptoParaTendenciaDeSalud()` rechaza `derivado`/`estimadoPorDriver`/`imputado` (I-9) |
| R-09 | Sin sensor de temperatura, el confusor de la salud no tiene solución | Cierta | Alto (salud) | No se testea una precisión que el equipo no da; se testea que toda conclusión de salud lleva su reserva y que el veredicto arranca en confianza `baja` (I-16, N-16) |
| R-07 | Retención y agregación no probadas contra el volumen real (~6,3 M filas/año) | Media | Bajo | Test de agregación con min/max y de retención (30 d muestra / 10 a agregado); validar tamaño de archivo (N-20 PENDIENTE) antes de producción |
| R-04 | El flag `LB` no fue observado; la política no debe depender de él | Alta | Medio | Test con serie sintética que nunca enciende `LB` y aun así dispara por tiempo en `OB` + `battery.voltage` (BT-20, ADR-12) |
| R-11 | La sustitución de SAI (CU-09) no tiene escenario de datos | Cierta | Bajo | Agregar un escenario E-9 al implementar el flujo (Etapa 4) para ejercitar la cobertura suplente |
| R-05/R-08 | Competencia por el nodo USB y decisión abierta de dónde vive NUT | Media/Alta | Medio/Bajo | Cerrar ADR-19 en Sprint 0; probar el anclaje por ruta física con el adaptador simulado, no contra hardware compartido |

## 5. Plan por etapa

Mapea cada etapa del Mini-Plan a su alcance de testing y entregables de calidad. Los CU y flujos son los de la trazabilidad del Mini-Plan.

| Etapa (Mini-Plan) | Alcance de testing | Entregables de calidad |
|---|---|---|
| Sprint 0 — Arranque (EP-01+EP-02) | Pipeline base (build, test, lint) operativo; andamiaje del proyecto de pruebas de invariantes; smoke test del panel base contra la maqueta | CI con gates 1, 2 y esqueleto de 3–5; proyecto `Domain.Tests` listo para I-1..I-21; acuerdo de la DoD con 08 |
| Etapa 1 — Persistencia, alta de admin y sesión (EP-03, CU-01) | Unitarias de `Valor<T>` con `Origen` (I-7); test de historia append-only (rechazo de escritura destructiva); integración de migraciones al arranque; e2e de alta de admin, login, logout y cambio de contraseña | I-7 verde en pipeline (N-24); gates 3 y 4 con cobertura Domain 90/85 en lo implementado; TC de CU-01 |
| Etapa 2 — Alta de equipos y políticas (EP-04, CU-02, CU-03, CU-10 siembra) | Unitarias de `ResolutorTemporal` y vínculos temporales (I-1..I-4), baja lógica (I-5), transición de estados (I-6), techo 540 s (I-10), siembra en `NuncaVerificado`; integración del descubrimiento USB contra adaptador simulado; e2e de alta (UF-1) | TC de CU-02, CU-03; I-1..I-6, I-10, I-21 verdes; cobertura por capa |
| Etapa 3 — Monitoreo, salud e históricos (EP-05, CU-04, CU-06, CU-07) | Unitarias de reglas de derivación versionadas (I-14), calidad de muestra y nulos (I-17), disparo sin `LB`, derivados de prueba de batería y comparabilidad (I-15, I-16), aptitud para tendencia (I-9); integración de sondeo y agregación con min/max; e2e de monitoreo en vivo (UF-3) | TC de CU-04, CU-06, CU-07; I-8, I-9, I-14..I-17 verdes; alerta a 3 sondeos (N-09) |
| Etapa 4 — Verificación y ciclo de vida (EP-06, CU-05, CU-08, CU-09, CU-10) | Unitarias del bloqueo por verificación (I-11), vencimiento de verificación (I-12), acción referida a versión de política (I-13), ciclo forzado, cuadre de costos (I-18) y `Dinero` con moneda y fecha; integración del recambio (cierre/apertura de vigencia, I-3); e2e del camino de apagado de UF-8 contra adaptador simulado; escenario E-9 para CU-09 | TC de CU-05, CU-08, CU-09, CU-10; I-3, I-11..I-13, I-18 verdes; F-3 declarado no automatizable |
| Etapa 5 — Integración e informes (EP-07, CU-11, CU-12) | Integración de la API de ingesta en los cuatro caminos (201/200/409/422, I-19); unitarias de idempotencia y coherencia temporal (I-18, coherencia); test del informe de período con cobertura y advertencia (I-20) y del costo por año en USD | TC de CU-11, CU-12; I-19, I-20 verdes; idempotencia 100 % (N-23) |

## 6. Recursos

- **Personas.** Un único desarrollador en el rol combinado de QA / SDET / Dev (Mini-Plan §1). No hay equipo de QA separado; la ejecución de la suite la automatiza el pipeline.
- **Ambientes.** Desarrollo: Dev Container en la máquina del desarrollador. CI: `ubuntu-latest` con .NET 10, contenedor `linux/amd64`. Producción: contenedor en el host `i7infra` con el USB anclado por ruta física. Sin staging.
- **Datasets.** Los ocho escenarios sintéticos E-1..E-8 del Intake §20, versionados con el código; el escenario E-9 de sustitución del SAI se agrega en la Etapa 4.
- **Herramientas.** Las realizaciones declaradas en `Estrategia-Testing-v1.0.md §3`: framework unitario (xUnit), aserciones fluidas (FluentAssertions), integración EF Core contra SQLite físico, host web de pruebas (WebApplicationFactory), arnés de componentes Blazor (bUnit), driver e2e (Playwright), y el adaptador de conexión simulado como pieza de aislamiento.
