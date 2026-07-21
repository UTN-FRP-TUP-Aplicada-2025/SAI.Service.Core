# Estrategia de calidad — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Estrategia-Calidad-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Documento transversal de calidad de la categoría 08. Fija qué significa calidad para este servicio, qué atributos ISO/IEC 25010 se priorizan, qué quality gates aplica el pipeline y con qué cadencia se revisa todo esto. Las decisiones de nivel de testing viven en `Estrategia-Testing-v1.0.md`; los criterios de release, en `Criterios-Validacion-v1.0.md`; el cierre de ítems, en `Definition-Of-Done-v1.0.md`, que es la fuente canónica y no se redefine acá.

---

## 1. Definición de calidad para el proyecto

Sai-Service-Core tiene calidad cuando, en el camino crítico, **se niega a apagar el host mientras no pueda probar que va a volver a encenderse**, y cuando, en el camino de datos, **ningún número que muestra o guarda miente sobre su origen**. Es un servicio con consecuencias irreversibles —decide el apagado de un servidor sin backups y construye el histórico de salud que fundamenta decisiones de compra— sobre un único host, para un único administrador. Por eso la calidad no se mide como ausencia de bugs en general, sino como dos garantías verificables: la seguridad operativa del apagado (arranque forzado en `SoloAlerta`, bloqueo por verificación, validación por efecto observado) y la trazabilidad de procedencia de cada valor (invariante I-7, sin excepción). Todo lo demás —usabilidad del panel, desempeño del planificador, mantenibilidad del dominio— sirve a esas dos garantías o las habilita.

## 2. Atributos de calidad priorizados (ISO/IEC 25010)

Se prioriza en orden descendente. Cada atributo declara su prioridad, la razón del rango y, cuando corresponde, la métrica numérica con su NFR de origen (N-01..N-25 de `05-Arquitectura-Tecnica/Arquitectura-Solucion-v1.0.md §8`).

| Prioridad | Atributo ISO/IEC 25010 | Por qué en este rango | Métrica / NFR de origen |
|---|---|---|---|
| 1 — Máxima | Fiabilidad (madurez, tolerancia a fallos, recuperabilidad) | El sistema decide el apagado de un host sin respaldo; una decisión errónea "falla de noche y sin testigos" (R-12). La historia append-only y la recuperación por rollback de imagen protegen el hecho registrado. | Detección de pérdida de comunicación 3 sondeos ⇒ `DesconexionUsb` (N-09); latencia de decisión del planificador < 1 s/ronda (N-06); idempotencia de ingesta 100 % (N-23); retención sin pérdida de hechos (N-19) |
| 2 — Máxima | Seguridad operativa (subsumida en Seguridad / Integridad de 25010, extendida a seguridad funcional del apagado) | Es la garantía diferencial del producto. No es seguridad informática sino seguridad de la acción física: no habilitar el apagado sin evidencia. | Arranque forzado en `SoloAlerta` y bloqueo por verificación (I-11); techo duro `ups.delay.shutdown` ≤ 540 s (N-01, I-10); vigencias de verificación 180/365 d / sin caducidad (N-13, N-14, N-15); procedencia 0 valores sin `Origen` (N-24, I-7) |
| 3 — Alta | Adecuación funcional (completitud, corrección, pertinencia) | Cada uno de los 12 CU debe cubrir su criterio Given-When-Then; los 21 invariantes I-1..I-21 son la corrección del dominio escrita como pruebas. | Cobertura CU↔test y RN↔test en `Matriz-Cobertura-Pruebas`; invariantes I-1..I-21 verdes |
| 4 — Alta | Mantenibilidad (modularidad, analizabilidad, testeabilidad) | Clean Architecture en cinco assemblies con el dominio sin dependencias de framework existe para que los invariantes se prueben sin infraestructura (R-10). Un solo desarrollador: el costo de mantenimiento pesa. | Cobertura por capa (N-21 global 80/70, N-22 Domain 90/85); build con cero warnings; análisis estático sin diagnósticos de severidad error |
| 5 — Media | Eficiencia de desempeño (comportamiento temporal, uso de recursos) | El planificador debe cerrar su ronda antes de la siguiente; el volumen anual (~6,3 M filas) exige que la agregación y la retención funcionen. No hay requisito de escalado horizontal. | Ronda < 1 s (N-06); intervalo de sondeo 5 s / 1 Hz en prueba (N-07, N-08); volumen ~6,3 M filas/año (N-18); tamaño de archivo SQLite **PENDIENTE** (N-20) |
| 6 — Media | Usabilidad (reconocibilidad, operabilidad, protección ante error del usuario) | El panel debe mostrar en pantalla principal —no enterrado— que la política está degradada, y marcar todo valor derivado o estimado. La validación de cierre de cada etapa es visual en el navegador (Intake §15). | Estado de supuestos visible (US-02, US-07); procedencia visible por valor (US-10); conformidad con la línea base visual de la maqueta (Fase B2) |
| 7 — Baja | Compatibilidad (coexistencia, interoperabilidad) | Superficie única de interoperación: la API REST `/api/v1/` versionada en la ruta y el contrato saliente hacia NUT. Sin otros proyectos con los que interoperar. | Contrato de ingesta 201/200/409/422 (N-23); versionado en la ruta |
| 8 — Baja | Portabilidad (adaptabilidad, instalabilidad) | Alcance exclusivamente Linux `linux/amd64`, dockerizado. La portabilidad multiplataforma está explícitamente fuera de alcance (§17.P.9). | Imagen `linux/amd64`; smoke test de arranque en CI (gate 8) |

