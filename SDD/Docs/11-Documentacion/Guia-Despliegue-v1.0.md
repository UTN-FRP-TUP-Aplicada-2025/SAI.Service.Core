---
doc_id: DOC-SAI-despliegue-01
doc_type: despliegue
title: Guía de despliegue — SAI.Service.Core
status: Vigente
rol_intervencion: [operador]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-24
  - ADR-25
  - ADR-26
  - ADR-29
---

# Guía de despliegue — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Operador
**Nivel:** Medio
**Tiempo estimado de lectura:** 8 min

## Resumen ejecutivo

Procedimiento para desplegar el servicio en el host de producción `i7infra` y su verificación. El modelo de ambientes es de dos niveles, **DEV y PROD, sin staging** (ADR-24): DEV es el Dev Container en la máquina del desarrollador; PROD es el servicio corriendo junto a NUT en el host que protege. La imagen de contenedor de producción es todavía una ranura pendiente (no hay Dockerfile); mientras tanto, el despliegue de referencia corre el host publicado sobre el mismo entorno del Dev Container. Esta guía fija el procedimiento y sus verificaciones; el contrato de ejecución (env, volúmenes, USB) vive en [Guia-Contenedor](Guia-Contenedor-v1.0.md).

## 1. Topologías

| Topología | Cuándo | Estado |
| --- | --- | --- |
| Proceso publicado en el host, junto a NUT nativo | Hoy: host doméstico/lab con NUT ya instalado (`upsmon`+`upssched`) | Soportada |
| Contenedor con NUT adentro, USB por `udev` | Objetivo de ADR-25 cuando se materialice la imagen | Pendiente (sin Dockerfile) |

No hay ambiente de staging: no habría a qué SAI conectarlo (ADR-24). La validación end-to-end del apagado real se hace en la ventana de mantenimiento (CU-10, UF-8) sobre el host real.

## 2. Prerrequisitos del entorno destino

- Host Linux con el SAI conectado por USB y **NUT ya operativo** (`upsd`, `upsmon`, `upssched`), verificable con `upsc sai@localhost ups.status`.
- .NET 10 disponible (dentro del Dev Container o instalado en el host de PROD).
- Acceso de escritura a dos carpetas persistentes: la del keyring de DataProtection y la de la base SQLite.
- Usuario NUT con permiso de escritura (`shutdown.return`) para habilitar el apagado real; sin él, el servicio queda en solo lectura.

## 3. Configuración por ambiente

| Variable | DEV | PROD |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `Sai__Adaptador` | `Simulado` | `Nut` |
| `Sai__Nut__Usuario` / `__Password` | — | usuario NUT de escritura (secreto) |
| `Jwt__ClaveFirma` | clave de desarrollo (en `appsettings.Development.json`) | secreto ≥ 32 bytes, inyectado por entorno |
| `DataProtection__RutaLlaves` | vacío (efímero) | carpeta persistente montada (p. ej. `/keys`) |
| `ConnectionStrings__Sai` | `Data Source=sai.db` | `Data Source=/data/sai.db` (en volumen) |
| TLS | HTTPS :8443 con `dev-certs` | certificado autofirmado en la LAN (ranura ADR-20) |

Ningún secreto va en `appsettings.json`. En PROD, `Jwt__ClaveFirma` y `Sai__Nut__Password` se inyectan por variable de entorno o gestor de secretos del host.

## 4. Procedimiento

```bash
# 1. Traer la versión a desplegar y compilar en Release (cero warnings)
git fetch origin && git checkout <tag-o-main>
./scripts/build-all.sh

# 2. Publicar el host (o construir la imagen cuando exista el Dockerfile)
dotnet publish src/SAI.Service.Core/SAI.Service.Core.Web \
  --configuration Release -o ./.publish

# 3. Exportar la configuración de PROD (secretos por entorno, no en archivos)
export ASPNETCORE_ENVIRONMENT=Production Sai__Adaptador=Nut \
  Jwt__ClaveFirma="<secreto>" DataProtection__RutaLlaves=/keys \
  ConnectionStrings__Sai="Data Source=/data/sai.db" \
  Sai__Nut__Usuario="<usuario>" Sai__Nut__Password="<secreto>"

# 4. Arrancar el proceso (aplica migraciones y siembra al iniciar)
dotnet ./.publish/SAI.Service.Core.Web.dll
```

Al arrancar, el servicio aplica las migraciones de EF Core a la base SQLite y siembra de forma idempotente el rol de administrador, las reglas de derivación, la política de apagado inicial (en solo aviso) y la fuente `fd-gmao-externo`. La primera vez, el panel desvía a `/alta-inicial` para crear el administrador único.

## 5. Verificación post-despliegue

```bash
curl -sf http://localhost:8080/health         # exit 0, {"estado":"ok",...}
upsc sai@localhost ups.status                 # OL (en línea) sin error de conexión
```

Además, en el panel: el estado en vivo debe mostrar el SAI **en línea** con tensión y carga reales (no los valores fijos del simulado), y el panel de verificaciones debe reflejar los cuatro supuestos en «no verificado» hasta correr la ventana de mantenimiento. Mientras los cuatro no estén verificados, la modalidad efectiva es solo aviso: el sistema no apagará el host (comportamiento correcto, RN-01/RN-02).

## 6. Rollback

```bash
# Detener el proceso, volver a la versión anterior y re-arrancar
git checkout <tag-anterior> && ./scripts/build-all.sh
dotnet publish ... && dotnet ./.publish/SAI.Service.Core.Web.dll
```

**Punto de no retorno: las migraciones de base.** EF Core aplica migraciones hacia adelante al arrancar; una versión anterior del código puede no entender un esquema ya migrado. Antes de desplegar una versión con migración nueva, respaldar el archivo `sai.db` (copia del archivo con el servicio detenido). El rollback de código sin rollback de esquema solo es seguro si la migración fue aditiva (el caso habitual en este proyecto, que es append-only). Rollback completado cuando `GET /health` responde `ok` y el panel muestra el estado en vivo con la versión anterior en el sello.
