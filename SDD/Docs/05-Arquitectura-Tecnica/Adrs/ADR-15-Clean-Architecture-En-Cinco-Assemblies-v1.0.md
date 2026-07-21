# ADR-15 — Clean Architecture en cinco assemblies con dependencias hacia el dominio

**Proyecto:** Sai-Service-Core
**Documento:** ADR-15-Clean-Architecture-En-Cinco-Assemblies-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Estilo

## 1. Contexto

El núcleo de valor del sistema es lógica de dominio pura con consecuencias irreversibles: resolución temporal, invariantes I-1 a I-21, cálculo de derivados y veredictos, degradación de modalidad. La fuente exige escribir esos invariantes como pruebas antes de codificar (R-10), lo que requiere un dominio testeable sin infraestructura. El proyecto es un monolito de un solo proceso y un solo despliegue (web-monolith). La estructura de cinco assemblies está impuesta por `Topologia-Proyecto-Solucion.md`. Esta decisión cubre el estilo arquitectónico y la separación de capas, dos de los cinco ADR obligatorios de web-monolith.

## 2. Decisión

Se adopta Clean Architecture en cinco assemblies —`Domain`, `Application`, `Infrastructure`, `Api`, `Web`— con la dirección de dependencias apuntando siempre hacia el dominio: `Web → Api → Infrastructure → Application → Domain`. El dominio no depende de frameworks: ni de EF Core, ni de Blazor, ni de NUT. Todo es un solo proyecto desplegable (un proceso, un contenedor), no una jerarquía de proyectos D8.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-15 del intake §17 P.11 y §17 P.2.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Clean Architecture en cinco assemblies hacia el dominio | Dominio testeable sin infraestructura; invariantes probables antes de codificar; aísla decisiones irreversibles del ORM y la UI | Más assemblies y ceremonia de puertos/adaptadores que un CRUD plano |
| Capas tradicionales con acceso a datos desde la UI | Menos ceremonia | Acopla la lógica de apagado al ORM y al framework web; encarece las pruebas de invariante (R-10) |
| Orientada a eventos con event store y CQRS | Auditoría, escalado | Desproporción para un usuario y un dispositivo; complejidad sin contrapartida (E-09) |

## 5. Consecuencias positivas

1. Los invariantes I-1 a I-21 se prueban en `Domain` sin infraestructura (P.6, cobertura 90/85 en `Domain`).
2. El adaptador de conexión (ADR-02) y el planificador (hosted service en `Application`) encajan como puertos con implementaciones en `Infrastructure`.
3. Cambiar EF Core, Blazor o NUT no toca el dominio: las dependencias apuntan hacia adentro (T-07).

## 6. Consecuencias negativas y trade-offs

1. Más assemblies, puertos y adaptadores que un monolito plano: ceremonia que se acepta por la testabilidad del núcleo irreversible.
2. La disciplina de dependencias debe hacerse cumplir (revisión y, si se adopta, pruebas de arquitectura) o degenera en capas acopladas.
3. Para un solo desarrollador, el costo de mantener la separación es real; se justifica por el peso de las decisiones de apagado.

## 7. Implementación

Cinco assemblies bajo el único proyecto: `SAI.Service.Core.Domain` (entidades, invariantes, `ResolutorTemporal`, veredictos), `Application` (puertos, planificador como hosted service, casos de uso), `Infrastructure` (EF Core/SQLite, adaptador NUT y simulado), `Api` (endpoints REST `/api/v1/`), `Web` (panel Blazor interactive server con MudBlazor). Dependencias solo hacia el dominio. El planificador ejecuta rondas de sondeo, evalúa políticas, usa temporizadores con cancelación y eleva la cadencia durante una prueba de batería.

## 8. Métricas de validación

- Cobertura de `Domain` ≥ 90 % líneas / 85 % ramas (P.6, quality gate del pipeline).
- `Domain` no referencia EF Core, Blazor ni NUT (verificable por dependencias del assembly).
- Los 21 invariantes existen como pruebas que corren (R-10).

## 9. Referencias

- Intake §17 P.2, P.6, P.11 (PA-15); §13, §14; `Topologia-Proyecto-Solucion.md`; riesgo R-10; exclusión E-09.
- Cubre los ADR obligatorios de web-monolith «estilo» y «separación de capas».
- Motiva y enmarca a todas las demás ADR de esta categoría.
- ADR relacionadas: ADR-02, ADR-04, ADR-18.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-15. |
