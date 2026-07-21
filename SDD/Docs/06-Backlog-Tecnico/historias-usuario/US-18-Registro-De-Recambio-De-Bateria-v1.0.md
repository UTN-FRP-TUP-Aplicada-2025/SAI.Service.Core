# US-18 — Registro de recambio de batería con cierre y apertura de vigencia

**Proyecto:** Sai-Service-Core
**Documento:** US-18-Registro-De-Recambio-De-Bateria-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Should
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero registrar el recambio de batería con su costo, sus hallazgos y su destino final, para que un solo acto cierre la vigencia vieja, abra la nueva y deje trazado quién ejecutó y adónde fue la batería retirada.

## 2. Contexto
NB-04 y NB-06: CU-08 registra la intervención (instante, dispositivo, baterías, proveedor, ejecutor) con costos que deben cuadrar (RN-08), todo importe con moneda y fecha (RN-07), cierra el `MontajeBateria` vigente y abre el nuevo sin hueco, cambia estados (retirada a baja lógica consultable, nueva a en servicio) y registra la `disposicionFinal` para trazabilidad ambiental. Corregir la fecha de un recambio reatribuye automáticamente el histórico afectado sin migrar datos (CL-18).

## 3. Criterios de aceptación
- Given un montaje de batería vigente, When el administrador registra el recambio con costos que cuadran y moneda y fecha, Then el sistema cierra la vigencia vieja, abre la nueva sin hueco y cambia los estados en un solo acto.
- Given una intervención cuyos costos no cuadran (total ≠ repuestos + mano de obra), When se registra, Then el sistema la rechaza (COSTOS_NO_CUADRAN, RN-08).
- Given un importe sin moneda o sin fecha, When se registra, Then el sistema lo rechaza (DINERO_SIN_MONEDA_O_FECHA, RN-07).
- Given una corrección de la fecha de un recambio ya cargado, When se aplica, Then el sistema reatribuye el histórico afectado sin migrar datos.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-04, NB-06 |
| CU cubiertos | CU-08 |
| BT derivadas | BT-11, BT-12, BT-26 |
| Tests previstos | acceptance/AT-18-recambio-bateria |

## 5. Prioridad y estimación
Should: completa el ciclo de vida de la batería; el MVP monitorea y prueba sin registrar el recambio, pero queda incompleto para decidir compras. 8 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-08
- [x] NB de origen (NB-04, NB-06) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-11, BT-12 y BT-26, planificadas antes

## 7. Notas y supuestos
La proyección de la ficha de vida útil se cubre en US-19. La batería dada de baja se puede consultar con todo su historial pero no operar (CL-20).
