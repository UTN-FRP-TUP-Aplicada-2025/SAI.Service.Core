# RN-12 — Baja lógica y coherencia temporal de las intervenciones

**Proyecto:** Sai-Service-Core
**Documento:** RN-12-Baja-Logica-Y-Coherencia-Temporal-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Una unidad física nunca se borra: se da de baja lógica con su fecha y su motivo, permanece consultable con toda su historia y no admite intervenciones fechadas después de su baja.

## 2. Justificación

El borrado físico no existe en este dominio: una batería agotada o un equipo dado de baja deben poder consultarse con su historial completo para los informes históricos, pero no deben poder operarse. Distingue las dos lecturas de la baja: la entidad sigue siendo consultable, pero no sigue siendo operable.

## 3. Ámbito de aplicación

- En el cambio de estado de una unidad física a dado de baja, con fecha y motivo.
- En el registro y la ingesta de intervenciones que referencian unidades.
- En las consultas históricas e informes de período.

## 4. Consecuencia si se viola

Un borrado físico, o una intervención fechada después de la baja de la unidad que afecta, viola la regla. La regla obliga a la baja lógica y a rechazar por coherencia temporal toda intervención posterior a la baja, permitiendo en cambio referenciar la unidad para consultar su historia.

## 5. CU afectados

CU-08 (Recambio de batería y ficha de vida útil), CU-09 (Reparación y sustitución del SAI), CU-11 (Ingesta automatizada de intervenciones), CU-12 (Informe de período, inclusión de bajas).

## 6. Pruebas que la verifican

Prueba de que una unidad dada de baja sigue apareciendo en las consultas históricas; prueba de que una intervención fechada después de la baja se rechaza por coherencia temporal, mientras que referenciarla para consultar es válido. Referencia tentativa a 08-Calidad-Y-Pruebas (invariantes I-5 e I-6).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-5 e I-6, SOLUTION-INTAKE §7 (CL-20) y §20.E-6, §20.E-8 |
