# Auditoría Fase D — Backlog Técnico y Plan de Sprint · Sai-Service-Core

**Fase auditada:** D (categorías 06-Backlog-Tecnico y 07-Plan-Sprint)
**Proyecto:** Sai-Service-Core (project_type: web-monolith, caso degenerado de un solo proyecto, layout aplanado)
**Equipo:** un único desarrollador (equipo_n = 1)
**Auditor:** Auditor independiente SDD
**Fecha:** 2026-07-21
**Reglas de referencia:** 06-Rules-Backlog-Tecnico v1.2 (§6, 11 ítems) y 07-Rules-Plan-Sprint v1.2 (§6, modo 1 dev)
**Alcance de la revisión:** Product-Backlog, Backlog-Tecnico, Definition-Of-Ready, README de 06, 26 historias de usuario individuales (US-01..US-26), Mini-Plan y README de 07. Trazabilidad contrastada contra 00 (Roadmap), 01 (NB-01..NB-08), 02 (CU-01..CU-12, RN) y 05 (ADR-01..ADR-22).

---

## 1. Resumen ejecutivo

Los entregables de la Fase D están completos, internamente consistentes y con una trazabilidad de extremo a extremo sólida: las 26 US trazan a CU sin huérfanas, las 30 BT tienen fuente upstream y consumidora declaradas, la matriz BT↔US↔CU cierra, el reparto MoSCoW es realista y el Mini-Plan no cita ningún identificador inexistente. El modo de un solo desarrollador está bien aplicado: existe el Mini-Plan y están correctamente ausentes los cuatro artefactos de equipo.

El único incumplimiento bloqueante es de contenido, no de trazabilidad: la categoría 06 (Backlog-Tecnico y varias US) nombra el stack concreto y protocolos del dominio fuente (herramienta de acceso al SAI por su nombre comercial, cliente y demonios de ese protocolo, algoritmo de hash, mecanismo de reglas del sistema operativo, formato de errores, registro de arranque del SO), cuando la regla exige que el stack viva en 05 y se referencie por ADR-XX. La categoría 07 respeta esta disciplina (abstrae a «herramienta de acceso al SAI»), lo que evidencia que 06 debió hacer lo mismo.

### Conteo por nivel

| Nivel | Cantidad | Efecto |
|---|---|---|
| P0 (RECHAZA) | 0 | — |
| P1 (BLOQUEA) | 1 | Bloquea la aprobación hasta corregir |
| P2 (MEDIO) | 1 | Corregir en el reproceso |
| P3 (BAJO) | 3 | Mejora recomendada, no bloqueante |

### Veredicto

**NO APROBADO — BLOQUEADO por 1 hallazgo P1.** No hay hallazgos P0: la trazabilidad, la estructura obligatoria, la numeración de dos dígitos y la ausencia correcta de los artefactos de equipo están todas en regla. El bloqueo se levanta con un reproceso acotado de la categoría 06 que sustituya las menciones de stack y protocolo concreto por referencias a las ADR correspondientes (ADR-01, ADR-02, ADR-03, ADR-14, ADR-16, ADR-17), tal como ya hace el Mini-Plan de 07. La categoría 07 pasa sin observaciones bloqueantes.

---

## 2. Matriz D1-D8

