# AGENTS.md — SAI.Service.Core

Contrato de contexto para agentes de codificación. Se deriva de `SDD/Docs/11-Documentacion/Contrato-Agentes-v1.0.md`; si divergen, ese contrato es la fuente y este archivo se regenera.

## Qué es

Servicio único (`web-monolith`) en .NET 10 que administra el ciclo de vida y el monitoreo de un SAI que respalda un host Linux. Panel Blazor Server + API REST de ingesta, persistencia SQLite. Clean Architecture en cinco assemblies bajo `src/SAI.Service.Core/`; el único proceso que arranca es `SAI.Service.Core.Web`.

## Construir

No hay `dotnet` en el host: el SDK de .NET 10 vive en el Dev Container.

```bash
devcontainer up --workspace-folder .
./scripts/build-all.sh          # esperado: Build succeeded, 0 Warning(s), 0 Error(s)
```

Sin `dotnet` disponible, usar el contenedor SDK efímero `mcr.microsoft.com/dotnet/sdk:10.0` montando el repositorio.

## Testear

```bash
dotnet test SAI.Service.Core.sln --configuration Release     # esperado: 0 failed
dotnet test tests/SAI.Service.Core.Domain.Tests              # subconjunto
```

Los tests contra SAI real por NUT quedan *skipped* sin hardware.

## Validar antes de cerrar un cambio

```bash
./scripts/build-all.sh                                       # 0 warnings (son errores)
dotnet test SAI.Service.Core.sln --configuration Release     # 0 failed
# si tocaste el modelo de datos:
dotnet ef migrations has-pending-model-changes \
  --project src/SAI.Service.Core/SAI.Service.Core.Infrastructure \
  --startup-project src/SAI.Service.Core/SAI.Service.Core.Infrastructure   # esperado: sin cambios
```

## Convenciones

- Commits **Conventional Commits** (`feat`, `fix`, `docs`, `chore`); versionado **SemVer 2.0.0**.
- **Cero-warnings** (`TreatWarningsAsErrors=true`), `Nullable enable`, `InvariantGlobalization` (sin culturas nombradas), español rioplatense en dominio y comentarios.
- Historia **append-only** (nada de `UPDATE`/`DELETE` sobre tablas de hechos) y **procedencia obligatoria** en todo valor de dominio.
- Agregar una funcionalidad, de adentro hacia afuera: Domain → Application (servicio + puerto) → Infrastructure (repo + `Modelo*.Configurar` + migración) → Api/Web → seed → tests → docs.

## Límites (no tocar sin confirmación humana)

- El framework SDD (`IA/IA.SDD`) y cualquier carpeta `PROMPTs/`: solo lectura.
- La disciplina append-only, la procedencia obligatoria y el estado base seguro (arranque en solo aviso; no apagar sin los cuatro supuestos verificados): no se relajan.
- Merge de PRs y borrado de ramas: los hace el humano; el agente deja la rama lista y pusheada.
- Todo acceso al SAI pasa por `IAdaptadorConexion` (nunca NUT directo).

## Dónde mirar

| Necesito | Archivo |
| --- | --- |
| Entender el sistema | `SDD/Docs/11-Documentacion/Vision-General-Sistema-v1.0.md` |
| Ubicar código | `SDD/Docs/11-Documentacion/Recorrido-Codigo-v1.0.md` |
| Agregar funcionalidad | `SDD/Docs/11-Documentacion/Guia-Contribucion-v1.0.md` |
| Extender el adaptador | `SDD/Docs/11-Documentacion/Guia-Extension-v1.0.md` |
| Operar / desplegar | `SDD/Docs/11-Documentacion/Runbook-Operacion-v1.0.md` · `Guia-Despliegue-v1.0.md` |
| Contrato de la API | `SDD/Docs/05-Arquitectura-Tecnica/Contratos-REST-v1.0.md` |
| Eventualidades vividas | `SDD/Docs/11-Documentacion/Bitacora-Eventualidades-v1.0.md` |
