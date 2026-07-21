# US-10 — Procedencia visible de cada valor

**Proyecto:** Sai-Service-Core
**Documento:** US-10-Procedencia-Visible-De-Cada-Valor-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 3 SP (Fibonacci)

## 1. Historia
Como administrador, quiero saber de qué origen viene cada número que veo, para no construir una conclusión sobre un valor que el driver interpoló.

## 2. Contexto
NB-03 exige procedencia por valor sin excepción (RN-05, ADR-06 `Valor<T>` con `Origen`). CU-04 y CU-06 muestran cada dato declarando si fue `medido`, `derivado`, `estimadoPorDriver`, `declarado`, `imputado` o `noCalculable`. Responde sin leer código la pregunta «¿este número lo midió el aparato o lo calculó el software?». El caso paradigmático es `battery.charge`, que se marca siempre como derivado.

## 3. Criterios de aceptación
- Given un valor mostrado en el panel o en una consulta, When el administrador lo inspecciona, Then el sistema indica su origen (medido, derivado, estimado por driver, declarado, imputado o no calculable).
- Given la carga de batería reportada por el driver, When se muestra, Then aparece marcada explícitamente como derivada.
- Given un derivado que no se puede calcular, When se muestra, Then el sistema lo indica como `noCalculable` con su motivo, en vez de un número inventado.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-03 |
| CU cubiertos | CU-04, CU-06 |
| BT derivadas | BT-08 |
| Tests previstos | acceptance/AT-10-procedencia-visible |

## 5. Prioridad y estimación
Must: la procedencia es un invariante del sistema (I-7, cero excepciones) y la base de su credibilidad. 3 SP porque el modelo `Valor<T>` ya se construye en BT-08; esta US expone la procedencia en la interfaz. Técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-04 y CU-06
- [x] NB de origen (NB-03) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-08, planificada antes

## 7. Notas y supuestos
La procedencia es transversal a todo dato; esta US garantiza su visibilidad en la interfaz. El invariante de «cero valores sin origen» se verifica como test de pipeline (métrica M-05 del intake).