| ID | Criterio | Veredicto | Evidencia |
|---|---|---|---|
| D1 | Idioma rioplatense técnico, correcto y sin ambigüedad | Conforme | Redacción uniforme en los 30 archivos; tildes correctas. |
| D2 | Sin emojis; sin negritas decorativas | Parcial (P3) | Sin emojis (los símbolos ↔, →, · son tipográficos, no pictográficos). Hay negritas decorativas inline: «**MVP**», «**vuelva a encenderse solo**» (Product-Backlog §1), «**diseñados pero no implementados**» (Backlog-Tecnico §2), «**No existen**», «**sustituye**» (Mini-Plan y READMEs). Las negritas de etiqueta de cabecera (`**Proyecto:**`) están sancionadas por la plantilla §4.1. |
| D3 | Codificación UTF-8 y saltos de línea LF | Conforme | Los 30 archivos son UTF-8; sin CRLF. |
| D4 | Filenames Título-Con-Guiones | Conforme | US-XX-<Titulo-Con-Guiones>-v1.0.md correcto en las 26 historias; los índices y la DoR también. |
| D5 | Sufijo -v1.0.md (sin `_v`, sin `.v`) | Conforme | Todos usan guion medio `-v1.0.md`; ningún `_` ni `.v`. |
| D6 | Sin apertura con `--` (07); H1 directo | Conforme | Ningún archivo de 06/07 abre con `--`; todos inician con H1. |
| D7 | Terminología: «equipos», «contraseña» | Conforme | Sin ocurrencias de «parque» ni «secreto»; se usa «equipos» y «contraseña» (p. ej. US-02, US-04). |
| D8 | Sin stack concreto en 06/07 | **No conforme en 06 (P1)** · Conforme en 07 | 06 nombra el nombre comercial de la herramienta de acceso, su cliente/demonios, el algoritmo de hash, la regla del SO, el registro de arranque del SO y el formato de errores (ver Hallazgo P1). 07 los abstrae correctamente. |

---

## 3. Matriz de estructura obligatoria

### 3.1 Categoría 06 — documentos y secciones

| Documento | Requisito | Estado |
|---|---|---|
| Product-Backlog-v1.0.md | 5 secciones §4.2 + épicas EP-XX dos dígitos | Conforme (Objetivos, Épicas EP-01..EP-07, Historias por épica, Métricas, Refinamiento) |
| Backlog-Tecnico-v1.0.md | 3 secciones §4.3 + matriz BT↔US↔CU | Conforme (Épicas técnicas, BT por épica, matriz de 30 filas) |
| Definition-Of-Ready-v1.0.md | DoR US 5-8 + BT 4-6 + excepciones + aprobador | Conforme (7 criterios US, 5 criterios BT, 3 excepciones, aprobador declarado) |
| README.md | Índice navegable | Conforme |
| historias-usuario/US-01..US-26 | 26 archivos (>20 US) con 7 secciones §4.4 | Conforme (26 archivos; cada uno con Historia, Contexto, Criterios, Trazabilidad, Prioridad y estimación, DoR check, Notas) |
| tareas-tecnicas/BT-XX | Individuales solo si >30 BT | Correctamente ausente (30 BT ≤ 30 → inline permitido) |

### 3.2 Las 26 historias de usuario

- **26/26 con las 7 secciones obligatorias**: verificado archivo por archivo.
- **26/26 con cabecera §4.1 completa**: Proyecto, Documento, Versión, Estado, Fecha, Autor, Épica, Prioridad MoSCoW, Estimación.
- **26/26 con ≥2 escenarios Given/When/Then** (todas tienen 3-4), incluidas las dos Could.
- **26/26 sin huérfanas de CU** (ver §4.1).
- Numeración US-01..US-26 contigua, dos dígitos, sin rastros de tres dígitos.

### 3.3 Categoría 07 — modo 1 dev

| Artefacto | Regla (equipo_n=1) | Estado |
|---|---|---|
| Mini-Plan-v1.0.md | Obligatorio (sustituye a los cuatro) | Presente y completo (Sprint 0 + 5 etapas; §1-§12) |
| README.md | Recomendado | Presente |
| Plan-Iteracion-Sprint-XX-v1.0.md | NO debe existir | Correctamente ausente |
| Template-Sprint-Review-v1.0.md | NO debe existir | Correctamente ausente |
| Template-Sprint-Retrospectiva-v1.0.md | NO debe existir | Correctamente ausente |
| Velocidad-Equipo-v1.0.md | NO debe existir | Correctamente ausente |

