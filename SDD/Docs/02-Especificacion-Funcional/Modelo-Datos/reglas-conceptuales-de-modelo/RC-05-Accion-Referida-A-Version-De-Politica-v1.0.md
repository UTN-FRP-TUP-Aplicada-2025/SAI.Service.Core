# RC-05 — Acción referida a versión de política

**Proyecto:** Sai-Service-Core
**Documento:** RC-05-Accion-Referida-A-Version-De-Politica-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Una Accion referencia siempre una VersionPolitica concreta e inmutable, nunca la Politica que la agrupa.

## 2. Entidades involucradas

Accion, VersionPolitica, Politica.

## 3. Tipo de restricción

Restricción referencial.

## 4. Mecanismo de verificación conceptual

La referencia de toda Accion apunta a una VersionPolitica; no existe una referencia válida de Accion directamente a Politica. Se comprueba confirmando que el vínculo de decisión de cada Accion resuelve a una versión, con sus parámetros congelados en el instante de la decisión.

## 5. RN o CU que la justifican

RN-11 (acción referida a una versión de política). CU-05 (ejecución del apagado ordenado); se apoya en CU-03 (configuración de políticas versionadas).

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-13 |
