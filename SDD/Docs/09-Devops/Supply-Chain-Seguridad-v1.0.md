# Supply chain y seguridad — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Supply-Chain-Seguridad-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

---

## 0. Alcance

Política de cadena de suministro del artefacto image-docker de `Sai-Service-Core`: inventario de dependencias (SBOM), firma, nivel SLSA objetivo, escaneo de dependencias, análisis estático y dinámico, y política de CVE. Todos estos controles se materializan como stages del pipeline (`Pipeline-CI-CD §1`); acá se declara la política que cada stage cumple. El contexto es de un solo host en LAN, sin exposición a internet, con un único operador.

---

## 1. SBOM

| Parámetro | Valor |
| --- | --- |
| Formato | **CycloneDX** (intake §17.P.8, gate 7) |
| Generador | Herramienta CycloneDX para .NET (`dotnet CycloneDX` sobre `SAI.Service.Core.sln`) |
| Formato de salida | JSON |
| Stage | STAGE-07 del pipeline |
| Publicación | Adjunto como artefacto del build; en un build de tag, queda asociado al release junto a la imagen |
| Criterio de éxito | El artefacto SBOM existe y es válido (gate 7); su ausencia bloquea el merge |

El SBOM inventaría las dependencias directas y transitivas (incluidos `Microsoft.EntityFrameworkCore.Sqlite`, `MudBlazor` y el resto del árbol NuGet). Es lo que permite responder ante una CVE de una dependencia sin auditar el código a mano. Se recomienda **firmar el propio SBOM** con cosign junto a la imagen (mismo mecanismo keyless de §2), para que el inventario sea tan verificable como el artefacto que describe.

---

## 2. Firma

| Parámetro | Valor |
| --- | --- |
| Herramienta | **cosign** (sigstore), modo **keyless** (intake §17.P.8, gate 9) |
| Qué se firma | La imagen de producción del tag; se recomienda firmar también el SBOM |
| Cuándo | **Solo en builds de tag** (STAGE-09); un `push` a `main` sin tag no firma |
| Identidad | OIDC efímero de GitHub Actions (`id-token: write`); sin clave de larga vida almacenada |
| Transparency log | Rekor (append-only); la firma queda registrada públicamente en el log de transparencia |
| Verificación | Se verifica **antes de publicar** (gate 9) y de nuevo **antes de desplegar** en PROD (`Guia-Publicacion-Image-Docker §2.2`) |

El modo keyless evita el problema de custodia de una clave privada de firma: no hay secreto de firma que rotar ni que filtrar. La verificación por el consumidor (el propio operador, al desplegar) se hace contra la identidad OIDC del workflow y el emisor esperado.

---

## 3. SLSA

**Nivel objetivo: SLSA Build L2**, con plan de elevación a L3.

| Requisito | Estado en este proyecto |
| --- | --- |
| Build con servicio hospedado (no en la máquina del dev) | Cumplido: GitHub Actions construye la imagen de release; el build de PROD no sale de la laptop |
| Procedencia generada y firmada | Cumplido en su base: firma cosign keyless con registro en Rekor (§2); se recomienda emitir además atestación de procedencia SLSA (`provenance`) del build |
| Procedencia disponible para el consumidor | Cumplido: transparency log + SBOM adjunto |
| Aislamiento del build (L3) | **Meta**: requiere ejecutores efímeros aislados y procedencia no falsificable por el build; alcanzable en GitHub Actions con runners efímeros y el generador de atestaciones SLSA. Es el plan de elevación de L2 a L3 |

L2 es realista con la configuración actual (build hospedado + firma + procedencia). L3 queda como meta declarada, no como estado. No se persigue L4 (dos revisores, build hermético reproducible bit a bit): el costo excede el valor para un artefacto de un solo consumidor interno.

---

## 4. Dependency scanning (SCA)

| Parámetro | Valor |
| --- | --- |
| Herramienta | `dotnet list package --vulnerable --include-transitive` (intake §17.P.8, gate 6) |
| Stage | STAGE-06; corre en cada PR/push y además en el `schedule` semanal |
| Frecuencia | Por cada cambio (gate de merge) y semanal sobre dependencias sin cambios de código, para detectar CVE nuevas |
| Complemento recomendado | Dependabot o Renovate para PRs automáticos de actualización de dependencias |

