---
doc_id: DOC-SAI-contenedor-01
doc_type: contenedor
title: Guía de contenedor — SAI.Service.Core
status: Vigente
rol_intervencion: [operador]
owner: Administrador único de SAI.Service.Core
version: 1.0
last_review: 2026-07-27
momento: 3
traces:
  - ADR-03
  - ADR-20
  - ADR-25
  - ADR-29
---

# Guía de contenedor — SAI.Service.Core

**Proyecto:** SAI.Service.Core
**Rol de intervención:** Operador
**Nivel:** Medio
**Tiempo estimado de lectura:** 8 min

## Resumen ejecutivo

Contrato de ejecución del servicio como contenedor: variables de entorno, puertos, volúmenes, el dispositivo USB del host y el healthcheck. El servicio es un `web-monolith` de un solo proceso (`SAI.Service.Core.Web`) que escribe su estado en SQLite y habla con NUT por TCP. **La imagen de producción todavía no está materializada** (no hay Dockerfile en el repositorio): esta guía fija el contrato que esa imagen debe cumplir cuando se construya, derivado de la configuración real del host y de los ADR de despliegue. Lo que hoy existe y corre es el Dev Container (ver [Guia-Inicio-Rapido](Guia-Inicio-Rapido-v1.0.md)).

## 1. Estado de la imagen

No hay `Dockerfile` ni `docker-compose.yml` en el repositorio; la contenedorización de producción es la ranura declarada en ADR-20 (TLS en la LAN) y ADR-25 (NUT en el contenedor). El `.devcontainer/devcontainer.json` provee el entorno de desarrollo sobre `mcr.microsoft.com/devcontainers/dotnet:1-10.0`, con Docker-in-Docker habilitado para construir esa imagen desde adentro cuando se aborde. Hasta entonces, el servicio se ejecuta con `./scripts/run.sh SAI.Service.Core.Web` dentro del Dev Container.

## 2. Variables de entorno

Override por variable de entorno con doble guion bajo `__` como separador de sección (convención estándar de ASP.NET Core; sin prefijo). Los secretos se inyectan por entorno, nunca en `appsettings.json`.

| Variable | Tipo | Default | Obligatoria | Efecto |
| --- | --- | --- | --- | --- |
| `ConnectionStrings__Sai` | string | `Data Source=sai.db` | No | Cadena SQLite; en contenedor apuntar al archivo dentro del volumen de datos |
| `Sai__Adaptador` | `Simulado`\|`Nut` | `Simulado` | En prod sí (`Nut`) | Elige el adaptador de conexión al SAI |
| `Sai__Nut__Host` | string | `127.0.0.1` | No | Host del `upsd` de NUT |
| `Sai__Nut__Puerto` | int | `3493` | No | Puerto del `upsd` |
| `Sai__Nut__Ups` | string | `sai` | No | Nombre del UPS en NUT |
| `Sai__Nut__TimeoutSegundos` | int | `5` | No | Timeout de las operaciones NUT |
| `Sai__Nut__Usuario` | string | `""` | Solo para apagar | Usuario NUT con permiso de escritura (`shutdown.return`) |
| `Sai__Nut__Password` | secreto | `""` | Solo para apagar | Clave del usuario NUT; vacío = solo lectura anónima |
| `Sai__Sondeo__IntervaloSeg` | int | `5` | No | Cadencia del sondeo |
| `Sai__Sondeo__Habilitado` | bool | `true` | No | Habilita/inhabilita el sondeo de fondo |
| `Sai__Apagado__ModalidadSolicitada` | enum | `SoloAlerta` | No | Semilla de la política inicial (la vigente manda) |
| `Sai__Apagado__TiempoReservadoSeg` | int | `120` | No | Semilla del tiempo reservado; se acota a 540 s (RN-04) |
| `Jwt__ClaveFirma` | secreto | `""` | **Sí en prod** | Clave HMAC de firma del token de la API; ≥ 32 bytes o el arranque falla |
| `Jwt__Emisor` | string | `sai-service-core` | No | Emisor del JWT |
| `Jwt__Audiencia` | string | `sai-service-core-api` | No | Audiencia del JWT |
| `DataProtection__RutaLlaves` | string | `""` | **Sí en prod** | Carpeta del keyring de DataProtection; vacío = efímero (solo dev) |
| `ASPNETCORE_ENVIRONMENT` | string | `Production` | No | `Development` habilita HTTPS :8443 y la clave JWT de desarrollo |
| `Kestrel__Endpoints__Http__Url` | string | `http://0.0.0.0:8080` | No | Endpoint HTTP |