El Mini-Plan declara el sprint goal de cada etapa como una sola frase orientada a valor sin bullets (§3.1, §4.1, §5.1, §6.1, §7.1, §8.1), referencia la DoD canónica de 08 sin duplicarla (§9), declara trazabilidad a CU y NB por etapa (§x.4 y §11 global) y lista ≥2 riesgos con mitigación por etapa (Sprint 0: 2, E1: 2, E2: 2, E3: 4, E4: 5, E5: 2). La etapa crítica (Etapa 4) concentra 5 riesgos de severidad crítica/alta. Nomenclatura correcta, ningún archivo abre con `--`.

---

## 4. Coherencia cross-doc y trazabilidad

### 4.1 NB ↔ CU ↔ US (cobertura upstream)

- **8/8 NB cubiertas por al menos una US.** NB-01 (US-05,14,15,16); NB-02 (US-03,07,08,09,25); NB-03 (US-08,10,11); NB-04 (US-03,04,18,19,20,23,24); NB-05 (US-01,02,05,15,16,17); NB-06 (US-12,13,18,19,24); NB-07 (US-06,14); NB-08 (US-21,22).
- **12/12 CU cubiertos por al menos una US.** CU-01 (US-01,02); CU-02 (US-03,04,05,25); CU-03 (US-06); CU-04 (US-07,08,09,10,25,26); CU-05 (US-14,15,17); CU-06 (US-10,11); CU-07 (US-12,13); CU-08 (US-18,19,24); CU-09 (US-20); CU-10 (US-05,15,16,17); CU-11 (US-21,22); CU-12 (US-19,23,24).
- **0 US huérfanas de CU**: las 26 declaran ≥1 CU en su columna `CU relacionados` y en la cabecera de trazabilidad del archivo individual.

### 4.2 BT ↔ US ↔ CU

- **30/30 BT con fuente upstream declarada** (ADR, NB, CU, componente de 05, contrato o capacidad de intake).
- **30/30 BT con US consumidora o justificación de infraestructura**: la matriz §3 del Backlog-Tecnico las nombra todas; BT-02 y BT-05 y BT-07 llevan justificación explícita de infraestructura compartida con su ADR.
- Matriz BT↔US↔CU completa y coherente con las columnas `BT derivadas` de cada US (verificación cruzada sobre US-05→BT-23, US-14→{BT-04,16,17,20,22,29}, US-25/26→BT-14, entre otras).
- Numeración BT-01..BT-30 contigua, dos dígitos, sin `BT-001`.

### 4.3 Identificadores de 07 ⊆ 06 (verificación clave)

- El Mini-Plan solo referencia US-01..US-26, BT-01..BT-30 y EP-01..EP-07: **todos existen en 06; ningún identificador inventado.**
- Cobertura completa: los 30 BT y las 24 US Must/Should quedan asignados a una etapa; las 2 US Could (US-25, US-26) quedan diseñadas y diferidas a v2 (no comprometidas), coherente con MoSCoW.
- Aritmética de puntos consistente 06↔07: Must 114 SP + Should 39 SP = 153 SP comprometidos; totales por etapa (24+24+58+79+91+47) reconcilian con las estimaciones de 06 sin re-estimación.
- Cobertura CU/NB por etapa (§11) recorre los 12 CU y las 8 NB a lo largo de Sprint 0 → Etapa 5.

### 4.4 Coherencia de alcance y decisiones abiertas

