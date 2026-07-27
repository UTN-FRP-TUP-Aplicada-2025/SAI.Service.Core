---
doc_id: DOC-SAI-contribucion-01
doc_type: contribucion
title: Guía de contribución — SAI.Service.Core
status: Vigente
rol_intervencion: [mantenedor]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-04
  - ADR-06
  - ADR-25
---

# Guía de contribución — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Mantenedor
**Nivel:** Medio
**Tiempo estimado de lectura:** 10 min

## Resumen ejecutivo

Cómo preparar el entorno, correr los tests y agregar una funcionalidad de punta a punta sin romper el diseño. La disciplina no negociable de este repositorio son tres cosas: cero-warnings (los warnings compilan como errores), historia append-only (las tablas de hechos no se editan ni se borran) y procedencia obligatoria (ningún valor de dominio se persiste sin declarar su origen). El resto es convención.

## 1. Setup del entorno

```bash
devcontainer up --workspace-folder .     # SDK de .NET 10 dentro del contenedor (ADR-25)
./scripts/build-all.sh                    # restaura y compila la solución en Release
```

Verificación: `build-all.sh` termina con `Build succeeded` y `0 Warning(s)`. Si aparece un warning, es un error: corregilo antes de seguir. No hay `dotnet` en el host; todo comando `dotnet` corre dentro del Dev Container.

## 2. Correr los tests

```bash
dotnet test SAI.Service.Core.sln --configuration Release
```

Devuelve el resumen por proyecto de test: Domain, Application e Integration. Los de integración usan `WebApplicationFactory` con una base SQLite temporal por instancia; algunos tests contra el SAI real por NUT se marcan como *skipped* cuando no hay hardware. Para correr un subconjunto: `dotnet test tests/SAI.Service.Core.Domain.Tests`. La estrategia de testing completa (pirámide, cobertura por capa) vive en [08-Calidad-Y-Pruebas](../08-Calidad-Y-Pruebas/); esta guía no la redefine.

Cuando no hay `dotnet` en el entorno de generación, la compilación, los tests y la generación de migraciones se corren en un contenedor SDK efímero (`mcr.microsoft.com/dotnet/sdk:10.0`) montando el repo; es también como se generaron las migraciones (escritas fuera de un IDE). Ver [Bitacora-Eventualidades](Bitacora-Eventualidades-v1.0.md) `EVE-02`.

## 3. Convenciones

- **Versionado SemVer 2.0.0** y **Conventional Commits** sin excepciones (`feat(...)`, `fix(...)`, `docs(...)`). Ejemplo correcto: `feat(politicas): configuración versionada (EP-04)`. Incorrecto: `cambios varios`.
- **Cero-warnings**: `Directory.Build.props` fija `TreatWarningsAsErrors=true` y analizadores en `latest-recommended`. Nombres de método sin guion bajo (CA1707), etc.
- **Globalization-invariant**: `InvariantGlobalization=true`. No usar culturas nombradas (`CultureInfo.GetCultureInfo("es-AR")` rompe en runtime); usar `InvariantCulture` o formato a mano. Ver `EVE-03`.
- **Español rioplatense** en identificadores de dominio y comentarios, coherente con el resto del código.

## 4. Cómo agregar una funcionalidad de punta a punta

Caso concreto recorrido: agregar una política de apagado versionada fue exactamente esto (EP-04). El orden es de adentro hacia afuera:

1. **Dominio** (`Domain/<Area>/`): la entidad y sus invariantes por construcción. Si es historia, implementa `IEntidadHistoria`.
2. **Aplicación** (`Application/<Flujo>/`): el puerto (interfaz de repositorio) y el servicio de caso de uso que orquesta el dominio.
3. **Infraestructura** (`Infrastructure/Persistencia/`): el repositorio EF, la clase `Modelo*.Configurar` y —si hay entidad persistida nueva— la migración. Registrar todo en `DependencyInjection.cs`.
4. **Superficie** (`Api/Endpoints/` o `Web/Components/Pages/`): el endpoint o el panel.
5. **Seed** (`Web/Program.cs`) si el arranque necesita datos iniciales idempotentes.
6. **Tests**: dominio (invariantes), aplicación (el servicio con un repositorio falso) e integración (`WebApplicationFactory`).
7. **Documentación**: actualizar el `CHANGELOG.md`, el mini-plan (07) y esta categoría 11 si cambia el recorrido o el contrato.

```yaml
entradas:
  - "entidad de dominio + invariantes"
  - "puerto + servicio de aplicación"
  - "repositorio EF + Modelo*.Configurar + migración"
salidas:
  - "endpoint/panel + seed + tests en verde"
validaciones:
  - "dotnet build: 0 warnings"
  - "dotnet test: 0 failed"
  - "dotnet ef migrations has-pending-model-changes: sin cambios"
```

## 5. Qué no hacer

- **No editar ni borrar filas de tablas de hechos.** Son append-only (ADR-04); una corrección se modela como un hecho nuevo, no como un `UPDATE`. El `InterceptorAppendOnly` lanza excepción si se intenta.
- **No persistir un valor sin procedencia.** `Valor<T>` exige `Origen` (ADR-06); un derivado marcado como medido produce una conclusión falsa.
- **No invertir dependencias.** Si el dominio necesita algo de infraestructura, va un puerto en `Application/Abstractions/`.
- **No hablar con NUT fuera del adaptador.** Todo acceso al SAI pasa por `IAdaptadorConexion` (ADR-27). Ver [Guia-Extension](Guia-Extension-v1.0.md).
- **No silenciar un warning.** Se corrige; no hay pragma de conveniencia.

## 6. Preguntas guía

- ¿Tu cambio agrega una tabla? Entonces hay migración y `has-pending-model-changes` debe quedar limpio.
- ¿Tu cambio toca el apagado o la verificación? Entonces revisá que el estado base siga siendo seguro (solo aviso) y agregá el test que lo prueba.
- ¿Tu cambio expone algo a un tercero? Entonces actualizá el contrato en [05/Contratos-REST](../05-Arquitectura-Tecnica/Contratos-REST-v1.0.md) y su ejemplo en [10-Examples](../10-Examples/).