Nota sobre disponibilidad: el SLO de disponibilidad del propio servicio (N-25) está **PENDIENTE** —no tiene respaldo numérico en la fuente; se propone «rondas completadas / esperadas ≥ 0,99 mensual» y se cierra en este ciclo de 08 o por ADR. Hasta entonces no es un umbral bloqueante.

## 3. Quality gates

Los diez gates son los del pipeline de CI/CD declarado en Intake §17.P.8, materializados como stages de GitHub Actions. Cada uno es bloqueante para mergear a `main`; los que dependen de tag se indican. Su realización downstream vive en la categoría 09.

| # | Gate | Condición (mecánica) | Herramienta | Consecuencia si falla |
|---|---|---|---|---|
| 1 | Build | Cero errores y cero warnings | `dotnet build -c Release` sobre .NET 10 con `TreatWarningsAsErrors` | Merge bloqueado |
| 2 | Lint / formato | Sin diferencias de formato; cero diagnósticos de severidad error | `dotnet format --verify-no-changes` + analizadores .NET nivel `recommended` | Merge bloqueado |
| 3 | Test unitario | 100 % de pruebas verdes; **cobertura ≥ 90 % líneas / 85 % ramas en `SAI.Service.Core.Domain`** (N-22) | framework de pruebas unitarias (xUnit) + aserciones fluidas (FluentAssertions); cobertura por recolector de .NET | Merge bloqueado |
| 4 | Test de integración | 100 % verdes; **cobertura global ≥ 80 % líneas / 70 % ramas** (N-21) | integración EF Core contra SQLite físico + host de pruebas web (WebApplicationFactory) + adaptador simulado | Merge bloqueado |
| 5 | Test end-to-end | 100 % verdes en los flujos críticos | arnés de componentes Blazor (bUnit) + driver e2e de navegador (Playwright) | Merge bloqueado |
| 6 | SCA (análisis de composición) | Cero vulnerabilidades de severidad alta o crítica | `dotnet list package --vulnerable --include-transitive` | Merge bloqueado |
| 7 | SBOM | El artefacto SBOM existe y es válido | Generación CycloneDX adjunta al build | Merge bloqueado |
| 8 | Build de imagen | La imagen de producción (runtime-only) construye y su smoke test arranca el servicio y responde el endpoint de salud | `docker build` + smoke test | Merge bloqueado |
| 9 | Firma de imagen | Firma keyless verificable (solo en builds de tag) | `cosign` modo keyless | Publicación bloqueada |
| 10 | Publicación | Push de la imagen etiquetada solo desde `main` con tag SemVer y solo si 1–9 pasaron | Registro privado | Sin publicación |

Los umbrales de cobertura de los gates 3 y 4 se reportan y evalúan **por capa**, nunca como un único número global (anti-patrón §4.10 de las reglas). El número global 80/70 es el piso del conjunto; el dominio tiene su piso separado y más alto.

## 4. Roles QA dentro del equipo

El proyecto es de **un único desarrollador** que combina propietario, implementador y beneficiario, y a la vez Scrum Master / Product Owner / Dev (Mini-Plan §1). El RACI es, por lo tanto, trivial: la misma persona diseña, ejecuta y aprueba. Se explicita para dejar registradas las responsabilidades por función, no por persona.

| Función QA | R (responsable) | A (aprueba) | C (consultado) | I (informado) |
|---|---|---|---|---|
| Diseño de tests y de la matriz de cobertura | Administrador (rol SDET) | Administrador | Artefactos upstream 02/05 | — |
| Ejecución de la suite y de los gates | Pipeline CI (GitHub Actions) | Administrador | — | Administrador |
| Aprobación de release | Administrador (rol QA) | Administrador | Criterios de validación de 08 | — |
| Ejecución de la ventana de mantenimiento (verificación física) | Administrador con presencia física | Administrador | Técnico externo si interviene | — |

Ninguna función queda sin titular. La titularidad de los artefactos de calidad es de AG-08; las revisiones sectoriales (trazabilidad con AG-02, NFR con AG-05, DoD referenciada desde el backlog con AG-06, gates materializados en 09 con AG-09) las absorbe el mismo administrador en el rol que corresponda.

## 5. Cadencia de revisión

- **Por etapa (Mini-Plan §2).** Al cierre de cada etapa, la matriz de cobertura y los criterios de validación se actualizan con los tests efectivamente implementados; se verifica que ningún CU o NFR que entró en alcance quedó sin test. La validación humana en el navegador es condición de cierre.
- **Ante cambio de umbral.** Cualquier cambio en los umbrales de cobertura, en los gates o en las vigencias de verificación se registra como cambio de versión del documento afectado y, si baja un piso, exige ADR (regla §2.2: los porcentajes son piso, no se bajan sin ADR).
- **Ante bug cerrado.** Todo defecto cerrado genera al menos un caso de prueba de regresión nuevo o extiende uno existente antes de cerrarse (anti-patrón §4.10; criterio de la DoD).
- **Al cerrar PENDIENTES.** Cuando se resuelvan N-03 (presupuesto de apagado, se mide en la ventana de mantenimiento), N-20 (tamaño de SQLite) y N-25 (SLO de disponibilidad), se incorporan como umbrales medibles y esta estrategia se versiona.
- **Revisión mayor.** Al pasar de v1.x a v2.0 (nuevo release mayor), la estrategia se revisa completa y la versión anterior se archiva en `_legacy/` con estado `Superado`.
