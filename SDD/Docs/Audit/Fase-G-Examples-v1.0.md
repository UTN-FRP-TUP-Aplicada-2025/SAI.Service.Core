# Auditoría Fase G — 11-Examples (Sai-Service-Core)

**Fase auditada:** G — Categoría 11 (Examples / Samples)
**Proyecto:** Sai-Service-Core (project_type: web-monolith, caso degenerado con layout aplanado — 11 directo bajo `SDD/Docs/`)
**Alcance:** `SDD/Docs/11-Examples/README.md`, `Ejemplo-01-Datos-Seed-v1.0.md`, `Ejemplo-02-Api-Ingesta-v1.0.md`. Se audita la documentación de los samples; el código ejecutable en `/samples` se materializa en la fase de codificación y su ausencia NO se considera falta.
**Reglas de referencia:** `IA/IA.SDD/SDD/Devs/Rules/11-Rules-Examples.md` (v1.2), §6 criterios de aceptación (14 ítems).
**Upstream verificado:** `SDD/Docs/02-Especificacion-Funcional/`, `SDD/Docs/05-Arquitectura-Tecnica/`, `SDD/Intake/SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` (§16.1, §18, §20).
**Auditor:** Auditor independiente SDD (Arquitecto de Soluciones + QA Senior; no participó de la generación).
**Fecha:** 2026-07-21

---

## 1. Resumen ejecutivo

Los tres entregables de la categoría 11 cumplen la totalidad de los criterios bloqueantes del §6 de las reglas constructivas. Existen los dos samples mínimos que el tipo `web-monolith` exige (§2.2), ambos con las nueve secciones obligatorias (§4.2) y la cabecera de metadatos completa (§4.1), con ≤5 pasos de arranque, nivel declarado, output exacto trazado a los escenarios `§20` del intake y trazabilidad a artefactos upstream que existen y están vigentes. El README trae la tabla maestra con todas las columnas requeridas. La nomenclatura es por capacidad (`datos-seed`, `api-ingesta`), sin nombres atados al dominio.

La única desviación relevante —el renombre del sample de ingesta de `ingesta-gmao` (nombre por dominio en el intake §16.1/§18) a `api-ingesta` (nombre por capacidad)— es la resolución **correcta** de un conflicto intake↔regla a favor de la regla del framework, y está documentada con vínculo explícito a §18. Se clasifica como P2 (informativa), no bloqueante.

### Conteo por nivel

| Nivel | Cantidad | Bloqueante |
|---|---|---|
| P0 (rechaza) | 0 | Sí |
| P1 (bloquea) | 0 | Sí |
| P2 (medio) | 2 | No |
| P3 (bajo) | 3 | No |

### Veredicto

**APROBADO.** Sin hallazgos P0 ni P1. Los entregables pueden avanzar. Las observaciones P2/P3 son de trazabilidad-de-desviación y metadatos, para atender en la promoción de estado, no para bloquear.

---

## 2. Matriz de estilo D1–D8

| Criterio | Resultado | Evidencia |
|---|---|---|
| Codificación UTF-8 | Cumple | `file` reporta UTF-8 en los tres archivos |
| Fin de línea LF | Cumple | 0 ocurrencias de CR en los tres archivos |
| Sin emojis | Cumple | Solo caracteres tipográficos legítimos (`→`, `⇒`, `≠`, `≈`, `Σ`, `±`, `…`) usados en notación técnica y de cambio; ningún pictograma |
| Sin negritas decorativas | Cumple con reserva (P3) | Predomina la negrita de etiqueta (término:definición), aceptada como estilo de la casa (las propias reglas y el intake la usan igual). Persisten pocas negritas enfáticas en prosa (`**dos**`, `**cuatro caminos**`, `**sin el SAI físico…**`) — ver H-4 |
| Español rioplatense técnico | Cumple | Registro técnico consistente; tildes correctas |
| Terminología (equipos, contraseña) | Cumple | Se usa «equipos», «catálogo/inventario», «administrador único»; no aparece terminología prohibida |
| Tooling técnico (Dev Container, .NET 10, Docker, curl) | Legítimo (no es violación) | La categoría 11 es técnica; el «cómo correrlo» exige nombrar el toolchain. Correctamente clasificado como legítimo por §6 ítem 1 |

---

## 3. Matriz de estructura (2 samples + README)

### 3.1 README de la sección

