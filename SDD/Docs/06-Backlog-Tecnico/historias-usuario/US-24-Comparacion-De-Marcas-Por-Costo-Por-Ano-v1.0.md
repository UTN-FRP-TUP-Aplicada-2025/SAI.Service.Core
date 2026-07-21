# US-24 — Comparación de marcas por costo por año de servicio en USD

**Proyecto:** Sai-Service-Core
**Documento:** US-24-Comparacion-De-Marcas-Por-Costo-Por-Ano-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Should
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero comparar modelos de batería por su costo por año de servicio normalizado a USD, para decidir qué marca comprar la próxima vez con datos y no con impresiones.

## 2. Contexto
NB-06 y NB-04: CU-12 agrupa las `FichaVidaUtil` cerradas por `ModeloBateria` y calcula el `costoPorAnioDeServicio` normalizado a USD, junto con `cumplioExpectativa` y `desvio`. Es el quinto diferenciador del intake: comparar por desempeño real observado. Con un solo modelo con ficha cerrada, el sistema advierte que se necesitan al menos dos para concluir (FA-1 de CU-12, métrica M-04).

## 3. Criterios de aceptación
- Given al menos dos modelos con ficha de vida útil cerrada, When el administrador solicita la comparación, Then el sistema agrupa por modelo y presenta el costo por año de servicio normalizado a USD, el cumplimiento de expectativa y el desvío.
- Given un solo modelo con ficha cerrada, When se solicita la comparación, Then el sistema la presenta pero advierte que se necesitan al menos dos modelos para concluir.
- Given importes en monedas y fechas distintas, When se comparan, Then cada uno se normaliza a USD marcado como derivado con su fuente de cotización, sin perder el valor original.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-06, NB-04 |
| CU cubiertos | CU-12, CU-08 |
| BT derivadas | BT-27, BT-30 |
| Tests previstos | acceptance/AT-24-comparacion-marcas |

## 5. Prioridad y estimación
Should: cierra el propósito de «decidir la próxima compra con datos» (F-23); el MVP funciona sin la comparación agregada. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-12 y CU-08
- [x] NB de origen (NB-06, NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-27 (ficha) y BT-30, planificadas antes

## 7. Notas y supuestos
El sistema no presenta magnitudes de salud cuantitativas ni conformidad con IEEE 1188 (E-08): la comparación es por costo por año y por cumplimiento de expectativa, no por SoH. Depende de que haya al menos dos fichas cerradas (US-19).
