# Guía de publicación — image-docker — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Guia-Publicacion-Image-Docker-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

---

## 0. Tipo de artefacto

El tipo de artefacto publicado es **image-docker**, por ser el proyecto `web-monolith` (regla §2.2). No hay paquete redistribuible (`redistribuible: false`, intake §13); no aplica ninguna guía de publicación a gestor de paquetes. El nombre del documento respeta el patrón parametrizado `guia-publicacion-<tipo-artefacto>-v<X.Y>.md` con `<tipo-artefacto> = image-docker`.

---

## 1. Pre-requisitos

| Requisito | Detalle |
| --- | --- |
| Registro privado | Un registro de contenedores privado; sin feed público (intake §17.P.7). La imagen se construye localmente y, de publicarse, va al registro privado |
| Credencial de acceso al registro | Token o credencial con scope de push al repositorio de la imagen; se inyecta como secreto del workflow de GitHub Actions, nunca en el repo |
| OIDC de GitHub Actions habilitado | Necesario para la firma cosign keyless (STAGE-09); no requiere clave de larga vida |
| Imagen runtime-only | El `Dockerfile` de producción (`src/SAI.Service.Core.Web/Dockerfile`) es **distinto** del Dev Container: solo runtime, sin SDK ni herramientas de desarrollo |
| Versión SemVer | Calculada por MinVer desde el tag de git (`Estrategia-Versionado §3`); solo tags limpios publican |
| Gates previos | STAGE-01 a STAGE-09 en verde (build, lint, tests, SCA, SBOM, build de imagen + smoke, firma) |

La publicación manual desde la máquina del operador es posible con las mismas credenciales, pero el camino canónico es el stage automatizado.

---

## 2. Comando o stage de publicación

### 2.1 Stage automatizado (camino canónico)

La publicación es **STAGE-10** del pipeline (`Pipeline-CI-CD §1`). Corre solo en el trigger de tag `v<X.Y.Z>`, solo desde `main`, y solo si STAGE-01 a STAGE-09 pasaron. Secuencia:

```bash
# STAGE-08: build de la imagen de producción (runtime-only), etiquetada con la versión de MinVer
docker build -f src/SAI.Service.Core.Web/Dockerfile \
  -t registro-privado/sai-service-core:$MINVER_VERSION \
  -t registro-privado/sai-service-core:latest .

# smoke test (parte del gate 8): arranca el servicio y comprueba el endpoint de salud
docker run -d --name smoke -p 8080:8080 registro-privado/sai-service-core:$MINVER_VERSION
curl -fsS http://localhost:8080/health   # debe responder 200
docker rm -f smoke

# STAGE-09: firma keyless (solo en tag)
cosign sign --yes registro-privado/sai-service-core:$MINVER_VERSION

# STAGE-10: push al registro privado
docker push registro-privado/sai-service-core:$MINVER_VERSION
docker push registro-privado/sai-service-core:latest
```

Variables de entorno requeridas en el workflow: la credencial de acceso al registro (secreto), y el token OIDC que GitHub Actions provee automáticamente a cosign (`id-token: write` en los permisos del job).

### 2.2 Deploy en el host `i7infra` (PROD)

Publicar la imagen no la despliega. El deploy en PROD, tras verificar la firma:

```bash
# 1. Verificar la firma antes de nada (falla ⇒ no se despliega)
cosign verify \
  --certificate-identity-regexp 'https://github.com/.+/sai-service-core' \
  --certificate-oidc-issuer https://token.actions.githubusercontent.com \
  registro-privado/sai-service-core:v<X.Y.Z>

# 2. Traer la imagen
docker pull registro-privado/sai-service-core:v<X.Y.Z>

# 3. Detener la versión en curso respetando el presupuesto de apagado (N-02)
docker stop --time 150 sai-service-core 2>/dev/null || true
docker rm sai-service-core 2>/dev/null || true

# 4. Arrancar la nueva versión (mapeo de USB según ADR-19; volumen del .db; env vars de PROD)
docker run -d --name sai-service-core \
  --restart unless-stopped \
  --env-file /etc/sai-service-core/prod.env \
  --stop-timeout 150 \
  -v /var/lib/sai-service-core:/data \
  --device-cgroup-rule='c 189:* rmw' \
  -p 8080:8080 \
  registro-privado/sai-service-core:v<X.Y.Z>
```