| Ítem §6 / §4.3 | Resultado | Evidencia |
|---|---|---|
| Existe README de la carpeta | Cumple | `SDD/Docs/11-Examples/README.md` |
| Tabla maestra con columnas Sample / Nivel / Tiempo de setup / CU / Ubicación | Cumple | Tabla en líneas 9–12; columnas exactas al §4.4 y §6 |
| Ambos samples listados con nivel, tiempo de setup, CU, ubicación | Cumple | Básico `< 5 min` CU-02/04/06/12; Intermedio `10–15 min` CU-11 |
| Convenciones de los samples | Cumple | §"Convenciones" (autocontenidos, ≤5 pasos, nivel declarado, output exacto, trazabilidad, nomenclatura por capacidad) |
| Cómo agregar un sample nuevo + referencia al §6 de reglas | Cumple | Sección con 5 pasos y remisión a `11-Rules-Examples.md` |
| Vínculo con 05 y con la guía conceptual | Cumple | Enlaza `Contratos-REST`, `ADR-02`, `ADR-17`; documenta la omisión de la 10 por `ADR-23` |
| README sin sufijo de versión (§3.1) | Cumple | `README.md` es el índice, correctamente sin `-vX.Y` |

### 3.2 Ejemplo-01-Datos-Seed-v1.0.md

| Ítem | Resultado | Evidencia |
|---|---|---|
| Cabecera §4.1 completa (incluye Nivel + Ubicación del código) | Cumple | Proyecto, Documento, Versión, Estado, Fecha, Autor, Nivel: Básico, Ubicación: `/samples/01-datos-seed/` |
| Las 9 secciones del §4.2 | Cumple | 1 Objetivo, 2 Nivel, 3 Prerequisites, 4 Cómo correrlo, 5 Estructura, 6 Qué esperar, 7 Variaciones, 8 Trazabilidad, 9 Control de cambios |
| ≤5 pasos para correr | Cumple | 4 pasos (§4) |
| Nivel declarado | Cumple | Básico, con justificación respecto al Ejemplo 02 |
| Output esperado exacto (§6) | Cumple | Estado en vivo, eventos, históricos, prueba e informe con valores de `E-1/E-2/E-4/E-5/E-7`, marcados como derivado los que corresponde |
| Prerequisites con versión mínima | Cumple | Docker 24.0, .NET SDK 10.0, Chromium 120 / Firefox 121 (devcontainer CLI «actual» — ver H-5) |
| Trazabilidad ≥1 fila (§8) | Cumple | CU-02/04/06/12, ADR-02, RN-01/02, §20 E-1…E-8 |
| Nombre por capacidad, sin dominio | Cumple | `datos-seed`; sin `gmao`/`sai`/`bateria` en el filename ni como nombre de sample |
| Sufijo `-v1.0.md` | Cumple | — |

### 3.3 Ejemplo-02-Api-Ingesta-v1.0.md

| Ítem | Resultado | Evidencia |
|---|---|---|
| Cabecera §4.1 completa | Cumple | Nivel: Intermedio, Ubicación: `/samples/02-api-ingesta/` |
| Las 9 secciones del §4.2 | Cumple | Secciones 1–9 presentes |
| ≤5 pasos para correr | Cumple | 5 pasos exactos (§4) — dentro del límite |
| Nivel declarado | Cumple | Intermedio, justificado respecto al Ejemplo 01 |
| Output esperado exacto (§6) | Cumple | Cuerpos JSON de los 4 caminos 201/200/409/422 idénticos a `§20.E-8` |
| Prerequisites con versión mínima | Cumple con reserva (P3) | Docker 24.0, .NET SDK 10.0; `curl` «cualquier versión moderna» (aceptable, versión-agnóstico, igual que el ejemplo genérico §7.2 de las reglas) |
| Trazabilidad ≥1 fila (§8) | Cumple | CU-11, F-20, ADR-17, RN-07/08/09, §20.E-8 |
| Nombre por capacidad, sin dominio | Cumple | `api-ingesta`; el dominio (`gmao`) queda fuera del filename y del nombre del sample |
| Sufijo `-v1.0.md` | Cumple | — |

### 3.4 Cantidad mínima de samples (§2.2 web-monolith)

Cumple. `web-monolith` exige 2 samples (datos seed + tema custom si hay punto de extensión visual). Se entregan 2: `01-datos-seed` y `02-api-ingesta`. El segundo base (`02-tema-custom`) **no aplica** —el panel Blazor/MudBlazor no es personalizable por terceros— y el README (§"Matriz tipo D8") documenta la sustitución justificada por `api-ingesta`, nombrada por capacidad. Desviación de §2.3 admisible y documentada; no es hallazgo.

