# ADR-08 — Agregado no hereda de Muestra

**Proyecto:** Sai-Service-Core
**Documento:** ADR-08-Agregado-No-Hereda-De-Muestra-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

El sistema conserva muestras a resolución completa durante 30 días y luego las agrega a resolución horaria por 10 años. Servir un agregado como si fuera una muestra es un modo de falla concreto: el promedio horario borra exactamente los microcortes que el sistema quiere estudiar (CL-16). El diseño debe hacer imposible confundir ambos: *"el compilador debe obligar a distinguirlos"*. Motivan la decisión la regla conceptual RC-04 (agregado no hereda de muestra), la regla de negocio RN-10 (agregado con cobertura y advertencia) y el caso de uso CU-06 (históricos y gráficas).

## 2. Decisión

`Agregado` no hereda de `Muestra`: son tipos distintos, sin relación de herencia ni flag compartido. Todo `Agregado` servido declara su `cobertura` y su advertencia (I-20), y nunca se sirve por el mismo canal que una muestra sin declarar que lo es. Para `input.voltage`, el agregado conserva mínimo y máximo además del promedio.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-08 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Tipos separados sin herencia | El compilador impide confundir muestra y agregado; cobertura obligatoria | Duplica algo de estructura entre ambos tipos |
| Herencia `Agregado : Muestra` con un flag | Reutiliza código | Un agregado puede pasar por muestra; borra microcortes (CL-16); el flag se olvida |
| Una sola tabla con resolución variable | Menos entidades | Imposible distinguir por tipo; la advertencia I-20 se pierde |

## 5. Consecuencias positivas

1. Es imposible en compilación tratar un agregado como muestra: se elimina el modo de falla de CL-16.
2. El mínimo y el máximo horarios preservan los microcortes que el promedio ocultaría; el conteo de microcortes sale de `Evento`, nunca de la serie agregada (CL-16).
3. Todo agregado viaja con su `cobertura` y su advertencia (I-20, RN-10), de modo que un informe no suma datos incompletos en silencio.

## 6. Consecuencias negativas y trade-offs

1. Hay estructura duplicada entre `Muestra` y `Agregado` que no se comparte por herencia; se acepta a cambio de la seguridad de tipos.
2. La capa de gráficas (CU-06) debe elegir explícitamente la fuente (muestra o agregado) según el rango consultado.
3. El proceso de agregación debe poblar `cobertura` correctamente o el invariante I-20 lo rechaza.

## 7. Implementación

`Muestra` y `Agregado` como tipos independientes en `Domain`. El job de agregación (ADR-18, retención P.4) transforma muestras `P30D` en agregados `PT1H`, conservando min/max/promedio para `input.voltage` y poblando `cobertura` y advertencia. La API y el panel sirven cada uno por su canal, marcando siempre cuál es. Prueba de invariante I-20.

## 8. Métricas de validación

- I-20: todo `Agregado` servido declara `cobertura` y advertencia (prueba de invariante).
- CL-16: el conteo de microcortes proviene de `Evento`, verificado en un escenario de agregación.
- Compilación: ningún punto del código trata un `Agregado` como `Muestra`.

## 9. Referencias

- Intake §17 P.4, P.10 (I-20), P.11 (PA-08); CL-16.
- RC-04 Agregado no hereda de Muestra; RN-10 Agregado con cobertura y advertencia.
- CU-06 Históricos y gráficas de evolución; F-19.
- ADR relacionadas: ADR-04, ADR-18.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-08. |
