# Pipeline CI/CD — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Pipeline-CI-CD-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

---

## 0. Alcance y posición

Este documento define el pipeline de integración y entrega continua del único proyecto de la solución, `Sai-Service-Core` (`web-monolith`, `redistribuible: false`). La solución es de un solo proyecto (caso degenerado del intake §13), así que **no existe pipeline de nivel solución**: el orden de build es trivial (un nodo, sin aristas) y toda la publicación se resuelve acá.

El pipeline **no redefine** la Definition of Done de 08 ni los quality gates de la Estrategia de Calidad: los **ejecuta como gates**. Los diez stages de este documento son la materialización downstream de los diez gates declarados en `08-Calidad-Y-Pruebas/Estrategia-Calidad-v1.0.md §3` y del pipeline del intake §17.P.8. Cuando este documento dice "criterio de éxito", ese criterio es el gate de 08 o el NFR de 05, referenciado, nunca reformulado.

Plataforma: **GitHub Actions**. Elegida en el intake §17.P.8 por no requerir infraestructura propia sobre un repositorio git y por su soporte nativo de contenedores Linux, que es el único target.

---

## 1. Stages obligatorios

Los diez stages son los del intake §17.P.8 y los diez gates de `Estrategia-Calidad §3`. Cada stage declara su comando, su tooling y su criterio de éxito (el gate de 08 o el NFR de 05 que verifica). Todos son bloqueantes para mergear a `main`, salvo indicación de dependencia de tag.

| # | Stage (`STAGE-XX`) | Comando | Tooling | Criterio de éxito (gate de 08 / NFR de 05) | Bloqueante |
| --- | --- | --- | --- | --- | --- |
| 1 | STAGE-01 Build | `dotnet build -c Release` sobre .NET 10 | SDK .NET 10; `TreatWarningsAsErrors=true` en `Directory.Build.props` | Cero errores **y cero warnings** (gate 1; DoD US/BT "compila en Release con cero warnings") | Sí |
| 2 | STAGE-02 Lint / formato | `dotnet format --verify-no-changes` y build con analizadores | `dotnet format` + analizadores .NET nivel `recommended` | Sin diferencias de formato; cero diagnósticos de severidad *error* (gate 2; DoD "análisis estático sin diagnósticos error y sin diferencias de formato") | Sí |
| 3 | STAGE-03 Test unitario | `dotnet test tests/SAI.Service.Core.Domain.Tests tests/SAI.Service.Core.Application.Tests --collect:"XPlat Code Coverage"` | xUnit + FluentAssertions; recolector de cobertura de .NET (coverlet) | 100 % de pruebas verdes; **cobertura ≥ 90 % líneas / 85 % ramas en `SAI.Service.Core.Domain`** (gate 3; N-22) | Sí |
| 4 | STAGE-04 Test de integración | `dotnet test tests/SAI.Service.Core.Integration.Tests --collect:"XPlat Code Coverage"` | xUnit + `WebApplicationFactory` + SQLite físico + adaptador simulado | 100 % verdes; **cobertura global ≥ 80 % líneas / 70 % ramas** (gate 4; N-21) | Sí |
| 5 | STAGE-05 Test end-to-end | `dotnet test` (bUnit) y ejecución de la suite Playwright sobre los flujos críticos | bUnit para componentes Blazor; Playwright para recorridos de panel | 100 % verdes en UF-1, UF-3 y el camino de apagado de UF-8 contra el adaptador simulado (gate 5) | Sí |
| 6 | STAGE-06 SCA | `dotnet list package --vulnerable --include-transitive` | dotnet CLI; base de avisos de NuGet/GitHub | Cero vulnerabilidades de severidad **alta o crítica** (gate 6; DoD BT "no introduce dependencia con vulnerabilidad alta/crítica") | Sí |
| 7 | STAGE-07 SBOM | Generación de SBOM CycloneDX y publicación como artefacto del build | Generador CycloneDX para .NET (`CycloneDX` dotnet tool) | El artefacto SBOM existe, es válido y queda adjunto al build (gate 7; DoD release "SBOM válido adjunto") | Sí |
| 8 | STAGE-08 Build de imagen | `docker build` de la imagen de producción (runtime-only) + smoke test | Docker; imagen runtime-only distinta de la del Dev Container | La imagen construye y su smoke test arranca el servicio y responde el endpoint de salud (gate 8; DoD release; N-02) | Sí |
| 9 | STAGE-09 Firma | `cosign sign` en modo keyless sobre la imagen del tag | cosign (sigstore), OIDC de GitHub Actions, transparency log Rekor | Firma keyless válida y registrada; **solo en builds de tag**; se verifica antes de publicar (gate 9) | Sí en tag |
| 10 | STAGE-10 Publicación | `docker push` de la imagen etiquetada al registro privado | Docker; registro privado (sin feed público) | Push solo desde `main` con tag SemVer y solo si STAGE-01..09 pasaron (gate 10) | Sí en tag |

