# Auditoría Fase F — DevOps (09) y omisión de Developer Guide (10) — Sai-Service-Core

**Fase auditada:** F (categoría 09-Devops generada + omisión registrada de 10-Developer-Guide)
**Proyecto:** Sai-Service-Core (`web-monolith`, `redistribuible: false`, caso degenerado: solución de un solo proyecto, layout aplanado sin `Solucion/`)
**Auditor:** Auditor independiente SDD
**Fecha:** 2026-07-21
**Rol:** Arquitecto de Soluciones + QA Senior, sin participación en la generación

---

## 1. Resumen ejecutivo

Se auditaron los cinco documentos obligatorios de la categoría 09 más su README, los dos ADR nuevos de la Fase F (ADR-23 y ADR-24) y el índice de decisiones actualizado, contra los criterios §6 de `09-Rules-Devops.md` (nivel proyecto), el §4.3 de `05-Rules` (secciones del ADR), el §2.2 de `10-Rules` (admisibilidad de la omisión) y las directrices de estilo D1–D8.

Todos los entregables obligatorios están presentes y bien formados. El pipeline ejecuta la DoD de 08 y los quality gates como gates, sin redefinirlos. Los 22 NFR con dato numérico tienen gate; los 3 PENDIENTE (N-03, N-20, N-25) se declaran como tales con vía de cierre, no se inventan. La desviación de ambientes DEV/PROD está justificada y registrada como ADR-24 (Aceptado), no como propuesta. La omisión de la categoría 10 está registrada como ADR-23 (Aceptado) con consolidación en el README raíz de la Fase H. El índice lista 24 ADR contiguos (ADR-01..24), 20 Aceptado y 4 Propuesto, sin huecos ni duplicados. No se detectó pipeline ni carpeta de nivel solución (correcto para el caso degenerado). No hay violaciones D1–D8.

Se registra un único hallazgo de nivel medio (P2): dos referencias textuales a «la developer guide de 10» en documentos de 09, que apuntan a un artefacto cuya categoría fue deliberadamente omitida por ADR-23. No es bloqueante.

### Conteo por nivel

| Nivel | Cantidad |
| --- | --- |
| P0 (rechaza) | 0 |
| P1 (bloquea) | 0 |
| P2 (medio) | 1 |
| P3 (bajo) | 1 |

### Veredicto

**APROBADO.** Sin hallazgos P0 ni P1. El entregable de la Fase F es apto. El hallazgo P2 y el P3 son recomendaciones de corrección menor que no condicionan la aprobación.

---

## 2. Matriz D1–D8 (estilo y forma)

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Rioplatense sin emojis | Cumple | No hay pictogramas (rangos U+1F000–1FAFF / U+2600–27BF vacíos). Solo símbolos tipográficos legítimos: flechas `→`, `⇒`, `↔` (p. ej. DEV→PROD, código↔artefacto). |
| Sin negritas decorativas | Cumple | El uso de `**…**` es semántico (cabecera de metadatos estándar SDD; términos clave como `PENDIENTE`, `image-docker`, `incompatible`). No hay énfasis ornamental. |
| UTF-8 | Cumple | `file` reporta text UTF-8 en los 6 docs de 09 y en ADR-23/24. |
| Fin de línea LF | Cumple | Sin CR en ningún archivo auditado (09, ADR-23, ADR-24, índice). |
| Filenames `-v1.0.md` | Cumple | Los 6 de 09, ADR-23, ADR-24 y el índice respetan el sufijo uniforme. |
| `guia-publicacion-<tipo-artefacto>` parametrizado | Cumple | `Guia-Publicacion-Image-Docker-v1.0.md`; `<tipo-artefacto>=image-docker` (no hardcodea gestor de paquetes). §0 lo declara explícito. |
| Terminología «equipos» (no «parque») | Cumple | Cero ocurrencias de «parque»; se usa «equipo». |
| Terminología «contraseña» vs «secreto» | Cumple | «Contraseña del administrador único» (Entornos §4); «secretos» se reserva a runtime/config (env vars, secreto del workflow, secreto de firma), uso correcto. |

