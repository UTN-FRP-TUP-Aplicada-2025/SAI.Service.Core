# Auditoría Fase C — 05 Arquitectura Técnica · Sai-Service-Core

**Fase:** C (categoría 05 — Arquitectura Técnica)
**Alcance:** SDD/Docs/05-Arquitectura-Tecnica/ de SAI.Service.Core. Proyecto `web-monolith`, caso degenerado de un único proyecto (layout aplanado, sin `Solucion/`).
**Auditor:** Auditor independiente SDD (Arquitecto de Soluciones + QA Senior; no participó de la generación)
**Fecha:** 2026-07-20
**Reglas aplicadas:** 05-Rules-Arquitectura-Tecnica.md v1.2 (§4.2, §4.3, §4.4, §4.5, §6); matriz D1-D8; trazabilidad §3.4.

---

## 1. Resumen ejecutivo

Los entregables de la categoría 05 están **completos, bien estructurados y trazables**. Se produjeron los siete artefactos maestros (documento de arquitectura, índice de decisiones, modelo lógico, flujo de ejecución, contrato REST, extensibilidad, README) y las 22 ADR individuales exigidas, todas con las diez secciones obligatorias, estado declarado y motivación upstream. La estructura obligatoria del §6 de 05-Rules se cumple sin excepción. No se detectaron rupturas de trazabilidad, documentos obligatorios omitidos, ADR consolidadas, ADR sin estado, ADR huérfanas ni tablas lógicas sin origen conceptual. Se verificó que `Solucion/` no existe (omisión correcta del caso degenerado).

Los únicos hallazgos son **inconsistencias de metadatos (nivel P2)** entre artefactos de la misma sección: discrepancia de `Autor` (el índice y las 22 ADR firman «Arquitecto de Software Senior (AG-05)»; los cinco documentos maestros restantes firman «Orquestador SDD (AG-05)») y discrepancia de `Estado` (cuatro maestros en «Vigente», el modelo lógico en «Borrador», el índice en «Aceptado»). No son bloqueantes: no afectan contenido, trazabilidad ni estructura.

### Conteo por nivel

| Nivel | Cantidad |
| --- | --- |
| P0 (bloqueante, rechaza) | 0 |
| P1 (alto, bloquea) | 0 |
| P2 (medio) | 2 |
| P3 (bajo) | 3 |

### Veredicto

**APROBADO CON OBSERVACIONES.** Ningún hallazgo bloqueante. Se recomienda unificar los metadatos de `Autor` y `Estado` de la sección antes de congelar la versión.

---

## 2. Matriz D1-D8 (por familia de artefactos)

| Criterio | ADR (22) | Maestros (arquitectura, flujo, contratos, extensibilidad, índice) | Modelo lógico | Contratos REST | README | Resultado |
| --- | --- | --- | --- | --- | --- | --- |
| Idioma rioplatense técnico | OK | OK | OK | OK | OK | Cumple |
| Sin emojis | Sin emojis (solo `→ ⇒ ↔ ⟷`, notación técnica) | Sin emojis | Sin emojis | Sin emojis | Sin emojis | Cumple |
| Sin negritas decorativas | Negritas solo de énfasis técnico | Idem | Idem | Idem | Idem | Cumple |
| Codificación UTF-8 | OK | OK | OK | OK | OK | Cumple |
| Fin de línea LF | LF | LF | LF | LF | LF | Cumple |
| Filename `Título-Con-Guiones-v1.0.md` | 22/22 correctos (guion medio `-v`) | Correctos | Correcto | Correcto | Correcto | Cumple |
| Terminología «equipos» (no «parque») | OK | OK | OK | OK | Declarada | Cumple |
| Terminología «contraseña» (login) / «secreto» (runtime) | «secreto» solo runtime (credenciales `upsd`, clave de certificado) | «contraseñas y secretos» de runtime; login = «contraseña» | — | — | Declarada | Cumple |

