# ADR-23 — Omisión de la categoría Developer Guide dedicada

**Proyecto:** Sai-Service-Core
**Documento:** ADR-23-Omision-De-Developer-Guide-Dedicada-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Estilo

## 1. Contexto

El framework SDD contempla una categoría 10-Developer-Guide con documentos de onboarding, conceptos, referencia y troubleshooting orientados a un desarrollador consumidor. Para el tipo `web-monolith`, la tabla de adaptabilidad del orquestador y las reglas de la categoría (`10-Rules-Developer-Guide.md` §2.2) declaran esta categoría como **opcional**, con el criterio explícito «sólo README del repositorio» y «resumido en el README». Este proyecto refuerza ese criterio: es un servicio monolítico con un único desarrollador que es a la vez propietario, implementador y operador (intake §2), no expone un SDK ni una superficie pública consumida por integradores externos, y su bandera `tiene_portal_developers` es `false`. La única superficie hacia un tercero es la API REST de ingesta, cuyo contrato ya está documentado en `Contratos-REST-v1.0.md` (ADR-17) y en 08.

## 2. Decisión

No se genera una categoría 10-Developer-Guide dedicada. El onboarding del desarrollador (puesta en marcha del dev container, ejecución del servicio, ejecución de las pruebas, convenciones de ramas y de commits) se consolida en el README raíz de la documentación de la solución, que produce la fase de consolidación (Fase H). Los conceptos de dominio ya viven en el glosario de 02 y en el glosario UX de 03; el troubleshooting operativo pertinente vive en la guía de operación de 09 y en la estrategia de pruebas de 08.

## 3. Estado

Aceptado — 2026-07-21.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| Categoría 10 dedicada completa (conceptos, onboarding, referencia, troubleshooting, glosario) | Cobertura documental máxima | No hay consumidor externo que la justifique; duplica el glosario de 02/03 y la operación de 09; ceremonia sin lector para un proyecto de un solo desarrollador |
| 10 mínima con solo un `README.md` de sección | Deja un punto de entrada en la carpeta 10 | Duplica el README raíz que la Fase H ya produce; dos índices de onboarding compitiendo |
| Omisión con consolidación en el README raíz (elegida) | Un único punto de onboarding, sin duplicación; fiel al criterio «resumido en el README» del tipo D8 | Requiere que la Fase H incluya la sección de onboarding del desarrollador |

## 5. Consecuencias positivas

1. Se evita documentación ceremonial sin lector para un proyecto de un solo desarrollador.
2. El onboarding queda en un único lugar (README raíz), sin índices compitiendo.
3. No se duplican el glosario (02/03) ni la operación (09).

## 6. Consecuencias negativas y trade-offs

1. Si en el futuro aparece un segundo desarrollador o un integrador externo, habrá que materializar la categoría 10: el trade-off de no anticiparla está aceptado, porque hoy no hay consumidor.
2. La Fase H queda obligada a incluir una sección de onboarding del desarrollador en el README raíz; si no lo hiciera, el onboarding quedaría huérfano. Se declara como requisito de la Fase H.

## 7. Implementación

La Fase H (consolidación) incluye en `SDD/Docs/README.md` una sección de onboarding del desarrollador con: cómo abrir el dev container, cómo ejecutar el servicio, cómo correr las pruebas (referencia a 08) y las convenciones de versionado y ramas (referencia a 09). No se crea la carpeta `SDD/Docs/10-Developer-Guide/`.

## 8. Métricas de validación

- La carpeta `10-Developer-Guide/` no existe en `SDD/Docs/`.
- El README raíz de la Fase H contiene una sección de onboarding del desarrollador con los cuatro puntos de §7.

## 9. Referencias

- `10-Rules-Developer-Guide.md` §2.2 (web-monolith: opcional, «sólo README del repositorio»).
- Tabla de adaptabilidad por tipo del master-prompt (§14): web-monolith, 10 «Opcional, suele colapsar en README».
- Flag `tiene_portal_developers` = false (§4 del master-prompt).
- ADR-17 (contrato REST de ingesta, única superficie hacia terceros); glosarios de 02 y 03; operación en 09.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Registro de la omisión de la categoría 10-Developer-Guide dedicada, con consolidación del onboarding en el README raíz de la Fase H. |
