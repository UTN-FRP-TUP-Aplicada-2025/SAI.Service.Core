# Necesidades de Negocio — Sai-Service-Core

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | Necesidades-Negocio-v1.0.md |
| Versión | 1.0 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Cantidad de NB | 8 |
| Versión del catálogo de NB | 1.2 |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §3, §4, §8, §10, §11; Vision-Producto-v1.0.md; Alcance-Proyecto-v1.0.md |
| Trazabilidad downstream | 02-Especificacion-Funcional (CU previstas); 06-Backlog-Tecnico, 07-Plan-Sprint (priorización MoSCoW); 08-Calidad-Y-Pruebas (criterios de éxito) |

## 1. Propósito

Este índice maestro consolida las necesidades de negocio (NB) del proyecto Sai-Service-Core, derivadas de los dolores reales relevados en el SOLUTION-INTAKE y del marco previo de la categoría 00. Cada NB articula un problema concreto del negocio, para quién, con qué métrica de éxito y con qué prioridad relativa. Las NB no descienden a flujos funcionales (categoría 02) ni a decisiones técnicas (categoría 05).

El proyecto es una solución de un único proyecto (caso degenerado, layout aplanado), de tipo web-monolith: un servicio web que monitorea el equipo de alimentación, decide y ejecuta el apagado ordenado del servidor y su reencendido, administra el ciclo de vida del parque y expone un panel y una interfaz de integración. Un único administrador concentra los roles de propietario, implementador y beneficiario.

## 2. Tabla resumen de NB

| ID | Necesidad | Impacto | Prioridad MoSCoW | CU que la cubren | Estado | Enlace |
| --- | --- | --- | --- | --- | --- | --- |
| NB-01 | Apagado ordenado y reencendido automático garantizado del host | Continuidad del servicio y camino crítico irreversible | Must Have | CU-05, CU-10 | Borrador | [NB-01](Necesidades-De-Negocio/NB-01-Apagado-Ordenado-Y-Reencendido-Garantizado-v1.0.md) |
| NB-02 | Monitoreo en vivo del estado del SAI y alertas de conectividad | Visibilidad operativa e insumo del apagado | Must Have | CU-04, CU-02, CU-06 | Borrador | [NB-02](Necesidades-De-Negocio/NB-02-Monitoreo-En-Vivo-Y-Alertas-v1.0.md) |
| NB-03 | Historia trazable de métricas con procedencia y corrección retroactiva | Confiabilidad de las conclusiones y auditabilidad | Must Have | CU-06, CU-02, CU-04 | Borrador | [NB-03](Necesidades-De-Negocio/NB-03-Historia-Trazable-Con-Procedencia-v1.0.md) |
| NB-04 | Gestión del ciclo de vida del parque de dispositivos y baterías | Trazabilidad del parque y decisión de compra con datos | Must Have | CU-02, CU-08, CU-09, CU-12 | Borrador | [NB-04](Necesidades-De-Negocio/NB-04-Ciclo-De-Vida-Del-Parque-v1.0.md) |
| NB-05 | Seguridad operativa: arranque seguro, bloqueo por verificación y validación por efecto observado | Garantía de no apagar sin poder probar el reencendido | Must Have | CU-01, CU-05, CU-10, CU-04 | Borrador | [NB-05](Necesidades-De-Negocio/NB-05-Seguridad-Operativa-Bloqueo-Por-Verificacion-v1.0.md) |
| NB-06 | Evaluación de la salud de las baterías por tendencia, con confianza y reservas | Planificación del recambio y diferenciador de valor | Must Have | CU-07, CU-08, CU-12 | Borrador | [NB-06](Necesidades-De-Negocio/NB-06-Evaluacion-De-Salud-De-Baterias-v1.0.md) |
| NB-07 | Configuración de políticas de apagado versionadas | Control y explicabilidad del camino crítico | Must Have | CU-03, CU-05 | Borrador | [NB-07](Necesidades-De-Negocio/NB-07-Configuracion-De-Politicas-De-Apagado-v1.0.md) |
| NB-08 | Ingesta automatizada de intervenciones desde un sistema externo | Alimentación automática del histórico con integridad ante reintentos | Must Have | CU-11, CU-08 | Borrador | [NB-08](Necesidades-De-Negocio/NB-08-Ingesta-Automatizada-De-Intervenciones-v1.0.md) |

Nota sobre MoSCoW: el SOLUTION-INTAKE §4 marca como Must Have el conjunto F-01 a F-20 (primera entrega) y remata que sin ese conjunto el servicio no cumple ninguno de sus dos propósitos. Las ocho NB recogen ese núcleo Must Have: NB-01 a NB-05 y NB-07 cubren el apagado, el monitoreo, la historia, el ciclo de vida, la seguridad operativa y las políticas; NB-06 cubre F-16 y F-17 (prueba de batería y veredicto de salud, el segundo propósito del servicio); y NB-08 cubre F-20 (API de ingesta idempotente). Los refinamientos de prioridad Should del intake sobre el ciclo de vida (F-21 sustitución del SAI) y sobre los informes derivados y la comparación de marcas (F-22, F-23) se documentan dentro de NB-04 y NB-06, como alcance posterior sobre el mismo modelo fundacional, sin degradar la prioridad Must Have de esas NB.

Nota sobre autenticación (F-15): el control de acceso del administrador único es capacidad Must Have del intake (F-15) y se materializa en CU-01 de 02-Especificacion-Funcional. Por decisión de alcance no tiene una NB propia —hay un único administrador y ninguna gestión de usuarios ni de roles que modelar (exclusión E-05)— y se trata como habilitador transversal de seguridad anclado a NB-05 (Seguridad operativa), donde queda documentado de forma explícita en su §1. No es una omisión: es una decisión registrada.