Notas de evidencia:
- Emojis: barrido de bloques pictográficos/dingbats/emoticons = 0 coincidencias. Los caracteres `→ ⇒ ↔ ⟷` son notación de dirección de dependencias y flujo, uso técnico legítimo.
- «secreto»: las tres apariciones (ADR-19, ADR-20, §7 de Arquitectura-Solucion) refieren a gestión de secretos de runtime (credenciales de `upsd`, clave del certificado TLS), correcto según el alcance. La contraseña de login se nombra «contraseña» (ADR-16).
- Stack: NUT, SQLite, EF Core, .NET 10, Blazor, MudBlazor, ASP.NET Core Identity, Docker se nombran como stack legítimo del proyecto (mandato §17 y pre-ADR del intake). No hay filtración del dominio fuente del bootstrap (impresora térmica / ESC-POS / DSL / Motor): barrido = 0 coincidencias.

---

## 3. Matriz de estructura obligatoria (§6 de 05-Rules)

| Ítem del §6 | Estado | Evidencia |
| --- | --- | --- |
| `Arquitectura-Solucion-v1.0.md` con 4 vistas mínimas | Cumple | §3 lógica, §4 procesos, §5 despliegue, §6 datos |
| Secciones §1-§10 del §4.2 | Cumple | §1 Objetivo … §10 Trazabilidad + Control de cambios |
| `Decisiones-Arquitectura-v1.0.md` indexa ADR con estado y fecha | Cumple | Tabla índice con ID/título/categoría/estado/fecha (22 filas) |
| ≥3 ADR individuales bajo `Adrs/`, 10 secciones c/u | Cumple | 22 ADR; cada una con §1 Contexto … §10 Control de cambios (verificado 10/10) |
| Cada ADR con Estado declarado | Cumple | 18 «Aceptado», 4 «Propuesto» (ADR-19..22) |
| `Modelo-Datos-Logico` con migración inicial referenciada | Cumple | §5: `20260720000001_InitialCreate`, `20260720000002_AddIdentitySchema` (EF Core) |
| Modelo lógico traza al modelo conceptual de 02 | Cumple | §7: 27 entidades conceptuales cubiertas; sin tabla sin origen |
| `Contratos-REST` con esquema / errores / versionado | Cumple | §2 OpenAPI 3.1 inline; §5 problem+json RFC 7807; §6 versionado en ruta + SemVer |
| Estilo con justificación vs ≥2 alternativas | Cumple | §2: descarta «capas tradicionales» y «event-driven + CQRS» (ADR-15) |
| Cada NFR con objetivo numérico y mecanismo | Cumple | §8: 25 NFR (N-01..N-25) con objetivo y mecanismo; PENDIENTE marcados |
| Trazabilidad NFR↔arquitectura↔ADR en ≥1 tabla | Cumple | §8 columna «ADR relacionada»; §10 tabla CU/RN/ADR |
| Ningún ADR consolidado (§3.3, archivos individuales) | Cumple | 22 archivos independientes; el índice no contiene cuerpos |
| README de la sección presente | Cumple (recomendado) | `README.md` con índice navegable |
| `Solucion/` omitido sin dejar huérfano | Cumple | No existe `Solucion/`; caso degenerado documentado en README |

Documentos opcionales presentes con justificación explícita: `Flujo-Ejecucion-v1.0.md` (§1 justifica inclusión: camino crítico irreversible, no CRUD) y `Extensibilidad-v1.0.md` (puerto del adaptador como único punto de extensión, ADR-02/22).

---

## 4. Coherencia cross-doc y trazabilidad

### 4.1 Índice ↔ archivos ADR

- Numeración contigua ADR-01 … ADR-22, sin huecos ni duplicados.
- Los 22 IDs, títulos y estados del índice `Decisiones-Arquitectura` coinciden con los archivos reales (títulos H1 verificados uno a uno; el índice ocasionalmente abrevia la cola del título —p. ej. ADR-09, ADR-14— sin contradicción semántica).
- Estados del índice (18 Aceptado / 4 Propuesto) coinciden con la cabecera de cada archivo.
- README replica el índice con títulos abreviados; consistente con el índice autoritativo.

