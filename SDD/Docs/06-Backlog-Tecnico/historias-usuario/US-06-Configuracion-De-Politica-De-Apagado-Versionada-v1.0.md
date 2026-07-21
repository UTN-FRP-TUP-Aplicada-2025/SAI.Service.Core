# US-06 — Configuración de política de apagado versionada

**Proyecto:** Sai-Service-Core
**Documento:** US-06-Configuracion-De-Politica-De-Apagado-Versionada-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-04 Alta de equipos y políticas de apagado
**Prioridad MoSCoW:** Must
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero configurar la política de apagado creando una versión nueva en vez de editar la vigente, para que cada decisión pasada se siga explicando con la configuración exacta que la produjo.

## 2. Contexto
NB-07 exige políticas versionadas. CU-03 crea una versión nueva inmutable a partir de la vigente ajustando modalidad (`SoloAlerta`, `SoloHost`, `HostLuegoUpsConRetorno`, `CicloForzado`), umbral de disparo, tiempo reservado y cancelación al volver la red. Se valida el techo duro de 540 s (RN-04) y que las verificaciones requeridas estén declaradas. Toda acción de apagado posterior queda ligada a su versión de política (RN-11).

## 3. Criterios de aceptación
- Given una política vigente, When el administrador ajusta parámetros y publica, Then el sistema crea una versión nueva con número incrementado y fecha de vigencia, y conserva las anteriores en el historial.
- Given una configuración con tiempo reservado que supera los 540 s, When el administrador intenta publicar, Then el sistema rechaza la versión (TIEMPO_APAGADO_EXCEDE_TECHO, RN-04).
- Given una modalidad que exige supuestos aún sin verificar, When se publica la versión, Then el sistema la acepta pero deja la modalidad efectiva degradada a `SoloAlerta` y lo indica.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-07 |
| CU cubiertos | CU-03 |
| BT derivadas | BT-16 |
| Tests previstos | acceptance/AT-06-politica-versionada |

## 5. Prioridad y estimación
Must: la política gobierna el camino crítico del apagado; sin versionado no se puede explicar una decisión pasada. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-03
- [x] NB de origen (NB-07) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-16, planificada antes

## 7. Notas y supuestos
Nunca se edita la versión vigente: cada cambio es una versión nueva (ADR-04 append-only aplicado a la configuración). La ejecución de la política se cubre en US-14.
