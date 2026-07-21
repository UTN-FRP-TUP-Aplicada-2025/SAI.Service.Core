# US-22 — Rechazo de conflictos de idempotencia e invariantes rotos

**Proyecto:** Sai-Service-Core
**Documento:** US-22-Rechazo-De-Conflictos-De-Idempotencia-E-Invariantes-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Must
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como sistema externo de gestión de mantenimiento, quiero recibir un rechazo claro cuando reenvío una clave con un cuerpo distinto o cuando mi cuerpo rompe un invariante, para no creer que una corrección se aplicó cuando no lo hizo.

## 2. Contexto
NB-08: CU-11 devuelve 409 (`conflicto_idempotencia`) cuando la misma clave llega con un cuerpo distinto, con `sha256Original`, `sha256Recibido` y `accionSugerida` (RN-09); nunca 200, porque «devolver 200 sería peor que duplicar: el emisor creería que su corrección se aplicó» (CL-21). Devuelve 422 cuando los costos no cuadran (RN-08), un importe no tiene moneda o fecha (RN-07), o el hecho se fecha después de la baja (coherencia temporal, RN-12). Los errores viajan en el formato de error estándar del contrato REST (ADR-17).

## 3. Criterios de aceptación
- Given una clave ya procesada, When llega la misma clave con un cuerpo distinto, Then el servicio responde 409 con las huellas sha256 original y recibida y una acción sugerida, y nunca 200.
- Given una intervención cuyos costos no cuadran o un importe sin moneda o fecha, When se envía, Then el servicio responde 422 (`validacion`) en el formato de error estándar del contrato REST.
- Given una intervención fechada después de la baja del dispositivo, When se envía, Then el servicio responde 422 (`coherencia_temporal`).

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-08 |
| CU cubiertos | CU-11 |
| BT derivadas | BT-03, BT-28 |
| Tests previstos | acceptance/AT-22-conflicto-invariantes; sample `ingesta-gmao/` |

## 5. Prioridad y estimación
Must: es el invariante que la ingesta externa rompe primero; sin él los costos agregados quedan mal en silencio (CL-22). 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-11
- [x] NB de origen (NB-08) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-28; el contrato de rectificación del 409 se cierra en BT-03

## 7. Notas y supuestos
La `accionSugerida` del 409 apunta a un endpoint de rectificación cuyo contrato queda por cerrar en BT-03 (ADR-21, P-05); hasta entonces la corrección de un hecho ingresado se marca como pendiente.

## 8. Control de cambios
| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial de la historia. |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D. |
