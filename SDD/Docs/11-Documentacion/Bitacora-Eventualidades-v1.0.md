---
doc_id: DOC-SAI-bitacora-01
doc_type: bitacora
title: Bitácora de eventualidades — SAI.Service.Core
status: Vigente
rol_intervencion: [operador, mantenedor]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-25
  - ADR-29
  - CU-05
---

# Bitácora de eventualidades — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Operador · Mantenedor
**Nivel:** Medio
**Tiempo estimado de lectura:** 8 min

## Resumen ejecutivo

Registro de las situaciones no previstas que aparecieron durante la construcción, el despliegue y la operación. Cada entrada conserva lo que un documento permanente nunca guarda: los intentos que no funcionaron. Eso es lo que evita que el siguiente los repita. Las eventualidades ya absorbidas por un documento permanente lo declaran en su campo `destino`.

## EVE-01 — upsmon no dispara el apagado del host

- **Ámbito:** solución (entorno NUT del host) · **Fecha:** 2026-07-24 · **Momento:** operación
- **Síntoma:** ante un corte, el host no se apagaba; `nut-monitor` fallaba y luego devolvía `ERR ACCESS-DENIED`.
- **Causa:** faltaba la línea `MONITOR` en `upsmon.conf`; y luego el usuario estaba declarado como `upsmon slave` en `upsd.users` cuando debía ser `master` para poder ordenar `shutdown.return`.
- **Resolución:** agregar `MONITOR sai@localhost 1 saimon saimon master` y cambiar el usuario a `upsmon master` en `upsd.users`; recargar NUT.
- **Intentos descartados:** reiniciar solo el driver (no aplica, el problema era de `upsmon`); asumir que el servicio disparaba el apagado del host (no: lo hace la cadena NUT, el servicio solo decide y ordena `shutdown.return`).
- **Destino:** [Runbook-Operacion](Runbook-Operacion-v1.0.md) `OPS-04`.

## EVE-02 — Sin `dotnet` en el entorno de generación

- **Ámbito:** solución · **Fecha:** 2026-07-22 · **Momento:** construcción
- **Síntoma:** no se podían compilar, testear ni generar migraciones porque no había SDK de .NET instalado fuera del Dev Container.
- **Causa:** el entorno de generación de artefactos no tiene `dotnet` en el PATH; el SDK vive dentro del contenedor (ADR-25).
- **Resolución:** correr build, test y `dotnet ef` en un contenedor SDK efímero (`mcr.microsoft.com/dotnet/sdk:10.0`) montando el repositorio, con `--startup-project` apuntando a Infrastructure (que referencia `EntityFrameworkCore.Design` y tiene la factory de diseño). Las primeras migraciones se escribieron a mano por la misma razón.
- **Intentos descartados:** `dotnet ef` con startup-project Web (falla: no referencia Design); correr `ef` como usuario no-root sobre archivos root del volumen (falla el `tar`); `dotnet build` omitido antes de `ef` (`NETSDK1004`, assets no encontrados).
- **Destino:** [Guia-Contribucion](Guia-Contribucion-v1.0.md) §2.

## EVE-03 — `CultureNotFoundException` en runtime

- **Ámbito:** SAI.Service.Core.Web · **Fecha:** 2026-07-23 · **Momento:** construcción
- **Síntoma:** el formateo relativo de fechas lanzaba `CultureNotFoundException` al iniciar.
- **Causa:** un inicializador estático usaba `CultureInfo.GetCultureInfo("es-AR")`, pero la solución corre con `InvariantGlobalization=true`: no hay culturas nombradas.
- **Resolución:** reemplazar por nombres de mes escritos a mano y `InvariantCulture`; se agregaron tests que cubren el formateo.
- **Intentos descartados:** desactivar `InvariantGlobalization` (rechazado: es una decisión de tamaño/arranque, no se revierte por un formateo); envolver en try/catch (oculta el problema en vez de resolverlo).
- **Destino:** [Guia-Contribucion](Guia-Contribucion-v1.0.md) §3 (convención globalization-invariant).

