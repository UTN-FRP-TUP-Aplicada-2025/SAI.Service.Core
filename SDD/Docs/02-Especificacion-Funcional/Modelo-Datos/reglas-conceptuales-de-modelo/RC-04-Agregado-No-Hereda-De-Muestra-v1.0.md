# RC-04 — Agregado no hereda de Muestra

**Proyecto:** Sai-Service-Core
**Documento:** RC-04-Agregado-No-Hereda-De-Muestra-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Agregado y Muestra son entidades distintas: Agregado no es un subtipo de Muestra ni se puede tratar como tal. Un agregado siempre declara su cobertura y su advertencia.

## 2. Entidades involucradas

Muestra, Agregado.

## 3. Tipo de restricción

Restricción de identidad y de tipo.

## 4. Mecanismo de verificación conceptual

El modelo impide que un Agregado ocupe el lugar de una Muestra en cualquier consulta o exposición: los dos tipos no son intercambiables. Al servir un Agregado se comprueba que lleve cobertura y advertencia, y que el conteo de eventos como los microcortes provenga de Evento y no de la serie agregada.

## 5. RN o CU que la justifican

RN-10 (agregado con cobertura y advertencia). CU-06 (históricos y gráficas), CU-12 (informe de período). Justificada por la decisión PA-08: servir un agregado como si fuera una muestra es el modo de falla que borra los microcortes.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de PA-08 y del invariante I-20 |