### 4.2 Referencias por ID a ADR

- Todas las referencias `ADR-XX` en los siete documentos resuelven dentro del rango ADR-01..ADR-22. No hay ADR citada inexistente.

### 4.3 Trazabilidad ADR ↔ upstream

- Cada ADR referencia su motivación upstream en §1 (Contexto) y §9 (Referencias): CU-01..12, RN-01..13, RC-01..09, NB-01..08, todos dentro del inventario real de 02 (12 CU, 13 RN, 9 RC) y 01 (8 NB). Cada ADR presenta 3 alternativas evaluadas (§4, ≥2 exigidas) y métricas de validación (§8).
- El índice aporta además una tabla «Trazabilidad de origen» (PA / obligatorio D8 / Sprint 0) por ADR.
- ADR-15 (estilo) se ancla en «estilo y separación de capas obligatorios de web-monolith» + riesgo R-10; motivación explícita documentada, no huérfana (ver P3-01).

### 4.4 Trazabilidad modelo lógico ↔ conceptual

- 27 entidades conceptuales de 02 (1.1..1.27) cubiertas: 22 como tablas (incluida `UnidadFisica` TPH que absorbe supertipo + 3 especializaciones, y `AspNetUsers`), `Valor con Origen` y `Dinero` como owned types, más 2 tablas puente N-N. Ninguna tabla sin origen conceptual.
- La `FichaVidaUtil` se rotula §1.26 en el modelo lógico y se reconcilia explícitamente con «1.24 en 02»; offset de numeración anotado, no defecto.

### 4.5 Contratos REST ↔ CU-11 y RN

- Contrato traza a CU-11 (ingesta de intervenciones) y a RN-07, RN-08, RN-09, RN-12; cuatro caminos 201/200/409/422; NFR N-23/N-24; ADR-17 (gobierna) y ADR-21 (Propuesto, rectificación del 409).

### 4.6 PENDIENTE marcados, no inventados

- N-03 (resto del apagado del SO), N-20 (tamaño SQLite tras agregación, R-07) y N-25 (SLO de disponibilidad) están marcados **PENDIENTE** con su motivo; ADR-19 (ubicación NUT), ADR-20 (TLS), ADR-21 (contrato 409) y ADR-22 (contrato adaptador) en estado **Propuesto** como decisiones abiertas de Sprint 0. Coherente con §11/§17 del intake; no hay valores inventados.

---

## 5. Hallazgos enumerados

### P2-01 — Discrepancia de `Autor` entre artefactos de la misma sección

- **Nivel:** P2 (medio, metadatos)
- **Archivos/sección:** cabeceras de `Decisiones-Arquitectura-v1.0.md` y las 22 `Adrs/ADR-*.md` (firman «Arquitecto de Software Senior (AG-05)») frente a `Arquitectura-Solucion-v1.0.md`, `Modelo-Datos-Logico-v1.0.md`, `Contratos-REST-v1.0.md`, `Flujo-Ejecucion-v1.0.md`, `Extensibilidad-v1.0.md` y `README.md` (firman «Orquestador SDD (AG-05)»).
- **Evidencia:** dos valores de `**Autor:**` conviven en la misma categoría 05 sin criterio declarado.
- **Recomendación:** unificar el campo `Autor` de todos los artefactos de la sección a un único rol (el titular del artefacto es AG-05 según §1.3 de las reglas), o declarar explícitamente por qué el índice/ADR firman distinto de los maestros.

### P2-02 — Discrepancia de `Estado` entre artefactos de la misma sección

