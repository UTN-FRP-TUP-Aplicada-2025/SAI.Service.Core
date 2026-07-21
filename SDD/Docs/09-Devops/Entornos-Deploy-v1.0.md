# Entornos de deploy — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Entornos-Deploy-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

---

## 1. Lista de ambientes

El proyecto es `web-monolith`. El **piso** de la regla constructiva §2.2 para este tipo es DEV / QA / STAGING / PROD. Este proyecto declara **solo dos ambientes: DEV y PROD**. Es una desviación deliberada del piso, y se documenta como tal en §1.1.

| Ambiente (`ENV-XX`) | Propósito | Destino | Aprobador | SLA / ventana |
| --- | --- | --- | --- | --- |
| DEV | Desarrollo y depuración interactiva | Dev Container en la máquina del desarrollador | Auto (el propio desarrollador) | — |
| PROD | Servicio en operación real | Contenedor en el host `i7infra` (`linux/amd64`) | Administrador único (release manager) | Acotado por la disponibilidad del host que protege; SLO propio N-25 **PENDIENTE** |

**DEV** es el Dev Container (spec containers.dev), levantado con «Reopen in Container» o `devcontainer up`. El SDK de .NET 10 vive dentro del contenedor; el único requisito del host es Docker (intake §17.P.1). La depuración va por `.vscode/launch.json` con F5 (coreclr), **nunca** por los scripts de build/run, que son para construir y correr, no para depurar (intake §17.P.8, §16).

**PROD** es el contenedor de producción (imagen runtime-only, distinta de la del Dev Container) corriendo en `i7infra`, con el dispositivo USB del SAI compartido por **ruta física de puerto** vía regla `udev` (ADR-03). Anclar por puerto —y no por serial, que el equipo no expone— significa "el SAI que esté enchufado ahí", de modo que un reemplazo de equipo no rompe el binding (intake §17.P.11 PA-03).

### 1.1 Desviación del piso DEV/QA/STAGING/PROD y su justificación

La regla §2.2 fija DEV/QA/STAGING/PROD como piso para `web-monolith` y advierte: *"El equipo puede agregar ambientes intermedios cuando el dominio lo exija, pero no quitar ninguno sin un ADR que lo justifique."*

Este proyecto **quita QA y STAGING**. La justificación proviene del intake §17.P.8, textual:

> *"No hay ambiente de staging: no habría a qué SAI conectarlo."*

El razonamiento es de dominio, no de conveniencia:

- El valor del servicio está atado a **un** SAI físico conectado a **un** host (`i7infra`) por USB. Un ambiente de STAGING sin ese SAI no ejercita el camino crítico —el apagado y el reencendido—, que es justamente lo que un STAGING existiría para validar. Sin el equipo real, STAGING probaría contra el adaptador simulado, que es exactamente lo que ya hacen los tests e2e de STAGE-05 en el pipeline: no agregaría cobertura.
- Un ambiente de QA separado tampoco tiene operador: el administrador único es a la vez propietario, implementador, QA y beneficiario (intake §2, §10). La función de QA la cumplen los diez gates del pipeline (que ejecutan la DoD de 08) más la validación visual del administrador al cierre de cada etapa (intake §15).
- La verificación del comportamiento real del firmware y la BIOS no se hace en un ambiente de software: se hace en la **ventana de mantenimiento física** UF-8 / CU-10, con presencia física y prueba destructiva sobre el equipo, y su resultado se registra como evidencia de una `Verificacion` (intake §17.P.6, DoD §2). Ningún STAGING la sustituye.

**Estado de la desviación.** Quitar ambientes del piso requiere un ADR que lo justifique (regla §2.2). La justificación existe y está en el intake §17.P.8; queda **registrada como [ADR-24](../05-Arquitectura-Tecnica/Adrs/ADR-24-Modelo-De-Ambientes-Dev-Prod-Sin-Staging-v1.0.md)** (Aceptado) en `05-Arquitectura-Tecnica/Adrs/`, con el contenido de esta sección como fundamento y respaldo trazable al intake §17.P.8.

---

## 2. Provisión (IaC)

La superficie de infraestructura es mínima: un host, un contenedor, un archivo SQLite, un dispositivo USB. No se adopta una herramienta de IaC pesada (Terraform, Pulumi, Bicep) porque no hay flota de recursos en la nube que provisionar; sería infraestructura sin contrapartida, coherente con el trade-off T-01 del intake (simplicidad operativa como valor explícito).

La provisión de PROD se declara y versiona en el repositorio como **configuración declarativa liviana**:

| Artefacto | Qué provisiona | Ubicación |
| --- | --- | --- |
| `Dockerfile` (runtime-only) | La imagen de producción | `src/SAI.Service.Core.Web/Dockerfile` |
| Regla `udev` | Anclaje del USB por ruta física de puerto (ADR-03) | Documentada; se instala en el host `i7infra` (`/etc/udev/rules.d/`) |
| Archivo de entorno de PROD | Variables de configuración y secretos por env vars | `/etc/sai-service-core/prod.env` en el host (fuera del repositorio) |
| Comando de arranque del contenedor | `docker run` con `--env-file`, mapeo del dispositivo y volumen del `.db` | `Guia-Publicacion-Image-Docker-v1.0.md §2` |

