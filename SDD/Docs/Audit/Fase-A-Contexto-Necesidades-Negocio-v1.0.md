# Auditoría Fase A — 00-Contexto y 01-Necesidades-Negocio

| Campo | Valor |
| --- | --- |
| Fase auditada | A (categorías 00-Contexto y 01-Necesidades-Negocio) |
| Solución | SAI.Service.Core — proyecto único `Sai-Service-Core`, `web-monolith`, caso degenerado (layout aplanado) |
| Alcance | Los 5 documentos de `SDD/Docs/00-Contexto/` (incluido README y la omisión de Acuerdo-Equipo) y los 10 de `SDD/Docs/01-Necesidades-Negocio/` (índice, README y 8 NB) |
| Reglas aplicadas | `00-Rules-Contexto.md` v1.3 §6 (11 ítems); `01-Rules-Necesidades-Negocio.md` v1.2 §6 (14 ítems); D1-D8; nombres canónicos §3.5 del Master-Prompt |
| Fuente de verdad | `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md`, `SOLUTION-MANIFEST-Sai-Service-Core-v1.0.md` |
| Auditor | Auditor independiente SDD |
| Fecha | 2026-07-20 |

---

## 1. Resumen ejecutivo

Los 15 entregables están presentes, bien estructurados y con alta fidelidad al intake: no se detectaron documentos obligatorios faltantes, ni cabeceras o checklists ausentes, ni vocabulario prohibido por D7, ni stack filtrado en Visión/Alcance, ni ruptura de trazabilidad, ni enlaces rotos, ni ciclos de dependencia entre NB. Los 9 PENDIENTE (P-01 a P-09) del intake se tratan correctamente como abiertos, sin cerrarlos con datos inventados. No hay hallazgos P0, por lo que la fase no se rechaza. Sí hay un hallazgo P1 de coherencia MoSCoW contra el §4 del intake (dos NB que implementan capacidades Must Have del intake se declaran Should Have) que debe corregirse antes de promover, más tres P2 de nombres de categoría downstream no canónicos (incluido el `03-Diseno-UX-UI` explícitamente prohibido) y tres P3 menores.

Conteo: **P0 = 0 · P1 = 1 · P2 = 3 · P3 = 3.**

Veredicto: **APROBADO CON OBSERVACIONES** (ver §7).

---

## 2. Matriz D1-D8 por documento

Convención: OK = conforme · — = no aplica · ! = hallazgo (ver §6).

| Documento | D1 sin emojis/negrita decorativa | D2 UTF-8/LF | D3 nombre Título-Con-Guiones | D4 sufijo `-v1.0` guion medio | D5 versión inicial | D6 trazabilidad declarada | D7 sin vocab bootstrap / sin stack en negocio | D8 tipo web-monolith |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Vision-Producto-v1.0.md | OK | OK | OK | OK | OK | OK (upstream+downstream) | OK (sin stack) | OK |
| Alcance-Proyecto-v1.0.md | OK | OK | OK | OK | OK | OK | OK (sin stack) | OK |
| Roadmap-Producto-v1.0.md | OK (`→` es tipográfico, no emoji) | OK | OK | OK | OK | OK | OK | OK |
| Compatibilidad-Plataformas-v1.0.md | OK | OK | OK | OK | OK | OK | OK (stack esperado en matriz de plataformas) | OK |
| 00-Contexto/README.md | OK | OK | OK (sin versión, correcto) | — | — | OK | OK | OK |
| Necesidades-Negocio-v1.0.md (índice) | OK (`→` tipográfico) | OK | OK | OK | OK | OK (+ Cantidad de NB, Versión catálogo) | OK | OK |
| 01/README.md | OK | OK | OK | — | — | OK | OK | OK |
| NB-01 … NB-08 (8 archivos) | OK | OK | OK | OK | OK | OK (upstream+downstream en cada cabecera) | OK | OK |

D1-D8 sin infracciones. El scan de emojis solo devolvió el carácter flecha `→` (U+2192) en Roadmap e índice, uso tipográfico legítimo (idéntico al del propio intake y master-prompt), no emoji. Las negritas presentes son etiquetas de cabecera del template §4.1 de 00-Rules, no decorativas. Todos los archivos son UTF-8 sin CRLF.

---

## 3. Matriz de estructura obligatoria por documento

### 3.1 Categoría 00 (cabecera §4.1 + secciones §4.2)