El mapeo exacto del dispositivo USB depende del ADR-19 (NUT en el contenedor o en el host); en la variante "NUT en el host" el `--device` no se pasa y el servicio es cliente TCP de `upsd`.

---

## 3. Verificación post-publish

| Verificación | Cómo | Criterio |
| --- | --- | --- |
| La imagen quedó en el registro | `docker manifest inspect registro-privado/sai-service-core:v<X.Y.Z>` | El manifiesto existe para `linux/amd64` |
| La firma es válida | `cosign verify …` (comando de §2.2) | La firma verifica contra la identidad OIDC del workflow y está en el transparency log |
| El servicio arranca | `docker ps` + log del contenedor | El contenedor está `Up` y loguea el arranque |
| El servicio responde (efecto observado, ADR-11) | `curl -fsS http://localhost:8080/health` | HTTP 200 del endpoint de salud |
| Las rondas de sondeo se reanudan | Log estructurado del contenedor | Aparecen rondas completadas en < 1 s (N-06) al intervalo configurado (N-07) |

La verificación se hace **por efecto observado**, no por ausencia de error: que el `docker run` no lance excepción no prueba que el servicio funcione (ADR-11, CL-07).

---

## 4. Rollback

Volver a la etiqueta de imagen anterior conocida buena y reiniciar el contenedor (intake §17.P.8; ver también `Pipeline-CI-CD §5`):

```bash
# 1. Verificar la firma de la etiqueta anterior
cosign verify … registro-privado/sai-service-core:v<X.Y.Z-1>

# 2. Detener la versión rota respetando el presupuesto de apagado
docker stop --time 150 sai-service-core && docker rm sai-service-core

# 3. Arrancar la etiqueta anterior con los mismos parámetros
docker run -d --name sai-service-core \
  --restart unless-stopped \
  --env-file /etc/sai-service-core/prod.env \
  --stop-timeout 150 \
  -v /var/lib/sai-service-core:/data \
  --device-cgroup-rule='c 189:* rmw' \
  -p 8080:8080 \
  registro-privado/sai-service-core:v<X.Y.Z-1>

# 4. Confirmar por efecto observado
curl -fsS http://localhost:8080/health
```

Ventana de gracia y garantías:

- El estado crítico —la **historia**— es append-only (ADR-04): un rollback de versión no pierde hechos registrados.
- Antes de un deploy que incluya una **migración destructiva** de EF Core, se respalda el archivo SQLite (`cp /var/lib/sai-service-core/sai.db sai.db.bak-<fecha>`), porque un rollback tras una migración destructiva necesita restaurar el archivo. Las migraciones se diseñan aditivas siempre que sea posible, justamente para que el rollback sea solo cambiar de etiqueta.
- El binding del USB por ruta física (ADR-03) sobrevive al reinicio del contenedor.

No hay "delist" de un feed público que gestionar: la imagen vive en un registro privado y el rollback es puramente local al host.

---

## 5. Métricas

Indicadores observables de la publicación y del artefacto en operación:

| Métrica | Fuente | Uso |
| --- | --- | --- |
| Tiempo de build + publicación de la imagen | Duración de STAGE-08 a STAGE-10 en Actions | Detectar regresiones de tiempo de release |
| Resultado del smoke test post-build | Gate 8 | Bloquea la publicación de una imagen que no arranca |
| Vulnerabilidades detectadas post-publish | `schedule` semanal de STAGE-06 (SCA) sobre la imagen publicada | Disparar un PATCH ante una CVE nueva (`Supply-Chain-Seguridad §6`) |
| Tiempo medio hasta detección de regresión en PROD | Log estructurado + verificación del endpoint de salud | Medir cuánto tarda en verse una regresión tras un deploy |
| Rondas de sondeo completadas / esperadas | Log del hosted service | Insumo del SLO N-25 (**PENDIENTE**: ≥ 0,99 mensual propuesto) |

No hay métricas de descargas ni de adopción externa: el artefacto no se distribuye a terceros (registro privado, un solo operador).

---

## 6. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Guía inicial de publicación de la imagen de contenedor (runtime-only, distinta del Dev Container): pre-requisitos, stage STAGE-10 y comandos de build/firma/push, deploy en `i7infra` con verificación de firma previa, verificación post-publish por efecto observado (endpoint de salud), rollback por etiqueta anterior con respaldo del SQLite ante migración destructiva, y métricas. Nombre parametrizado `<tipo-artefacto> = image-docker`. |
