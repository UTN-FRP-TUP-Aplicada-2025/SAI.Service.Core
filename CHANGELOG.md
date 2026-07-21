# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Añadido

- **Fase H del SDD — README raíz y handoff**: `SDD/Docs/README.md` (índice maestro de
  toda la documentación, tabla de proyectos, mapa de las categorías 00→11 con las
  omisiones declaradas —04 por no-LLM, 10 por ADR-23—, flujos de lectura por audiencia,
  glosario y la sección de onboarding del desarrollador comprometida en ADR-23). Auditoría
  final consolidada del entregable completo: **APROBADO CON OBSERVACIONES, apto para el
  handoff a codificación** (13/13 enlaces del README sin roturas, cadena de trazabilidad
  D6 completa sin huérfanos, terminología consistente). Con esto la documentación SDD
  (Fases A-H) queda generada y auditada.

- **Fase G del SDD — 11-Examples**: dos samples documentados —`Ejemplo-01-Datos-Seed`
  (básico: explorar el sistema con datos precargados y el adaptador de conexión simulado,
  sin hardware) y `Ejemplo-02-Api-Ingesta` (intermedio: los cuatro caminos del contrato de
  ingesta 201/200/409/422)— más el README con la tabla maestra. Nombrados por capacidad,
  no por dominio. Auditoría independiente de la fase.

- **Fase F del SDD — 09-Devops**: pipeline CI/CD (10 stages que ejecutan la
  Definition-of-Done de 08 como quality gates), estrategia de versionado (SemVer 2.0.0,
  Conventional Commits, MinVer, trunk-based), entornos de despliegue, guía de publicación
  de la imagen Docker y política de supply-chain (SBOM CycloneDX, firma cosign). La
  categoría 10-Developer-Guide se omite (proyecto de un solo desarrollador sin portal),
  registrada en **ADR-23**; el modelo de dos ambientes DEV/PROD sin staging se registra
  en **ADR-24**. Índice de decisiones a 24 ADR. Auditoría independiente de la fase.

- **Fase E del SDD — 08-Calidad-Y-Pruebas**: estrategia de calidad (ISO 25010,
  10 quality gates), estrategia de testing (pirámide 70/25/5, cobertura por capa con
  Domain 90/85 y global 80/70), plan de pruebas por etapa, matriz de cobertura
  (12 CU / 13 RN / 25 NFR / 21 invariantes ↔ tests), 40 casos de prueba referenciales,
  criterios de validación de release, Definition-of-Done de cuatro capas, guía de testing
  de extensibilidad del adaptador de conexión, y la `Matriz-Sensado-Deriva-v1.0.md`
  (142 comprobaciones que contrastan lo construido contra la línea de base de la maqueta
  de la Fase B2). Auditoría independiente de la fase.

- **Fase D del SDD — 06-Backlog-Tecnico y 07-Plan-Sprint**: Product Backlog
  (7 épicas, 26 historias de usuario en archivos individuales, MoSCoW 18/6/2 con
  estimación Fibonacci), Backlog Técnico (30 tareas técnicas trazadas a ADR/componentes/
  contratos), Definition-of-Ready y, por ser proyecto de un solo desarrollador,
  `Mini-Plan-v1.0.md` (6 etapas: Sprint 0 de arranque + una por flujo, 17 riesgos) en
  lugar del plan de sprint completo. Auditoría independiente de la fase.

- **Fase C del SDD — 05-Arquitectura-Tecnica**: documento maestro de arquitectura
  (cuatro vistas, cross-cutting, 25 NFR numéricos, riesgos; estilo Clean Architecture
  en cinco assemblies justificado contra dos alternativas), 22 ADR individuales
  (18 Aceptado derivados de los pre-ADR del intake + autenticación, errores de la API y
  motor SQLite; 4 Propuesto por las decisiones abiertas de Sprint 0) con su índice,
  modelo de datos lógico (23 tablas SQLite con EF Core, herencia TPH, 26 índices,
  migración inicial, trazado al modelo conceptual), flujo de ejecución del planificador,
  contratos REST de la API de ingesta y documento de extensibilidad del adaptador de
  conexión. Auditoría independiente de la fase.