- **Nivel:** P2 (medio, metadatos)
- **Archivos/sección:** cabeceras `**Estado:**` de los artefactos maestros.
- **Evidencia:** `Arquitectura-Solucion`, `Contratos-REST`, `Flujo-Ejecucion` y `Extensibilidad` en «Vigente»; `Modelo-Datos-Logico` en «Borrador»; `Decisiones-Arquitectura` (índice) en «Aceptado»; `README` sin campo `Estado`. Mismos proyecto y fecha (2026-07-20), tres vocabularios de estado distintos y un artefacto (el modelo lógico) marcado como borrador mientras sus hermanos están vigentes.
- **Recomendación:** homogeneizar el vocabulario de estado de los documentos maestros (p. ej. todos «Vigente») y elevar `Modelo-Datos-Logico` de «Borrador» si en efecto está cerrado, o justificar por qué permanece en borrador respecto de la sección.

### P3-01 — Ancla upstream de ADR-15 apoyada en riesgo y obligación D8

- **Nivel:** P3 (bajo)
- **Archivo/sección:** `Adrs/ADR-15-...md`, §9 Referencias.
- **Evidencia:** ADR-15 (estilo/separación de capas) motiva por «obligatorios D8» + riesgo R-10, sin un ID CU/RN/NB/NFR directo en §9 (a diferencia del resto). La motivación es explícita y el índice documenta su origen; no es huérfana, pero es el ancla más débil del conjunto.
- **Recomendación:** opcional, añadir en §9 la referencia a los NFR de cobertura (N-21/N-22) que la decisión materializa, ya presentes en §8 del maestro.

### P3-02 — NFR N-25 sin ADR asociada

- **Nivel:** P3 (bajo)
- **Archivo/sección:** `Arquitectura-Solucion-v1.0.md`, §8, fila N-25.
- **Evidencia:** N-25 (SLO de disponibilidad) tiene objetivo propuesto y mecanismo, pero la columna «ADR relacionada» es «—». Está correctamente marcado PENDIENTE/[derivado].
- **Recomendación:** al cerrar el SLO en 08, generar la ADR correspondiente y completar la referencia.

### P3-03 — Títulos abreviados en README frente al índice autoritativo

- **Nivel:** P3 (bajo, estilo)
- **Archivo/sección:** `README.md`, tabla de ADR.
- **Evidencia:** README abrevia varios títulos (ADR-01, ADR-05, ADR-09, ADR-14) respecto del índice y del H1 de los archivos. Sin contradicción semántica.
- **Recomendación:** opcional, reproducir el título completo del índice para navegación exacta.

---

## 6. Confirmaciones solicitadas

1. **22 ADR con estado y 10 secciones:** confirmado. 22 archivos individuales, cada uno con las diez secciones del §4.3 y `Estado` declarado (18 Aceptado, 4 Propuesto). Cada ADR con 3 alternativas (≥2) y métricas de validación.
2. **Índice coincide con archivos:** confirmado. IDs, títulos y estados del índice `Decisiones-Arquitectura` concuerdan con los archivos reales; referencias `ADR-XX` sin cabos sueltos; numeración contigua 01..22.
3. **Modelo lógico traza a conceptual sin huérfanos:** confirmado. 27 entidades conceptuales de 02 cubiertas; ninguna tabla física sin origen conceptual; migración inicial `InitialCreate` referenciada.
4. **Estilo justificado vs 2 alternativas:** confirmado. §2 del maestro y ADR-15 descartan «capas tradicionales» y «event-driven + CQRS» con criterios de decisión.
5. **NFR numéricos:** confirmado. 25 NFR con objetivo numérico y mecanismo de medición; PENDIENTE (N-03, N-20, N-25) marcados como tales, no inventados; trazabilidad NFR↔ADR en la tabla del §8.

---

## 7. Veredicto final

**APROBADO CON OBSERVACIONES.**

Cero hallazgos P0/P1. La categoría 05 cumple la estructura obligatoria, las reglas D1-D8 y la trazabilidad completa (ADR↔upstream, modelo↔conceptual, contratos↔CU/RN, índice↔archivos). Las dos observaciones P2 son inconsistencias de metadatos (`Autor` y `Estado`) que conviene subsanar antes de congelar la versión, pero no bloquean el avance a las categorías 06-11.