---

## 3. Matriz de estructura (criterios §6 de 09-Rules, nivel proyecto)

| Criterio §6 | Estado | Evidencia |
| --- | --- | --- |
| `Pipeline-CI-CD` con stages (lint/build/test/SCA/SBOM/firma/publish) + matriz SO/runtime + caché + artefactos + promotion + rollback + notificaciones | Cumple | 10 stages (§1); matriz un eje ubuntu-latest/.NET 10/linux-amd64 (§2); caché NuGet (§3.1); artefactos y retención (§3.2); promotion DEV→PROD (§4); rollback con comando (§5); notificaciones (§6). |
| `Estrategia-Versionado` con SemVer 2.0.0 + Conventional Commits 1.0.0 + herramienta + branching + canales + deprecation | Cumple | SemVer 2.0.0 (§1); Conventional Commits 1.0.0 (§2); MinVer (§3); trunk-based (§4); canales (§5); deprecation v1→v2 (§7). |
| `Entornos-Deploy` con modelo correcto para web-monolith | Cumple | DEV/PROD con desviación del piso justificada y registrada (§1, §1.1); 12-factor (§3); secretos (§4); promoción (§5). |
| ≥1 `guia-publicacion-<tipo-artefacto>` con pre-req + comando/stage + verificación + rollback + métricas | Cumple | `Guia-Publicacion-Image-Docker`: pre-req (§1), stage/comandos (§2), verificación post-publish (§3), rollback (§4), métricas (§5). |
| `Supply-Chain-Seguridad` con SBOM + firma + SLSA + scanning + SAST/DAST + CVE | Cumple | SBOM CycloneDX (§1); cosign keyless (§2); SLSA L2 objetivo/L3 plan (§3); SCA (§4); SAST/DAST con DAST no-aplicable justificado (§5); política CVE con SLA (§6). |
| El pipeline ejecuta la DoD de 08 como gates, sin redefinir | Cumple | Ver §4 de este informe. |
| Cada NFR numérico de 05 con stage/gate | Cumple | Pipeline §8: 22/22 NFR numéricos con gate; N-03/N-20/N-25 declarados PENDIENTE con vía de cierre. |
| Cada ambiente/canal con aprobador y SLA/ventana | Cumple | Entornos §1: DEV (auto), PROD (administrador único / release manager), SLA acotado a disponibilidad del host + SLO N-25 PENDIENTE. |
| Rollback documentado por tipo de artefacto con comando concreto | Cumple | Pipeline §5 y Guia §4: bloque `docker stop --time 150` / `docker run …` / `curl …/health`. |
| SBOM y firma automáticos y adjuntos al release | Cumple | STAGE-07 (SBOM adjunto) y STAGE-09 (cosign en tag, verificado antes de publicar). |
| Sin stacks/protocolos del dominio fuente del bootstrap | No aplica / Cumple | El tooling DevOps (GitHub Actions, .NET 10, Docker, cosign, CycloneDX, MinVer) y el dominio del SAI (NUT/upsd, SQLite) son legítimos (09 es categoría técnica). No existe dominio fuente impresora/ESC-POS/DSL. |
| 6 criterios de nivel solución omitidos (caso degenerado) | Cumple | No existe `Solucion/` ni `Pipeline-Solucion`. README §10 y Pipeline §0 declaran la omisión explícita del pipeline de solución. |

---

## 4. Coherencia cross-doc

### 4.1 Pipeline ↔ DoD de 08 (no la redefine)

Cumple. Los 10 stages del `Pipeline-CI-CD §1` corresponden uno a uno con los 10 quality gates de `08/Estrategia-Calidad §3` y con la DoD de `08/Definition-Of-Done`. El pipeline declara explícitamente (§0) que «no redefine» la DoD: «cuando este documento dice "criterio de éxito", ese criterio es el gate de 08 o el NFR de 05, referenciado, nunca reformulado». Verificado stage por stage: build cero warnings (gate 1 / DoD US), lint+formato (gate 2), cobertura Domain 90/85 (gate 3 / N-22) y global 80/70 (gate 4 / N-21), e2e (gate 5), SCA alta/crítica bloqueante (gate 6 / DoD BT), SBOM (gate 7 / DoD release), imagen+smoke (gate 8), firma cosign en tag (gate 9), publicación (gate 10). Los umbrales de cobertura se evalúan por capa, evitando el anti-patrón de número global único.

