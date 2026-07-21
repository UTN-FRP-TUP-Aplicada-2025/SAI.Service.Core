# ADR-04 — Historia append-only sin event store ni CQRS

**Proyecto:** Sai-Service-Core
**Documento:** ADR-04-Historia-Append-Only-Sin-Event-Store-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Persistencia

## 1. Contexto

El valor del sistema depende de que los hechos registrados (muestras, eventos, pruebas, intervenciones, acciones) sean inmutables: la historia debe poder explicar por qué se tomó cada decisión con los datos que existían en ese momento. La tentación de un event store completo con CQRS aparece, pero el volumen y la concurrencia son mínimos: un usuario, un dispositivo, un proceso escritor. La fuente resuelve el punto explícitamente. Motivan la decisión la regla conceptual RC-06 (historia append-only) y las necesidades NB-03 (historia trazable) y NB-04 (ciclo de vida).

## 2. Decisión

Las tablas de hechos son append-only: no se actualizan ni se borran. No se adopta event store ni CQRS. La inmutabilidad se obtiene como disciplina de escritura, no como infraestructura: *"Alcanza con que las tablas de historia sean append-only… Es una disciplina, no una tecnología."*

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-04 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Tablas de historia append-only | Inmutabilidad de hechos sin infraestructura extra; rollback de versión no pierde hechos; respaldo trivial | Exige disciplina de escritura verificada por pruebas de invariante |
| Event sourcing completo + CQRS | Auditoría total; separación lectura/escritura | Desproporción para un usuario y un dispositivo; complejidad operativa sin contrapartida (E-09) |
| Tablas mutables con updates | Modelo CRUD familiar | Destruye la trazabilidad histórica; una corrección borraría el porqué de una decisión pasada |

## 5. Consecuencias positivas

1. Los hechos son inmutables: la corrección de la fecha de un recambio reatribuye el histórico sin migrar datos (CL-18), porque la historia guarda dispositivo e instante (ADR-05, ADR-07).
2. Un rollback de versión de la aplicación no pierde hechos registrados (P.8 rollback).
3. Simplicidad operativa: un archivo SQLite, respaldo por copia (ADR-18).

## 6. Consecuencias negativas y trade-offs

1. Las correcciones se modelan como hechos nuevos, no como updates: hay que educar cada flujo de escritura en esa disciplina.
2. El crecimiento se controla por agregación y retención (ADR-18, F-19), no por borrado de historia.
3. La inmutabilidad no la garantiza el motor por sí sola: requiere pruebas de invariante que fallen ante un update (R-10).

## 7. Implementación

Las tablas `Muestra`, `Agregado`, `Evento`, `PruebaBateria`, `Intervencion`, `Accion`, `Verificacion`, `SesionSondeo`, `ReglaDerivacion`, `Politica`/`VersionPolitica` y `FuenteDatos` se escriben solo por inserción. El código de acceso no expone operaciones de update/delete sobre ellas. Las correcciones (fechas, rectificaciones) se resuelven por nuevos vínculos temporales y por el `ResolutorTemporal` (ADR-05). Se cubre con pruebas de invariante en `Domain`/`Infrastructure`.

## 8. Métricas de validación

- Prueba de invariante: intento de update/delete sobre una tabla de historia rechazado.
- CL-18: corregir la fecha de un recambio reatribuye el histórico sin migración de datos.
- Rollback de versión sin pérdida de hechos (smoke de P.8).

## 9. Referencias

- Intake §17 P.2, P.4, P.11 (PA-04); exclusión E-09.
- RC-06 Historia append-only; NB-03 Historia trazable con procedencia; NB-04 Ciclo de vida de los equipos.
- ADR relacionadas: ADR-05, ADR-07, ADR-18.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-04. |
