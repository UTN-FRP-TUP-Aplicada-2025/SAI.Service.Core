# 07 — Plan de Sprint · Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Versión de la sección:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-07)

Índice navegable de la planificación de sprints de Sai-Service-Core. Punto de entrada para las revisiones acotadas de AG-06 (prioridad y sprint goal), AG-08 (casos de aceptación y DoD) y AG-05 (impacto de ADR en el alcance).

## Modo de un solo desarrollador

Este es un proyecto de **un único desarrollador**. Por la regla §2.2 de 07-Rules-Plan-Sprint (modo 1 dev), la categoría 07 se reduce a un **Mini-Plan** condensado que **sustituye** a los cuatro artefactos de equipo. En consecuencia:

- **No existen** `Plan-Iteracion-Sprint-XX-v1.0.md` (planes de iteración por sprint).
- **No existen** `Template-Sprint-Review-v1.0.md` ni `Template-Sprint-Retrospectiva-v1.0.md`.
- **No existe** `Velocidad-Equipo-v1.0.md` (tracking de velocity con promedio móvil).

El refinamiento y el cierre de etapa son ligeros y autogestionados: el administrador único, en el rol combinado de Scrum Master / Product Owner / Dev, valida cada etapa antes de arrancar la siguiente.

## Documentos

- [`Mini-Plan-v1.0.md`](Mini-Plan-v1.0.md) — plan único condensado: información general, estructura por etapas, sprint goal y compromiso de cada etapa, criterios de cierre, riesgos con mitigación, trazabilidad a CU/NB y bitácora de avance semanal.

## Estructura de etapas

| Etapa | Épica(s) | Fase | Flujos UF | Release |
|---|---|---|---|---|
| Sprint 0 — Arranque | EP-01 + EP-02 | F0 + F1 (1-2) | — habilitante | v0.2 |
| Etapa 1 — Persistencia, administrador y sesión | EP-03 | F1 (3-4) | — habilitante | v0.4 |
| Etapa 2 — Alta de equipos y políticas | EP-04 | F2 | UF-1 → UF-2 | v0.6 |
| Etapa 3 — Monitoreo, salud e históricos | EP-05 | F3 | UF-3 → UF-5 → UF-4 | v0.9 |
| Etapa 4 — Verificación y ciclo de vida | EP-06 | F4 | UF-8 → UF-6 → UF-7 | v0.12 |
| Etapa 5 — Integración e informes | EP-07 | F5 | UF-10 → UF-9 | v1.0 |

## Etapa actual

**Sprint 0 — Arranque** (EP-01 + EP-02): cierre de las decisiones de arranque como ADR (ADR-19, ADR-20, ADR-21, ADR-22) y levantamiento del esqueleto que compila, corre por script y se navega. No entrega valor de negocio; habilita las etapas siguientes.

## Referencias

- Definition of Ready (filtro de entrada): [`../06-Backlog-Tecnico/Definition-Of-Ready-v1.0.md`](../06-Backlog-Tecnico/Definition-Of-Ready-v1.0.md).
- Definition of Done canónica (filtro de cierre): **08-Calidad-Y-Pruebas / Definition-of-Done — pendiente de generación**.
- Upstream: [`../00-Contexto/Roadmap-Producto-v1.0.md`](../00-Contexto/Roadmap-Producto-v1.0.md), [`../06-Backlog-Tecnico/`](../06-Backlog-Tecnico/), [`../02-Especificacion-Funcional/`](../02-Especificacion-Funcional/), [`../05-Arquitectura-Tecnica/Adrs/`](../05-Arquitectura-Tecnica/Adrs/).
