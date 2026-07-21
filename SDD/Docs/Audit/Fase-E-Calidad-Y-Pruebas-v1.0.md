# Auditoría Fase E — Calidad y pruebas (categoría 08)

**Proyecto:** Sai-Service-Core
**Fase auditada:** E — Calidad y pruebas (categoría 08-Calidad-Y-Pruebas)
**Alcance:** web-monolith, caso degenerado (layout aplanado: 08 cuelga directo de `SDD/Docs/`, sin `Proyectos/`). Los 10 entregables de 08.
**Reglas de referencia:** `08-Rules-Calidad-Y-Pruebas.md` v1.3 (§6, §4, §2.2) y `Deriva-Rules.md` v1.0 (§2.3, §3) para la Matriz-Sensado-Deriva.
**Upstream cotejado:** 02 (12 CU, 13 RN, 21 invariantes I-1..I-21), 05 (25 NFR N-01..N-25), 03 (Linea-Base-Visual: 11 SUP / 19 CMP / 79 EST / 9 NAV; Contrato-Datos-Maqueta: 24 DM), 06 (Definition-Of-Ready), 07 (Mini-Plan), intake §17.
**Auditor:** Auditor independiente SDD
**Fecha:** 2026-07-21

---

## 1. Resumen ejecutivo

Los 10 entregables de la categoría 08 están presentes, completos y trazados. La Matriz-Cobertura contiene las tres tablas obligatorias más invariantes y cobertura por capa; cada NFR con objetivo numérico cerrado tiene test o mecanismo de medición; los 40 casos de prueba referencian al menos un CU cada uno; la Matriz-Sensado-Deriva tiene exactamente una fila `SD-XX` por cada uno de los 142 elementos de la línea de base (11 SUP + 19 CMP + 79 EST + 9 NAV + 24 DM), con conteos que coinciden con 03; la DoD despliega las cuatro capas sin solapar la DoR de 06. La reconciliación de la pirámide (70/25/5 del intake vs 70/20/10 del default) está documentada, y los cinco PENDIENTE (N-03, N-20, N-25, F-3 no automatizable, contrato 409) se declaran como gaps, no se inventan.

No se detectaron hallazgos P0 ni P1. Se registra un hallazgo P2 de coherencia (umbrales de cobertura de la capa Infraestructura/Api discrepantes entre tres documentos) y hallazgos P3 de estilo y trazabilidad menor.

| Nivel | Cantidad |
| --- | --- |
| P0 (rechaza) | 0 |
| P1 (bloquea) | 0 |
| P2 (medio) | 1 |
| P3 (bajo) | 3 |

**Veredicto: APROBADO.** La Fase E cumple las reglas §6 de 08-Rules y §2.3/§3 de Deriva-Rules. Se recomienda subsanar el P2 de coherencia de umbrales antes de congelar los pisos de cobertura del pipeline.

---

## 2. Matriz D1-D8 (idioma, formato, nomenclatura, terminología)

| Criterio | Resultado | Evidencia |
| --- | --- | --- |
| D1 idioma rioplatense técnico | Conforme | Prosa técnica en español rioplatense en los 10 docs; tildes correctas |
| D2 sin emojis / negritas decorativas | Conforme con observación | Sin emojis. Uso de negritas para énfasis de términos carga-clave (p. ej. Estrategia-Calidad §1, Estrategia-Testing §1); borderline decorativo, ver P3-01 |
| D3/D4 identificadores de dos dígitos | Conforme | `TC-01..TC-40`, `SD-01..SD-142`, `SUP/CMP/EST/NAV/DM-XX` de dos dígitos |
| D5 codificación UTF-8/LF | Conforme | Archivos UTF-8; sin CRLF observado |
| Filenames `-v1.0.md` sin sufijo de dominio | Conforme | Los 9 artefactos + README siguen `-v1.0.md`; ningún `-motor` ni marcador temático. `Guia-Testing-Extensibilidad-v1.0.md` (no `-Motor`) |
| Terminología `equipos` / `contraseña` | Conforme | Se usa «equipos» (no «parque») y «contraseña» (no «password»/«clave» de login) de forma sistemática |
| Cabecera de metadatos uniforme | Conforme | Los 10 docs abren con H1 + bloque Proyecto/Documento/Versión/Estado/Fecha/Autor |

Nota sobre el tooling y el dominio: el nombramiento de xUnit, FluentAssertions, WebApplicationFactory, bUnit, Playwright, EF Core/SQLite (realización elegida del intake §17.P.6) y del vocabulario de dominio del SAI/NUT (adaptador de conexión, invariantes I-1..I-21, `battery.voltage`, estados OB/LB) es legítimo para una categoría técnica análoga a 05. No constituye violación de §4.3.3 y no se marca como hallazgo.