| Documento | Cabecera completa | Secciones obligatorias | Resultado |
| --- | --- | --- | --- |
| Vision-Producto | OK | §1-§10 completas (problema, stakeholders, propuesta, visión 3 años, SMART, métricas, restricciones, riesgos, glosario, trazabilidad) | Completo |
| Alcance-Proyecto | OK | §1-§10 completas (propósito, descripción, objetivos, incluido, excluido, supuestos, restricciones, criterios aceptación, gestión de cambios, trazabilidad) | Completo |
| Roadmap-Producto | OK | §1-§6 completas (propósito, fases, matriz fase→épica→sprint→release, dependencias, criterios de transición `- [ ]`, trazabilidad downstream a 06/07) | Completo |
| Compatibilidad-Plataformas | OK | §1-§6 completas (resumen, matriz, restricciones justificadas, alternativas, estado, trazabilidad) | Completo |
| README 00 | OK (recomendado) | Enumera los 4 docs, orden de lectura, omisión de Acuerdo-Equipo justificada, stakeholders, trazabilidad | Completo |

Acuerdo-Equipo-v1.0.md se omite correctamente (proyecto de 1 desarrollador) con nota explícita en el README citando §2.2 de 00-Rules. Compatibilidad es "recomendado" para web-monolith y está presente, declarando todas las plataformas de §17 P.9 del intake.

### 3.2 Categoría 01 (índice + NB, §4.2 diez secciones)

| Documento | Cabecera | §1-§10 en orden | ≥4 criterios SMART (§5) | ≥3 stakeholders (§6) | MoSCoW (§9) | Dep. acíclicas ≤3 (§8) | Resultado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Índice Necesidades-Negocio | OK (+ Cantidad NB=8, Versión catálogo) | Índice: propósito, resumen, dependencias, trazabilidad, catálogo CU, control de cambios | — | — | Coherente índice↔NB | Grafo acíclico declarado | Completo |
| NB-01 | OK | 10/10 | 5 | 3 (prop/impl/benef-host) | Must | 3 dep, sin ciclo | Completo |
| NB-02 | OK | 10/10 | 5 | 4 | Must | 2 dep | Completo |
| NB-03 | OK | 10/10 | 5 | 3 | Must | 1 dep | Completo |
| NB-04 | OK | 10/10 | 5 | 3 | Must | 0 dep (raíz) | Completo (ver P3-01) |
| NB-05 | OK | 10/10 | 5 | 3 | Must | 2 dep | Completo |
| NB-06 | OK | 10/10 | 6 | 3 | Should | 2 dep | Completo (ver P1-01) |
| NB-07 | OK | 10/10 | 5 | 3 | Must | 1 dep | Completo |
| NB-08 | OK | 10/10 | 5 | 3 | Should | 2 dep | Completo (ver P1-01, P3-03) |

Todos los filenames NB cumplen el regex `^NB-\d{2}-([A-Z][A-Za-z0-9]*)(-[A-Z][A-Za-z0-9]*)*-v\d+\.\d+\.md$`. La subcarpeta `Necesidades-De-Negocio/` existe; el índice vive en la raíz de la categoría. README presente y obligatorio (8 NB > 5). Estados dentro del enum cerrado (todos "Borrador").

---

## 4. Coherencia cross-doc

| Verificación | Resultado |
| --- | --- |
| Enlaces del índice a las 8 NB | Los 8 paths `Necesidades-De-Negocio/NB-0X-...-v1.0.md` resuelven contra archivos reales |
| IDs NB únicos | NB-01 a NB-08 sin duplicados |
| Catálogo de CU (índice §5) vs `§7` de cada NB | CU-01 a CU-19 consistentes; CU compartidas (CU-06 NB-04/NB-08, CU-08 NB-01/NB-05, CU-09 NB-04/NB-06) coinciden en ambos lados |
| MoSCoW índice §2 vs §9 de cada NB | Coherente en las 8 (índice y NB declaran la misma prioridad) |
| MoSCoW índice/NB vs §4 del intake | **Incoherente** en NB-06 y NB-08 (hallazgo P1-01) |
| Grafo de dependencias entre NB | Acíclico; ninguna NB depende de >3; el orden topológico declarado (NB-04→NB-03→NB-02→NB-07→NB-05→NB-01, con NB-06/NB-08 al final) es consistente entre índice §3 y README |
| Glosario Visión vs uso en NB | Sin contradicciones; términos (SAI, procedencia, ciclo forzado, vínculo temporal, salud de batería) usados de forma consistente |
| Trazabilidad upstream declarada | Cada NB y cada doc 00 declaran secciones del intake; consistentes con el contenido |
| Trazabilidad downstream — nombres canónicos §3.5 | **No canónicos** en 3 puntos (hallazgos P2-01/02/03) |
| Fidelidad al intake / PENDIENTE | P-01 a P-09 tratados como abiertos y citados por su ID; sin afirmaciones inventadas; `i7infra`, proveedor ficticio, líneas base, techos de tiempo, todos con respaldo en la fuente |

