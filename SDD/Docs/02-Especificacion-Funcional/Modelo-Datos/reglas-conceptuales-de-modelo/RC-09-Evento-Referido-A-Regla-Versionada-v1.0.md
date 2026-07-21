# RC-09 — Evento referido a regla de derivación versionada

**Proyecto:** Sai-Service-Core
**Documento:** RC-09-Evento-Referido-A-Regla-Versionada-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Todo Evento referencia la ReglaDerivacion que lo produjo y la versión concreta de esa regla vigente en el instante de la derivación.

## 2. Entidades involucradas

Evento, ReglaDerivacion.

## 3. Tipo de restricción

Restricción referencial y de versión.

## 4. Mecanismo de verificación conceptual

Cada Evento resuelve a una regla de derivación y a un número de versión de esa regla. Se comprueba que ese par exista y que una consulta de tendencia que abarque varias versiones pueda distinguirlas o normalizarlas, para no mezclar eventos derivados con umbrales distintos.

## 5. RN o CU que la justifican

CU-04 (derivación de eventos en el monitoreo), CU-06 (históricos que no deben mezclar versiones). Justificada por SOLUTION-INTAKE §7 (CL-15): mezclar versiones sin normalizar corrompe la serie histórica.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-14 y de §20.E-1, §20.E-3 |
