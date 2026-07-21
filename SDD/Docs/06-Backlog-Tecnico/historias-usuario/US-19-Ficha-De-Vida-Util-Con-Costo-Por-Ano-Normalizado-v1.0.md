# US-19 — Ficha de vida útil con costo por año normalizado

**Proyecto:** Sai-Service-Core
**Documento:** US-19-Ficha-De-Vida-Util-Con-Costo-Por-Ano-Normalizado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Should
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el recambio proyecte una ficha de vida útil con el costo por año de servicio normalizado, para saber cuánto duró de verdad una batería y cuánto costó por año.

## 2. Contexto
NB-06 y NB-04: CU-08 proyecta la `FichaVidaUtil` con días en servicio, cumplimiento de expectativa contra la vida de flotación esperada, eventos soportados, tendencia y `costoPorAnioDeServicio` normalizado a USD (marcado como derivado con su fuente de cotización). Comparar 52.000 ARS de 2026 con 180.000 ARS de 2024 no significa nada sin normalizar (restricción macroeconómica del intake §10).

## 3. Criterios de aceptación
- Given un recambio registrado que cierra una vigencia de batería, When se proyecta la ficha, Then incluye días en servicio, cumplimiento de expectativa y `costoPorAnioDeServicio` normalizado a USD marcado como derivado con su fuente de cotización.
- Given una batería cuya vida real difiere de la esperada, When se calcula la ficha, Then registra `cumplioExpectativa` y el `desvio` contra la `vidaFlotacionEsperada`.
- Given un importe original en moneda local con su fecha, When se normaliza, Then el equivalente en USD viaja marcado como derivado y nunca reemplaza al valor original.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-06, NB-04 |
| CU cubiertos | CU-08, CU-12 |
| BT derivadas | BT-27 |
| Tests previstos | acceptance/AT-19-ficha-vida-util |

## 5. Prioridad y estimación
Should: es el insumo de la comparación de marcas (US-24) y de la decisión de compra; el MVP puede cerrar sin la ficha proyectada. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-08 y CU-12
- [x] NB de origen (NB-06, NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-27 y de US-18, planificadas antes

## 7. Notas y supuestos
El costo normalizado sin marcar es el mismo error que la carga de batería sin marcar (intake §10): siempre derivado y con fuente. Se necesitan al menos dos fichas cerradas para una comparación concluyente (métrica M-04).