## EVE-04 — Formularios SSR devolvían HTTP 400 antiforgery

- **Ámbito:** SAI.Service.Core.Web · **Fecha:** 2026-07-21 · **Momento:** construcción
- **Síntoma:** el alta inicial y el cambio de contraseña devolvían 400 al enviar el formulario.
- **Causa:** doble token antiforgery (el de `EditForm` más uno agregado a mano) y el middleware antiforgery ejecutándose antes de la autenticación, con lo que el token no coincidía con el usuario.
- **Resolución:** emitir un único token (el de `EditForm`) y correr `UseAntiforgery()` **después** de `UseAuthentication()` en el pipeline.
- **Intentos descartados:** deshabilitar antiforgery en esos formularios (rechazado: baja la seguridad); regenerar el token por JS (innecesario una vez corregido el orden del middleware).
- **Destino:** absorbida en el orden del pipeline de `Web/Program.cs` (documentado en [Recorrido-Codigo](Recorrido-Codigo-v1.0.md) y en 05).

## EVE-05 — Sesiones perdidas al reiniciar el contenedor

- **Ámbito:** solución · **Fecha:** 2026-07-21 · **Momento:** despliegue
- **Síntoma:** cada reinicio deslogueaba a todos y rompía los formularios con 400.
- **Causa:** el keyring de DataProtection era efímero (en memoria por contenedor); al reiniciar, cambiaba la clave que cifra la cookie de sesión y los tokens antiforgery.
- **Resolución:** persistir el keyring en un volumen con nombre de aplicación estable (`SetApplicationName` + `PersistKeysToFileSystem`), configurable por `DataProtection:RutaLlaves` (ADR-29).
- **Intentos descartados:** alargar la vigencia de la cookie (no resuelve: el problema es la clave, no la expiración).
- **Destino:** [Runbook-Operacion](Runbook-Operacion-v1.0.md) `OPS-03` y [Guia-Contenedor](Guia-Contenedor-v1.0.md) §4.

## EVE-06 — Conflicto de merge al integrar ramas paralelas

- **Ámbito:** solución · **Fecha:** 2026-07-26 · **Momento:** construcción
- **Síntoma:** al mergear una rama que salía de antes de otra ya integrada, chocaban el `CHANGELOG.md`, el `SaiDbContext.cs` (dos DbSet nuevos en el mismo lugar) y el snapshot de migraciones.
- **Causa:** dos incrementos en ramas paralelas agregaron contenido en el mismo punto y una migración cada uno; el snapshot de EF quedó a reconciliar.
- **Resolución:** resolver conservando ambos bloques; **regenerar la migración del segundo increment sobre el modelo ya fusionado** para que su Designer y el `ModelSnapshot` queden consistentes (`has-pending-model-changes` sin cambios).
- **Intentos descartados:** editar el snapshot a mano (frágil); quedarse con el snapshot auto-fusionado por git sin regenerar (deja el Designer de la migración mintiendo sobre el modelo).
- **Destino:** [Guia-Contribucion](Guia-Contribucion-v1.0.md) §4 (orden de agregado de una funcionalidad con migración).

## EVE-07 — Jerga de NUT filtrándose a la interfaz del operador

- **Ámbito:** SAI.Service.Core · **Fecha:** 2026-07-23 · **Momento:** construcción
- **Síntoma:** un error de conexión mostraba al operador el detalle crudo de NUT (`shutdown.return`, claves de configuración).
- **Causa:** el adaptador propagaba la excepción técnica de NUT directamente a la UI.
- **Resolución:** patrón de dos audiencias: mensaje en lenguaje de operador a la UI y detalle técnico al log (vía `ILogger`/`NullLogger` con `LoggerMessage`); se agregó un test anti-jerga.
- **Intentos descartados:** mostrar el mensaje crudo con un prefijo amable (no alcanza: sigue siendo jerga).
- **Destino:** [Runbook-Operacion](Runbook-Operacion-v1.0.md) §2 (los detalles técnicos van al log, no a la UI).
