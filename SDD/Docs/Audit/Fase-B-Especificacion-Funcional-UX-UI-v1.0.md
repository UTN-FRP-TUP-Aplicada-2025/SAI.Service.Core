# Auditoría Fase B — Especificación Funcional (02) y UX/UI/DX (03)

| Campo | Valor |
| --- | --- |
| Solución | SAI.Service.Core |
| Fase auditada | B (categorías 02-Especificacion-Funcional y 03-UX-UI-DX) |
| project_type | web-monolith (caso degenerado, layout aplanado) |
| Alcance | 02: índice, README, 12 CU, 13 RN, modelo conceptual, 9 RC. 03: Experiencia-De-Uso, 8 wireframes, Glosario-UX, README |
| Omisión legítima | 04-Prompts-AI (usa_llm=false); Fase B2 aún no ejecutada (maqueta no aprobada) |
| Reglas aplicadas | 02-Rules-Especificacion-Funcional v1.2; 03-Rules-UX-UI-DX v1.6 |
| Fuente de verdad | SOLUTION-INTAKE-Sai-Service-Core-v1.0.md |
| Auditor | Auditor independiente SDD (Arquitecto de Soluciones + QA Senior) |
| Fecha | 2026-07-20 |

---

## 1. Resumen ejecutivo

Los entregables de las categorías 02 y 03 son de **calidad interna alta**: la estructura obligatoria está completa en las 35 piezas revisadas, la trazabilidad NB↔CU es bidireccionalmente completa a nivel de la categoría 02, no hay emojis, no hay stack ni protocolos del dominio fuente en el cuerpo funcional de 02, no hay tipos físicos en el modelo conceptual, la nomenclatura y el encoding cumplen D1-D8, y no hay IDs duplicados ni versionado paralelo.

El bloqueo no viene de defectos internos de 02/03 sino de **una inconsistencia de referencias cruzadas con el catálogo 01**: las 8 NB declaran en su §7 "Trazabilidad a CU" identificadores concretos (CU-11, CU-12, CU-08, etc.) que en la numeración que 02 realmente adoptó designan casos de uso **distintos**. La divergencia está reconocida en prosa por el índice de 02 (§6) pero no reconciliada con una tabla de equivalencia, de modo que cada §7 de las NB resuelve a un CU equivocado.

| Nivel | Cantidad |
| --- | --- |
| P0 (bloqueante, rechaza) | 0 |
| P1 (alto, bloquea hasta corrección) | 1 |
| P2 (medio) | 3 |
| P3 (bajo) | 2 |

**Veredicto: APROBADO CON OBSERVACIONES.** No hay hallazgos P0: ningún documento obligatorio falta, no hay NB huérfana ni CU huérfano, no hay violación de vocabulario/stack en el cuerpo funcional. El único P1 es una reconciliación de referencias cross-doc (01→02) que debe cerrarse antes de que la cadena alimente 05/06. Los P2 y P3 son mejoras recomendadas no bloqueantes.

---

## 2. Matriz D1-D8 por familia de documento

| Familia | Idioma rioplatense / sin emojis | UTF-8 / LF | Filename Título-Con-Guiones -v1.0.md | Sin stack en cuerpo | Sin protocolo dominio fuente en cuerpo | Sin tipos físicos | Resultado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 02 índice + README | OK (los `→` de la matriz NB→CU son flechas tipográficas, no emojis) | OK | OK | OK | OK | n/a | Conforme |
| 12 CU | OK | OK | OK | OK (0 menciones) | OK (0 menciones de NUT/Megatec/Voltronic/upsd/wtmp/ups.status) | n/a | Conforme |
| 13 RN | OK | OK | OK | OK | OK | n/a | Conforme |
| Modelo conceptual | OK | OK | OK | OK | OK | OK (ninguna: `int-20260905-001` es un id de ejemplo, no un tipo) | Conforme |
| 9 RC | OK | OK | OK | OK | OK | OK | Conforme |
| 03 Experiencia + wireframes | OK | OK | OK | Parcial: `Blazor/MudBlazor` solo en fila de trazabilidad (permitido); pero `NUT`, `SQLite`, `Docker`, `upsd`, `TLS/reverse proxy` aparecen en cuerpo (ver H-2) | Parcial (ver H-2) | n/a | Observado (P2) |
| 03 Glosario + README | OK | OK | OK | OK | OK | n/a | Conforme |

