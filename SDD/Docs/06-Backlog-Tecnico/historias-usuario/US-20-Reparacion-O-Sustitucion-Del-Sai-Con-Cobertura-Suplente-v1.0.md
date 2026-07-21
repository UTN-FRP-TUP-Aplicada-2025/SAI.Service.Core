# US-20 — Reparación o sustitución del SAI con cobertura suplente

**Proyecto:** Sai-Service-Core
**Documento:** US-20-Reparacion-O-Sustitucion-Del-Sai-Con-Cobertura-Suplente-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-06 Verificación y ciclo de vida de los equipos
**Prioridad MoSCoW:** Should
**Estimación:** 8 SP (Fibonacci)

## 1. Historia
Como administrador, quiero registrar que un SAI se fue a reparación y otro lo cubrió, para que el histórico diga qué equipo protegía al host en cada tramo y cuántos días quedó sin protección.

## 2. Contexto
NB-04: CU-09 modela coberturas como vínculos temporales sin solaparse (RN-12, a lo sumo una cobertura vigente por host). Registra la intervención (reparación o reemplazo), cierra la cobertura vigente en el instante, y si hay suplente abre una nueva sin solapamiento; si hay tramo descubierto deja el hueco (días sin protección). Con la vigencia como entidad con intervalo (ADR-05), mover un equipo entre roles «ni siquiera es representable» con el modelo descartado (CL-17).

## 3. Criterios de aceptación
- Given una cobertura del host vigente por el equipo saliente y un suplente en stock, When se registra la sustitución, Then el sistema cierra la cobertura vigente y abre la nueva sin solapamiento.
- Given una sustitución sin suplente inmediato, When se registra, Then el sistema deja el tramo descubierto y lo contabiliza como días sin protección.
- Given un intento de abrir una cobertura que se solapa con otra vigente para el mismo host, When se registra, Then el sistema lo rechaza (COBERTURA_SOLAPADA, RN-12).
- Given una sustitución por otro modelo de SAI, When se registra, Then las verificaciones de firmware vuelven a `NuncaVerificado` y el panel dispara el procedimiento de caracterización (CL-27).

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-04 |
| CU cubiertos | CU-09 |
| BT derivadas | BT-11, BT-12, BT-26 |
| Tests previstos | acceptance/AT-20-sustitucion-sai |

## 5. Prioridad y estimación
Should: completa el ciclo de vida del SAI (F-21); el MVP protege un único equipo sin modelar su reemplazo. 8 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-09
- [x] NB de origen (NB-04) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Depende de BT-11, BT-12 y BT-26, planificadas antes

## 7. Notas y supuestos
El anclaje del USB por ruta física de puerto (ADR-03) hace que el binding no se rompa al sustituir el equipo. Tras la sustitución, la modalidad vuelve a degradarse hasta reverificar los supuestos contra el nuevo equipo.