## 3. Mapa de dependencias entre NB

Dependencias declaradas (acíclicas, ninguna NB depende de más de 3 otras):

| NB | Depende de | Es prerequisito de |
| --- | --- | --- |
| NB-01 | NB-02, NB-05, NB-07 | (ninguna) |
| NB-02 | NB-03, NB-04 | NB-01, NB-05, NB-07 |
| NB-03 | NB-04 | NB-02, NB-06, NB-08 |
| NB-04 | (ninguna) | NB-02, NB-03, NB-06, NB-08 |
| NB-05 | NB-02, NB-07 | NB-01 |
| NB-06 | NB-03, NB-04 | (ninguna) |
| NB-07 | NB-02 | NB-01, NB-05 |
| NB-08 | NB-03, NB-04 | (ninguna) |

Orden topológico de construcción sugerido: NB-04 → NB-03 → NB-02 → NB-07 → NB-05 → NB-01 → NB-06 → NB-08. Ningún ciclo: NB-04 es la raíz sin dependencias y NB-01 es el sumidero del camino crítico.

## 4. Trazabilidad agregada

Upstream (de dónde vienen las NB):

| Fuente | Secciones | NB que alimenta |
| --- | --- | --- |
| SOLUTION-INTAKE §1 (idea y problema) | Cuatro carencias y el problema crítico del reencendido | NB-01 a NB-08 |
| SOLUTION-INTAKE §4 (alcance MoSCoW) | F-04, F-05, F-06 a F-20 (Must); F-21 a F-25 (Should) | NB-01 a NB-08 |
| SOLUTION-INTAKE §8 (métricas de éxito) | M-01 a M-05 (targets propuestos, P-01) | NB-01, NB-03, NB-04, NB-05, NB-06 |
| SOLUTION-INTAKE §7 (casos límite) | CL-01 a CL-27 | NB-01 a NB-08 |
| SOLUTION-INTAKE §11 (riesgos) | R-01, R-02, R-03, R-06, R-09, R-12, R-13, R-14 | NB-01, NB-02, NB-03, NB-05, NB-06 |
| Vision-Producto-v1.0.md | §1, §3, §4, §5, §8 | NB-01 a NB-08 |
| Alcance-Proyecto-v1.0.md | §3, §4 (C-01 a C-16), §8 | NB-01 a NB-08 |

Downstream (adónde van las NB):

| Categoría destino | Qué recibe |
| --- | --- |
| 02-Especificacion-Funcional | Los CU que cubren cada NB, ahora vigentes y en estado aprobada: los 12 CU alineados uno a uno con los flujos de usuario UF-1 a UF-10 más la autenticación (CU-01) |
| 06-Backlog-Tecnico, 07-Plan-Sprint | La priorización MoSCoW de cada NB ordena el backlog y las etapas |
| 08-Calidad-Y-Pruebas | Cada criterio de éxito de la §5 de cada NB es input de los criterios de aceptación |

## 5. Catálogo de CU vigentes

Los 12 CU vigentes en 02-Especificacion-Funcional, alineados uno a uno con los flujos de usuario UF-1 a UF-10 más la autenticación; todos en estado aprobada. La categoría 02 adoptó esta granularidad de 12 CU por flujo en lugar de los CU de grano fino que el catálogo 01 había anticipado; cada necesidad conserva cobertura completa (ver la ambigüedad de granularidad declarada en 02 §6).

| CU | Título vigente en 02 | NB que la cubre |
| --- | --- | --- |
| CU-01 | Autenticación y gestión de la sesión del administrador | NB-05 |
| CU-02 | Alta del parque y puesta en marcha | NB-02, NB-03, NB-04 |
| CU-03 | Configuración de políticas de apagado versionadas | NB-07 |
| CU-04 | Monitoreo en vivo del estado del SAI | NB-02, NB-03, NB-05 |
| CU-05 | Ejecución del apagado ordenado ante corte sostenido | NB-01, NB-05, NB-07 |
| CU-06 | Históricos y gráficas de evolución del suministro | NB-02, NB-03 |
| CU-07 | Prueba de batería y veredicto de salud | NB-06 |
| CU-08 | Registro de recambio de batería y ficha de vida útil | NB-04, NB-06, NB-08 |
| CU-09 | Reparación y sustitución del SAI con cobertura suplente | NB-04 |
| CU-10 | Ventana de mantenimiento y verificación de supuestos | NB-01, NB-05 |
| CU-11 | Ingesta automatizada de intervenciones | NB-08 |
| CU-12 | Informe de período y comparación de marcas | NB-04, NB-06 |

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Índice maestro inicial con 8 NB derivadas del SOLUTION-INTAKE y de la categoría 00 |
| 1.1 | 2026-07-20 | Corrección de prioridad MoSCoW de NB-06 y NB-08 a Must Have por alineación con SOLUTION-INTAKE §4 (F-16, F-17, F-20) tras audit de Fase A; catálogo de NB a 1.1 |
| 1.2 | 2026-07-20 | Reconciliación de trazabilidad con los 12 CU vigentes de 02 tras audit de Fase B: tabla resumen §2, matriz §4 y catálogo §5 reapuntados a los CU reales (estado aprobada); nota de autenticación (F-15, CU-01) agregada en §2; catálogo de NB a 1.2 |