---

## 3. Matriz de estructura obligatoria (§6 de 08-Rules + tabla maestra §2.1)

| # | Documento | Presente | Contenido obligatorio | Resultado |
| --- | --- | --- | --- | --- |
| 1 | Estrategia-Calidad-v1.0.md | Sí | Definición de calidad; 8 atributos ISO/IEC 25010 priorizados con NFR de origen; 10 quality gates con condición/herramienta/consecuencia; RACI de roles; cadencia | Conforme |
| 2 | Estrategia-Testing-v1.0.md | Sí | Pirámide numérica 70/25/5 con reconciliación; cobertura por capa; tooling por capacidad; BDD; mocks/fixtures; datos; ambiente; prueba imposible F-3 declarada | Conforme |
| 3 | Plan-Pruebas-v1.0.md | Sí | Alcance por etapa; criterios de entrada (5) y salida (7); 9 riesgos con prob/impacto/mitigación; plan por etapa mapeado al Mini-Plan; recursos | Conforme |
| 4 | Matriz-Cobertura-Pruebas-v1.0.md | Sí | 3 tablas (CU↔, NFR↔, RN↔) + invariantes↔ + cobertura por capa + gaps | Conforme (ver §4) |
| 5 | Casos-Prueba-Referenciales-v1.0.md | Sí | 40 TC con tipo/cubre/setup/GWT/expected/actual/status; ≥1 TC por CU crítico | Conforme |
| 6 | Criterios-Validacion-v1.0.md | Sí | Criterios funcionales por CU; NFR con SLA; regresión; calidad de código; excepciones con ADR | Conforme |
| 7 | Definition-Of-Done-v1.0.md | Sí | 4 capas (US/BT/sprint/release), criterios mecánicos con `- [ ]`, excepciones, vigencia canónica | Conforme (ver §5) |
| 8 | Guia-Testing-Extensibilidad-v1.0.md | Sí | Motor de extensión interno (puerto del adaptador): contrato de 4 operaciones, 3 niveles de prueba, doble simulado, procedimiento de incorporación, checklist | Conforme |
| 9 | Matriz-Sensado-Deriva-v1.0.md | Sí | 142 filas SD-XX (una por elemento de línea base), método/evidencia/umbral menor+mayor por fila | Conforme (ver §6) |
| 10 | README.md | Sí | Índice de artefactos, quality gates en CI, enlace a DoD canónica | Conforme |

La Guia-Testing-Extensibilidad es correcta pese a que `web-monolith` no la exige por defecto: la regla §2.2 la habilita «salvo motor de extensión interno», y el puerto `IAdaptadorConexion` (ADR-02/ADR-22) es exactamente ese motor. Presencia justificada.

---

## 4. Trazabilidad y coherencia cross-doc

### 4.1 Matriz-Cobertura: tres tablas obligatorias + cobertura por capa

- **CU↔Tests (§2):** los 12 CU (CU-01..CU-12) con criterio GWT y al menos un TC. Ningún CU huérfano. Conforme.
- **NFR↔Tests (§3):** los 25 NFR. Los 22 con objetivo numérico cerrado tienen test o mecanismo de medición con su tooling; N-03, N-20 y N-25 se declaran PENDIENTE y se derivan a §6 (gaps), en línea con intake §17.P.10 que los deja sin dimensionar. Conforme.
- **RN↔Tests (§4.1):** las 13 RN (RN-01..RN-13) con ≥1 TC. Conforme.
- **Invariantes↔Tests (§4.2):** los 21 invariantes I-1..I-21, uno por TC unitario (TC-01..TC-21). Conforme.
- **Cobertura por capa (§5):** reportada por capa, no como número global único. Conforme al anti-patrón §4.10 (ver P2-01 por la discrepancia numérica).

### 4.2 TC ↔ CU/RN/NFR (cada TC referencia upstream)

Los 40 casos (TC-01..TC-40, contiguos) declaran su línea «Cubre:» con al menos un CU en cada uno; los de invariante añaden I-x y RC-x, y varios enlazan RN y N-xx. Ningún TC huérfano; ningún requisito funcional sin TC. Todos los TC referenciados por las matrices (cobertura, deriva) existen en el catálogo (rango TC-01..TC-40, incluido TC-40 de contrato). Conforme.

### 4.3 Matriz-Sensado-Deriva ↔ línea de base (verificación clave)

Conteo cotejado contra 03. Coincidencia exacta, sin elementos de línea base sin fila:

