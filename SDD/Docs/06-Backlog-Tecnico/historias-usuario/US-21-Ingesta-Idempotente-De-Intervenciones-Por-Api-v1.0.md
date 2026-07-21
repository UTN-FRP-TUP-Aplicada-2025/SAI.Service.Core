# US-21 — Ingesta idempotente de intervenciones por API

**Proyecto:** Sai-Service-Core
**Documento:** US-21-Ingesta-Idempotente-De-Intervenciones-Por-Api-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-07 Integración e informes
**Prioridad MoSCoW:** Must
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como sistema externo de gestión de mantenimiento, quiero empujar intervenciones por la interfaz de integración con una clave de idempotencia, para que un reintento de red no duplique el hecho ni corrompa el histórico.

## 2. Contexto
NB-08: CU-11 recibe la intervención con `X-Idempotency-Key` y `X-Fuente-Datos`, valida el cuerpo y, si la clave es nueva, la registra con confianza `media` (origen externo, menor que lo medido localmente) devolviendo el identificador y los dos tiempos (cuándo ocurrió y cuándo se registró). El contrato del endpoint `POST /api/v1/intervenciones` responde 201 al crear y 200 ante un reintento idéntico con el mismo id (ADR-17). El reintento idéntico es el caso normal, no el excepcional (CL-21).

## 3. Criterios de aceptación
- Given una intervención válida con una clave de idempotencia nueva, When el sistema externo la envía, Then el servicio responde 201 con el id, `creado: true`, la confianza media y los dos tiempos.
- Given la misma clave con exactamente el mismo cuerpo, When se reintenta, Then el servicio responde 200 con `creado: false` y el mismo id, sin duplicar el hecho.
- Given una intervención sin la cabecera de clave de idempotencia o de fuente de datos, When se envía, Then el servicio la rechaza por cabecera obligatoria faltante.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-08 |
| CU cubiertos | CU-11 |
| BT derivadas | BT-26, BT-28 |
| Tests previstos | acceptance/AT-21-ingesta-idempotente; sample `ingesta-gmao/` |

## 5. Prioridad y estimación
Must: la ingesta idempotente es una capacidad Must (F-20) y la única superficie del sistema consumida por terceros. 8 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-11
- [x] NB de origen (NB-08) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-26 y BT-28, planificadas antes

## 7. Notas y supuestos
El dato externo entra con confianza media, por debajo de lo medido por el poller local. Los caminos de conflicto (409) e invariante roto (422) se cubren en US-22. El cliente de referencia se ilustra en el sample `ingesta-gmao/` (11-Examples).
