# RC-08 — Baja lógica y coherencia temporal de la unidad física

**Proyecto:** Sai-Service-Core
**Documento:** RC-08-Baja-Logica-Y-Coherencia-Temporal-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Una UnidadFisica nunca se borra: se marca con estado, fecha de baja y motivo de baja. Una unidad dada de baja sigue siendo consultable con toda su historia, pero no admite operaciones fechadas después de su baja.

## 2. Entidades involucradas

UnidadFisica y sus especializaciones Host, Dispositivo y Bateria; Intervencion.

## 3. Tipo de restricción

Restricción de valor permitido de estado y referencial temporal.

## 4. Mecanismo de verificación conceptual

No existe una operación conceptual de borrado de una unidad física; el retiro se representa por cambio de estado con fecha y motivo. Al aceptar una intervención que opera sobre una unidad, se comprueba que su instante no sea posterior a la baja de esa unidad; referenciar la unidad para consultar su historia sí es válido.

## 5. RN o CU que la justifican

RN-12 (baja lógica y coherencia temporal). CU-08 (recambio, baja de la batería), CU-09 (baja o reparación del equipo), CU-11 (rechazo de intervención posterior a la baja), CU-12 (inclusión de bajas en informes).

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-5 e I-6 y de SOLUTION-INTAKE §7 (CL-20) |
