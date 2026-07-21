# US-08 — Sondeo periódico y persistencia de muestras con calidad

**Proyecto:** Sai-Service-Core
**Documento:** US-08-Sondeo-Periodico-Y-Persistencia-De-Muestras-Con-Calidad-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el servicio sondee el equipo cada intervalo configurable y persista cada muestra con su calidad y procedencia, para construir un histórico fiable que tolere respuestas incompletas o perdidas.

## 2. Contexto
NB-02 y NB-03 exigen persistir la observación con procedencia. CU-04 persiste una `Muestra` con calidad `completa`, `parcial` (se conserva: descartarla entera perdería las variables que sí llegaron) o `perdida`. Los cálculos deben tolerar nulos por variable, no solo por muestra (CL-12). La retención es de 30 días en resolución completa, agregados horarios por 10 años y eventos indefinidos (F-19).

## 3. Criterios de aceptación
- Given el planificador activo con intervalo por defecto de 5 s, When llega una respuesta completa del equipo, Then persiste una `Muestra` con `calidad = completa` y la procedencia de cada valor.
- Given una respuesta incompleta del equipo, When se procesa, Then persiste la muestra con `calidad = parcial` conservando las variables que sí llegaron y dejando en `null` las ausentes.
- Given una respuesta que no llega, When vence el intervalo, Then registra la muestra con `calidad = perdida` e incrementa el contador de sondeos fallidos.
- Given muestras fuera de la ventana de retención de 30 días, When corre la agregación, Then produce el agregado horario conservando mínimo y máximo además del promedio.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-02, NB-03 |
| CU cubiertos | CU-04 |
| BT derivadas | BT-07, BT-08, BT-09, BT-15, BT-17, BT-18 |
| Tests previstos | acceptance/AT-08-sondeo-persistencia-calidad |

## 5. Prioridad y estimación
Must: sin persistencia con calidad y procedencia no hay histórico ni salud fiables. 8 SP por el planificador, la calidad de muestra y la agregación; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-04
- [x] NB de origen (NB-02, NB-03) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-07, BT-08, BT-15, BT-17 y BT-18, planificadas antes

## 7. Notas y supuestos
Un parser que exija `battery.voltage` no nulo falla contra datos reales (CL-12): los cálculos toleran nulos por variable. El conteo de microcortes sale de los eventos, nunca de la serie agregada (CL-16).