Notas de trazabilidad:

- Los umbrales de cobertura de STAGE-03 y STAGE-04 se reportan y evalúan **por capa**, nunca como un único número global. El 80/70 es el piso del conjunto (N-21); el dominio tiene su piso separado y más alto, 90/85 (N-22). Es el anti-patrón de cobertura global único, explícitamente evitado por 08.
- STAGE-05 cubre el camino de apagado solo contra el **adaptador simulado**: el ciclo físico de apagado y reencendido (F-3) no es automatizable y se verifica en la ventana de mantenimiento UF-8 / CU-10, cuya evidencia se registra como `Verificacion`, no como test (intake §17.P.6, DoD §2 "verificación física operativa").
- El endpoint de salud del smoke test (STAGE-08) es el mismo endpoint anónimo declarado en el intake §17.P.5 (accesible sin autenticación).

### 1.1 Triggers por evento

Se declaran triggers explícitos por evento; se evita el trigger único y opaco sobre `push`.

| Evento | Stages que corren | Propósito |
| --- | --- | --- |
| `pull_request` hacia `main` | STAGE-01 a STAGE-08 | Gates de merge; ninguna firma ni publicación |
| `push` a `main` | STAGE-01 a STAGE-08 | Validación de la integración ya mergeada |
| `push` de tag `v<X.Y.Z>` (SemVer) | STAGE-01 a STAGE-10 | Release: agrega firma (STAGE-09) y publicación (STAGE-10) |
| `schedule` semanal | STAGE-06 | Reevaluación de SCA sobre dependencias sin cambios de código (detecta CVE nuevas) |

La firma (STAGE-09) y la publicación (STAGE-10) corren **solo en el trigger de tag**. Un `push` a `main` sin tag valida pero no publica.

---

## 2. Matriz de SO y runtime

Un solo eje, porque el target es único.

| Trigger | Sistema operativo del runner | Runtime | Plataforma de imagen |
| --- | --- | --- | --- |
| Todos | `ubuntu-latest` | .NET 10.0 | `linux/amd64` |

**Justificación (intake §17.P.8, §17.P.9, T-09):** el proyecto es exclusivamente Linux por decisión de alcance. Administra un SAI conectado a un host Linux concreto (`i7infra`, `linux/amd64`); la portabilidad a Windows, macOS o `linux/arm64` no tendría consumidor. Una matriz cruzada gastaría minutos de CI sin cobertura de ningún consumidor real. No se prueba en Windows ni en macOS.

---

## 3. Caché y artefactos

### 3.1 Caché de NuGet

Se cachea el store global de paquetes NuGet para no re-descargar dependencias en cada corrida.

| Parámetro | Valor |
| --- | --- |
| Directorio cacheado | `~/.nuget/packages` |
| Llave de caché | `nuget-${{ runner.os }}-${{ hashFiles('**/packages.lock.json', '**/*.csproj') }}` |
| Llave de respaldo | `nuget-${{ runner.os }}-` |
| Habilitación | `actions/setup-dotnet` con `cache: true`, o `actions/cache` sobre el directorio |
| Invalidación | Cambio de cualquier `*.csproj` o del lockfile de paquetes |

Se recomienda fijar dependencias con `packages.lock.json` (`RestorePackagesWithLockFile=true`) para que la llave de caché sea determinista y el restore sea reproducible.

### 3.2 Artefactos producidos y retención

| Artefacto | Stage productor | Retención |
| --- | --- | --- |
| Reporte de cobertura (Cobertura XML + resumen) | STAGE-03, STAGE-04 | 90 días |
| Reporte de resultados de tests (TRX / JUnit) | STAGE-03, STAGE-04, STAGE-05 | 90 días |
| Reporte de SCA (`--vulnerable` en texto/JSON) | STAGE-06 | 90 días |
| SBOM CycloneDX (JSON) | STAGE-07 | Con el release, indefinido (adjunto al tag) |
| Imagen de producción (`linux/amd64`) | STAGE-08 | En el registro privado según política de retención del registro |
| Firma cosign + entrada en transparency log | STAGE-09 | Indefinido (Rekor es append-only) |

---

## 4. Promotion rules entre ambientes

El modelo de ambientes es **DEV → PROD**, sin QA ni STAGING. La desviación respecto del piso DEV/QA/STAGING/PROD de la regla §2.2 y su justificación están en `Entornos-Deploy-v1.0.md §1` (intake §17.P.8: *"no habría a qué SAI conectarlo"*).