---

## 5. Detalle por criterio §6 de las reglas

00-Rules §6 (11 ítems): visión sin stack OK; alcance con 16 capacidades y 8 exclusiones justificadas OK (≥5 y ≥3); roadmap con checklists `- [ ]` en §5 OK (≥3 hitos); 5 objetivos SMART OK (≥3); stakeholders con propietario/implementador/beneficiario nominales OK; glosario con 10 términos del dominio OK (≥5 para proyecto individual); compatibilidad declara las plataformas de §17 P.9 OK; acuerdo-equipo omitido con nota OK; trazabilidad upstream/downstream declarada OK; nombres con guion medio OK; sin emojis/negrita decorativa/stack hardcodeado en negocio OK.

01-Rules §6 (14 ítems): índice maestro presente OK; ≥3 NB (8) OK; 10 secciones en orden OK; ≥4 criterios SMART por NB OK; MoSCoW en §9 con justificación OK; upstream explícito OK; §7 con estado `a generar` OK; ≥3 stakeholders nominales OK (ver P3-03 para el matiz de NB-08); regex de filename OK; enlaces del índice resuelven OK; ≤3 dependencias y sin ciclos OK; README presente (>5 NB) OK; estado en enum cerrado OK; sin emojis/negrita/vocab D7 OK.

---

## 6. Hallazgos enumerados

### P1-01 — Incoherencia MoSCoW contra el §4 del intake (NB-06 y NB-08)
- Nivel: P1 (alto — bloquea hasta corrección).
- Archivos/sección: `Necesidades-De-Negocio/NB-06-Evaluacion-De-Salud-De-Baterias-v1.0.md` §9; `Necesidades-De-Negocio/NB-08-Ingesta-Automatizada-De-Intervenciones-v1.0.md` §9; `Necesidades-Negocio-v1.0.md` §2 (tabla resumen y nota MoSCoW).
- Evidencia: el intake §4 clasifica **F-16** ("Prueba de batería programada… y manual"), **F-17** ("Veredicto de salud calculado por el servicio") y **F-20** ("API REST de ingesta idempotente") como **Must Have**, y remata: *"sin F-01 a F-20 el servicio no cumple ninguno de sus dos propósitos (garantizar el reencendido y construir el histórico de salud)"*. El propio índice §2 lo reconoce: *"el SOLUTION-INTAKE §4 marca como Must Have el conjunto F-01 a F-20 (primera entrega)"*. Sin embargo, NB-06 (que declara upstream `§4 (F-16, F-17)`) se rotula **Should Have** y NB-08 (upstream `§4 (F-20)`) también **Should Have**. Además, la justificación de NB-06 §9 es internamente contradictoria: dice *"el intake la ubica en la primera entrega después del núcleo"*, y "primera entrega" es precisamente lo que la regla de derivación del §4 mapea a Must Have. La construcción del histórico de salud es uno de los dos propósitos centrales declarados, no un Should.
- Impacto: degrada a Should dos capacidades Must del origen de verdad; desordena la priorización que 06-Backlog-Tecnico y 07-Plan-Sprint derivan de la MoSCoW de cada NB.
- Recomendación: alinear la prioridad de NB-06 y NB-08 con el §4 del intake (Must Have), o —si se decide reprioritizar deliberadamente— escalar el cambio como decisión de producto explícita en Visión/Alcance y registrarlo, no resolverlo en silencio dentro de la §9 de la NB con una justificación que contradice la fuente. Corregir también la nota MoSCoW del índice §2 para que no afirme el conjunto Must y a la vez rotule dos de sus miembros como Should.

### P2-01 — Nombre de categoría downstream no canónico `03-Diseno-UX-UI`
- Nivel: P2 (medio — no rompe resolución de enlaces; la categoría 03 aún no existe y la referencia es textual).
- Archivos/sección: `Vision-Producto-v1.0.md` (cabecera línea 10 y §10 línea 120); `Alcance-Proyecto-v1.0.md` (cabecera línea 10 y §10 línea 112); `00-Contexto/README.md` (§Trazabilidad, línea 36). 5 ocurrencias.
- Evidencia: el nombre canónico del §3.5 del Master-Prompt es `03-UX-UI-DX`. Los documentos usan `03-Diseno-UX-UI`, exactamente la "variante inventada" prohibida.
- Recomendación: reemplazar todas las ocurrencias por `03-UX-UI-DX`.