- **Fase B2 del SDD — validación visual de maqueta**: maqueta navegable estática en
  `SDD/Maquetas/Sai-Service-Core/` (11 superficies, HTML/CSS/JS con Bootstrap 5, sin
  build), aprobada por el administrador. Emite en `03-UX-UI-DX/` los artefactos de línea
  de base del sensado de deriva: `Linea-Base-Visual-v1.0.md`, `Contrato-Datos-Maqueta-v1.0.md`
  y `Bitacora-Validacion-Maqueta-v1.0.md`.

### Cambiado

- **Unificación de terminología** (retroalimentación de la Fase B2, propagada a toda la
  cadena: intake, 00, 01, 02, 03 y maqueta):
  - Dominio: **«parque» → «equipos»** (el término se juzgó jerga; «Dispositivo» e
    «Inventario» colisionaban con entidades/capas del modelo conceptual, «equipos» no).
    El flujo UF-1 pasa a «Alta de equipos y puesta en marcha».
  - Acceso: **«secreto» → «contraseña»** en la superficie de login (los «Secretos en
    runtime/CI» del intake §17 P.5 no se tocan: son gestión de secretos, otro concepto).
  - Renombrado de `NB-04`, `CU-02` y el wireframe de alta a sus nuevos slugs; intake a
    v1.2, manifiesto a v1.1.

- Intake actualizado a v1.1: **Parte D — Anexos de datos** (§20 escenarios `E-1`…`E-8`
  con JSON completo; §21 cobertura, invariantes I-1 a I-21 y flujos end-to-end). El intake
  pasa a ser autocontenido.
- `SOLUTION-MANIFEST-Sai-Service-Core-v1.0.md`, manifiesto derivado de §13 del intake
  durante la fase de validación del orquestador SDD (caso degenerado: un proyecto
  `web-monolith`, layout aplanado).
- **Fase A del SDD** — documentación de nivel solución:
  - `00-Contexto/`: Visión de producto, Alcance, Roadmap y Compatibilidad de plataformas.
    `Acuerdo-Equipo` omitido por ser proyecto de un solo desarrollador.
  - `01-Necesidades-Negocio/`: índice más 8 necesidades de negocio (NB-01 a NB-08),
    todas trazables al intake.
- **Fase B del SDD** — especificación y experiencia:
  - `02-Especificacion-Funcional/`: 12 casos de uso (CU-01 a CU-12), 13 reglas de negocio,
    modelo conceptual (27 entidades) y 9 reglas conceptuales de integridad.
  - `03-UX-UI-DX/`: marco de experiencia, 11 wireframes y glosario UX, con las cuatro
    extensiones de capacidad del arquetipo de panel monolítico (configuración por esquema,
    primer arranque, acceso monousuario, identidad de versión).
  - `04-Prompts-AI` omitida (el proyecto no usa modelos de lenguaje).
- Informes de auditoría independiente de las Fases A y B en `SDD/Docs/Audit/`.

### Notas

- Cadena de trazabilidad D6 cubierta hasta el modelo funcional: Intake → 00 → NB → CU →
  RN → modelo conceptual, sin artefactos huérfanos.
- Pendiente: Fase B2 (validación visual de maqueta) y Fases C a H.

## [0.1.0] - 2026-07-20

### Añadido

- Estructura base de documentación SDD (`SDD/Docs/`, `SDD/Intake/`, `SDD/Maquetas/`).
- `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` (v1.0, estado borrador), documento de intake
  de la solución con:
  - **Parte A — Negocio**: idea y problema, audiencia y stakeholders, propuesta de valor,
    alcance funcional MoSCoW, historias de usuario, flujos típicos, casos límite, métricas
    de éxito, exclusiones, restricciones, riesgos y glosario del dominio (§1–§12).
  - **Parte B — Composición**: proyectos de la solución, estilo arquitectónico, esquema de
    descomposición y delivery, y estructura de repositorio (§13–§16).
  - **Parte C — Técnica**: bloque técnico de `Sai-Service-Core`, estrategia de demo/samples
    y checklist de completitud (§17–§19).
  - Sección de trazabilidad downstream y control de cambios.
- Definición del stack principal: .NET 10 + Blazor (interactive server) + Entity Framework
  Core + SQLite.

### Notas

- El intake marca explícitamente cada afirmación como **[derivado]** o **PENDIENTE** según
  su respaldo en las fuentes; los ítems PENDIENTE requieren respuesta del stakeholder antes
  de derivar el `SOLUTION-MANIFEST`.

## [0.0.1] - 2026-07-18

### Añadido

- Commit inicial: `README.md` y `.gitignore`.
