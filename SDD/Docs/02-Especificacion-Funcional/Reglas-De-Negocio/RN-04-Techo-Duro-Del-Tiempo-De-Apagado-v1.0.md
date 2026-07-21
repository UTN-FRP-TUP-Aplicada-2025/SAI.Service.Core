# RN-04 — Techo duro del tiempo reservado de apagado

**Proyecto:** Sai-Service-Core
**Documento:** RN-04-Techo-Duro-Del-Tiempo-De-Apagado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

El tiempo reservado para el apagado de una versión de política no puede superar los 540 segundos.

## 2. Justificación

El retardo de corte del equipo tiene un techo duro verificado de 540 segundos, no negociable, impuesto por el equipamiento. Si el apagado del sistema operativo no cabe en ese techo, el equipo corta con el host a medio bajar, con riesgo de corrupción.

## 3. Ámbito de aplicación

- En la creación y publicación de cada versión de política de apagado.
- En la preparación de la secuencia de apagado por el planificador.

## 4. Consecuencia si se viola

Si se admitiera un tiempo reservado mayor al techo, el equipo cortaría antes de que el host termine de apagarse. La regla obliga a rechazar toda versión de política cuyo tiempo reservado supere 540 segundos.

## 5. CU afectados

CU-03 (Configuración de políticas de apagado versionadas), CU-05 (Ejecución del apagado ordenado ante corte sostenido).

## 6. Pruebas que la verifican

Prueba de que un tiempo reservado de 540 segundos se acepta y uno de 541 o más se rechaza; prueba de que la ejecución respeta el valor de la versión vigente. Referencia tentativa a 08-Calidad-Y-Pruebas (invariante I-10).

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-10 y de SOLUTION-INTAKE §10 y §17 P.10 |
