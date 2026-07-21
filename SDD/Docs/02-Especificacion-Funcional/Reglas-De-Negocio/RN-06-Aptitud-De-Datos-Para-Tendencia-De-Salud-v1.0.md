# RN-06 — Aptitud de datos para la tendencia de salud

**Proyecto:** Sai-Service-Core
**Documento:** RN-06-Aptitud-De-Datos-Para-Tendencia-De-Salud-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Un valor cuya procedencia sea derivado, estimado por el driver o imputado no es apto para la tendencia de salud de batería; y una prueba de batería con diferencia de carga concurrente fuera de tolerancia no es comparable y no entra en la tendencia.

## 2. Justificación

La tendencia de salud es lo único que el método adoptado puede afirmar, y solo si se compara lo comparable: descargas controladas a carga igualada, sobre valores realmente medidos. Usar un valor interpolado por el driver, como la carga de batería, o una prueba tomada con carga distinta, produce una conclusión falsa sobre la salud.

## 3. Ámbito de aplicación

- En la construcción de la tendencia de salud a partir de pruebas de batería.
- En la evaluación de comparabilidad de cada prueba contra la línea base.

## 4. Consecuencia si se viola

Si un valor no apto o una prueba no comparable entrara en la tendencia, la serie de salud quedaría corrupta sin que nada lo indicara. La regla obliga a rechazar los valores no aptos y a marcar y excluir las pruebas no comparables.

## 5. CU afectados

CU-07 (Prueba de batería y veredicto de salud), CU-04 (Monitoreo en vivo, marca de la carga de batería como derivada), CU-08 (ficha de vida útil y su tendencia).

## 6. Pruebas que la verifican

Prueba de que una tendencia de salud rechaza entradas de procedencia derivado o estimado por el driver; prueba de que una prueba con carga concurrente fuera de tolerancia se marca no comparable y queda excluida. Referencia tentativa a 08-Calidad-Y-Pruebas (invariantes I-9 e I-16).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-9 e I-16 y de PA-13 |