| Tipo | Línea de base (03) | Filas SD en la matriz | Rango SD | Coincide |
| --- | --- | --- | --- | --- |
| Superficies SUP | 11 | 11 | SD-01..SD-11 | Sí |
| Componentes CMP | 19 | 19 (18 + CMP-17 instrumento) | SD-12..SD-30 | Sí |
| Estados EST | 79 | 79 | SD-31..SD-109 | Sí |
| Rutas NAV | 9 | 9 | SD-110..SD-118 | Sí |
| Campos DM | 24 | 24 | SD-119..SD-142 | Sí |
| **Total** | **142** | **142** | SD-01..SD-142 | **Sí** |

Desglose EST por superficie verificado uno a uno (8/9/7/6/7/6/5/8/8/8/7 = 79) contra las subsecciones 3.1–3.11 de la matriz: coincide. Cada fila declara método de verificación resuelto por AG-08 (test automatizado con su `TC-XX`, o inspección), evidencia esperada en el formato de Deriva-Rules §1, y umbral por dimensión con expansión menor+mayor en §0.1. El instrumento de maqueta (CMP-17/SD-28, NAV-09/SD-118) se verifica por ausencia en el producto, correctamente. Los `TC-XX` citados por la matriz de deriva existen todos en Casos-Prueba. Conforme.

### 4.4 Coherencia de parámetros del intake

- Pirámide 70/25/5 tomada del intake §17.P.6, reconciliada explícitamente contra el 70/20/10 del default de `web-monolith` (Estrategia-Testing §1.1), sin bajar pisos, por tanto sin ADR requerida. Conforme.
- Cobertura Domain 90/85 (N-22) y global 80/70 (N-21) del intake presentes y bloqueantes. Conforme.
- 10 quality gates del intake §17.P.8 materializados en Estrategia-Calidad §3 y replicados en README §2. Conforme.
- PENDIENTE (N-03, N-20, N-25, F-3 no automatizable, contrato 409 de rectificación) declarados como gaps con plan de remediación en Matriz-Cobertura §6 y Criterios-Validacion §6, no inventados. Conforme.

### 4.5 DoD vs DoR (no solapamiento)

La DoR de 06 filtra la entrada (valor explícito, trazabilidad a CU/NB, GWT presentes, estimación, dependencias, alcance). La DoD de 08 filtra el cierre (compila sin warnings, tests verdes, cobertura por capa, gates, SemVer, imagen firmada, SBOM, rollback). No hay criterios repetidos entre ambos documentos; ambos se declaran mutuamente disjuntos (DoR §9 encabezado; DoD §3). Conforme, sin solapamiento.

### 4.6 DoD referenciada, no redefinida

La DoD se declara fuente canónica (§3) y el Plan-Pruebas, la Estrategia-Calidad y los Criterios-Validacion la referencian sin redefinirla; el Mini-Plan de 07 la referencia (DoD §1 y §3, «Mini-Plan §9»). Conforme.

---

## 5. Hallazgos

### P2-01 — Umbral de cobertura de Infraestructura/Api discrepante entre documentos

- **Nivel:** P2 (medio, coherencia)
- **Archivos/secciones:** `Matriz-Cobertura-Pruebas-v1.0.md §5` vs `Estrategia-Testing-v1.0.md §2` y `Criterios-Validacion-v1.0.md §5`
- **Evidencia:** Estrategia-Testing §2 fija Infraestructura **70 % líneas / 60 % ramas** (citando regla §4.9) y agrupa Presentación (Web+Api) en 60/50. Criterios-Validacion §5 repite «Infraestructura 70/60, Presentación 60/50». En cambio, Matriz-Cobertura §5 fija `SAI.Service.Core.Infrastructure` en **80/70** y `SAI.Service.Core.Api` en **80/70** (separando Api de presentación), con la nota «el resto de la solución cae bajo el gate global 80/70». Los tres documentos son bloqueantes en el pipeline y dan números distintos para la misma capa.
- **Impacto:** ambigüedad sobre el piso real del gate de cobertura de Infraestructura y de Api. Los pisos son «piso, no techo» (la Matriz es más estricta, así que no viola ninguna regla de mínimo), pero un lector no puede resolver qué umbral rige el merge. La Matriz confunde el piso global agregado (80/70 sobre toda la solución, N-21) con el piso por capa de §4.9 (Infra 70/60).
- **Recomendación:** unificar la tabla de cobertura por capa en un único origen (preferiblemente Estrategia-Testing §2, que sigue §4.9) y que Matriz-Cobertura §5 y Criterios-Validacion §5 la citen textualmente. Decidir explícitamente si Api se clasifica como presentación (60/50) o se le exige 80/70, y reflejarlo en las tres tablas.

### P3-01 — Uso de negritas de énfasis borderline decorativas