## 3. Puertos expuestos

| Puerto | Protocolo | Propósito | Publicar |
| --- | --- | --- | --- |
| 8080 | HTTP | Panel Blazor + API REST | Sí (o detrás de proxy TLS) |
| 8443 | HTTPS | Panel + API con certificado | Solo en Development; en prod, TLS es ranura de ADR-20 |

En producción, el TLS autofirmado se resuelve montando el certificado y declarando `Certificates:Default:{Path,Password}` (o `KeyPath`), según la ranura de `appsettings.json`; hoy no está implementado.

## 4. Volúmenes

| Ruta interna | Propósito | Si no se monta |
| --- | --- | --- |
| Carpeta de `DataProtection__RutaLlaves` (p. ej. `/keys`) | Keyring que cifra la cookie de sesión y los tokens antiforgery (ADR-29) | El keyring es efímero: reiniciar el contenedor invalida sesiones y rompe los formularios antiforgery |
| Carpeta de la base SQLite (p. ej. `/data`, con `ConnectionStrings__Sai=Data Source=/data/sai.db`) | Historia append-only, inventario, verificaciones | Se pierde toda la persistencia al recrear el contenedor |

Política de permisos: ambas carpetas deben ser escribibles por el usuario del proceso; el keyring además debe ser privado (solo el propietario).

## 5. Dispositivo del host requerido

El servicio no toma el USB directamente: lo hace el driver de NUT, que debe correr con acceso al nodo USB del SAI (ADR-03/ADR-25). Cuando NUT corre en el mismo contenedor, el nodo se pasa por ruta física del puerto —no por número de serie, que puede reaparecer con otra ruta tras una reconexión (R-06)—, con una regla `udev` en el host que fija un symlink estable y el dispositivo compartido al contenedor. **Este anclaje es configuración de despliegue y no vive en el repositorio**; se documenta en la [Guia-Despliegue](Guia-Despliegue-v1.0.md) y en el `Installer-Guide` del entorno real. Verificación de disponibilidad: `upsc sai@localhost ups.status` devuelve un estado (`OL`, `OB`, …) sin error de conexión.

## 6. Healthcheck

| Campo | Valor |
| --- | --- |
| Endpoint | `GET /health` (anónimo) |
| Respuesta esperada | exit code `0` y cuerpo `{"estado":"ok","servicio":"SAI.Service.Core","utc":...}` |
| Comando | `curl -sf http://localhost:8080/health` |
| Período sugerido | 30 s |
| Umbral de reintentos | 3 fallos consecutivos → contenedor no saludable |

## 7. Dependencias de arranque

El servicio arranca sin dependencias externas obligatorias: crea y migra su propia base SQLite al iniciar. NUT (`upsd`) solo es necesario cuando `Sai__Adaptador=Nut`; si no está disponible, la lectura de estado queda como «no alcanzable» y el servicio sigue en pie sirviendo el panel. Con `Sai__Adaptador=Simulado` no hay dependencia externa alguna.

## 8. Límites de recursos sugeridos

Estimados para un host doméstico/de laboratorio con un solo SAI y sondeo a 5 s: 256–512 MB de memoria y 0,5 vCPU alcanzan holgadamente; el costo dominante es la escritura periódica de muestras a SQLite. No es un servicio de alta concurrencia: lo consume un administrador único y un GMAO externo esporádico. Ajustar al alza solo si se baja el intervalo de sondeo o crece mucho la retención de muestras.

## 9. Contrato para agente

```yaml
entradas:
  env_obligatorias_prod: [Sai__Adaptador=Nut, Jwt__ClaveFirma, DataProtection__RutaLlaves]
  volumenes: [keyring, sqlite]
  dispositivo: "nodo USB del SAI, vía NUT y regla udev del host"
salidas:
  puerto_http: 8080
  health: "GET /health -> 200 {estado: ok}"
validaciones:
  - "curl -sf http://localhost:8080/health tiene exit code 0"
  - "Jwt__ClaveFirma >= 32 bytes, o el proceso no arranca"
```
