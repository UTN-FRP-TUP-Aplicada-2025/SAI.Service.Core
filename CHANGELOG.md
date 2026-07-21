# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Añadido

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