---

## 4. Coherencia cross-doc y trazabilidad (existencia de CU/RN/ADR/F/NFR)

Todas las referencias upstream declaradas existen y están vigentes (`-v1.0`):

| Referencia | Declarada en | Existe en | Estado |
|---|---|---|---|
| CU-02 Alta de equipos y puesta en marcha | Ejemplo-01 §8 | `02/Casos-De-Uso/CU-02-…-v1.0.md` | Verificado |
| CU-04 Monitoreo en vivo | Ejemplo-01 §8 | `02/Casos-De-Uso/CU-04-…-v1.0.md` | Verificado |
| CU-06 Históricos y gráficas | Ejemplo-01 §8 | `02/Casos-De-Uso/CU-06-…-v1.0.md` | Verificado |
| CU-12 Informe de período | Ejemplo-01 §8 | `02/Casos-De-Uso/CU-12-…-v1.0.md` | Verificado |
| CU-11 Ingesta automatizada de intervenciones | Ejemplo-02 §8 | `02/Casos-De-Uso/CU-11-…-v1.0.md` | Verificado |
| RN-01 Arranque seguro en SoloAlerta | Ejemplo-01 §8 | `02/Reglas-De-Negocio/RN-01-…-v1.0.md` | Verificado |
| RN-02 Bloqueo por verificación | Ejemplo-01 §8 | `02/Reglas-De-Negocio/RN-02-…-v1.0.md` | Verificado |
| RN-07 Todo importe con moneda y fecha | Ejemplo-02 §8 | `02/Reglas-De-Negocio/RN-07-…-v1.0.md` | Verificado |
| RN-08 Cuadre de costos de intervención | Ejemplo-02 §8 | `02/Reglas-De-Negocio/RN-08-…-v1.0.md` | Verificado |
| RN-09 Idempotencia de la ingesta | Ejemplo-02 §8 | `02/Reglas-De-Negocio/RN-09-…-v1.0.md` | Verificado |
| ADR-02 Adaptador de conexión con tres implementaciones | Ejemplo-01 §8, README | `05/Adrs/ADR-02-…-v1.0.md` | Verificado |
| ADR-17 Manejo de errores de la API de ingesta | Ejemplo-02 §8, README | `05/Adrs/ADR-17-…-v1.0.md` | Verificado |
| ADR-23 Omisión de developer guide dedicada | README | `05/Adrs/ADR-23-…-v1.0.md` | Verificado |
| F-20 API REST de ingesta idempotente | Ejemplo-02 §8 | Intake §5 (línea 102, Must Have) | Verificado |
| Contratos-REST-v1.0.md | README, ambos samples | `05/Contratos-REST-v1.0.md` | Verificado |
| Escenarios §20 E-1…E-8 | Ambos samples | Intake Parte D §20 | Verificado |

**Ejemplo-01** ilustra CU de exploración (CU-02/04/06/12) + ADR-02 (adaptador simulado): correcto y completo respecto al alcance esperado.
**Ejemplo-02** ilustra CU-11 + RN-07/08/09 + ADR-17 + los cuatro caminos 201/200/409/422: correcto y completo.

**Coincidencia de output con la fuente:** los cuatro cuerpos JSON de la §6 de Ejemplo-02 (201 con `confianzaAsignada: media` y `motivoConfianza`; 200 con `creado:false`; 409 `conflicto_idempotencia` con `huellaOriginal/huellaRecibida`; 422 `campo: costos.total`, invariante `Costos.cuadra()`) reproducen literalmente `§20.E-8` del intake (líneas 1873–1915). Cabeceras `X-Idempotency-Key: gmao-ext-ot-88213` y `X-Fuente-Datos: fd-gmao-externo` idénticas. Los valores del panel de Ejemplo-01 (costo `67.000,00 ARS`, repuestos 52000 + mano de obra 15000) son coherentes entre ambos samples y con E-7/E-8. Sin valores inventados.

---

## 5. Evaluación de la desviación de naming (intake `ingesta-gmao` → regla `api-ingesta`)

