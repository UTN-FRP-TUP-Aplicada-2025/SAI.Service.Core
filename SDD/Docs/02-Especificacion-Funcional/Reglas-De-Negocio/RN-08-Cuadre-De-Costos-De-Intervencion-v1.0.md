# RN-08 — Cuadre de costos de una intervención

**Proyecto:** Sai-Service-Core
**Documento:** RN-08-Cuadre-De-Costos-De-Intervencion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

El total de costos de una intervención debe igualar la suma de sus repuestos y su mano de obra.

## 2. Justificación

Es el invariante que la ingesta externa rompe primero. Sin él, los costos agregados de los informes de período quedan mal en silencio, y las comparaciones de marcas por costo por año de servicio se apoyan en cifras que no cierran.

## 3. Ámbito de aplicación

- En el registro manual de una intervención por el administrador.
- En la ingesta de intervenciones desde un sistema externo.

## 4. Consecuencia si se viola

Una intervención cuyo total no iguala la suma de repuestos y mano de obra se rechaza por validación, indicando el campo y el invariante, y no se aplica ningún efecto.

## 5. CU afectados

CU-08 (Recambio de batería y ficha de vida útil), CU-11 (Ingesta automatizada de intervenciones).

## 6. Pruebas que la verifican

Prueba de que una intervención con repuestos por 52.000 y mano de obra por 15.000 y total 60.000 se rechaza, y con total 67.000 se acepta. Referencia tentativa a 08-Calidad-Y-Pruebas (cuadre de costos).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §7 (CL-22), §17 P.3 y §20.E-8 |
