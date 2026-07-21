# US-11 — Históricos y gráficas de evolución con marcas de eventos

**Proyecto:** Sai-Service-Core
**Documento:** US-11-Historicos-Y-Graficas-Con-Marcas-De-Eventos-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero graficar voltajes y carga superpuestos en un período con las marcas de eventos encima, para evaluar la calidad del suministro durante la vida del host.

## 2. Contexto
NB-03 pide historia consultable y graficable. CU-06 distingue la serie de muestras completas (dentro de la retención de 30 días) de la serie de agregados horarios (fuera), que viaja con cobertura y advertencia obligatorias (RN-10). El conteo de microcortes sale de los eventos, nunca del promedio horario (CL-16). Una consulta que mezcle versiones de regla sin normalizar produce una serie corrupta (CL-15).

## 3. Criterios de aceptación
- Given un período dentro de la retención, When el administrador elige variables, Then el sistema grafica la serie de muestras completas, individual o superpuesta, con las marcas de eventos encima.
- Given un período fuera de la retención, When se consulta, Then el sistema usa la serie de agregados horarios e incluye la cobertura y la advertencia (RN-10), y conserva mínimo y máximo además del promedio.
- Given un período sin datos, When se consulta, Then el sistema informa PERIODO_SIN_DATOS y no dibuja una serie vacía.
- Given un agregado horario sin cobertura declarada, When se intenta graficar, Then el sistema lo rechaza (AGREGADO_SIN_COBERTURA, RN-10).

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-03 |
| CU cubiertos | CU-06 |
| BT derivadas | BT-09, BT-18, BT-19 |
| Tests previstos | acceptance/AT-11-historicos-graficas |

## 5. Prioridad y estimación
Must: es la vista que permite decidir sobre la calidad del suministro y preparar recambios. 8 SP por la distinción muestra/agregado y la superposición de eventos; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-06
- [x] NB de origen (NB-03) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-09, BT-18 y BT-19, planificadas antes

## 7. Notas y supuestos
La serie de muestras y la de agregados nunca se mezclan sin marcar la transición. El promedio horario borra los microcortes que el sistema quiere estudiar (CL-16): por eso el conteo sale de `Evento`.
