---
doc_id: DOC-SAI-inicio-rapido-01
doc_type: inicio-rapido
title: Guía de inicio rápido — SAI.Service.Core
status: Vigente
rol_intervencion: [mantenedor, operador]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-25
  - ADR-26
  - VER-01
---

# Guía de inicio rápido — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Mantenedor · Operador
**Nivel:** Básico
**Tiempo estimado de lectura:** 5 min

## Resumen ejecutivo

Levanta el servicio completo en una máquina limpia usando el Dev Container, sin instalar el SDK de .NET en el host. El único requisito del host es Docker. Al terminar, el panel responde en el navegador y `GET /health` devuelve estado `ok`. En desarrollo no hace falta hardware: el adaptador de conexión simulado reemplaza al SAI físico.

## 1. Resultado esperado

Al completar los pasos, `curl -sf http://localhost:8080/health` devuelve exit code `0` y un cuerpo JSON con `"estado":"ok"`, y el panel Blazor carga en `http://localhost:8080` desviando al alta del administrador en el primer arranque.

## 2. Prerrequisitos

| Herramienta | Versión mínima | Verificación |
| --- | --- | --- |
| Docker Engine | 24.0 | `docker --version` |
| CLI `devcontainer` | actual | `devcontainer --version` |
| Navegador | Chromium ≥120 / Firefox ≥121 | — |

El SDK de .NET 10 vive dentro del Dev Container (imagen `mcr.microsoft.com/devcontainers/dotnet:1-10.0`); no se instala en el host (ADR-25).

## 3. Arranque

Desde la raíz del repositorio, en un entorno limpio:

```bash
# 1. Levantar el Dev Container (restaura la solución y genera el certificado de desarrollo)
devcontainer up --workspace-folder .

# 2. Compilar la solución completa (cero warnings; los warnings son errores)
./scripts/build-all.sh

# 3. Correr el host (único proceso; web-monolith)
./scripts/run.sh SAI.Service.Core.Web
```

El paso 3 arranca el proceso, aplica las migraciones de EF Core a la base SQLite (`sai.db`) y siembra el rol de administrador, las reglas de derivación, la política de apagado inicial (en solo aviso) y la fuente de datos externa `fd-gmao-externo`. El adaptador de conexión arranca en modo `Simulado` por defecto (`Sai:Adaptador`).

## 4. Orden de arranque

No aplica un orden entre proyectos: la solución es un `web-monolith` de un solo proceso. `SAI.Service.Core.Web` es el único que arranca (`dotnet run`); las demás bibliotecas se cargan en ese proceso. La base SQLite se crea sola al aplicar las migraciones; no hay un servicio de base separado que levantar antes.

## 5. Verificación final

```bash
curl -sf http://localhost:8080/health
# Esperado (exit code 0):
# {"estado":"ok","servicio":"SAI.Service.Core","utc":"..."}
```

Y en el navegador, `http://localhost:8080` debe cargar y —en el primer arranque, sin administrador— desviar a `/alta-inicial`. El puerto HTTPS de desarrollo es `8443` (certificado autofirmado por `dotnet dev-certs https`). Ambos puertos están reenviados por el Dev Container.

## 6. Si falla

- **`/health` no responde o el puerto está ocupado.** Verificá que el paso 3 sigue corriendo en primera plana y que el 8080 no lo tome otro proceso (`ss -ltnp | grep 8080`). El puerto se declara en `appsettings.json` (`Kestrel:Endpoints:Http:Url`).
- **La compilación falla por un warning.** La política es cero-warnings (`TreatWarningsAsErrors=true`): el mensaje es un error real, no ruido. Corregilo; no hay flag para silenciarlo.
- **El panel carga pero pide credenciales que no tenés.** En el primer arranque no hay administrador: seguí el alta inicial en `/alta-inicial`. Si ya existe uno y perdiste la clave, ver el [Runbook](Runbook-Operacion-v1.0.md).

Para el resto de los síntomas, el [Runbook-Operacion](Runbook-Operacion-v1.0.md) tiene los incidentes conocidos (`OPS-XX`).