### 4.2 NFR ↔ gates

Cumple. `Pipeline-CI-CD §8` traza los 25 NFR: los 22 con dato numérico tienen stage/gate (N-01, N-02, N-04..N-19, N-21..N-24); los 3 sin dato en la fuente (N-03, N-20, N-25) se declaran PENDIENTE con vía de cierre explícita (ventana de mantenimiento UF-8, medición bajo carga, medición de runtime), sin inventar gate. Coherente con la DoD §2 y con `Criterios-Validacion`.

### 4.3 Ambientes ↔ ADR-24

Cumple. `Entornos-Deploy §1.1` declara la desviación DEV/PROD (sin QA/STAGING) respecto del piso §2.2 de 09-Rules, con justificación de dominio del intake §17.P.8, y la registra como ADR-24 (Aceptado), con enlace relativo correcto al archivo. Las referencias en 09 apuntan a ADR-24 (4 ocurrencias en Pipeline/Entornos/README); no hay referencia a un número inexistente. ADR-24 está en estado Aceptado, no Propuesto.

### 4.4 Omisión de 10 ↔ ADR-23

Cumple con salvedad (ver hallazgo H-1). ADR-23 (Aceptado) registra la omisión de la categoría 10-Developer-Guide, admisible por `10-Rules §2.2` (web-monolith: Opcional, «Sólo README del repositorio»), con motivo (un solo desarrollador, `tiene_portal_developers=false`, sin superficie pública) y consolidación del onboarding en el README raíz de la Fase H (§2, §7). No existe la carpeta `SDD/Docs/10-Developer-Guide/`. Salvedad: dos documentos de 09 aún citan «la developer guide de 10» como si existiera (H-1).

### 4.5 Índice de ADR consistente

Cumple. `Decisiones-Arquitectura-v1.0.md` (v1.2) lista 24 ADR con numeración contigua ADR-01..24, sin huecos ni duplicados. Conteo por estado: 20 Aceptado (ADR-01..18, ADR-23, ADR-24) y 4 Propuesto (ADR-19..22), consistente con el encabezado, la sección de notas y el control de cambios. ADR-23 (categoría Estilo) y ADR-24 (categoría Despliegue) figuran en el índice con estado Aceptado y fecha 2026-07-21, y en la tabla de trazabilidad de origen (Fase F). Ambas categorías son válidas según el enumerado de `05-Rules §4.3`.

### 4.6 Coherencia con el intake (§17 P.7/P.8/P.9)

Cumple. Versionado P.7: SemVer 2.0.0, Conventional Commits 1.0.0, MinVer, trunk-based, canal único de imagen, sin feed público — todos presentes. Pipeline P.8: los 10 stages, ambientes DEV/PROD, rollback por etiqueta, no-staging «no habría a qué SAI conectarlo» citado textual. Matriz P.9: un solo eje Linux justificado (host `i7infra`, linux/amd64). PENDIENTE (N-03/N-20/N-25, TLS ADR-20, ubicación NUT ADR-19) referidos, no inventados.

---

## 5. Validación de los ADR nuevos (§4.3 de 05-Rules)

| Sección obligatoria §4.3 | ADR-23 | ADR-24 |
| --- | --- | --- |
| 1. Contexto | Sí | Sí |
| 2. Decisión | Sí | Sí |
| 3. Estado (Aceptado, con fecha) | Sí — Aceptado 2026-07-21 | Sí — Aceptado 2026-07-21 |
| 4. Alternativas consideradas (tabla) | Sí (3 alternativas) | Sí (3 alternativas) |
| 5. Consecuencias positivas (lista) | Sí | Sí |
| 6. Consecuencias negativas / trade-offs | Sí | Sí |
| 7. Implementación | Sí | Sí |
| 8. Métricas de validación | Sí | Sí |
| 9. Referencias | Sí | Sí |
| 10. Control de cambios | Sí | Sí |
| Categoría válida | Estilo | Despliegue |