- **Nivel:** P3 (bajo, estilo)
- **Archivos/secciones:** Estrategia-Calidad §1; Estrategia-Testing §1 y §1.2; varios encabezados de viñeta con `**término.**`
- **Evidencia:** negritas aplicadas a frases y términos para énfasis (p. ej. «**se niega a apagar el host…**», «**70 % unitarias / 25 % integración / 5 % end-to-end**»). No son emojis ni negritas puramente ornamentales de sección, pero rozan el límite de D2.
- **Recomendación:** reservar la negrita para etiquetas de lista o valores numéricos load-bearing; parafrasear el énfasis retórico. No bloqueante.

### P3-02 — Atribución de CU del flujo F-3 inconsistente

- **Nivel:** P3 (bajo, trazabilidad menor)
- **Archivos/secciones:** `Matriz-Cobertura-Pruebas-v1.0.md §6` (gap F-3) vs `Estrategia-Testing-v1.0.md §8`, `Plan-Pruebas-v1.0.md §1` y `Casos-Prueba §TC-36`
- **Evidencia:** el gap F-3 de Matriz-Cobertura lo rotula «UF-8/CU-05», mientras Estrategia-Testing, Plan-Pruebas y TC-36 lo asocian a la ventana de mantenimiento «CU-10». F-3 (ciclo físico de apagado + reencendido) abarca la ejecución de apagado (CU-05) y la verificación física (CU-10), pero la etiqueta debería ser uniforme.
- **Recomendación:** citar «CU-05 (apagado) / CU-10 (ventana de mantenimiento)» de forma consistente en los cuatro puntos.

### P3-03 — Framing del motor de extensión: puerto vs capa de add-ons

- **Nivel:** P3 (bajo, coherencia de redacción)
- **Archivos/secciones:** `README.md §1` (nota de aplicabilidad) vs `Guia-Testing-Extensibilidad-v1.0.md §1`
- **Evidencia:** el README describe el motor de extensión como «la capa de add-ons de dialecto (F-26, US-26)», mientras la Guía lo define primariamente como «el puerto del adaptador de conexión (`IAdaptadorConexion`)», con los add-ons de dialecto como subcapa diseñada no implementada. Ambos son el mismo mecanismo, pero conviene una descripción única.
- **Recomendación:** alinear el README con la Guía: el punto de extensión es el puerto del adaptador; los add-ons de dialecto son su subcapa diferida.

---

## 6. Confirmaciones solicitadas

1. **Los 10 documentos presentes:** Sí. Estrategia-Calidad, Estrategia-Testing, Plan-Pruebas, Matriz-Cobertura, Casos-Prueba-Referenciales, Criterios-Validacion, Definition-Of-Done, Guia-Testing-Extensibilidad, Matriz-Sensado-Deriva y README, todos `-v1.0.md`, sin sufijo de dominio.
2. **Matriz con 3 tablas + cobertura por capa:** Sí. CU↔Tests, NFR↔Tests, RN↔Tests, más invariantes↔Tests y cobertura por capa (con el P2-01 sobre el número de Infra/Api).
3. **Cada NFR numérico con test:** Sí. Los 22 NFR con objetivo numérico cerrado tienen test o mecanismo de medición; N-03, N-20, N-25 declarados PENDIENTE/gap (sin dimensionar en la fuente), no inventados.
4. **Matriz-Sensado-Deriva con fila por cada elemento de línea base:** Sí. 142 filas SD-01..SD-142 = 11 SUP + 19 CMP + 79 EST + 9 NAV + 24 DM; conteos coinciden exactamente con Linea-Base-Visual y Contrato-Datos-Maqueta de 03. Umbral menor+mayor por dimensión declarado. Sin elementos de línea base sin fila.
5. **DoD 4 capas sin solapar DoR:** Sí. US/BT/sprint/release con criterios mecánicos y excepciones; disjunta de la DoR de 06 (entrada vs cierre).

**Informe:** `/home/fernando/workspaces/workspace-dev/DEV/SAI.Service.Core/SDD/Docs/Audit/Fase-E-Calidad-Y-Pruebas-v1.0.md`

---

## 7. Veredicto final

**APROBADO — sin hallazgos P0 ni P1.** La categoría 08 de Sai-Service-Core satisface los criterios de aceptación §6 de `08-Rules-Calidad-Y-Pruebas.md` v1.3 y §2.3/§3 de `Deriva-Rules.md` v1.0. La trazabilidad CU/RN/NFR/invariantes está cerrada, la Matriz-Sensado-Deriva cubre uno a uno los 142 elementos de la línea de base, y la DoD no solapa la DoR. Se recomienda subsanar P2-01 (unificar los umbrales de cobertura por capa de Infraestructura/Api entre los tres documentos) antes de congelar los pisos del pipeline, y atender los P3 en la próxima revisión editorial. Ninguno de estos hallazgos bloquea el cierre de la Fase E.