| Transición | Trigger | Prerequisitos | Aprobador |
| --- | --- | --- | --- |
| Código → DEV | `Reopen in Container` / `devcontainer up` en la máquina del desarrollador | Ninguno (entorno de trabajo) | Auto (el propio desarrollador) |
| DEV → PROD | `push` de tag `v<X.Y.Z>` SemVer sobre `main` | STAGE-01 a STAGE-09 en verde; imagen firmada; firma verificada | Administrador único (rol combinado propietario / release manager) |

No hay promoción automática a PROD sin la creación deliberada de un tag: el gate humano es la decisión de etiquetar. Con un solo desarrollador que es a la vez propietario y operador, el aprobador de PROD y el autor del tag son la misma persona; el registro auditable es el propio tag de git firmado más la entrada en el transparency log de cosign.

---

## 5. Rollback

Procedimiento por tipo de artefacto. El artefacto publicado es **image-docker** (§2.2 web-monolith). El detalle operativo de publicación y rollback de la imagen vive en `Guia-Publicacion-Image-Docker-v1.0.md §4`; acá se declara el procedimiento canónico.

Comando concreto (volver a la etiqueta de imagen anterior y reiniciar el contenedor en `i7infra`):

```bash
# 1. Identificar la etiqueta estable previa
docker image ls registro-privado/sai-service-core --format '{{.Tag}}'

# 2. Detener el contenedor en curso (respeta el shutdown-timeout de 150 s — N-02)
docker stop --time 150 sai-service-core

# 3. Reiniciar con la etiqueta anterior conocida buena
docker run -d --name sai-service-core \
  --env-file /etc/sai-service-core/prod.env \
  --device-cgroup-rule='c 189:* rmw' \
  registro-privado/sai-service-core:v<X.Y.Z-1>

# 4. Verificar por efecto observado: el endpoint de salud responde
curl -fsS http://localhost:8080/health
```

Garantías que hacen seguro el rollback (intake §17.P.8):

- El estado crítico —la **historia**— es append-only (ADR-04): un rollback de versión no pierde hechos registrados. La DoD de release lo exige como criterio ("el histórico sobrevive a un rollback sin perder hechos").
- Las migraciones de EF Core se diseñan **aditivas** siempre que sea posible. Una migración destructiva exige respaldo previo del archivo SQLite, que es una copia de un único archivo (`cp sai.db sai.db.bak-<fecha>`).
- El binding del USB por ruta física de puerto (ADR-03) no se rompe al reiniciar el contenedor: la regla `udev` ancla el nodo por puerto, no por serial.

---

## 6. Notificaciones

Contexto de un solo desarrollador/operador; las notificaciones son mínimas y locales, sin canales corporativos.

| Evento | Canal | Severidad |
| --- | --- | --- |
| Falla de cualquier stage en PR o `main` | Estado del check de GitHub Actions en el PR; correo de GitHub al autor | Bloqueante |
| Falla de STAGE-06 en el `schedule` semanal (CVE nueva) | Issue automático en el repositorio con la CVE y la dependencia | Alta |
| Falla de firma o publicación en tag (STAGE-09/10) | Estado del workflow del tag; el release no se marca publicado | Bloqueante |
| Smoke test de PROD post-deploy | Log local estructurado del contenedor + verificación manual del endpoint de salud | Informativo |

Dashboard visible: la pestaña Actions del repositorio hace de tablero de estado de gates. No se declara integración con Slack, PagerDuty ni similares: no hay equipo que escalar (un solo operador).

---

## 7. Reproducibilidad local

Todo stage es reproducible en la máquina del desarrollador con las mismas versiones, dentro del Dev Container (intake §17.P.1). El README raíz de la solución (Fase H, ADR-23) referencia la reproducción local; acá se listan los comandos como referencia:

| Stage | Comando local equivalente |
| --- | --- |
| STAGE-01 | `./scripts/build-all.sh` (envuelve `dotnet build -c Release`) |
| STAGE-02 | `dotnet format --verify-no-changes` |
| STAGE-03 | `dotnet test tests/SAI.Service.Core.Domain.Tests` |
| STAGE-04 | `dotnet test tests/SAI.Service.Core.Integration.Tests` |
| STAGE-06 | `dotnet list package --vulnerable --include-transitive` |
| STAGE-07 | `dotnet CycloneDX SAI.Service.Core.sln -o ./sbom` |
| STAGE-08 | `docker build -f src/SAI.Service.Core.Web/Dockerfile -t sai-service-core:local .` |

El pipeline no depende de comportamiento que solo exista en el runner: los mismos comandos corren en local con el mismo SDK del Dev Container.

---

## 8. Trazabilidad NFR numérico → gate

