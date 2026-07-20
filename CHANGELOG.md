# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado sigue [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

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