### P2-02 — Nombre de categoría downstream no canónico `09-DevOps`
- Nivel: P2.
- Archivos/sección: `Compatibilidad-Plataformas-v1.0.md` (cabecera línea 10 y §6 línea 62); `00-Contexto/README.md` (§Trazabilidad, línea 36). 3 ocurrencias.
- Evidencia: el canónico §3.5 es `09-Devops`. Se usa `09-DevOps` (mayúscula intermedia distinta).
- Recomendación: normalizar a `09-Devops`.

### P2-03 — Nombre de categoría downstream truncado `08-Calidad`
- Nivel: P2.
- Archivos/sección: `Compatibilidad-Plataformas-v1.0.md` (cabecera línea 10 y §6 línea 62). 2 ocurrencias.
- Evidencia: el canónico §3.5 es `08-Calidad-Y-Pruebas`. Se usa la forma abreviada `08-Calidad`.
- Recomendación: usar `08-Calidad-Y-Pruebas`.

### P3-01 — NB Must Have que empaqueta capacidades Should Have del intake
- Nivel: P3 (bajo — está divulgado y justificado).
- Archivo/sección: `NB-04-Ciclo-De-Vida-Del-Parque-v1.0.md` cabecera (upstream `F-21, F-22, F-23`), §9 y control de cambios.
- Evidencia: NB-04 es Must Have pero incorpora F-21 (sustitución del SAI), F-22 y F-23 (informe/comparación de marcas), que el intake §4 marca Should Have. El propio doc lo declara. El efecto es que el backlog no puede diferir limpiamente esas capacidades Should sin partir la NB.
- Recomendación: aceptable si se conserva la nota; considerar separar el alcance Should (comparación de marcas / sustitución) de modo que la priorización downstream pueda diferirlo sin arrastrar el núcleo Must.

### P3-02 — Cabecera del índice con lista upstream incompleta respecto del cuerpo
- Nivel: P3.
- Archivo/sección: `Necesidades-Negocio-v1.0.md` cabecera (upstream `§1, §3, §4, §8, §10, §11`) vs §4 (tabla que además cita `§7 casos límite`).
- Evidencia: el cuerpo referencia §7 del intake como fuente de varias NB, pero la cabecera del índice no lo lista.
- Recomendación: agregar §7 a la trazabilidad upstream de la cabecera del índice para que coincida con el cuerpo.

### P3-03 — NB-08 sin fila de stakeholder etiquetada como beneficiario
- Nivel: P3.
- Archivo/sección: `NB-08-Ingesta-Automatizada-De-Intervenciones-v1.0.md` §6.
- Evidencia: las tres filas son propietario, implementador y "Integrador / consumidor" (sistema externo). La categoría beneficiario no tiene fila explícita (el administrador, beneficiario de fondo del histórico alimentado, figura solo como propietario/implementador). El resto de las NB sí explicitan una fila beneficiario.
- Recomendación: agregar/renombrar una fila para dejar explícita la categoría beneficiario, por consistencia con el criterio §6 de 01-Rules.

---

## 7. Veredicto final

**APROBADO CON OBSERVACIONES** (no hay P0; hay 1 P1, 3 P2, 3 P3).

Condiciones para promover a la Fase B:
1. Bloqueante (P1-01): resolver la incoherencia MoSCoW de NB-06 y NB-08 contra el §4 del intake — alinear a Must Have, o escalar la reprioritización como decisión de producto explícita — y corregir la nota MoSCoW del índice §2.
2. Recomendado antes de promover (P2-01/02/03): normalizar los nombres de categoría downstream a los canónicos del §3.5 (`03-UX-UI-DX`, `09-Devops`, `08-Calidad-Y-Pruebas`) en Visión, Alcance, Compatibilidad y README 00.
3. Opcional (P3-01/02/03): aislar el alcance Should dentro de NB-04, completar la lista upstream de la cabecera del índice y explicitar el beneficiario en NB-08 §6.

No se detectaron defectos de trazabilidad, documentos obligatorios faltantes, cabeceras/checklists ausentes, vocabulario prohibido por D7, stack en documentos de negocio, ni cierre de PENDIENTE con datos inventados: la base es sólida y la corrección del P1 más los P2 deja la fase lista para aprobación plena.
