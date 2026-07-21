# Sai-Service-Core

| Campo | Valor |
| --- | --- |
| Solución | Sai-Service-Core |
| Versión del documento | 1.0 |
| Estado | Borrador |
| Fecha | 2026-07-21 |
| Stack principal | .NET 10 + Blazor (interactive server) + Entity Framework Core + SQLite |
| Composición | 1 proyecto (caso degenerado) |
| Proyecto principal | Sai-Service-Core |
| Documento | README raíz de la solución |
| Autor | Orquestador SDD (AG-ROOT) |

Este README es el ancla del árbol de documentación SDD de la solución. Presenta la identidad, la composición, el stack y el mapa navegable de cada categoría, y delega el detalle a las carpetas numeradas. No replica contenido: cuando un tema tiene una categoría propia, este documento enlaza y no copia.

---

## 1. Identidad de la solución

Sai-Service-Core es un servicio web propio, dockerizado y exclusivamente Linux, que protege un servidor doméstico o de laboratorio respaldado por un SAI (Sistema de Alimentación Ininterrumpida) que se comunica por USB. El monitoreo básico y el apagado ordenado ya los resuelve NUT (`upsmon` + `upssched`); esta solución cubre lo que NUT y ninguna herramienta libre o comercial resuelven para este equipo: garantizar el reencendido automático del host tras un corte, construir el histórico de salud de la batería, modelar el ciclo de vida de los equipos y exponer un panel y una API REST de administración.

La propuesta de valor tiene dos promesas centrales. La primera es de seguridad: el servicio garantiza que el host vuelva a encenderse solo tras un corte y se niega a apagarlo mientras no pueda probarlo, porque la BIOS solo dispara el autoencendido cuando detecta una transición de ausencia a presencia de energía y, si el apagado se cancela a mitad de camino, el host puede quedar apagado indefinidamente. La segunda es de conocimiento: el servicio calcula por sí mismo una tendencia de salud de batería a partir de la caída de tensión durante el autotest, con procedencia y límites declarados, porque el equipo relevado nunca emite la bandera «replace battery» y un monitoreo convencional no alertaría jamás.

La audiencia es acotada y bien definida: un administrador único que combina los roles de propietario, implementador y beneficiario sobre el host Linux `i7infra`. La solución también admite una fuente externa que empuja intervenciones por la API REST con clave de idempotencia. No hay gestión de usuarios ni jerarquía de roles: es una decisión de alcance explícita, no una omisión.

Un rasgo transversal de diseño atraviesa toda la solución y explica varias de sus reglas: nada se afirma sin evidencia. Toda acción sobre el SAI se valida por su efecto observado y no por la ausencia de error; cada valor almacenado declara su procedencia; y la política de apagado se degrada a modo de solo alerta mientras alguno de sus supuestos no esté verificado. La documentación de las categorías 02, 05 y 08 desarrolla estas reglas en detalle.

---

## 2. Proyectos de la solución

La solución es un caso degenerado: un único proyecto, que es a la vez el principal. La tabla refleja el `SOLUTION-MANIFEST-Sai-Service-Core-v1.0` sin divergencias.

| Proyecto | Tipo D8 | Rol | Dependencias | Redistribuible |
| --- | --- | --- | --- | --- |
| Sai-Service-Core (principal) | web-monolith | Servicio web único que monitorea el SAI, decide y ejecuta el apagado ordenado, administra el ciclo de vida de los equipos y expone panel y API REST | — | false |

El nombre de código del proyecto es `SAI.Service.Core`, materializado como cinco assemblies internos (`Domain`, `Application`, `Infrastructure`, `Api`, `Web`) bajo `src/SAI.Service.Core/`. Esas capas son internas al único proyecto D8, no proyectos de la solución. Al ser un caso degenerado, el layout de la documentación está aplanado: las categorías 00 a 11 viven directo bajo `SDD/Docs/`, sin el subnivel `Proyectos/<Nombre>/` ni la carpeta `Solucion/`, y no hay vista ni pipeline de solución.

---

## 3. Stack y composición

| Componente | Elección | Versión mínima |
| --- | --- | --- |
| Lenguaje y runtime | C# sobre .NET | 10.0 (sin fallback a versiones anteriores) |
| Framework de UI | Blazor, render mode interactive server | .NET 10.0 |
| Librería de componentes | MudBlazor | — |
| Acceso a datos | Entity Framework Core, proveedor SQLite | — |
| Motor de base de datos | SQLite | — |
| Acceso al SAI | NUT (`nutdrv_qx` + `upsd`), consumido por el adaptador de conexión | 2.8.0 |
| Empaquetado | Contenedor Docker sobre Linux | Docker Engine 24.0 |
| Entorno de desarrollo | Dev Container (spec containers.dev); el SDK vive dentro del contenedor | Docker en el host |

