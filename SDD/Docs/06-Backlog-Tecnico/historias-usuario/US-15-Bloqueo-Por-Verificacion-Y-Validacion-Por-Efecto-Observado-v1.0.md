# US-15 — Bloqueo por verificación y validación por efecto observado

**Proyecto:** Sai-Service-Core
**Documento:** US-15-Bloqueo-Por-Verificacion-Y-Validacion-Por-Efecto-Observado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el servicio se niegue a apagar el host si algún supuesto requerido no está verificado y que confirme cada acción por su efecto observado, para no dejar el servidor apagado por confiar en un supuesto falso o en la ausencia de un error.

## 2. Contexto
NB-05 fija el bloqueo por verificación (ADR-10) y la validación por efecto observado (ADR-11). CU-05 evalúa las verificaciones requeridas antes de apagar: si alguna está en `NuncaVerificado`, `Vencido` o `Refutado`, la acción queda `BloqueadaPorVerificacion` y la modalidad degrada a `SoloAlerta`. Un comando puede no llegar al equipo sin producir error (CL-07): asumir que «no hubo excepción» equivale a «se ejecutó» hace mentir al servicio.

## 3. Criterios de aceptación
- Given un supuesto requerido en estado `Vencido` o `Refutado`, When se dispara un corte que superaría el umbral, Then el sistema no apaga el host, marca la acción `BloqueadaPorVerificacion` y degrada a `SoloAlerta`.
- Given una orden de apagado o de corte enviada al equipo, When se ejecuta, Then el sistema la da por realizada solo tras confirmar su efecto observado, no por ausencia de excepción.
- Given un comando que no produce error pero tampoco el efecto esperado, When se valida, Then el sistema lo registra como efecto no confirmado (EFECTO_NO_CONFIRMADO, RN-03) y no avanza como si hubiera ocurrido.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-05, NB-01 |
| CU cubiertos | CU-05, CU-10 |
| BT derivadas | BT-23, BT-24, BT-29 |
| Tests previstos | acceptance/AT-15-bloqueo-efecto-observado |

## 5. Prioridad y estimación
Must: es la garantía de seguridad que ninguna alternativa ofrece y la que evita el peor resultado posible (servidor apagado indefinidamente). 8 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-05 y CU-10
- [x] NB de origen (NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-23, BT-24 y del simulado BT-29, planificadas antes o en paralelo

## 7. Notas y supuestos
`Refutado` no es `Vencido`: una prueba fallida bloquea permanentemente hasta que alguien lo resuelva; una vencida solo pide repetirla. La renovación de supuestos por evidencia se cubre en US-17.
