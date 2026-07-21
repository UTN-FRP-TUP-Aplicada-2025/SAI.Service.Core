# RC-03 — Sucesión de vínculos sin hueco al cerrar y abrir

**Proyecto:** Sai-Service-Core
**Documento:** RC-03-Sucesion-De-Vinculos-Sin-Hueco-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

Cuando una intervención cierra un vínculo temporal y abre otro en continuidad, el instante de cierre del primero coincide con el instante de apertura del segundo: no queda hueco no intencional.

## 2. Entidades involucradas

MontajeBateria, CoberturaHost, Intervencion.

## 3. Tipo de restricción

Restricción referencial y de continuidad temporal.

## 4. Mecanismo de verificación conceptual

Al aplicar los efectos de una intervención de recambio o de reemplazo en continuidad, se comprueba que el fin del vínculo cerrado sea igual al inicio del vínculo abierto. Un hueco intencional, cuando el host quedó sin protección, es legítimo y se mide como días sin protección; lo que se prohíbe es el hueco por error en una sucesión que debía ser continua.

## 5. RN o CU que la justifican

RN-12. CU-08 (recambio de batería, continuidad de montaje), CU-09 (sustitución con cobertura suplente).

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada del invariante I-3 y de §20.E-6 |