Plataformas soportadas (§17 P.9 del intake). Toda combinación no listada se considera no soportada.

| Dimensión | Soportado | Versión mínima |
| --- | --- | --- |
| Sistema operativo del host | Linux x86-64 | Kernel 6.1 |
| Arquitectura de CPU | `linux/amd64` | — |
| Runtime | .NET | 10.0 |
| Contenedores | Docker Engine | 24.0 |
| Navegadores del panel | Chromium / Google Chrome; Mozilla Firefox | Chromium 120; Firefox 121 |
| Acceso al SAI | NUT | 2.8.0 (relevado: 2.8.1) |

No se soporta Windows ni macOS como host de producción, ni `linux/arm64`, ni Safari, ni el acceso al panel desde fuera de la LAN, ni equipos SAI que `nutdrv_qx` no cubra.

Internamente, el único proyecto se organiza en cinco capas (`Domain`, `Application`, `Infrastructure`, `Api`, `Web`) bajo `src/SAI.Service.Core/`. El acceso al SAI se aísla detrás de un adaptador de conexión con dos implementaciones —una real sobre NUT y una simulada—, de modo que las políticas de apagado y la lógica de dominio se prueban sin hardware ni riesgo. El detalle de esta estructura y sus decisiones está en [05-Arquitectura-Tecnica](05-Arquitectura-Tecnica/).

---

## 4. Mapa de la documentación

Cada categoría existente bajo `SDD/Docs/` con su propósito y enlace relativo. Los enlaces resuelven a carpetas verificadas.

| Categoría | Propósito | Responsable | Enlace |
| --- | --- | --- | --- |
| 00-Contexto | Visión, alcance, roadmap y compatibilidad de plataformas | AG-00 | [00-Contexto](00-Contexto/) |
| 01-Necesidades-Negocio | Necesidades de negocio y su justificación | AG-01 | [01-Necesidades-Negocio](01-Necesidades-Negocio/) |
| 02-Especificacion-Funcional | Casos de uso, reglas de negocio y contrato funcional | AG-02 | [02-Especificacion-Funcional](02-Especificacion-Funcional/) |
| 03-UX-UI-DX | Diseño de experiencia del panel y del flujo de administración | AG-03 | [03-UX-UI-DX](03-UX-UI-DX/) |
| 05-Arquitectura-Tecnica | Arquitectura de la solución, ADRs, contratos REST y modelo de datos | AG-05 | [05-Arquitectura-Tecnica](05-Arquitectura-Tecnica/) |
| 06-Backlog-Tecnico | Backlog técnico derivado de la especificación | AG-06 | [06-Backlog-Tecnico](06-Backlog-Tecnico/) |
| 07-Plan-Sprint | Planes de sprint y secuenciación del trabajo | AG-07 | [07-Plan-Sprint](07-Plan-Sprint/) |
| 08-Calidad-Y-Pruebas | Estrategia de calidad, testing, cobertura y Definition of Done | AG-08 | [08-Calidad-Y-Pruebas](08-Calidad-Y-Pruebas/) |
| 09-Devops | Versionado, ramas, pipeline CI/CD, entornos y publicación de imagen | AG-09 | [09-Devops](09-Devops/) |
| 11-Examples | Ejemplos progresivos de uso e integración | AG-11 | [11-Examples](11-Examples/) |

Dos categorías del catálogo estándar se omiten en esta solución, de forma deliberada:

- 04-Prompts-AI: omitida porque el proyecto no usa LLM en su producto ni en su operación. No hay prompts de IA que documentar.
- 10-Developer-Guide: omitida por decisión de ADR-23. El onboarding del desarrollador se consolida en la sección 6 de este README y refiere a las categorías 08 y 09.

Artefactos complementarios, fuera del árbol numerado:

| Artefacto | Propósito | Enlace |
| --- | --- | --- |
| Maqueta aprobada | Prototipo navegable de las pantallas del panel | [Maquetas/Sai-Service-Core](../Maquetas/Sai-Service-Core/) |
| Auditorías por fase | Registros de auditoría de las fases A a G del proceso SDD | [Audit](Audit/) |

### 4.1 Cadena de trazabilidad

