# 06 — Backlog Técnico · Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Versión de la sección:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)

Índice navegable del backlog de Sai-Service-Core. Punto de entrada para las revisiones acotadas de AG-02 (trazabilidad funcional), AG-05 (justificación técnica), AG-07 (secuencia y capacidad) y AG-08 (verificabilidad).

## Documentos

- [`Product-Backlog-v1.0.md`](Product-Backlog-v1.0.md) — objetivos del producto y MVP, épicas EP-XX, índice de historias, métricas y refinamiento.
- [`Backlog-Tecnico-v1.0.md`](Backlog-Tecnico-v1.0.md) — épicas técnicas, 30 tareas técnicas BT-XX inline y matriz BT↔US↔CU.
- [`Definition-Of-Ready-v1.0.md`](Definition-Of-Ready-v1.0.md) — DoR vigente para US y BT.
- [`historias-usuario/`](historias-usuario/) — 26 historias individuales US-01 a US-26 con criterios BDD, trazabilidad y DoR check.

## Épicas vigentes

| Épica | Nombre | Fase | Release |
|---|---|---|---|
| EP-01 | Fundaciones y decisiones de arranque | F0 | v0.0 |
| EP-02 | Esqueleto caminante y panel base | F1 | v0.2 |
| EP-03 | Persistencia, alta de administrador y sesión | F1 | v0.4 |
| EP-04 | Alta de equipos y políticas de apagado | F2 | v0.6 |
| EP-05 | Monitoreo, salud e históricos | F3 | v0.9 |
| EP-06 | Verificación y ciclo de vida de los equipos | F4 | v0.12 |
| EP-07 | Integración e informes | F5 | v1.0 |

## US Must del MVP

El MVP lo definen las 18 historias Must (114 SP):

- **Sesión:** US-01 alta inicial del administrador, US-02 login/cierre/cambio de contraseña.
- **Alta y políticas:** US-03 descubrimiento y prueba de conexión, US-04 catálogo e inventario, US-05 siembra de verificaciones y arranque en solo aviso, US-06 política versionada.
- **Monitoreo y salud:** US-07 estado en vivo, US-08 sondeo y persistencia con calidad, US-09 eventos y alerta de conectividad, US-10 procedencia visible, US-11 históricos y gráficas, US-12 prueba de batería, US-13 veredicto de salud.
- **Apagado y verificación:** US-14 apagado ordenado ante corte, US-15 bloqueo por verificación y efecto observado, US-16 ventana de mantenimiento guiada.
- **Integración:** US-21 ingesta idempotente, US-22 rechazo de conflictos e invariantes rotos.

Historias Should (39 SP): US-17 renovación por evidencia, US-18 recambio de batería, US-19 ficha de vida útil, US-20 sustitución del SAI, US-23 informe de período, US-24 comparación de marcas. Historias Could (8 SP), diferidas a v2: US-25 adaptador directo, US-26 add-ons de dialecto.

## BT prioritarias

- **Sprint 0 (spikes de decisión):** BT-01 ubicación de la herramienta de acceso al SAI, BT-02 TLS en la LAN, BT-04 firma del adaptador, BT-03 contrato de rectificación.
- **Camino crítico del apagado garantizado (NB-01):** BT-16 políticas versionadas, BT-20 disparo sin flag LB, BT-22 ciclo forzado, BT-23 bloqueo por verificación, BT-24 validación por efecto observado, BT-25 verificación de BIOS por comportamiento.
- **Fundaciones de confianza (NB-03, NB-05):** BT-08 `Valor<T>` con `Origen`, BT-09 historia append-only, BT-12 vigencia con `ResolutorTemporal`.

## DoR vigente

`Definition-Of-Ready-v1.0.md`: 7 criterios para US, 5 para BT, con excepciones para spikes, historias Could diferidas y la ventana de mantenimiento. Aprobador: el administrador único (rol combinado Scrum Master / Product Owner); titularidad del AG-06. La DoR filtra el arranque; la Definition of Done de 08 filtra el cierre.
