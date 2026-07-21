# Estrategia de versionado — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Estrategia-Versionado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-09)

---

## 0. Documento bisagra

Este documento marca la frontera entre el **código** (Conventional Commits, branching) y el **artefacto publicado** (SemVer, canal, deprecation). Lo consumen tanto el autor —para saber cómo etiquetar un commit y una rama— como el consumidor de la API `/api/v1/`, que necesita saber cuándo un cambio rompe el contrato. Deriva del intake §17.P.7 y se alinea con el branching por etapas de la descomposición de delivery (intake §15) y del plan de 07.

---

## 1. SemVer 2.0.0

Se adopta **SemVer 2.0.0 sin excepciones** (intake §17.P.7). Formato:

```
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILDMETADATA]
```

Reglas de incremento para este proyecto:

| Componente | Se incrementa cuando… | Ejemplo |
| --- | --- | --- |
| `MAJOR` | Cambio **incompatible** del contrato de la API `/api/v1/` (abre `/api/v2/`) o del esquema de datos que requiera intervención manual (una migración destructiva no automatizable) | `1.4.2 → 2.0.0` |
| `MINOR` | Capacidad nueva **retrocompatible**; típicamente, cada etapa de §15 cerrada (un flujo de usuario nuevo, un endpoint aditivo, un campo opcional) | `1.4.2 → 1.5.0` |
| `PATCH` | Corrección de comportamiento existente sin cambio de contrato | `1.4.2 → 1.4.3` |
| `-PRERELEASE` | Preestreno: sufijo `-alpha.N` derivado de la distancia al tag (ver §5) | `1.5.0-alpha.3` |
| `+BUILDMETADATA` | Metadatos de build opcionales (hash de commit); no afectan la precedencia | `1.5.0+a1b2c3d` |

La versión se materializa como **etiqueta de la imagen de contenedor** (no hay paquete redistribuible). Un cambio incompatible del contrato REST es la única fuente realista de `MAJOR` en este proyecto, porque la API de ingesta es el único contrato formal hacia terceros (intake §14, §17.P.3).

---

## 2. Conventional Commits 1.0.0

Se adopta **Conventional Commits 1.0.0 sin excepciones** (intake §17.P.7). Tipos admitidos: `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`. El marcador `BREAKING CHANGE` en el pie del mensaje, o `!` tras el tipo, eleva a `MAJOR`.

| Prefijo de commit | Bump SemVer | Ejemplo |
| --- | --- | --- |
| `feat` | MINOR | `feat(politica): agregar modalidad CicloForzado a la configuración` |
| `fix` | PATCH | `fix(resolutor): corregir reatribución de histórico al mover un recambio` |
| `feat!` o `BREAKING CHANGE` en el pie | MAJOR | `feat(api)!: migrar contrato de ingesta de v1 a v2` |
| `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf` | Ninguno | `refactor(planificador): extraer la ronda de sondeo a un método` |

El tipo del commit que cierra una etapa determina el bump del release de esa etapa. Con merge por rama de etapa (ver §4), el mensaje de merge lleva el Conventional Commit que define el incremento.

---

## 3. Herramienta de versionado — MinVer

**MinVer** calcula la versión desde el tag de git más cercano (intake §17.P.7). Se elige por ser la de menor fricción en .NET: un solo paquete, sin archivo de configuración, y sin un paso de escritura en el repositorio durante el build (la versión no se commitea, se deriva).

| Parámetro | Valor |
| --- | --- |
| Paquete | `MinVer` como `PackageReference` en `Directory.Build.props` |
| Prefijo de tag | `v` (por ejemplo `v1.5.0`) |
| Versión de un commit sobre un tag | El tag `+` altura del commit como `-alpha.N` (por ejemplo, tres commits sobre `v1.4.2` ⇒ `1.4.3-alpha.3`) |
| Versión de un commit **en** el tag | El tag exacto, sin sufijo (`1.4.2`) |
| Consumo en el build | La imagen de contenedor se etiqueta con `$(MinVerVersion)`; el pipeline publica solo cuando la versión no lleva sufijo de preestreno (tag limpio) |

MinVer no requiere un stage propio: se ejecuta como parte de `dotnet build` (STAGE-01) y su salida alimenta la etiqueta de la imagen en STAGE-08 y la decisión de publicación de STAGE-10.

---

## 4. Branching — trunk-based

**Trunk-based development** (intake §17.P.7). Con un solo desarrollador, GitFlow es sobrecarga pura.