La documentación se lee como una cadena en la que cada eslabón justifica al siguiente, sin saltos: la visión y el alcance (00) fundan las necesidades de negocio (01), que se concretan en casos de uso y reglas de negocio (02); esos requisitos y la maqueta (03) alimentan la arquitectura y sus decisiones registradas como ADRs (05); la arquitectura se descompone en backlog técnico (06) y se secuencia en planes de sprint (07); cada elemento se valida contra la estrategia de calidad y sus pruebas (08) y se despliega por el pipeline (09), con los ejemplos (11) como verificación práctica del camino feliz. Este README no repite esos contenidos: es el índice que los enlaza.

---

## 5. Flujo de lectura recomendado por audiencia

Cada audiencia tiene un punto de entrada y un recorrido propios; seguir el orden sugerido evita perderse en documentación que no le corresponde. Las categorías se citan por su número; los enlaces están en la sección 4.

| Rol | Orden recomendado | Por qué |
| --- | --- | --- |
| Administrador / operador | 00 → 03 → maqueta → 02 | Necesita entender qué protege el servicio y cómo se opera el panel antes de la instalación; la maqueta muestra las pantallas reales y 02 detalla los flujos de alta, verificación y monitoreo. |
| Desarrollador que retoma el proyecto | 00 → 02 → 05 → 06 → 07 → 11 | Necesita el contexto, luego la especificación funcional, después la arquitectura y sus ADRs, y por último el backlog, el plan de sprint y los ejemplos para ubicarse en el trabajo pendiente y en las convenciones. |
| Revisor de arquitectura | 00 → 05 → 09 → 08 | Necesita la visión y el alcance, luego la arquitectura completa con sus decisiones, el pipeline y los entornos, y la estrategia de calidad para validar coherencia técnica de punta a punta. |
| Responsable de calidad / pruebas | 02 → 08 → 05 | Necesita los requisitos y reglas de negocio, luego la estrategia de testing, cobertura y casos referenciales, y la arquitectura para ubicar los puntos de verificación. |

---

## 6. Cómo contribuir y cómo regenerar la documentación

Toda la documentación SDD se genera con el orquestador SDD y sus subagentes AG-00 a AG-11 a partir de los insumos de intake (`SOLUTION-INTAKE` y `SOLUTION-MANIFEST`). Para regenerar una categoría, se corre el orquestador apuntando al subagente correspondiente; para regenerar este README raíz, se invoca al subagente AG-ROOT. Las contribuciones manuales se limitan a los insumos de intake y a las correcciones marcadas en las auditorías de la carpeta [Audit](Audit/); el contenido derivado no se edita a mano para no divergir de su fuente.

Por tratarse de un proyecto interno de un único desarrollador y sin consumidores externos, esta solución no incluye los archivos satélite `CHANGELOG.md`, `CONTRIBUTING.md` ni `LICENSE.md` en `SDD/Docs/`: el control de cambios vive en la cabecera de cada documento y en la sección 10 de este README, y el proceso de contribución es el descrito en esta sección. Tampoco hay presupuesto ni fecha objetivo impuestos; el ritmo lo marcan las etapas de validación humana, y cada etapa cierra con una verificación del administrador antes de habilitar la siguiente.

### 6.1 Onboarding del desarrollador (ADR-23)

Esta subsección concentra el onboarding que ADR-23 consolida en el README raíz en lugar de una categoría 10 separada. No duplica el detalle: refiere a las categorías que lo contienen.

- Abrir el Dev Container: el único requisito del host es Docker; el SDK de .NET 10 vive dentro del contenedor. Se abre con «Reopen in Container» en el editor o con `devcontainer up`. La depuración va por `.vscode/launch.json` con F5, nunca por scripts sueltos. Detalle de entornos en [09-Devops](09-Devops/).
- Ejecutar el servicio: en desarrollo, el servicio corre dentro del Dev Container; en producción, es el contenedor corriendo en el host `i7infra`, con el dispositivo USB compartido por ruta física de puerto vía regla `udev`. No hay ambiente de staging. Ver [09-Devops](09-Devops/).
- Correr las pruebas: la estrategia de testing, la cobertura mínima exigida y los casos referenciales están en [08-Calidad-Y-Pruebas](08-Calidad-Y-Pruebas/). El adaptador de conexión simulado permite probar políticas sin hardware ni riesgo.
- Convenciones de versionado y ramas: el versionado sigue SemVer 2.0.0 y los mensajes de commit siguen Conventional Commits, sin excepciones. El pipeline CI/CD tiene quality gates bloqueantes. Todo el detalle en [09-Devops](09-Devops/).

---

## 7. Estado actual y roadmap

Todas las categorías están en estado Borrador y fueron auditadas por fase (ver [Audit](Audit/)). El roadmap detallado no se replica acá: vive en [00-Contexto/Roadmap-Producto-v1.0.md](00-Contexto/Roadmap-Producto-v1.0.md).