Ambos ADR tienen las 10 secciones, estado Aceptado con fecha y categoría válida.

---

## 6. Hallazgos

### H-1 (P2) — Referencia a «la developer guide de 10» en documentos de 09

- **Archivo/sección:** `09-Devops/Pipeline-CI-CD-v1.0.md §7` (línea 162) y `09-Devops/README.md` (Nota sobre workflows reales, línea 39).
- **Evidencia:** Ambos textos afirman que «la developer guide de 10 cita los comandos exactos de cada stage para reproducción local». La categoría 10 fue deliberadamente omitida por ADR-23, que consolida el onboarding en el README raíz de la Fase H, no en una developer guide dedicada. La referencia apunta a un artefacto inexistente y contradice la decisión registrada en ADR-23.
- **Impacto:** Coherencia cross-doc; un lector podría buscar un documento que no se producirá. No afecta la operatividad del pipeline. No bloqueante.
- **Recomendación:** Reemplazar «la developer guide de 10» por «el README raíz de la solución (Fase H, ADR-23)» o «esta sección», alineando con la consolidación decidida en ADR-23.

### H-2 (P3) — Imprecisión menor en el conteo de NFR del README

- **Archivo/sección:** `09-Devops/README.md`, Vínculos upstream (línea 33).
- **Evidencia:** Describe «los 25 NFR N-01..N-25 con objetivo numérico y mecanismo de medición». En rigor, 22 tienen objetivo numérico y 3 (N-03, N-20, N-25) están PENDIENTE sin dato en la fuente, como el propio `Pipeline-CI-CD §8` distingue correctamente («Los 22 NFR con dato numérico tienen gate»).
- **Impacto:** Estilístico; la trazabilidad de los 25 sigue siendo correcta (22 con gate + 3 PENDIENTE declarados). No bloqueante.
- **Recomendación:** Matizar la redacción a «los 25 NFR (22 con objetivo numérico y gate; 3 PENDIENTE con vía de cierre)».

---

## 7. Confirmaciones solicitadas

- **5 documentos obligatorios de 09 + README:** confirmado. Pipeline-CI-CD, Estrategia-Versionado, Entornos-Deploy, Guia-Publicacion-Image-Docker, Supply-Chain-Seguridad y README, todos `-v1.0.md`. No existe `Solucion/` ni pipeline de nivel solución (caso degenerado correcto).
- **Pipeline ejecuta la DoD de 08 sin redefinir:** confirmado. 10 stages ↔ 10 gates de Estrategia-Calidad §3 ↔ DoD; los criterios de éxito referencian, no reformulan.
- **Cada NFR numérico con gate:** confirmado. 22/22 con gate; N-03/N-20/N-25 PENDIENTE declarados, no inventados.
- **Ambientes DEV/PROD con ADR-24:** confirmado. Desviación justificada (intake §17.P.8) y registrada como ADR-24 Aceptado; referencias en 09 apuntan a ADR-24.
- **Omisión de 10 con ADR-23:** confirmado. Omisión admisible (10-Rules §2.2), motivo registrado, consolidación en README raíz de Fase H; carpeta 10 inexistente. Salvedad menor H-1.
- **Índice de 24 ADR consistente y contiguo:** confirmado. ADR-01..24 sin huecos ni duplicados; 20 Aceptado, 4 Propuesto; conteo coherente en encabezado, notas y control de cambios.

---

## 8. Veredicto final

**APROBADO — sin hallazgos P0 ni P1.**

La categoría 09-Devops es completa, internamente coherente y trazable a 05 y 08; la omisión de 10 y la desviación de ambientes están correctamente formalizadas como ADR-23 y ADR-24 (Aceptados), y el índice de ADR es consistente y contiguo. Se recomienda corregir H-1 (P2, referencia a un artefacto omitido) y H-2 (P3, redacción del conteo de NFR) en la próxima revisión menor de los documentos afectados, sin que ello condicione la aprobación de la Fase F.
