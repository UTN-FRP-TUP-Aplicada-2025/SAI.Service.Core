# US-07 — Panel de estado en vivo desde la LAN

**Proyecto:** Sai-Service-Core
**Documento:** US-07-Panel-De-Estado-En-Vivo-Desde-La-Lan-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-05 Monitoreo, salud e históricos
**Prioridad MoSCoW:** Must
**Estimación:** 5 SP (Fibonacci)

## 1. Historia
Como administrador, quiero ver el estado del SAI en vivo desde cualquier equipo de la LAN, para enterarme de un problema sin estar sentado frente al servidor.

## 2. Contexto
NB-02 pide una vista única en tiempo real. CU-04 (flujo del 80 % del tiempo) muestra estado y tensiones, batería (con la carga marcada como derivada), conectividad, `n de m` supuestos verificados y eventos recientes con su regla y versión. Si falta algún supuesto, el panel avisa en la pantalla principal que la política está degradada a `SoloAlerta`.

## 3. Criterios de aceptación
- Given equipos dados de alta con sesión de sondeo activa, When el administrador abre el panel desde un equipo de la LAN, Then ve estado, tensiones, carga (marcada como derivada), conectividad, supuestos verificados y eventos recientes actualizándose en vivo.
- Given una política degradada por supuestos sin verificar, When se muestra el panel principal, Then el aviso de degradación a `SoloAlerta` aparece en la pantalla principal, no enterrado en configuración.
- Given una pérdida temporal del circuito de tiempo real, When se restablece, Then el panel vuelve a reflejar el estado vigente sin mostrar un valor viejo como actual.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-02 |
| CU cubiertos | CU-04 |
| BT derivadas | BT-02, BT-06, BT-14, BT-15, BT-17 |
| Tests previstos | acceptance/AT-07-panel-estado-en-vivo |

## 5. Prioridad y estimación
Must: es el uso cotidiano del sistema y la base de la observación. 5 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-04
- [x] NB de origen (NB-02) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-06, BT-15 y BT-17, planificadas antes

## 7. Notas y supuestos
La cadencia de sondeo y la persistencia se cubren en US-08; esta US se centra en la vista en vivo. El cifrado del acceso por la LAN se decide en BT-02 (ADR-20). Las notificaciones externas no son el mecanismo primario de alerta (F-30, Won't v1): el panel lo es.