Política por severidad:

| Severidad | Acción |
| --- | --- |
| **Crítica** | Bloquea el merge (gate 6). Remediación inmediata; ver §6 |
| **Alta** | Bloquea el merge (gate 6). Sin excepción salvo ADR + BT de remediación con plan y plazo |
| **Media** | No bloquea; se registra como BT en el backlog para remediar en la etapa siguiente |
| **Baja** | Se registra; se remedia en actualización de rutina |

El criterio de bloqueo (cero altas/críticas) es exactamente el de la DoD BT de 08 ("no introduce dependencia con vulnerabilidad de severidad alta o crítica").

---

## 5. SAST y DAST

**SAST (análisis estático):** los **analizadores de .NET** en nivel `recommended`, ejecutados en STAGE-02 junto con `dotnet format --verify-no-changes` (gate 2). Criterio de bloqueo de PR: cero diagnósticos de severidad *error* y sin diferencias de formato. Con `TreatWarningsAsErrors` (gate 1), todo diagnóstico de compilación relevante es además bloqueante. Es el mismo control que la DoD US de 08 ("análisis estático sin diagnósticos de severidad error").

**DAST (análisis dinámico): no aplica en el alcance actual.** Justificación: el servicio corre en LAN, sin exposición a internet (intake §17.P.5). Su única superficie hacia terceros es la API de ingesta `/api/v1/`, consumida por un GMAO interno declarado. No hay superficie pública que un escáner dinámico (tipo ZAP) deba barrer, y montarlo sería infraestructura sin consumidor. Se declara explícitamente como **no aplicable**, no como omisión: si en el futuro el panel o la API se expusieran fuera de la LAN, se incorpora DAST y se revisa esta decisión (ligada a la resolución PENDIENTE de TLS del panel y la API, ADR-20).

---

## 6. Política de CVE

SLA de remediación por severidad, para dependencias detectadas por SCA (§4) en runtime o post-publish:

| Severidad | SLA de remediación | Acción |
| --- | --- | --- |
| Crítica | Inmediata (mismo día) | PATCH con la actualización de la dependencia; deploy y verificación por endpoint de salud |
| Alta | ≤ 7 días | PATCH; si no hay fix upstream, mitigación documentada + BT |
| Media | ≤ 30 días | Actualización en la etapa siguiente |
| Baja | Próxima actualización de rutina | Sin plazo duro |

Detección post-publish: el `schedule` semanal de STAGE-06 evalúa la imagen ya publicada contra la base de avisos actualizada; una CVE nueva sobre una dependencia ya desplegada dispara el flujo de PATCH del `Estrategia-Versionado §1` (bump PATCH) y el redeploy del `Guia-Publicacion-Image-Docker §2.2`.

Comunicación al consumidor: con un único operador interno que es a la vez quien remedia, la "comunicación" es el registro en el backlog (BT) y en el registro de cambios del release. El único consumidor externo de contrato (el GMAO de la API `/api/v1/`) solo se notifica si una remediación implicara un cambio incompatible del contrato, lo que dispararía la deprecation policy v1→v2 (`Estrategia-Versionado §7`).

Ventana entre detección y publicación de fix: acotada por el SLA de la tabla; el pipeline permite construir, firmar y publicar un PATCH en una sola corrida de tag, así que la ventana operativa es del orden de minutos más el tiempo de revisión del cambio.

---

## 7. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Política inicial de supply chain: SBOM CycloneDX JSON adjunto al build (gate 7), firma cosign keyless solo en tag verificada antes de publicar y de desplegar (gate 9), SLSA Build L2 objetivo con plan a L3, SCA por `dotnet list package --vulnerable` con política por severidad (cero altas/críticas bloqueante), SAST por analizadores .NET (gate 2), DAST declarado no aplicable en LAN con condición de revisión, y política de CVE con SLA por severidad. |