No hay flujo `plan`/`apply` porque no hay estado remoto de IaC. La decisión abierta de Sprint 0 sobre **si NUT corre dentro del contenedor o en el host** (ADR-19, riesgos R-05/R-08) afecta la provisión: en la variante "NUT en el host", el contenedor no necesita el dispositivo y el servicio es cliente TCP de `upsd`; en la variante "NUT en el contenedor", el contenedor recibe el dispositivo USB. La provisión final se cierra con ese ADR.

---

## 3. Configuración por ambiente (12-factor)

Configuración por **variables de entorno** (12-factor, intake §17.P.5): nunca en código, nunca en el repositorio. La configuración de desarrollo usa el *user-secrets* del SDK dentro del Dev Container.

Mapa de variables por ambiente (nombres ilustrativos; se cierran al implementar la infraestructura):

| Variable | DEV | PROD | Notas |
| --- | --- | --- | --- |
| `ConnectionStrings__Sqlite` | Ruta a un `.db` local del contenedor de dev | Ruta al `.db` en el volumen persistente del host | Secreto de bajo riesgo, pero fuera del repo igual |
| `Sondeo__IntervaloSegundos` | 5 | 5 | N-07; configurable por entorno |
| `Nut__Modo` | `simulado` o `upsd` | `upsd` o `upsc` según ADR-19 | Selecciona la implementación del adaptador de conexión |
| `Nut__Host` / `Nut__Puerto` | — (adaptador simulado) | Host y puerto de `upsd` si NUT corre en el host | Solo en la variante "NUT en el host" |
| `Upsd__Usuario` / `Upsd__Password` | — | Credenciales de acceso al equipo vía `upsd` | **Secreto** (ver §4) |
| `Docker__ShutdownTimeout` | — | 150 s | N-02; presupuesto del `shutdown-timeout` del demonio |
| `Ups__DelayStart` | — | 180 s | N-04; validado contra la variable leída del equipo |
| `AspNetCore__Environment` | `Development` | `Production` | Habilita/inhabilita el detalle de errores |

Los NFR de configuración (N-02, N-04, N-07) se validan al arranque de PROD: el servicio loguea los valores efectivos y el operador los coteja. N-04 (`ups.delay.start` = 180 s) además se contrasta con la variable real leída del equipo por el adaptador.

---

## 4. Secretos

Los secretos viven en variables de entorno del contenedor (12-factor), nunca en el repositorio. El scope es de un solo host y un solo operador.

| Secreto | Dónde vive en DEV | Dónde vive en PROD | Rotación |
| --- | --- | --- | --- |
| Cadena de conexión a SQLite | *user-secrets* del SDK dentro del Dev Container | `--env-file /etc/sai-service-core/prod.env` (permisos `600`, dueño root) | Baja sensibilidad (archivo local del host); se rota si se recrea el host |
| Credenciales de acceso al equipo (`upsd`) | No aplican (adaptador simulado) | `--env-file` en el host | Se rotan al cambiar la config de NUT; scope local, sin exposición a internet |
| Contraseña del administrador único | No es un secreto de deploy: se fija en el alta inicial y se guarda **hasheada** (ASP.NET Core Identity, PBKDF2) en el `.db` | Idem | La cambia el propio administrador desde la barra superior (intake §17.P.5, etapa 4) |

Prohibición explícita: ningún secreto se commitea. No hay secretos de CI/CD de publicación externa, porque no hay publicación a registros externos ni credenciales de publicación que gestionar en el alcance actual (intake §17.P.5); la firma cosign keyless usa el OIDC efímero de GitHub Actions, sin clave de larga vida almacenada (ver `Supply-Chain-Seguridad §2`).

No se adopta un vault (HashiCorp Vault, Key Vault, Secrets Manager): sería infraestructura sin contrapartida para un único host sin exposición a internet. La superficie de secretos es un archivo `.env` con permisos restrictivos en un host que el propio operador administra.

---

## 5. Promoción DEV → PROD

Procedimiento de promoción, integrado con el pipeline (`Pipeline-CI-CD §4`):

1. El desarrollador cierra una etapa en su rama `etapa/NN-<slug>` y valida el criterio de cierre en el navegador (intake §15).
2. Merge del PR a `main` con los diez gates en verde.
3. Creación del tag SemVer `v<X.Y.Z>` sobre `main` (el bump lo define el Conventional Commit de cierre; ver `Estrategia-Versionado §2`).
4. El trigger de tag corre STAGE-01 a STAGE-10: build, gates, imagen, **firma cosign** y **push al registro privado**.
5. En el host `i7infra`, el operador ejecuta el deploy de la nueva etiqueta (`Guia-Publicacion-Image-Docker §2`), verificando la firma antes de arrancar.
6. Verificación post-deploy por **efecto observado** (ADR-11): el endpoint de salud responde y las rondas de sondeo se reanudan.

Aprobador requerido: el administrador único, en su rol de release manager. **Registro de auditoría**: el tag de git firmado, la entrada en el transparency log de cosign (Rekor) y el log local estructurado del contenedor con cada acción y su resultado observado (intake §17.P.10). No hay promoción automática a PROD: el gate humano es la decisión deliberada de etiquetar.

---

## 6. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Modelo de ambientes DEV (Dev Container, F5) y PROD (contenedor en `i7infra`, USB por `udev`/ADR-03). Documenta la desviación del piso DEV/QA/STAGING/PROD (sin QA ni STAGING) con la justificación del intake §17.P.8, registrada como ADR-24. Configuración 12-factor por env vars, secretos (SQLite, credenciales de `upsd`) por env vars y user-secrets en dev, promoción DEV→PROD con aprobador y registro de auditoría. |
