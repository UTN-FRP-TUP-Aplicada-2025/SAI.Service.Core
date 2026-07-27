---
doc_id: DOC-SAI-contrato-agentes-01
doc_type: contrato-agentes
title: Contrato de agentes — SAI.Service.Core
status: Vigente
rol_intervencion: [integrador, mantenedor, operador]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-04
  - ADR-06
  - ADR-15
  - ADR-25
---

# Contrato de agentes — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Todos
**Nivel:** Medio
**Tiempo estimado de lectura:** 6 min

## Resumen ejecutivo

Artefacto versionado del que se deriva el `AGENTS.md` de la raíz del repositorio. Fija el contexto que un agente de codificación necesita para intervenir SAI.Service.Core sin romper sus invariantes: cómo se construye y se testea, qué convenciones rigen, qué debe validar antes de dar por terminado un cambio y qué no se toca sin confirmación humana. Si este contrato y el `AGENTS.md` divergen, este es la fuente y el `AGENTS.md` se regenera.

## 1. Qué es este repositorio

Servicio único (`web-monolith`) en .NET 10 que administra el ciclo de vida y el monitoreo de un SAI que respalda un host Linux, con panel Blazor Server, persistencia SQLite y una API REST de ingesta. Clean Architecture en cinco assemblies bajo `src/SAI.Service.Core/`; el único proceso que arranca es `SAI.Service.Core.Web`.

## 2. Cómo se construye

```bash
devcontainer up --workspace-folder .   # el SDK de .NET 10 vive en el Dev Container (no hay dotnet en el host)
./scripts/build-all.sh                 # restaura y compila en Release
```

Salida esperada: `Build succeeded`, `0 Warning(s)`, `0 Error(s)`. Un warning es un error (`TreatWarningsAsErrors=true`). Sin `dotnet` local, usar el contenedor SDK efímero `mcr.microsoft.com/dotnet/sdk:10.0` montando el repo.

## 3. Cómo se corren los tests

```bash
dotnet test SAI.Service.Core.sln --configuration Release        # suite completa
dotnet test tests/SAI.Service.Core.Domain.Tests                 # un subconjunto
```

Debe devolver `0 failed`. Los tests que requieren SAI real por NUT quedan *skipped* sin hardware. La generación/validación de migraciones: `dotnet ef migrations has-pending-model-changes --project <Infrastructure> --startup-project <Infrastructure>` debe responder «sin cambios» antes de cerrar un cambio con entidad persistida.

## 4. Convenciones

- Commits: **Conventional Commits** (`feat`, `fix`, `docs`, `chore`); versionado **SemVer 2.0.0**.
- Código: cero-warnings, analizadores en `latest-recommended`, `Nullable enable`, `InvariantGlobalization` (sin culturas nombradas), español rioplatense en dominio y comentarios.
- Datos: historia **append-only** (nada de `UPDATE`/`DELETE` sobre tablas de hechos) y **procedencia obligatoria** en todo valor de dominio.

## 5. Comandos de validación antes de cerrar

```bash
./scripts/build-all.sh                                          # 0 warnings
dotnet test SAI.Service.Core.sln --configuration Release        # 0 failed
dotnet ef migrations has-pending-model-changes ...              # sin cambios (si tocó el modelo)
```

## 6. Límites de intervención (no tocar sin confirmación humana)

- El **framework SDD** (`IA/IA.SDD`) y cualquier carpeta `PROMPTs/` de los repositorios: solo lectura.
- La disciplina **append-only** y la **procedencia obligatoria**: no se relajan.
- El **estado base seguro** (arranque en solo aviso; no apagar sin los cuatro supuestos verificados): no se altera.
- Merge de PRs y borrado de ramas: los hace el humano; el agente deja la rama lista.
- Acciones outward-facing (publicar, desplegar, enviar): confirmación previa.

## 7. Punteros por intención

| Intención | Documento |
| --- | --- |
| Entender el sistema | [Vision-General-Sistema](Vision-General-Sistema-v1.0.md) |
| Ubicar código | [Recorrido-Codigo](Recorrido-Codigo-v1.0.md) |
| Agregar funcionalidad | [Guia-Contribucion](Guia-Contribucion-v1.0.md) |
| Extender el adaptador | [Guia-Extension](Guia-Extension-v1.0.md) |
| Levantar / operar | [Guia-Inicio-Rapido](Guia-Inicio-Rapido-v1.0.md) · [Runbook-Operacion](Runbook-Operacion-v1.0.md) |
| Contrato de la API | [05/Contratos-REST](../05-Arquitectura-Tecnica/Contratos-REST-v1.0.md) |
| Eventualidades vividas | [Bitacora-Eventualidades](Bitacora-Eventualidades-v1.0.md) |
