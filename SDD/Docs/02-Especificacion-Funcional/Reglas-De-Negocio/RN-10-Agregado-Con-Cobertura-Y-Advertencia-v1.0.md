# RN-10 — Agregado con cobertura y advertencia obligatorias

**Proyecto:** Sai-Service-Core
**Documento:** RN-10-Agregado-Con-Cobertura-Y-Advertencia-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Toda respuesta que contenga un agregado declara su cobertura y su advertencia, y un agregado nunca se sirve por el mismo canal que una muestra sin decir que es un agregado.

## 2. Justificación

El promedio horario borra exactamente los microcortes que el sistema quiere estudiar. Servir un agregado como si fuera una muestra, o sin su cobertura, hace que un informe presente un promedio como verdad completa y mienta por omisión sobre lo que falta.

## 3. Ámbito de aplicación

- En toda consulta histórica y todo informe que use series agregadas.
- En la exposición de calidad de suministro construida sobre agregados horarios.

## 4. Consecuencia si se viola

Un agregado servido sin cobertura o sin advertencia, o presentado como muestra, viola la regla. La regla obliga a no servir la serie sin esos campos, y a que el conteo de microcortes salga de los eventos, nunca del promedio.

## 5. CU afectados

CU-06 (Históricos y gráficas de evolución), CU-12 (Informe de período y comparación de marcas).

## 6. Pruebas que la verifican

Prueba de que una serie de agregados incluye siempre cobertura y advertencia; prueba de que el conteo de microcortes proviene de los eventos y no de la serie agregada. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-20).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-20, PA-08 y §20.E-2, §20.E-7 |