Cada NFR con objetivo numérico de `05-Arquitectura-Tecnica/Arquitectura-Solucion-v1.0.md §8` tiene un stage o gate que lo verifica antes de promover. Los tres NFR **PENDIENTE** (N-03, N-20, N-25) no tienen dato numérico en la fuente y se referencian como tales: no se inventa un gate para ellos, se declara su vía de cierre.

| NFR | Objetivo | Gate / stage que lo verifica |
| --- | --- | --- |
| N-01 | `ups.delay.shutdown` ≤ 540 s | STAGE-03 (test de invariante I-10 en `Domain`) |
| N-02 | Presupuesto shutdown-timeout Docker = 150 s | STAGE-08 (config del contenedor verificada en el smoke test; `docker stop --time 150`) |
| N-03 | Resto del apagado del SO | **PENDIENTE de medición** — no es un gate de pipeline; se cronometra en UF-8 / CU-10 y lo verifica `ver-presupuesto-apagado` (evidencia, no test) |
| N-04 | `ups.delay.start` = 180 s | Config validada en el arranque + lectura de la variable del equipo (verificación de configuración de PROD, `Entornos-Deploy §3`) |
| N-05 | Umbral de disparo = 300 s de partida | STAGE-03 (test de la lógica de temporizador sobre `VersionPolitica`) |
| N-06 | Latencia de decisión < 1 s por ronda | STAGE-04 (aserción sobre la duración de ronda logueada por el hosted service) |
| N-07 | Intervalo de sondeo = 5 s | STAGE-04 (test de cadencia; config por entorno) |
| N-08 | Cadencia de prueba = 1 Hz | STAGE-03 / STAGE-04 (densidad de muestreo registrada en `PruebaBateria`) |
| N-09 | 3 sondeos fallidos ⇒ `DesconexionUsb` | STAGE-04 (test de integración del contador de sondeos fallidos) |
| N-10 | `input.voltage` en [198, 242] V | STAGE-03 (test de la `ReglaDerivacion` versionada) |
| N-11 | Umbral de microcorte < 60 s (regla v2) | STAGE-03 (test de derivación con normalización por versión de regla) |
| N-12 | Alarma dura de batería (13,3 / 14,5 V) | STAGE-03 (test de umbrales de alarma sobre la muestra) |
| N-13 | Vigencia `ver-presupuesto-apagado` = 180 d | STAGE-03 (test de caducidad de la `Verificacion`) |
| N-14 | Vigencia `ver-bios-autoencendido` / `ver-flag-ob` = 365 d | STAGE-03 (test de caducidad) |
| N-15 | `ver-shutdown-return` sin caducidad | STAGE-03 (test: `Verificacion` sin vencimiento) |
| N-16 | ≥ 4 pruebas comparables para tendencia | STAGE-03 (test de conteo de `PruebaBateria` con `comparable = true`) |
| N-17 | Cadencia de prueba programada trimestral | STAGE-03 / STAGE-04 (test de la programación del planificador) |
| N-18 | Volumen ~6,3 M filas/año | STAGE-04 (validación de agregación y retención bajo volumen simulado) |
| N-19 | Retención (Muestra P30D, Agregado PT1H/P10Y, Evento indefinido) | STAGE-04 (test del job de agregación y descarte) |
| N-20 | Tamaño máximo del archivo SQLite | **PENDIENTE** — no dimensionado en la fuente (R-07); se mide bajo carga simulada antes de producción, fuera del pipeline de merge |
| N-21 | Cobertura global ≥ 80 % líneas / 70 % ramas | STAGE-04 (gate de cobertura) |
| N-22 | Cobertura `Domain` ≥ 90 % líneas / 85 % ramas | STAGE-03 (gate de cobertura) |
| N-23 | Idempotencia de ingesta 100 % | STAGE-04 (test de los cuatro caminos 201/200/409/422; I-19) |
| N-24 | Procedencia en todo valor (I-7, sin excepción) | STAGE-03 (test de invariante I-7) |
| N-25 | SLO de disponibilidad del servicio | **PENDIENTE** — no está en la fuente; se propone «rondas completadas / esperadas ≥ 0,99 mensual» y se mide en runtime, no en el pipeline |

Los 22 NFR con dato numérico tienen gate. Los 3 PENDIENTE (N-03, N-20, N-25) se declaran como tales, con su vía de cierre (ventana de mantenimiento, medición bajo carga, medición de runtime), coherente con la DoD §2 y con `Criterios-Validacion`.

---

## 9. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Pipeline inicial: 10 stages de GitHub Actions (build, lint, unit, integración, e2e, SCA, SBOM, build de imagen, firma cosign, publicación), matriz de un eje (ubuntu-latest / .NET 10 / linux-amd64), caché de NuGet, promoción DEV→PROD, rollback por etiqueta de imagen y trazabilidad de los 25 NFR de 05 a su gate. Ejecuta la DoD de 08 como gates, sin redefinirla. |
