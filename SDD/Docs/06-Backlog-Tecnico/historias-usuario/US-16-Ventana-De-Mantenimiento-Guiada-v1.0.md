# US-16 — Ventana de mantenimiento guiada de los cuatro supuestos

**Proyecto:** Sai-Service-Core
**Documento:** US-16-Ventana-De-Mantenimiento-Guiada-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador con presencia física, quiero una ventana de mantenimiento guiada que verifique los cuatro supuestos uno por uno, para desbloquear el apagado automático con evidencia y no con optimismo.

## 2. Contexto
NB-01 y NB-05: sin esta ventana el servicio nunca sale de `SoloAlerta`. CU-10 recorre el checklist con presencia física: cronometra el apagado bajo carga (`ver-presupuesto-apagado`, vigencia 180 días), corta la red y observa la señal en batería (`ver-flag-ob`, 365 días), ejecuta el corte con retorno y, si el host arranca solo, verifica `ver-bios-autoencendido` y `ver-shutdown-return`. Es un flujo destructivo por naturaleza. El adaptador simulado (BT-29) permite cubrir el camino en pruebas automatizadas.

## 3. Criterios de aceptación
- Given los cuatro supuestos sin verificar y presencia física, When el administrador inicia la ventana, Then el panel muestra el checklist y guía la verificación de cada supuesto uno por uno registrando su evidencia.
- Given el apagado cronometrado bajo carga con los contenedores detenidos, When se registra el tiempo, Then `ver-presupuesto-apagado` pasa a `Verificado` con vigencia de 180 días.
- Given el corte de red y la observación de la señal en batería, When se confirma, Then `ver-flag-ob` pasa a `Verificado` con vigencia de 365 días.
- Given el corte con retorno ejecutado y la energía restaurada, When el host no arranca solo, Then `ver-bios-autoencendido` pasa a `Refutado` y bloquea permanentemente hasta que alguien lo resuelva.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-01, NB-05 |
| CU cubiertos | CU-10 |
| BT derivadas | BT-25, BT-29 |
| Tests previstos | acceptance/AT-16-ventana-mantenimiento |

## 5. Prioridad y estimación
Must: es el único camino para que el sistema salga del modo degradado y pueda apagar el host (métrica M-01: de 0 de 4 a 4 de 4). 8 SP por la guía paso a paso y el registro de evidencia; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-10
- [x] NB de origen (NB-01, NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-25 y del simulado BT-29; la ejecución real es una actividad operativa posterior (excepción DoR §3)

## 7. Notas y supuestos
La US entrega la interfaz guiada y el registro de evidencias; la ejecución real exige presencia física y pruebas destructivas y queda como actividad operativa. La lectura del ajuste de BIOS por software está descartada (F-32, Won't v1): se verifica por comportamiento.
