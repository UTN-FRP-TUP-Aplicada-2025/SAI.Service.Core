# US-12 — Prueba de batería programada y manual con cadencia densa

**Proyecto:** Sai-Service-Core
**Documento:** US-12-Prueba-De-Bateria-Programada-Y-Manual-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el servicio pruebe la batería trimestralmente y también a pedido, midiendo la caída de tensión con muestreo denso, para tener la serie que permite evaluar la degradación.

## 2. Contexto
NB-06 exige medir la salud por la caída de tensión (F-16). CU-07 eleva la cadencia a 1 Hz, congela el `montajeBateriaId` vigente en el registro de la prueba, ordena el autotest y recoge la serie de tensión tratando las muestras perdidas como estado de primera clase. La precondición es un tiempo mínimo en flotación tras el último corte (CL-25): una prueba poco después de un corte mide otra cosa.

## 3. Criterios de aceptación
- Given una batería con el tiempo mínimo en flotación cumplido, When se dispara la prueba (programada o manual), Then el sistema eleva la cadencia a 1 Hz, congela el montaje vigente y recoge la serie de tensión.
- Given una prueba disparada sin cumplir el tiempo mínimo en flotación, When se solicita, Then el sistema la rechaza (FLOTACION_INSUFICIENTE).
- Given muestras perdidas durante la prueba a 1 Hz (el equipo deja de atender mientras conmuta), When se procesa la serie, Then las pérdidas se registran como estado explícito y no se interpolan (CL-13).
- Given el fin de la prueba, When termina, Then el sistema restaura la cadencia normal de sondeo.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-06 |
| CU cubiertos | CU-07 |
| BT derivadas | BT-17, BT-29 |
| Tests previstos | acceptance/AT-12-prueba-bateria |

## 5. Prioridad y estimación
Must: es el segundo propósito del servicio (medir salud) y la fuente de la serie de degradación. 8 SP por la cadencia densa y el manejo de pérdidas; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-07
- [x] NB de origen (NB-06) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-17 y BT-29 (simulado para probar sin hardware), planificadas antes

## 7. Notas y supuestos
El congelado del `montajeBateriaId` asegura que la prueba quede atribuida a la batería correcta aunque luego se recambie. El veredicto de salud se cubre en US-13.
