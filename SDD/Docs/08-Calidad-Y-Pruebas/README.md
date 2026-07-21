# 08 — Calidad y pruebas · Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** README.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Índice navegable de los artefactos de calidad y pruebas del proyecto. Layout aplanado del caso degenerado: estos artefactos cuelgan directamente de `SDD/Docs/08-Calidad-Y-Pruebas/`. Tipo D8: `web-monolith`. Un único desarrollador.

---

## 1. Artefactos de la sección

| Artefacto | Rol | Estado | Emisor |
|---|---|---|---|
| [Estrategia-Calidad-v1.0.md](Estrategia-Calidad-v1.0.md) | Definición de calidad, atributos ISO/IEC 25010 priorizados, quality gates, roles, cadencia | Borrador | AG-08 |
| [Estrategia-Testing-v1.0.md](Estrategia-Testing-v1.0.md) | Pirámide 70/25/5, cobertura por capa, tooling, BDD, mocks/fixtures, datos, ambiente | Borrador | AG-08 |
| [Plan-Pruebas-v1.0.md](Plan-Pruebas-v1.0.md) | Alcance por etapa, criterios de entrada/salida, riesgos, plan por etapa, recursos | Borrador | AG-08 |
| [Criterios-Validacion-v1.0.md](Criterios-Validacion-v1.0.md) | Criterios funcionales, no funcionales (N-01..N-25), regresión, calidad de código, excepciones | Borrador | AG-08 |
| [Definition-Of-Done-v1.0.md](Definition-Of-Done-v1.0.md) | DoD canónica por capa (US, BT, sprint, release) | Borrador | AG-08 |
| `Matriz-Cobertura-Pruebas-v1.0.md` | Trazabilidad CU↔Tests, NFR↔Tests, RN↔Tests y cobertura por capa | Borrador | AG-08 |
| `Casos-Prueba-Referenciales-v1.0.md` | Catálogo de TC-XX con setup, pasos, expected y status | Borrador | AG-08 |
| `Guia-Testing-Extensibilidad-v1.0.md` | Cómo testear la capa de add-ons de dialecto de protocolo (diseñada, no implementada en v1) | Borrador | AG-08 |
| `Matriz-Sensado-Deriva-v1.0.md` | Contraste de lo construido contra la línea base visual y el contrato de datos de la maqueta aprobada (Fase B2) | Vigente | AG-03M (incorporada por AG-08) |

Notas de aplicabilidad:

- La guía de testing de extensibilidad no es obligatoria para `web-monolith`; se incluye por el único motor de extensión interno del proyecto —la capa de add-ons de dialecto (F-26, ADR-02, US-26)—, que queda diseñada pero no implementada en v1. Se materializa cuando aparezca un equipo que NUT no cubra.
- La Matriz-Sensado-Deriva aplica porque el proyecto ejecutó la Fase B2 (existe maqueta navegable en `SDD/Maquetas/` y la línea base visual y el contrato de datos en `03-UX-UI-DX/`). La emite AG-03M al cerrar la Fase B2; AG-08 la incorpora resolviendo qué filas se cubren con test automatizado y cuáles quedan como inspección. Su regla vive en `Deriva-Rules.md`.

## 2. Quality gates configurados en CI

Diez gates bloqueantes en el pipeline (GitHub Actions), detallados en `Estrategia-Calidad-v1.0.md §3` y materializados en la categoría 09.

| # | Gate | Condición |
|---|---|---|
| 1 | Build | Cero errores y cero warnings (Release, .NET 10) |
| 2 | Lint / formato | Sin diferencias de formato; cero diagnósticos de severidad error |
| 3 | Test unitario | 100 % verdes; cobertura `Domain` ≥ 90 % líneas / 85 % ramas (N-22) |
| 4 | Test de integración | 100 % verdes; cobertura global ≥ 80 % líneas / 70 % ramas (N-21) |
| 5 | Test end-to-end | 100 % verdes en los flujos críticos |
| 6 | SCA | Cero vulnerabilidades de severidad alta o crítica |
| 7 | SBOM | Artefacto CycloneDX válido |
| 8 | Build de imagen | Imagen construye; smoke test responde el endpoint de salud |
| 9 | Firma | Firma keyless verificable (builds de tag) |
| 10 | Publicación | Push a registro privado solo desde `main` con tag SemVer y con 1–9 en verde |

Cobertura reportada siempre **por capa**, nunca como número global único.

## 3. Definition of Done canónica

El cierre de todo ítem se rige por la DoD canónica del proyecto:

- **[Definition-Of-Done-v1.0.md](Definition-Of-Done-v1.0.md)** — fuente única. Los planes de 07 la referencian, no la redefinen. Complementa a la Definition of Ready de 06 (`06-Backlog-Tecnico/Definition-Of-Ready-v1.0.md`): la DoR filtra la entrada, la DoD filtra el cierre.
