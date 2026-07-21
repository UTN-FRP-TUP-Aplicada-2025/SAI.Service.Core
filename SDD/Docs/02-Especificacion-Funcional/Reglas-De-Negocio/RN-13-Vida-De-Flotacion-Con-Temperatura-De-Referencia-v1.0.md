# RN-13 — Vida de flotación esperada con temperatura de referencia

**Proyecto:** Sai-Service-Core
**Documento:** RN-13-Vida-De-Flotacion-Con-Temperatura-De-Referencia-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

La vida de flotación esperada de un modelo de batería es inválida si no declara su temperatura de referencia.

## 2. Justificación

Sin la temperatura de referencia, el dato de vida esperada es incomparable entre modelos: la misma batería se declara de tres a cinco años a 20 grados según una convención y de hasta cinco años a 25 grados según otra. La comparación de marcas por vida útil exige que el dato lleve siempre su temperatura de referencia.

## 3. Ámbito de aplicación

- En el alta y la edición del catálogo de modelos de batería.
- En la proyección de la ficha de vida útil que compara la vida real contra la esperada.

## 4. Consecuencia si se viola

Una vida de flotación esperada sin temperatura de referencia se rechaza en el alta del modelo. Sin esta regla, las comparaciones entre modelos mezclarían convenciones distintas sin advertirlo.

## 5. CU afectados

CU-02 (Alta del parque y puesta en marcha, alta del modelo de batería); se apoya en CU-08 y CU-12 para la comparación por vida útil.

## 6. Pruebas que la verifican

Prueba de que un modelo de batería con vida de flotación esperada sin temperatura de referencia se rechaza, y con temperatura de referencia se acepta. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-21).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-21 y de §20.E-1 |
