# RC-07 — Resolución temporal de la batería

**Proyecto:** Sai-Service-Core
**Documento:** RC-07-Resolucion-Temporal-De-La-Bateria-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

La historia no guarda a qué batería pertenece una métrica: guarda el dispositivo y el instante. La batería se resuelve consultando el MontajeBateria vigente en ese instante. Una PruebaBateria congela el montaje resuelto en el instante de la prueba.

## 2. Entidades involucradas

Muestra, Evento, PruebaBateria, MontajeBateria, Dispositivo, Bateria.

## 3. Tipo de restricción

Restricción de derivación y de resolución referencial temporal.

## 4. Mecanismo de verificación conceptual

Para atribuir una métrica a una batería, se toma su dispositivo e instante y se busca el MontajeBateria cuyo intervalo contiene ese instante. Corregir la fecha de un recambio reatribuye automáticamente el histórico afectado, sin migrar datos, porque la atribución se recalcula por el vínculo. La PruebaBateria, en cambio, fija su montaje en el momento de la prueba y no se recalcula.

## 5. RN o CU que la justifican

RN-06 (aptitud y comparabilidad de pruebas). CU-07 (prueba de batería, congelado del montaje), CU-08 (recambio y corrección retroactiva), CU-12 (informe por período). Justificada por la decisión PA-05 y el patrón de acceso del intake §17 P.4.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-15 y del patrón de resolución temporal del intake |
