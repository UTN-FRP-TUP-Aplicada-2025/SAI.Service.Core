# ADR-12 — Disparo del apagado sin depender del flag LB

**Proyecto:** Sai-Service-Core
**Documento:** ADR-12-Disparo-Sin-Dependencia-Del-Flag-Lb-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Comunicación

## 1. Contexto

El estándar de facto para disparar el apagado ante batería baja es el flag `LB` (low battery) del firmware. En este equipo `LB` nunca fue observado durante el relevamiento (CL-06, riesgo R-04), y `battery.runtime` directamente no existe. Además, `battery.charge` es derivado por el driver, no medido, y usarlo como umbral reproduce el modo de falla de conclusión falsa (R-13). El sistema necesita un criterio de disparo que dependa solo de señales confiables del equipo. Motivan la decisión el caso de uso CU-05 (apagado ante corte) y la necesidad NB-01.

## 2. Decisión

La política de apagado no depende del flag `LB` ni de `battery.runtime`. El disparo se decide por tiempo en `OB` (on battery) más `battery.voltage`. No se usa un umbral sobre `battery.charge`, que es derivado por el driver.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-12 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Tiempo en `OB` + `battery.voltage` | Señales observadas y confiables en este equipo; no depende de flags ausentes | Requiere calibrar umbrales de tensión y tiempo por versión de política |
| Umbral por flag `LB` | Estándar de facto; simple | `LB` nunca se observó en este equipo (CL-06); la política nunca dispararía |
| Umbral por `battery.charge` | Disponible como variable | Es derivado por el driver, no medido (R-13); construir el disparo sobre él es conclusión falsa |

## 5. Consecuencias positivas

1. El disparo depende de señales realmente observadas en este equipo, no de un flag que nunca llega (CL-06).
2. Coherente con la procedencia (ADR-06): no se decide una acción irreversible sobre un valor `derivado`.
3. Los umbrales son parte de la `VersionPolitica` (ADR-15, RN-11), ajustables y trazables.

## 6. Consecuencias negativas y trade-offs

1. Hay que calibrar el umbral de disparo (`umbralDisparoSegundos`, 300 s como punto de partida) y las alarmas de tensión, en vez de leer un flag listo.
2. La alarma dura de batería usa umbrales específicos de esta química (`battery.voltage` < 13,3 V o > 14,5 V), no el 11 V genérico de baterías inundadas.
3. Si un firmware futuro sí emitiera `LB`, no se aprovecha; se acepta a cambio de no depender de él.

## 7. Implementación

La lógica de disparo en `Domain`/`Application` evalúa el tiempo sostenido en `OB` y `battery.voltage` contra los umbrales de la `VersionPolitica` vigente. `umbralDisparoSegundos` parte de 300 s. La alarma dura marca `battery.voltage` < 13,3 V (potencial celda en corto) y > 14,5 V (potencial celda abierta). El planificador (ADR-15) usa temporizadores con cancelación para no actuar ante microcortes.

## 8. Métricas de validación

- Prueba: con `LB` ausente, la política dispara igual por tiempo en `OB` + `battery.voltage`.
- Ningún camino de decisión de apagado lee `battery.charge` ni `battery.runtime`.
- Alarmas de tensión disparan en los umbrales 13,3 V / 14,5 V.

## 9. Referencias

- Intake §17 P.10, P.11 (PA-12); CL-06; riesgos R-04, R-13.
- CU-05 Ejecución del apagado ordenado ante corte; NB-01 Apagado ordenado y reencendido garantizado.
- ADR relacionadas: ADR-06, ADR-09, ADR-15.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-12. |