**Hecho.** El intake nombra el sample de ingesta por dominio en tres lugares: §16.1 materialización de `/samples` (`samples/ingesta-gmao/`, líneas 443/461), §18 estrategia de demo (`samples/ingesta-gmao/`, líneas 754/762) y la tabla de mapeo de cierre (`Ejemplo-01-Ingesta-Gmao-v1.0.md`, línea 2101). La categoría 11 lo renombró a `api-ingesta` / `Ejemplo-02-Api-Ingesta-v1.0.md`.

**Regla aplicable.** El §6 ítem 7, el §3.1 y el anti-patrón §4.5 prohíben nombrar por dominio del proyecto y obligan a nombrar por nivel o por capacidad. «gmao» es el producto del consumidor externo (GMAO Corporativo v4, actor `fd-gmao-externo`): es dominio, no capacidad. El renombre a `api-ingesta` cumple la regla.

**Vínculo documentado.** La desviación NO queda huérfana:
- Ejemplo-02 §1: «Este sample corresponde al de la estrategia de demo del intake (§18)».
- README §"Matriz tipo D8": «…la API de ingesta declarada en §18 del intake. El nombre es por capacidad (`api-ingesta`), no por el dominio del consumidor».

**Clasificación.** Conflicto intake↔regla resuelto correctamente a favor de la regla del framework, con vínculo a §18 explícito y trazable. Severidad **P2** (medio, informativa). Al estar el vínculo documentado, NO se eleva de nivel. No es bloqueante y no requiere corrección del entregable; sí conviene, opcionalmente, que una futura revisión del intake alinee su §16.1/§18/tabla de mapeo con el nombre por capacidad para eliminar la discordancia de origen.

---

## 6. Hallazgos

| ID | Nivel | Archivo / Sección | Evidencia | Recomendación |
|---|---|---|---|---|
| H-1 | P2 | Ejemplo-02 (todo) + README §"Matriz tipo D8" / vs Intake §16.1/§18/§ mapeo | El intake nombra el sample `ingesta-gmao` (por dominio); la categoría 11 lo entrega como `api-ingesta` (por capacidad). Desviación correcta y con vínculo a §18 documentado | Ninguna sobre el entregable (resolución correcta). Opcional: alinear el intake para eliminar la discordancia de origen |
| H-2 | P2 | Ambos samples — cabecera §4.1 (Estado) | Ambos documentos están en `Estado: Borrador` | Promover a `Vigente` (o `Aprobado`) al cerrar la Fase G; hoy es metadato, no bloquea |
| H-3 | P3 | README §"Matriz tipo D8 → /samples" | Sustitución del base `02-tema-custom` por `02-api-ingesta`: desviación de §2.3 correctamente justificada por ausencia de punto de extensión visual | Ninguna; se registra por transparencia |
| H-4 | P3 | Ejemplo-01 §1/§6, Ejemplo-02 §1, README | Persisten pocas negritas enfáticas en prosa corrida (`**dos**`, `**cuatro caminos de respuesta**`, `**sin el SAI físico…**`) además de la negrita de etiqueta aceptada | Reservar la negrita para etiquetas término:definición; quitar el énfasis en prosa. Consistente con el uso de las propias reglas, por eso no se eleva |
| H-5 | P3 | Ejemplo-01 §3 y Ejemplo-02 §3 (Prerequisites) | «CLI `devcontainer` actual» y «`curl` cualquier versión moderna» sin versión pinneada | Aceptable para herramientas rolling/versión-agnósticas (el ejemplo genérico §7.2 de las reglas usa la misma fórmula para curl); opcionalmente fijar un piso |

No se registran hallazgos P0 ni P1.

---

## 7. Veredicto final

**APROBADO — sin hallazgos bloqueantes (0 P0, 0 P1).**

Los entregables de la Fase G (11-Examples) de Sai-Service-Core satisfacen los 14 criterios de aceptación del §6 de `11-Rules-Examples.md`: README con tabla maestra completa, los dos samples mínimos de `web-monolith`, cada uno con cabecera §4.1 y las nueve secciones §4.2, ≤5 pasos de arranque, nivel declarado, output exacto trazado a `§20`, prerequisites con versión, trazabilidad a CU/RN/ADR/F que existen y están vigentes, y nomenclatura por capacidad sin nombres de dominio. La desviación de naming intake↔regla está bien resuelta y documentada (P2). Las cinco observaciones P2/P3 son de trazabilidad-de-desviación, metadatos y estilo, atendibles en la promoción de estado y no condicionan el avance.

---

*Auditor independiente SDD — 2026-07-21. No participó de la generación de los entregables. Este informe audita; no modifica los entregables.*
