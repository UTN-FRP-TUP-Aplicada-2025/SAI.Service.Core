# 09 · DevOps — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** README.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

Índice de los artefactos DevOps del proyecto. `Sai-Service-Core` es `web-monolith`, solución de un solo proyecto (caso degenerado): **no hay pipeline de nivel solución** (`Pipeline-Solucion` se omite). El artefacto publicado es **image-docker**.

Estos documentos **no redefinen** la Definition of Done ni los quality gates de 08: los **ejecutan como gates**. Cada gate del pipeline referencia el criterio de DoD de 08 o el NFR de 05 que verifica.

---

## Orden de lectura sugerido

| Orden | Documento | Estado | Qué cubre |
| --- | --- | --- | --- |
| 1 | [Estrategia-Versionado-v1.0.md](Estrategia-Versionado-v1.0.md) | Borrador | SemVer 2.0.0, Conventional Commits, MinVer, branching trunk-based (`main` + `etapa/NN`), canal de imagen, deprecation v1→v2 de la API |
| 2 | [Pipeline-CI-CD-v1.0.md](Pipeline-CI-CD-v1.0.md) | Borrador | Los 10 stages de GitHub Actions, matriz de un eje (ubuntu-latest / .NET 10 / linux-amd64), caché de NuGet, promoción DEV→PROD, rollback, y trazabilidad de los NFR de 05 a su gate (22 numéricos con gate; 3 PENDIENTE) |
| 3 | [Entornos-Deploy-v1.0.md](Entornos-Deploy-v1.0.md) | Borrador | Ambientes DEV (Dev Container) y PROD (`i7infra`); desviación del piso DEV/QA/STAGING/PROD (sin QA/STAGING); config 12-factor y secretos |
| 4 | [Guia-Publicacion-Image-Docker-v1.0.md](Guia-Publicacion-Image-Docker-v1.0.md) | Borrador | Pre-requisitos, stage de publicación al registro privado, verificación post-publish, rollback y métricas de la imagen |
| 5 | [Supply-Chain-Seguridad-v1.0.md](Supply-Chain-Seguridad-v1.0.md) | Borrador | SBOM CycloneDX, firma cosign keyless, SLSA L2 objetivo, SCA, SAST/DAST y política de CVE |

El orden va del contrato de versión (bisagra código ↔ artefacto), al pipeline que lo materializa, a los ambientes donde despliega, a la guía de publicación del artefacto, y a la política de cadena de suministro que lo protege.

---

## Vínculos upstream

- **08 — Calidad y pruebas:** `Definition-Of-Done-v1.0.md` (los 10 gates ejecutan la DoD, no la redefinen) y `Estrategia-Calidad-v1.0.md §3` (los 10 quality gates). El pipeline es la realización downstream de esos gates.
- **05 — Arquitectura:** `Arquitectura-Solucion-v1.0.md §8` (los 25 NFR N-01..N-25, de los cuales 22 tienen objetivo numérico y mecanismo de medición y 3 quedan PENDIENTE —N-03, N-20, N-25—) y los ADR (en particular ADR-03 anclaje del USB, ADR-04 historia append-only, ADR-11 validación por efecto observado). Cada NFR numérico tiene su gate en `Pipeline-CI-CD §8`.
- **06 / 07 — Backlog y plan:** el branching por etapas (`etapa/NN-<slug>`) se alinea con la descomposición de delivery del intake §15 y el Mini-Plan de 07.
- **Intake:** `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md §17.P.7` (versionado) y `§17.P.8` (pipeline, ambientes, rollback).

## Nota sobre workflows reales

Estos documentos son la especificación de DevOps, no los workflows ejecutables. Los archivos reales de GitHub Actions (`.github/workflows/`) se implementan en el repositorio siguiendo esta especificación; el README raíz de la solución (Fase H) documenta la reproducción local, según ADR-23; los comandos exactos de cada stage están en `Pipeline-CI-CD §7`.

## PENDIENTE trazados

Se arrastran del intake y de 05, declarados como tales (no inventados):

- **N-25** SLO de disponibilidad del propio servicio (se propone «rondas completadas / esperadas ≥ 0,99 mensual»).
- **N-20** tamaño máximo del archivo SQLite tras la agregación (se mide bajo carga simulada antes de producción; R-07).
- **N-03** resto del apagado del SO (se cronometra en la ventana de mantenimiento UF-8, no en el pipeline).
- **TLS** del panel y de la API en la LAN (ADR-20, Sprint 0), ligado a la decisión de no aplicar DAST.
- La desviación DEV/PROD sin QA/STAGING está registrada en **ADR-24** (05-Arquitectura-Tecnica), fundamentada en `Entornos-Deploy §1.1`.
- **ADR-19** ubicación de NUT (contenedor o host), que afecta el mapeo del USB en el deploy.

---

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Índice inicial de la categoría 09 con orden de lectura (versionado → pipeline → entornos → publicación → supply chain), estado de cada artefacto, vínculos upstream a 05/06/07/08 y PENDIENTE trazados. |
