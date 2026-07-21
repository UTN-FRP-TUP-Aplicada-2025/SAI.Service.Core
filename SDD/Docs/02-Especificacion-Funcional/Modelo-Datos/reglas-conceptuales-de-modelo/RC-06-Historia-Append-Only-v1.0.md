# RC-06 — Historia append-only

**Proyecto:** Sai-Service-Core
**Documento:** RC-06-Historia-Append-Only-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Las entidades de historia se agregan pero no se actualizan ni se borran: un hecho registrado es inmutable. Las correcciones se expresan por nuevos hechos y por la reatribución que resuelven los vínculos temporales, no por edición del hecho original.

## 2. Entidades involucradas

Muestra, Agregado, Evento, PruebaBateria, Accion, Verificacion (evidencia), Intervencion, SesionSondeo.

## 3. Tipo de restricción

Restricción de identidad y de inmutabilidad.

## 4. Mecanismo de verificación conceptual

Sobre las entidades de historia no existen operaciones conceptuales de modificación ni de borrado de un hecho ya registrado. Una corrección de fecha de una intervención, por ejemplo, no reescribe muestras: cambia el instante de un vínculo, y la reatribución del histórico se resuelve consultando ese vínculo. Es una disciplina de escritura, no una tecnología.

## 5. RN o CU que la justifican

RN-12 (baja lógica, sin borrado). CU-04 (persistencia de muestras y eventos), CU-08 (corrección de fecha con reatribución), CU-11 (ingesta idempotente sin sobrescritura). Justificada por la decisión PA-04 del intake.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de PA-04 y de SOLUTION-INTAKE §7 (CL-18) |
