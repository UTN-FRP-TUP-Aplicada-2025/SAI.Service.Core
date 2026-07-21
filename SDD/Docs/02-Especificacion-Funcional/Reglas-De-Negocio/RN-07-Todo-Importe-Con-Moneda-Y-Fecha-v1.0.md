# RN-07 — Todo importe con moneda y fecha

**Proyecto:** Sai-Service-Core
**Documento:** RN-07-Todo-Importe-Con-Moneda-Y-Fecha-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Todo importe monetario declara su moneda y su fecha; y todo equivalente normalizado a otra moneda viaja marcado como derivado, con su fuente de cotización.

## 2. Justificación

En un contexto de inflación alta, un costo normalizado sin marcar es el mismo error que presentar un valor interpolado como medido. Comparar importes de años distintos sin moneda ni fecha no significa nada; por eso cada cifra debe llevar consigo cuándo y en qué moneda se expresó.

## 3. Ámbito de aplicación

- En el registro de todo costo de adquisición y de intervención.
- En los informes de período y en la comparación de modelos por costo por año de servicio.

## 4. Consecuencia si se viola

Un importe sin moneda o sin fecha se rechaza. Sin esta regla, los costos agregados y las comparaciones de marcas quedarían mal en silencio.

## 5. CU afectados

CU-08 (Recambio de batería y ficha de vida útil), CU-11 (Ingesta automatizada de intervenciones), CU-12 (Informe de período y comparación de marcas), CU-02 (costos de adquisición en el alta).

## 6. Pruebas que la verifican

Prueba de que un importe sin moneda o sin fecha se rechaza; prueba de que el equivalente normalizado aparece marcado como derivado con su fuente de cotización. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-18).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-18 y de SOLUTION-INTAKE §10 |