- Rama `main`: **siempre desplegable**. Es la única rama de larga vida. Todo tag SemVer se crea sobre `main`.
- Ramas de trabajo: **cortas, una por etapa**, con el patrón `etapa/NN-<slug>`, donde `NN` es el número de etapa de §15 y `<slug>` la describe (por ejemplo `etapa/05-uf1-alta-de-equipos`). Se integran a `main` por merge y se borran al cerrar la etapa.
- Alineación con §15 y con 07: el orden de las ramas de etapa respeta el grafo de dependencias de los flujos (UF-1 → UF-2 → UF-3 → UF-5 → UF-4 → UF-8 → UF-6 → UF-7 → UF-10 → UF-9). Ningún flujo se construye antes que aquellos de los que depende.

Protección de rama y política de PR (adaptada a un solo desarrollador):

| Regla | Valor |
| --- | --- |
| Merge a `main` | Solo vía PR desde una rama `etapa/NN-<slug>` |
| Gates requeridos para mergear | Los diez stages del pipeline (STAGE-01 a STAGE-08 en PR; ver `Pipeline-CI-CD §1.1`) |
| Aprobación humana | El propio administrador único aprueba el PR; el gate real es el pipeline verde, no una segunda persona |
| Historial | Merge con mensaje Conventional Commits que define el bump de la etapa |

El cierre de cada etapa incluye la **validación del agente humano** con entregable tangible (intake §15): la siguiente etapa no arranca hasta que esa validación pase. Ese criterio de cierre es upstream de la creación del tag SemVer de la etapa.

---

## 5. Canales

**Canal único** (intake §17.P.7): la imagen de contenedor. No hay canales preview/stable separados por feed, porque no hay feed público ni consumidor externo de imágenes.

| Etiqueta | Origen | Uso |
| --- | --- | --- |
| `v<X.Y.Z>` (SemVer) | Tag limpio sobre `main` | Release estable; es la etiqueta que se despliega en PROD |
| `latest` | Se reasigna al último `v<X.Y.Z>` estable publicado | Conveniencia para el operador; apunta siempre al estable más reciente |
| `<X.Y.Z>-alpha.N` | Distancia al tag calculada por MinVer | Preestreno; se construye pero **no se publica** al registro (solo tags limpios publican, gate 10) |

Semántica de sufijos: solo se usa `-alpha.N`, generado automáticamente por MinVer desde la altura del commit sobre el último tag. No se adoptan `-beta` ni `-rc`: con un solo operador y despliegue en un único host, la cadena alpha → estable es suficiente y los canales intermedios no compran nada.

---

## 6. Feed y redistribución

- **Sin feed público** (intake §17.P.7). La imagen se construye localmente y, si en el futuro se publicara, sería a un **registro privado**. No hay paquetes NuGet redistribuibles: `redistribuible: false` (intake §13).
- La publicación al registro privado ocurre solo desde `main` con tag SemVer (gate 10). El detalle operativo está en `Guia-Publicacion-Image-Docker-v1.0.md`.

---

## 7. Deprecation policy

Aplica al único contrato formal hacia terceros: la API REST `/api/v1/` (intake §17.P.3).

- Un cambio **incompatible** del contrato abre `/api/v2/` (bump MAJOR); `v1` se mantiene mientras haya un consumidor declarado (hoy, el GMAO externo `fd-gmao-externo`).
- Los cambios **aditivos** (campos opcionales nuevos, endpoints nuevos) no rompen versión: son MINOR sobre `v1`.
- Al deprecar `v1` en favor de `v2`, se anuncia el cambio con anticipación al consumidor declarado y se documenta en el registro de cambios del release. Con un único consumidor externo conocido, la coordinación es directa; no se fija una ventana de N minor rígida, pero no se remueve `v1` mientras exista un consumidor que la use.
- El panel Blazor no está versionado ni tiene deprecation policy: se despliega junto al servidor y no expone contrato estable a terceros (intake §17.P.3).

---

## 8. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Estrategia inicial: SemVer 2.0.0 (MAJOR por contrato `/api/v1/` o esquema; MINOR por etapa cerrada; PATCH por fix), Conventional Commits 1.0.0, MinVer como herramienta de versión derivada del tag, branching trunk-based (`main` desplegable + ramas `etapa/NN-<slug>`), canal único de imagen (SemVer + `latest` + preestreno `-alpha.N`), sin feed público (registro privado, `redistribuible: false`) y deprecation policy v1→v2 de la API. |
