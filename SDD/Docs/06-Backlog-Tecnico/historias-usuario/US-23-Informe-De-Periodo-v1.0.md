# US-23 — Informe de período

**Proyecto:** Sai-Service-Core
**Documento:** US-23-Informe-De-Periodo-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Should
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero un informe de período que reúna dispositivos activos, cobertura, baterías intervinientes, intervenciones, costos, eventos y calidad de suministro, para cerrar un año con una vista integral de lo que pasó.

## 2. Contexto
NB-04: CU-12 interseca intervalos y devuelve, para un host y período, los dispositivos y su cobertura, los días con y sin protección, las baterías con sus intervalos recortados al período, los costos por tipo, los eventos y la calidad de suministro. Incluye las bajas lógicas; cuando la serie sale de agregados, adjunta cobertura y advertencia (RN-10). Todos los importes con moneda y fecha (RN-07).

## 3. Criterios de aceptación
- Given un host y un período con historia, When el administrador solicita el informe, Then el sistema interseca los intervalos y devuelve dispositivos, cobertura, días con y sin protección, baterías intervinientes, intervenciones y costos, eventos y calidad de suministro.
- Given un período que usa agregados horarios, When se arma el informe, Then las series agregadas adjuntan su cobertura y su advertencia (RN-10).
- Given un período sin datos, When se solicita, Then el sistema informa PERIODO_SIN_DATOS en vez de un informe vacío.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-04 |
| CU cubiertos | CU-12 |
| BT derivadas | BT-12, BT-30 |
| Tests previstos | acceptance/AT-23-informe-periodo |

## 5. Prioridad y estimación
Should: agrega valor de cierre y auditoría, pero el MVP funciona sin el informe consolidado (F-22). 8 SP por la intersección de intervalos y la variedad de dimensiones; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-12
- [x] NB de origen (NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-12 y BT-30, planificadas antes

## 7. Notas y supuestos
Las baterías con vigencia que cruza el borde del período se recortan al intervalo consultado. La comparación de marcas se cubre en US-24.
