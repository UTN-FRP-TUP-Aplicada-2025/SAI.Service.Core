# RC-01 — Procedencia por valor

**Proyecto:** Sai-Service-Core
**Documento:** RC-01-Procedencia-Por-Valor-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Todo valor almacenado en una entidad de historia se representa como un Valor con Origen: no existe una magnitud persistida sin su procedencia declarada, y un valor de origen derivado incluye las variables de las que deriva.

## 2. Entidades involucradas

Valor con Origen (objeto de valor), Muestra, Agregado, Evento, PruebaBateria, Intervencion, SesionSondeo.

## 3. Tipo de restricción

Restricción de valor y de derivación.

## 4. Mecanismo de verificación conceptual

Al construir cualquier instancia de una entidad de historia, cada campo de magnitud se acompaña de su procedencia; una construcción sin procedencia, o un derivado sin sus variables de origen, no es una instancia válida del modelo. Se comprueba recorriendo los valores de la entidad y confirmando que todos exponen origen.

## 5. RN o CU que la justifican

RN-05 (Procedencia obligatoria y origen declarado de todo valor). CU-04, CU-06, CU-07, CU-08, CU-12.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-7 e I-8 |
