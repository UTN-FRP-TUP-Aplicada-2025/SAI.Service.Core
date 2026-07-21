# US-13 — Veredicto de salud con confianza y comparación a carga igualada

**Proyecto:** Sai-Service-Core
**Documento:** US-13-Veredicto-De-Salud-Con-Confianza-Y-Comparacion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero un veredicto de salud de la batería con su confianza y su reserva, comparado contra la línea base a carga igualada, para planificar el recambio antes de que falle sin que el sistema afirme más de lo que puede probar.

## 2. Contexto
NB-06 pide un veredicto propio del servicio (el equipo no emite ninguno), como tendencia relativa y no como porcentaje (ADR-13, F-17). CU-07 calcula los derivados (reposo, mínima, caída, caída relativa, recuperación) marcando lo no calculable con su motivo, compara contra la línea base evaluando comparabilidad, y emite el veredicto con confianza (baja hasta ≥4 pruebas comparables) y reserva por falta de sensor de temperatura. Si la carga concurrente cambió más allá de la tolerancia, la prueba se marca `comparable: false` y no entra en la tendencia (CL-26).

## 3. Criterios de aceptación
- Given una prueba con la serie recogida y carga concurrente dentro de tolerancia, When se calcula, Then el sistema emite el veredicto como tendencia relativa comparada contra la línea base a carga igualada, con confianza y reserva explícitas.
- Given menos de cuatro pruebas comparables acumuladas, When se emite el veredicto, Then la confianza se declara baja.
- Given una prueba con carga concurrente fuera de tolerancia, When se evalúa, Then se registra pero se marca `comparable: false` y se excluye de la tendencia (RN-06).
- Given un derivado que no se puede calcular con los datos disponibles, When se emite el veredicto, Then aparece como no calculable con su motivo, sin cifra inventada.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-06 |
| CU cubiertos | CU-07 |
| BT derivadas | BT-21 |
| Tests previstos | acceptance/AT-13-veredicto-salud |

## 5. Prioridad y estimación
Must: el veredicto de salud es el diferenciador central del producto. 8 SP por el cálculo de derivados, la comparabilidad y la confianza; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-07
- [x] NB de origen (NB-06) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-21 y de la serie de US-12, planificadas antes

## 7. Notas y supuestos
El sistema no presenta magnitudes cuantitativas ni conformidad con IEEE 1188 (E-08, Won't v1): solo tendencia relativa en unidades arbitrarias. La oscilación estacional de temperatura sin sensor es una reserva declarada de todo veredicto (CL-24, riesgo S-9).
