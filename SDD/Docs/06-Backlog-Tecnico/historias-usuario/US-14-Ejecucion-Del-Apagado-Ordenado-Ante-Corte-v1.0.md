# US-14 — Ejecución del apagado ordenado ante corte sostenido

**Proyecto:** Sai-Service-Core
**Documento:** US-14-Ejecucion-Del-Apagado-Ordenado-Ante-Corte-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Must
**Estimación:** 13 SP (Fibonacci)

## 1. Historia
Como administrador, quiero que el servicio apague el host de forma ordenada ante un corte sostenido y corte la salida del SAI con retorno, para que el servidor sin respaldo no se corrompa y vuelva a encenderse solo cuando regrese la energía.

## 2. Contexto
NB-01 es el propósito central: garantizar el reencendido. CU-05 detecta la batería sostenida durante el umbral (por ejemplo 300 s), crea una acción referida a la versión de política (RN-11), evalúa la modalidad efectiva, ordena el apagado respetando el tiempo reservado (≤540 s, RN-04) y el corte de salida con retorno. El disparo no depende del flag `LB` (ADR-12): decide por tiempo en batería más `battery.voltage`. Iniciada la secuencia, el corte del SAI no se cancela aunque vuelva la red (`CicloForzado`, ADR-09), porque si el SAI cancela su corte no hay transición y la BIOS no reenciende.

## 3. Criterios de aceptación
- Given una modalidad `HostLuegoUpsConRetorno` con todos los supuestos verificados y un corte que supera el umbral, When se dispara, Then el sistema apaga el host de forma ordenada dentro del tiempo reservado y ordena el corte de salida con retorno.
- Given una secuencia de apagado ya iniciada, When la energía de red vuelve durante la cuenta regresiva, Then el corte del SAI no se cancela (no se usa `shutdown.stop`) y el ciclo se completa (CL-01).
- Given un corte sostenido sin que el flag `LB` se encienda nunca, When se evalúa el disparo, Then el sistema dispara igual por tiempo en batería más tensión (CL-06).
- Given la energía restaurada tras el ciclo, When el SAI restablece la salida, Then el host arranca solo sin intervención manual.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-01, NB-07 |
| CU cubiertos | CU-05 |
| BT derivadas | BT-04, BT-16, BT-17, BT-20, BT-22, BT-29 |
| Tests previstos | acceptance/AT-14-apagado-ordenado |

## 5. Prioridad y estimación
Must: es la única acción irreversible y el corazón del propósito del sistema. 13 SP por su criticidad, la orquestación del ciclo forzado y la validación por efecto observado; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-05
- [x] NB de origen (NB-01) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-16, BT-20, BT-22 y del simulado BT-29; la firma del adaptador (BT-04) debe estar cerrada

## 7. Notas y supuestos
El apagado real solo se habilita cuando los cuatro supuestos están verificados (US-15, US-16); mientras tanto la modalidad efectiva es `SoloAlerta`. Se prueba con el adaptador simulado (BT-29) para no ejecutar cortes destructivos en el pipeline. El apagado de otros equipos de la red está fuera de alcance (F-28, Won't v1).
