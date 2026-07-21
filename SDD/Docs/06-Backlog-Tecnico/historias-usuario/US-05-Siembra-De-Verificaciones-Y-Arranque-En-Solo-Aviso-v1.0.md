# US-05 — Siembra de verificaciones y arranque forzado en solo aviso

**Proyecto:** Sai-Service-Core
**Documento:** US-05-Siembra-De-Verificaciones-Y-Arranque-En-Solo-Aviso-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-04 Alta de equipos y políticas de apagado
**Prioridad MoSCoW:** Must
**Estimación:** 3 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el alta siembre las cuatro verificaciones en estado no verificado y fuerce el modo solo aviso, para que el sistema no apague el host hasta que yo haya probado que el reencendido funciona.

## 2. Contexto
NB-05 fija el arranque seguro y el bloqueo por verificación (ADR-10). CU-02 siembra las cuatro verificaciones en `NuncaVerificado`, lo que degrada la modalidad efectiva a `SoloAlerta`; el panel muestra un aviso permanente «operativo · 0 de 4 supuestos verificados» con enlace a la ventana de mantenimiento (US-16). Es la garantía de que el sistema nunca arranca creyendo que puede apagar sin evidencia.

## 3. Criterios de aceptación
- Given un alta de equipos recién completada, When el sistema termina la puesta en marcha, Then crea las cuatro verificaciones en `NuncaVerificado` y fuerza la modalidad efectiva a `SoloAlerta`.
- Given el estado inicial con supuestos sin verificar, When el administrador abre el panel principal, Then ve el aviso «0 de 4 supuestos verificados» y el enlace a la ventana de mantenimiento, no enterrado en configuración.
- Given un sistema en `SoloAlerta` por supuestos sin verificar, When ocurre un corte sostenido, Then el sistema avisa pero no apaga el host (BloqueadaPorVerificacion).

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-05, NB-01 |
| CU cubiertos | CU-02, CU-10 |
| BT derivadas | BT-23 |
| Tests previstos | acceptance/AT-05-siembra-solo-aviso |

## 5. Prioridad y estimación
Must: es la regla de seguridad que impide que el servicio apague el host sin evidencia. 3 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-02 y CU-10
- [x] NB de origen (NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-23, planificada en EP-06 (la siembra se implementa en EP-04, la evaluación completa en EP-06)

## 7. Notas y supuestos
Las cuatro verificaciones son: presupuesto de apagado, señal en batería, reencendido por placa y corte con retorno. Su verificación real se cubre en US-16 (ventana de mantenimiento); esta US solo las siembra y fuerza el modo degradado.
