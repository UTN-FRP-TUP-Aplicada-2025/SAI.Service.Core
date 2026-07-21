# RC-02 — Vigencia como entidad con intervalo

**Proyecto:** Sai-Service-Core
**Documento:** RC-02-Vigencia-Como-Entidad-Con-Intervalo-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Enunciado

La vigencia se modela como entidad con intervalo, no como atributo de la unidad. Para un mismo dispositivo y posición, los intervalos de MontajeBateria no se solapan y a lo sumo uno está vigente; lo mismo para CoberturaHost por host.

## 2. Entidades involucradas

MontajeBateria, CoberturaHost, Dispositivo, Bateria, Host.

## 3. Tipo de restricción

Restricción de cardinalidad y de no solapamiento temporal.

## 4. Mecanismo de verificación conceptual

Para cada clave (dispositivo y posición, u host), se comprueba que no existan dos vínculos cuyos intervalos se intersequen, y que haya como máximo un vínculo con fin abierto. Un fin abierto significa vigente, no desconocido.

## 5. RN o CU que la justifican

RN-12 (baja lógica y coherencia temporal) de forma complementaria. CU-02 (alta de vínculos), CU-08 (recambio), CU-09 (reparación y sustitución). Justificada por la decisión PA-05 del intake: con la vigencia como atributo, el caso de un equipo en reparación cubierto por otro no sería representable.

## 6. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de los invariantes I-1, I-2 e I-4 y de PA-05 |