Verificaciones automáticas ejecutadas: barrido de rangos de emoji (solo `→` U+2192), barrido de términos de stack (.NET/Blazor/EF/SQLite/C#), barrido de protocolos del dominio fuente (NUT/Megatec/Voltronic/ViewPower/upsmon/upssched/nutdrv/wtmp/upsd) — **0 coincidencias en cuerpos de 02**; barrido de tipos físicos (varchar/int/decimal/datetime/guid…) en Modelo-Datos — **0 coincidencias reales**; barrido de CRLF — **0**; patrón de versión `-v` — **100 %**.

---

## 3. Matriz de estructura obligatoria

### 3.1 Categoría 02 (§6 de 02-Rules, 12 ítems)

| Criterio §6 | Exigido web-monolith | Observado | Cumple |
| --- | --- | --- | --- |
| Índice maestro con matriz NB→CU→RN→US | Sí | `Especificacion-Funcional-v1.0.md` §2 | Sí |
| Mínimo de CU | ≥ 8 | 12 | Sí |
| 11 secciones obligatorias por CU | Sí | 12 secciones/CU (11 obligatorias + §13 opcional multiusuario) en los 12 | Sí |
| ≥ 3 Given/When/Then con valores por CU | Sí | 4–5 filas CA- por CU (mínimo 4) | Sí |
| 7 secciones por RN + CU afectados explícitos | Sí | 7 secciones y §5 "CU afectados" presente en las 13 | Sí |
| Modelo conceptual con diagrama | Sí | `Modelo-Conceptual-v1.0.md`, 9 secciones, `erDiagram` Mermaid embebido | Sí |
| RC si modelo > 10 entidades (6 secciones) | Sí (27 entidades) | 9 RC, 6 secciones cada una | Sí |
| Patrón `-vX.Y.md` / slug sin mayúsc.-acentos-espacios | Sí | Todos conformes | Sí |
| Sin versiones paralelas en carpeta principal | Sí | Sin `_legacy/`, sin duplicados | Sí |
| Sin stacks/productos/protocolos del dominio fuente | Sí | 0 en cuerpos de CU/RN/RC/modelo | Sí |
| README de sección | Recomendado | Presente | Sí |
| IDs contiguos y sin duplicar | Sí | CU-01..12, RN-01..13, RC-01..09; sin duplicados | Sí |

### 3.2 Categoría 03 (§6 de 03-Rules, 14 ítems)

| Criterio §6 | Exigido | Observado | Cumple |
| --- | --- | --- | --- |
| Variante declarada y coherente con D8 | UX/UI | Declarada en cabeceras | Sí |
| Experiencia-De-Uso con 11 secciones | Sí | 11 secciones exactas | Sí |
| ≥ 4 wireframes con 9 secciones | Sí (mínimo 4) | 8 wireframes, 9 secciones cada uno | Sí |
| Estados vacío/cargando/con datos/error por wireframe | Sí | Los 4 estados presentes en la tabla Estados de los 8 | Sí |
| WCAG 2.2 AA como piso | Sí | Declarado en Experiencia §5; notas de accesibilidad en los 8 wireframes | Sí |
| Trazabilidad upstream/downstream por artefacto | Sí | CU origen + catálogo de diseño + US/tests en todos | Sí |
| Nombre canónico de superficie (requiere_maqueta true) | Sí | Declarado en §1 de los 8 wireframes | Sí |
| Catálogo de diseño declarado | Sí | Design-Rules-Web-Generico + Blazor-MudBlazor en fila de trazabilidad | Sí |
| Glosario sin duplicar 02 con semántica distinta | Sí | Glosario-UX presente, sin contradicción detectada | Sí |
| Sin stack/protocolo del dominio fuente en cuerpo | Sí | **Parcial** (H-2) | Observado |
| Artefactos Fase B2 (Linea-Base/Contrato-Datos/Bitacora) | Solo con maqueta aprobada | Ausentes — legítimo: B2 no ejecutada | n/a |

---

## 4. Coherencia interna

- **IDs y numeración:** CU-01..CU-12, RN-01..RN-13, RC-01..RC-09 contiguos, sin duplicados (verificado sobre filenames y cabeceras).
- **Modelo ↔ RC:** el modelo (§5) enlaza las 9 RC; las RC declaran entidades involucradas y RN/CU que las justifican. Coherente.
- **Índice §4 RN→CU vs "CU afectados" de cada RN:** divergencias menores (el archivo de la RN suele listar más CU que la columna del índice; p. ej. RN-01 agrega CU-01, RN-13 agrega CU-08 y CU-12). Ver H-5 (P3).
- **Colocación de sección opcional en CU:** la sección web-monolith §13 "Interacción multiusuario y concurrencia" se insertó como §11, desplazando "Control de cambios" a §12. Contenido completo; numeración no canónica. Ver H-4 (P3).

---

## 5. Coherencia cross-doc y trazabilidad

### 5.1 Cobertura NB↔CU (bidireccional, según la matriz real de 02)

| NB | Necesidad | CU que la implementan en 02 (verificado en cuerpo del CU) | Cobertura |
| --- | --- | --- | --- |
| NB-01 | Apagado ordenado y reencendido garantizado | CU-05, CU-10 | Cubierta |
| NB-02 | Monitoreo en vivo y alertas | CU-04, CU-02, CU-06 | Cubierta |
| NB-03 | Historia trazable con procedencia | CU-06, CU-02, CU-04 | Cubierta |
| NB-04 | Ciclo de vida de los equipos | CU-02, CU-08, CU-09, CU-12 | Cubierta |
| NB-05 | Seguridad operativa y bloqueo por verificación | CU-01, CU-05, CU-10, CU-04 | Cubierta |
| NB-06 | Evaluación de salud de baterías | CU-07, CU-08, CU-12 | Cubierta |
| NB-07 | Configuración de políticas de apagado | CU-03, CU-05 | Cubierta |
| NB-08 | Ingesta automatizada de intervenciones | CU-11, CU-08 | Cubierta |

Cada uno de los 12 CU declara al menos una NB en su tabla de trazabilidad (verificado por barrido): CU-01→NB-05; CU-02→NB-02/03/04/05; CU-03→NB-07; CU-04→NB-02/03/05; CU-05→NB-01/05; CU-06→NB-02/03; CU-07→NB-06; CU-08→NB-04/06; CU-09→NB-04; CU-10→NB-01/05; CU-11→NB-08; CU-12→NB-04/06. **No hay NB huérfana ni CU huérfano.** (No se emite P0.)

### 5.2 Consistencia de referencias forward NB→CU (§7 de las NB)

Esta es la falla bloqueante. El catálogo 01 anticipó ~19 CU de grano fino (CU-01..CU-19) y 02 produjo 12 CU con **otra numeración**, alineada 1:1 a los flujos UF-1..UF-10 más autenticación. Las NB **no fueron actualizadas**: su §7 sigue citando identificadores que hoy colisionan. Ejemplo verificado en NB-01 §7 "Trazabilidad a CU":

| NB-01 apunta a… | Significado en el catálogo 01 | Lo que ese ID significa en 02 |
| --- | --- | --- |
| CU-11 | "Ejecución de la secuencia de apagado ordenado" | **CU-11 = Ingesta automatizada de intervenciones** |
| CU-12 | "Reencendido y verificación continua por evidencia" | **CU-12 = Informe de período y comparación de marcas** |
| CU-08 | "Ventana de mantenimiento y verificación de supuestos" | **CU-08 = Recambio de batería y ficha de vida útil** |

El apagado que NB-01 quiere referenciar es en 02 el CU-05, y la ventana de mantenimiento es CU-10. El fenómeno se repite en las 8 NB (§2 y §5 del índice de NB, y §7 de cada archivo NB). Ver H-1 (P1).

### 5.3 Autenticación (F-15) sin NB dedicada

F-15 "Autenticación mínima de administrador único" es Must Have (intake §4) pero el catálogo 01 no tiene NB de control de acceso. CU-01 se traza a NB-05 "por proximidad" (declarado en índice 02 §6 y en el cuerpo de CU-01). NB-05 §7 no prevé autenticación (prevé CU-17/CU-18/CU-08 del esquema viejo). CU-01 **no queda huérfano** (declara NB-05), por lo que no es P0; pero la ancla es semánticamente artificial. Ver H-3 (P2).

### 5.4 CU ↔ wireframe

| Wireframe | CU origen declarado | CU existe en 02 |
| --- | --- | --- |
| Acceso-Login | CU-01 | Sí |
| Alta-Inicial-Administrador | CU-01 | Sí |
| Alta-De-Equipos | CU-02 | Sí |
| Configuracion-De-Politicas | CU-03 | Sí |
| Panel-Estado-En-Vivo | CU-04, CU-05 | Sí |
| Panel-De-Verificaciones | CU-05, CU-10 | Sí |
| Historicos-Y-Graficas | CU-06 | Sí |
| Prueba-De-Bateria | CU-07 | Sí |

Todos los CU citados por los wireframes existen en 02. CU-05 (actor Planificador interno) y CU-11 (actor Sistema externo) correctamente **no** requieren wireframe. Quedan **sin wireframe** tres CU con interacción humana del administrador: **CU-08** (recambio de batería), **CU-09** (reparación/sustitución del SAI) y **CU-12** (informe de período y comparación). Se supera el mínimo de 4, pero no se cubre la totalidad de los CU con interacción humana relevante. Ver H-6 (P2).

---

## 6. Hallazgos enumerados

### H-1 · P1 — Referencias forward NB→CU colisionan con la numeración real de 02

- **Archivos:** `01-Necesidades-Negocio/Necesidades-Negocio-v1.0.md` §2 y §5; `01-.../Necesidades-De-Negocio/NB-01..NB-08-*.md` §7; contrastados contra `02-.../Especificacion-Funcional-v1.0.md` §3 y los 12 CU.
- **Sección:** §7 "Trazabilidad a CU" de cada NB / §2 matriz y §3 tabla de CU de 02.
- **Evidencia:** NB-01 §7 apunta a CU-11, CU-12, CU-08 como "apagado", "reencendido" y "ventana de mantenimiento"; en 02 esos IDs son "Ingesta automatizada", "Informe de período" y "Recambio de batería". El apagado real es CU-05 y la ventana es CU-10. El índice de 02 §6 reconoce la divergencia de granularidad (19 CU previstos → 12 CU adoptados) en prosa, pero no publica una tabla de equivalencia y las NB no se re-emitieron.
- **Impacto:** cualquier consumidor downstream (05, 06, 08) que siga las referencias de las NB llega a un CU equivocado; la cadena D6 queda rota a nivel de resolución de identificadores aunque la cobertura conceptual esté completa.
- **Recomendación:** reconciliar en una sola operación. Opción A (mínima): agregar al índice de 02 §6 una tabla de equivalencia "CU previsto (01) → CU vigente (02)" con los 19→12 mapeos. Opción B (canónica): re-emitir el catálogo de NB a v1.2 corrigiendo el §7 de cada NB para citar los CU vigentes de 02 (CU-05, CU-10, etc.), moviendo la v1.1 a `_legacy/`. Preferible B para que las NB no sigan mintiendo sobre sus CU.

### H-2 · P2 — Nombres de stack y de protocolo del dominio fuente en el cuerpo de 03

- **Archivos:** `03-.../Experiencia-De-Uso-v1.0.md` (líneas 17 y 61); `03-.../Wireframes-Configuracion-De-Politicas-v1.0.md` (línea 108).
- **Sección:** cuerpo (audiencia y frontera aplicación/entorno), fuera de la fila de trazabilidad "catálogo de diseño aplicado".
- **Evidencia:** aparecen `NUT`, `SQLite` (cadena de conexión), `Docker` (`shutdown-timeout`), `upsd`, `TLS/reverse proxy` en la narrativa y en la tabla de frontera de configuración. El criterio D1-D8 admite stack solo en la fila de trazabilidad; `Blazor/MudBlazor` sí están correctamente confinados allí.
- **Mitigación:** la extensión `Design-Rules-Config-Esquema` obliga a declarar la frontera aplicación/entorno y a nombrar lo que la superficie **no** gobierna, lo que induce a citar esos ítems. No es una fuga gratuita.
- **Recomendación:** genericizar en el cuerpo ("cadena de conexión de la base", "ubicación del adaptador de conexión", "tiempo de gracia del orquestador de contenedores", "terminación TLS del proxy de borde") y dejar los nombres concretos únicamente en la fila de catálogo de diseño / en 09-Entorno.

### H-3 · P2 — Trazabilidad de autenticación (F-15) anclada por proximidad

- **Archivos:** `02-.../Casos-De-Uso/CU-01-Autenticacion-Del-Administrador-v1.0.md` §9; `02-.../Especificacion-Funcional-v1.0.md` §6.
- **Evidencia:** F-15 es Must Have; no existe NB de control de acceso; CU-01 se traza a NB-05 "por proximidad"; NB-05 §7 no prevé autenticación. CU-01 no es huérfano (por eso no es P0), pero la ancla es artificial y está declarada como pendiente de decisión del orquestador.
- **Recomendación:** resolver la ambigüedad declarada: (a) crear NB-09 "Control de acceso del administrador único" en 01 y re-trazar CU-01, o (b) ratificar formalmente la autenticación como habilitador transversal sin NB propia y anotarlo como tal en la matriz de 02 (no como traza NB-05). Elegir una y cerrar el pendiente.

### H-4 · P3 — Sección opcional §13 colocada como §11 en los CU

- **Archivos:** los 12 CU (verificado en CU-01: §11 = "Interacción multiusuario y concurrencia", §12 = "Control de cambios").
- **Evidencia:** la sección opcional web-monolith (§13 según 02-Rules §4.3) se numeró como §11, desplazando la obligatoria "Control de cambios" a §12. Todo el contenido obligatorio está presente; solo la numeración no es canónica.
- **Recomendación:** renumerar la opcional como §13 (o §12) y mantener "Control de cambios" como cierre, para no desplazar la numeración de las obligatorias.

### H-5 · P3 — Divergencias menores índice §4 (RN→CU) vs "CU afectados" de cada RN

- **Archivos:** `02-.../Especificacion-Funcional-v1.0.md` §4 vs `Reglas-De-Negocio/RN-*.md` §5.
- **Evidencia:** el archivo de la RN suele listar más CU que la columna del índice (RN-01 agrega CU-01; RN-13 agrega CU-08 y CU-12).
- **Recomendación:** unificar ambas listas (preferible tomar la del archivo RN como fuente y reflejarla en el índice).

### H-6 · P2 — CU con interacción humana sin wireframe (CU-08, CU-09, CU-12)

- **Archivos:** carpeta `03-UX-UI-DX/` (conjunto de wireframes) vs CU-08/09/12 de 02.
- **Evidencia:** CU-08 (recambio de batería), CU-09 (reparación/sustitución del SAI) y CU-12 (informe de período y comparación) tienen actor primario Administrador y flujo con formulario/lectura, pero no hay wireframe que los origine. Se supera el mínimo de 4, pero la guía §5.1 de 03-Rules pide que cada CU con interacción humana significativa mapee a un artefacto de 03.
- **Recomendación:** agregar wireframes para las tres superficies (registro de intervención de batería, registro de reparación/sustitución con cobertura suplente, informe de período), o justificar explícitamente en Experiencia-De-Uso por qué se difieren.

---

## 7. Veredicto final

**APROBADO CON OBSERVACIONES.**

Fundamento: no hay hallazgos P0 —ningún documento obligatorio falta, no hay NB huérfana ni CU huérfano, no hay stack ni protocolo del dominio fuente en el cuerpo funcional de 02, no hay tipos físicos en el modelo, la nomenclatura/encoding cumplen D1-D8 y la estructura obligatoria está completa en 02 y en 03. La calidad interna de ambas categorías es alta.

**Condición bloqueante para avanzar a 05/06 (cerrar el P1 H-1):**
1. Reconciliar las referencias forward NB→CU: publicar la tabla de equivalencia 19→12 en 02 §6 o, preferentemente, re-emitir el catálogo 01 a v1.2 corrigiendo el §7 de cada NB para citar los CU vigentes de 02, con la versión anterior a `_legacy/`.

**Recomendaciones no bloqueantes (P2/P3):** resolver el ancla de autenticación (H-3), genericizar los nombres de stack/protocolo en el cuerpo de 03 (H-2), agregar los wireframes de CU-08/09/12 o justificar su diferimiento (H-6), renumerar la sección opcional de los CU (H-4) y unificar las listas RN→CU (H-5).

Nota de alcance: 04-Prompts-AI se omite legítimamente (usa_llm=false) y los tres artefactos de Fase B2 (`Linea-Base-Visual`, `Contrato-Datos-Maqueta`, `Bitacora-Validacion-Maqueta`) están ausentes de forma legítima porque la maqueta aún no fue aprobada; los wireframes ya declaran nombre canónico estable, satisfaciendo el criterio pre-B2 de `requiere_maqueta == true`.