La carpeta de auditoría reúne un registro por fase del proceso SDD: contexto y necesidades de negocio (A), especificación funcional y UX/UI (B), arquitectura técnica (C), backlog y plan de sprint (D), calidad y pruebas (E), devops (F) y ejemplos (G). Cada registro documenta hallazgos y correcciones aplicadas sobre la categoría correspondiente, y es la referencia para entender por qué un documento tiene su forma actual.

| Categoría | Estado | Versión vigente |
| --- | --- | --- |
| 00-Contexto | Borrador | 1.0 |
| 01-Necesidades-Negocio | Borrador | 1.0 |
| 02-Especificacion-Funcional | Borrador | 1.0 |
| 03-UX-UI-DX | Borrador | 1.0 |
| 05-Arquitectura-Tecnica | Borrador | 1.0 |
| 06-Backlog-Tecnico | Borrador | 1.0 |
| 07-Plan-Sprint | Borrador | 1.0 |
| 08-Calidad-Y-Pruebas | Borrador | 1.0 |
| 09-Devops | Borrador | 1.0 |
| 11-Examples | Borrador | 1.0 |

---

## 8. Glosario rápido

Términos esenciales del dominio, una línea cada uno. El glosario completo está en el intake §12 y en la categoría 03.

| Término | Definición breve |
| --- | --- |
| SAI | Sistema de Alimentación Ininterrumpida: equipo que sostiene la alimentación del host cuando falla la red eléctrica. |
| Equipos | Las unidades físicas que la solución administra en su ciclo de vida: el host protegido, el SAI y la batería. |
| Apagado ordenado | Apagar el sistema operativo deteniendo contenedores y sincronizando discos antes de perder la alimentación. |
| Reencendido automático | Que el host arranque solo al restablecerse la energía, lo que exige que el SAI corte su salida aunque el host ya esté apagado. |
| Ciclo forzado | Modalidad en la que, iniciada la secuencia de apagado, el corte del SAI no se cancela aunque vuelva la red. |
| Adaptador de conexión | Componente que dialoga con el SAI a través de NUT; tiene una implementación real y una simulada para pruebas sin hardware. |
| Procedencia | Origen declarado de cada valor almacenado: medido, derivado, estimadoPorDriver, declarado, imputado o noCalculable. |
| Política de apagado | Configuración versionada que define la modalidad de respuesta ante un corte, con sus supuestos requeridos. |
| Verificación | Prueba del estado de un supuesto del que depende la política, con estados NuncaVerificado, Verificado, Vencido o Refutado. |
| Salud de batería | Tendencia relativa en unidades arbitrarias, derivada de la caída de tensión durante el autotest a carga igualada; no es un porcentaje ni un SoH. |
| Microcorte | Parpadeo breve de la red: transición OL→OB seguida de OB→OL en menos de 60 s (regla vigente v2). |
| Agregado | Resumen de una ventana de tiempo con su función, cantidad de muestras y cobertura obligatoria. |
| Ingesta idempotente | Ingesta por API en la que reenviar el mismo hecho no lo duplica, gracias a la clave `X-Idempotency-Key`. |
| Vínculo temporal | Relación «qué estuvo con qué, cuándo» modelada como entidad con intervalo (MontajeBateria, CoberturaHost). |
| Baja lógica | Marcar una unidad como retirada sin borrarla; el borrado físico no existe en este dominio. |
| NUT | Network UPS Tools: el ecosistema de software libre que ya dialoga con el equipo. |

---

## 9. Contacto y responsables

| Rol | Responsable | Canal |
| --- | --- | --- |
| Administrador único (propietario, implementador y beneficiario) | `usr-admin`, administrador del host `i7infra` | Repositorio local `DEV/SAI.Service.Core` |
| Fuente externa de intervenciones | Sistema GMAO externo (`fd-gmao-externo`) | API REST `POST /api/v1/intervenciones` con `X-Idempotency-Key` |
| Autoría de la documentación SDD | Orquestador SDD (subagentes AG-00 a AG-11 y AG-ROOT) | Proceso de regeneración descrito en la sección 6 |

---

## 10. Control de cambios

El versionado de este README sigue la regla D5 del proceso SDD: arranca en 1.0 y avanza al regenerarse. El estado pertenece al enum cerrado Borrador, Propuesto, Aprobado, Vigente, Superado o Archivado.

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-21 | README raíz inicial de la solución Sai-Service-Core (caso degenerado, layout aplanado). Refleja el manifiesto v1.1 y el intake v1.2. |