- **MoSCoW ≠ 100 % Must**: Must 18 US / 114 SP, Should 6 US / 39 SP, Could 2 US / 8 SP (69 % / 23 % / 8 % por cantidad). Coherente con §4 del intake (F-01..F-20 Must, F-21..F-25 Should, F-26/F-27 Could, F-28..F-33 Won't documentadas fuera del backlog).
- **ADR Propuestos ADR-19..ADR-22 tratados como Sprint 0**: BT-01..BT-04 los cierran como spikes con caja temporal en el bloque A del Sprint 0.
- **Pendientes no inventados**: P-05 (contrato de rectificación del 409) queda abierto vía BT-03/ADR-21 y US-22 lo marca «por cerrar», no resuelto; R-07 (volumen de retención) se expresa como riesgo en Etapa 3 y Etapa 5 sin fabricar una solución cerrada.

---

## 5. Hallazgos

### P1-01 — Stack y protocolos concretos del dominio fuente en la categoría 06

- **Nivel:** P1 (bloquea). Incumple 06-Rules §6 ítem 11 y marca como No Conforme la celda D8 de la matriz; no rompe trazabilidad.
- **Archivos y secciones:**
  - `06-Backlog-Tecnico/Backlog-Tecnico-v1.0.md` §1 y §2: BT-01 «ubicación de NUT (contenedor vs host)»; BT-04 «implementación por NUT»; BT-15 «Implementación NUT del adaptador (cliente TCP de upsd/upsc)» y «dialoga con el SAI vía NUT como cliente TCP»; BT-10 «hash PBKDF2 y sesión por cookie»; BT-13 «regla udev» / «`ID_PATH`»; BT-25 «cruce con wtmp»; BT-28 «problem+json»; nota §2 «adaptador directo sin NUT».
  - `06-Backlog-Tecnico/Product-Backlog-v1.0.md` §3: título de US-25 «…no cubiertos por NUT».
  - `06-Backlog-Tecnico/README.md`: «BT-01 ubicación de NUT».
  - `historias-usuario/US-25-*.md` (título y §1, §2, §3, §5, §6) y `US-26-*.md` (§5, §7): NUT nombrado múltiples veces; `US-17-*.md` (§2, §3) y `US-22-*.md` (§2, §3): «wtmp», «problem+json»; `US-03-*.md` (§6): «ubicación de NUT».
- **Evidencia de que es corregible sin pérdida:** el Mini-Plan de 07 ya abstrae el mismo concepto («BT-15 · Implementación de la herramienta de acceso al SAI», «BT-25 · Verificación del ajuste de arranque por comportamiento») y cita el stack solo por ADR («ADR-15 (cinco assemblies)»). La regla fija que el stack vive en 05 y se referencia por ADR-XX; 06 tiene las ADR fuente en la columna correspondiente (ADR-01 acceso al SAI, ADR-02 puerto, ADR-03 anclaje, ADR-14 verificación por comportamiento, ADR-16 autenticación, ADR-17 errores de API), por lo que puede referirlas sin nombrar la tecnología.
- **Recomendación:** reprocesar 06 sustituyendo los nombres concretos por la capacidad y su ADR: «herramienta de acceso al SAI (ADR-01)» en lugar de NUT/upsd/upsc; «hash de contraseña y sesión según ADR-16» en lugar de PBKDF2/cookie; «anclaje del nodo por ruta física del puerto (ADR-03)» en lugar de udev/ID_PATH; «verificación por comportamiento cruzando el registro de arranque del SO (ADR-14)» en lugar de wtmp; «formato de error estándar de la API (ADR-17)» en lugar de problem+json. Renombrar US-25 a «Adaptador de conexión directo para equipos no cubiertos por la herramienta de acceso» y ajustar su archivo (el slug del filename puede conservarse por versionado, pero el título y el cuerpo deben abstraerse).

### P2-01 — Fuente upstream no canónica en dos BT

- **Nivel:** P2 (medio). No rompe trazabilidad (ambas BT tienen US consumidora y CU aguas arriba), pero la columna `Fuente` se aparta de la taxonomía §4.3 (NB, CU, ADR o contrato).
- **Archivos y secciones:** `Backlog-Tecnico-v1.0.md` §2 — BT-19 declara como única fuente «F-08» (capacidad de intake, no NB/CU/ADR/contrato); BT-06 declara «UX (03)» como fuente principal (categoría de diseño, no un componente/ADR de 05).
- **Recomendación:** anclar BT-19 a la ADR o RN que gobierna la derivación versionada de eventos (o al CU-04 que la consume) además de F-08; para BT-06, referenciar el componente/entregable de 05 o la maqueta aprobada de 03 como «contrato de UX» de forma explícita. El resto de las BT que citan F-XX ya lo acompañan de una NB/ADR/CU y no requieren cambio.

### P3-01 — Negritas decorativas inline

- **Nivel:** P3 (bajo, estilo). Ver D2 en §2.
- **Recomendación:** limitar la negrita a etiquetas de cabecera; quitar el énfasis decorativo en prosa (Product-Backlog §1, Backlog-Tecnico §2, Mini-Plan, READMEs).

### P3-02 — El DoR check de las US enumera 6 de los 7 criterios de la DoR

- **Nivel:** P3 (bajo, consistencia documental). En las 26 US el bloque «DoR check» lista seis ítems y omite el criterio 7 de `Definition-Of-Ready-v1.0.md` §1 («Alcance acotado a la etapa»). El alcance está implícito en la asignación a épica/etapa, por lo que no invalida el filtro.
- **Recomendación:** agregar la casilla del criterio 7 al checklist de cada US, o renumerar la DoR para reflejar los seis puntos efectivamente verificados.

### P3-03 — Término de stack «assemblies» en el Mini-Plan

- **Nivel:** P3 (bajo). El Mini-Plan de 07 usa «cinco assemblies» (BT-05, §3.4 y §3.6), atribuido a ADR-15, lo que lo hace tolerable, pero «assemblies» es terminología de plataforma.
- **Recomendación:** preferir «cinco proyectos/ensamblados según ADR-15» o dejar solo la referencia «estructura de ADR-15».

---

## 6. Confirmaciones solicitadas

1. **26 US con 7 secciones y sin huérfanas de CU:** Confirmado. Las 26 historias tienen las siete secciones §4.4 y cabecera §4.1; cada una declara ≥1 CU (0 huérfanas). Los 12 CU quedan cubiertos.
2. **30 BT con upstream:** Confirmado. Las 30 tareas técnicas declaran fuente upstream y US consumidora o justificación de infraestructura; matriz BT↔US↔CU completa. Observación P2-01 sobre la fuente no canónica de BT-06 y BT-19 (no bloqueante para trazabilidad).
3. **MoSCoW no 100 % Must:** Confirmado. 69 % Must / 23 % Should / 8 % Could por cantidad; coherente con §4 del intake.
4. **Mini-Plan sin IDs inexistentes:** Confirmado. Todos los US-XX/BT-XX/EP-XX del Mini-Plan existen en 06; numeración contigua US-01..US-26 y BT-01..BT-30; puntos reconciliados sin re-estimación.
5. **Cuatro artefactos de equipo correctamente ausentes:** Confirmado. En 07 solo existen Mini-Plan-v1.0.md y README.md; no hay Plan-Iteracion-Sprint-XX, Template-Sprint-Review, Template-Sprint-Retrospectiva ni Velocidad-Equipo. Ausencia correcta por equipo_n=1 (no se marca como faltante).

---

## 7. Veredicto final

**NO APROBADO — BLOQUEADO.** 0 P0, 1 P1, 1 P2, 3 P3. La Fase D es estructural y trazablemente sólida y el modo de un solo desarrollador está correctamente aplicado; el único bloqueo es el hallazgo P1-01 (stack y protocolos concretos del dominio fuente en la categoría 06), corregible con un reproceso acotado que sustituya los nombres de tecnología por referencias a las ADR de 05, como ya hace el Mini-Plan de 07. Corregido P1-01 (y, deseablemente, P2-01), la Fase D queda en condiciones de aprobación. La categoría 07 no tiene observaciones bloqueantes.
