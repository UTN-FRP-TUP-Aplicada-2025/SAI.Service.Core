# RN-11 — Acción referida a una versión de política

**Proyecto:** Sai-Service-Core
**Documento:** RN-11-Accion-Referida-A-Version-De-Politica-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Toda acción de apagado referencia una versión de política concreta, nunca la política en general.

## 2. Justificación

Conviene poder saber con qué configuración exacta se tomó una decisión pasada. Si una acción refiriera a la política y no a su versión, al cambiar la configuración se perdería la trazabilidad de con qué parámetros se decidió apagar en cada momento.

## 3. Ámbito de aplicación

- En la creación de toda acción por el planificador ante una condición de disparo.

## 4. Consecuencia si se viola

Una acción que referencie la política en lugar de una de sus versiones viola la regla. La regla obliga a que la acción cite siempre la versión vigente en el instante de la decisión, que es inmutable.

## 5. CU afectados

CU-05 (Ejecución del apagado ordenado ante corte sostenido); se apoya en CU-03 (Configuración de políticas de apagado versionadas).

## 6. Pruebas que la verifican

Prueba de que toda acción registrada referencia una versión de política y no la política; prueba de que la versión referida es la vigente en el instante de la decisión. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-13).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-13 y de §20.E-4 |
