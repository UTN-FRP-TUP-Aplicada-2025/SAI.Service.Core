# Definition of Done — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Definition-Of-Done-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Fuente canónica de "terminado" para el proyecto. La Definition of Done (DoD) define **cuándo un ítem está terminado**; es distinta de la Definition of Ready de 06 (`06-Backlog-Tecnico/Definition-Of-Ready-v1.0.md`), que define cuándo un ítem puede empezar. La DoR habla de arranque; esta DoD, de cierre. Los planes de 07 referencian este documento y no lo redefinen (Mini-Plan §9).

Cada criterio se responde con sí o no y se valida con una operación mecánica: un comando, un gate del pipeline o una métrica de un reporte. Las cuatro capas (US, BT, sprint, release) son acumulativas hacia arriba: un release Done exige que sus sprints estén Done, y así sucesivamente.

---

## 1. DoD por capa

### 1.1 Historia de usuario (US)

- [ ] El código compila en Release con cero warnings — `dotnet build -c Release` (gate 1).
- [ ] Cada criterio Given-When-Then de la US tiene una prueba automatizada verde que lo cubre — verificable en el reporte de tests y en la `Matriz-Cobertura-Pruebas`.
- [ ] La US referencia al menos un CU y el o los tests lo nombran; sin test huérfano ni criterio sin test — columna CU↔Test de la matriz.
- [ ] Cobertura por capa de lo tocado cumple su piso: Domain ≥ 90 % líneas / 85 % ramas, resto según capa — reporte de cobertura por assembly (gates 3 y 4).
- [ ] Ningún valor de dominio se persiste sin `Origen` (I-7) — test de invariante en el pipeline (N-24).
- [ ] Si la US toca el panel, la pantalla se validó en el navegador contra la línea base visual de la maqueta (Fase B2) — inspección del administrador.
- [ ] Análisis estático sin diagnósticos de severidad error y sin diferencias de formato — `dotnet format --verify-no-changes` + analizadores (gate 2).
- [ ] Si la US cerró un defecto, existe un test de regresión nuevo o extendido que lo previene — reporte de tests.

### 1.2 Tarea técnica (BT)

- [ ] El código compila en Release con cero warnings — gate 1.
- [ ] Los criterios de aceptación técnicos de la BT (compila, contrato o invariante respetado) tienen prueba verde — reporte de tests.
- [ ] Si la BT implementa un invariante I-1..I-21, la prueba del invariante existe y está verde — reporte de `Domain.Tests`.
- [ ] Si la BT es un spike de Sprint 0, su ADR de cierre está redactada — archivo ADR en `05-Arquitectura-Tecnica/Adrs/`.
- [ ] La BT no introduce dependencia con vulnerabilidad de severidad alta o crítica — gate 6 (SCA).
- [ ] La cobertura por capa afectada no baja de su piso — reporte de cobertura.

### 1.3 Sprint / etapa

- [ ] Todos los ítems comprometidos de la etapa (Mini-Plan) están US-Done o BT-Done, o su arrastre está registrado en la bitácora con motivo — bitácora del Mini-Plan §10.
- [ ] Los 10 quality gates pasan sobre la rama de la etapa — pipeline de CI (Estrategia-Calidad §3).
- [ ] Cobertura reportada por capa cumple los pisos sobre el conjunto acumulado; global ≥ 80/70, Domain ≥ 90/85 — reporte de cobertura.
- [ ] Cada CU que avanza en la etapa tiene sus tests verdes y trazados — `Matriz-Cobertura-Pruebas`.
- [ ] Cada NFR que entra en alcance de la etapa tiene test o medición; los PENDIENTE (N-03, N-20, N-25) marcados como tales — `Criterios-Validacion §3`.
- [ ] Cero defectos blocker abiertos — tablero de defectos.
- [ ] La suite de regresión de etapas anteriores sigue verde — reporte de tests.
- [ ] El criterio de cierre de la etapa se validó en el navegador por el administrador — Mini-Plan (criterio de cierre de la etapa).

### 1.4 Release

- [ ] Todas las etapas comprometidas para el release están Sprint-Done — bitácora del Mini-Plan.
- [ ] Los criterios de validación de release se cumplen: CU críticos verdes, NFR con SLA cumplido, regresión verde, calidad de código sin warnings — `Criterios-Validacion-v1.0.md`.
- [ ] La versión SemVer está calculada desde el tag de git — herramienta de versión (MinVer).
- [ ] La imagen de producción construye, su smoke test arranca el servicio y responde el endpoint de salud — gate 8.
- [ ] La imagen de tag está firmada y la firma se verifica antes de publicar — gate 9.
- [ ] SBOM válido adjunto al build — gate 7.
- [ ] Toda excepción a un criterio de validación está aceptada con ADR y BT de remediación — `Criterios-Validacion §6`.
- [ ] El histórico (append-only) sobrevive a un rollback de versión sin perder hechos — verificable con rollback a la etiqueta anterior.

## 2. Excepciones admitidas

Un ítem o un release puede declararse Done sin cumplir un criterio cuando:

- **Deuda técnica documentada.** El criterio incumplido queda registrado como una BT explícita en el backlog, con su plan de remediación. Sin BT, no hay excepción.
- **PENDIENTE con respaldo de origen.** Los NFR sin dato numérico en la fuente (N-03 presupuesto de apagado, N-20 tamaño de SQLite, N-25 SLO de disponibilidad) pueden quedar fuera del cierre de v1 si se aceptan con ADR y BT de remediación; se resuelven con su medición cuando el respaldo exista.
- **Verificación física operativa.** La ventana de mantenimiento (CU-10 / US-16) depende de una actividad física destructiva no automatizable; su parte de software puede estar Done aunque la ejecución real quede pendiente, cubierta en pruebas con el adaptador simulado. Hasta la ejecución física, el servicio permanece forzado en `SoloAlerta`.

La aprobación de cualquier excepción es del administrador único en su rol de Product Owner / QA.

## 3. Vigencia

Este documento es la **fuente canónica** de la Definition of Done del proyecto. Los planes de 07 (Mini-Plan) y cualquier plan de etapa lo **referencian, no lo redefinen**. Un cambio en cualquiera de los criterios versionables de las cuatro capas se registra como cambio de versión de este documento y se comunica al cierre de la etapa siguiente. La DoD no se solapa con la DoR de 06: la DoR filtra la entrada de cada ítem, esta DoD filtra su cierre.

## 4. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | DoD inicial canónica con cuatro capas (US, BT, sprint, release), cada criterio verificable mecánicamente, excepciones con BT explícito y PENDIENTE con ADR. No se solapa con la DoR de 06. |
