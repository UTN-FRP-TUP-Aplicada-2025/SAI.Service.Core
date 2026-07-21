# RN-05 — Procedencia obligatoria y origen declarado de todo valor

**Proyecto:** Sai-Service-Core
**Documento:** RN-05-Procedencia-Obligatoria-De-Todo-Valor-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Todo valor almacenado declara su origen, sin excepción; y todo valor de origen derivado declara al menos una variable de la que deriva.

## 2. Justificación

El modo de falla más probable del sistema no es un error de código, sino una conclusión falsa sobre datos que parecían medidos pero fueron interpolados por el driver. Declarar el origen de cada valor responde, sin leer código, si un número lo midió el aparato o lo calculó el software, y permite auditar la cadena de derivación.

## 3. Ámbito de aplicación

- En la persistencia de toda muestra, agregado, evento, prueba, intervención y medición.
- En la exposición de cualquier valor por el panel o por la interfaz de integración.

## 4. Consecuencia si se viola

Un valor sin origen, o un derivado sin su lista de variables de origen, se rechaza en la escritura. Sin esta regla, los cálculos y las conclusiones se apoyarían en datos cuya naturaleza se desconoce.

## 5. CU afectados

CU-04 (Monitoreo en vivo), CU-06 (Históricos y gráficas), CU-07 (Prueba de batería), CU-08 (Recambio de batería), CU-12 (Informe de período). De forma transversal, todo CU que persista o exponga valores.

## 6. Pruebas que la verifican

Prueba de que ningún valor se persiste sin origen; prueba de que un valor derivado sin variables de origen se rechaza. Referencia tentativa a 08-Calidad-Y-Pruebas (invariantes I-7 e I-8).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-7 e I-8, PA-06 y R-13 |
