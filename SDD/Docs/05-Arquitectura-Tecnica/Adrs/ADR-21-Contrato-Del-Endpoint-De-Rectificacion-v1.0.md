# ADR-21 — Contrato del endpoint de rectificación del 409

**Proyecto:** Sai-Service-Core
**Documento:** ADR-21-Contrato-Del-Endpoint-De-Rectificacion-v1.0.md
**Versión:** 1.0
**Estado:** Propuesto
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Comunicación

## 1. Contexto

Cuando la API de ingesta recibe la misma `X-Idempotency-Key` con un cuerpo distinto, responde `409 conflicto_idempotencia` con las huellas `sha256` original y recibida más una `accionSugerida` (ADR-17, CL-21). Esa acción sugerida apunta a un endpoint de rectificación que permitiría corregir un hecho ya registrado, pero la fuente lo menciona sin definir su contrato: es un pendiente (P-05). Sin él, un emisor que quiere corregir una intervención no tiene camino formal. Es una decisión abierta: este ADR documenta el problema y las opciones, no cierra una elección. Motivan la decisión el caso de uso CU-11 (ingesta automatizada) y la regla RN-09 (idempotencia).

## 2. Decisión

PENDIENTE (P-05, Sprint 0; se cierra en la especificación funcional / categoría 02 según el intake). Se documentan las opciones de contrato para el endpoint de rectificación. No se adopta ninguna todavía.

## 3. Estado

Propuesto el 2026-07-20. Decisión abierta (§17 P.3, P.11; pendiente P-05). Se convertirá en una ADR aceptada nueva al definirse el contrato, y esta pasará a `Superado por ADR-YY`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Endpoint de rectificación explícito (nueva ruta) que crea un hecho correctivo | Coherente con historia append-only: la corrección es un hecho nuevo, no un update | Requiere definir su cuerpo, su idempotencia y cómo referencia al hecho corregido |
| Reenvío con una clave de idempotencia nueva más una referencia al hecho original | Reusa el POST existente; sin ruta nueva | El vínculo con el hecho corregido queda en el cuerpo; menos explícito |
| Permitir sobrescritura con la misma clave (que el 409 no bloquee) | Cliente más simple | Rompe la idempotencia (RN-09) y la inmutabilidad (ADR-04); descartada de plano |

## 5. Consecuencias positivas

1. (Esperadas al cerrar) El emisor externo tiene un camino formal para corregir un hecho sin violar la idempotencia.
2. La corrección se modela como hecho nuevo, coherente con la historia append-only (ADR-04) y la reatribución temporal (ADR-05).
3. Completa el contrato de la API de ingesta que ADR-17 deja abierto en su punto de rectificación.

## 6. Consecuencias negativas y trade-offs

1. Mientras siga abierto, la `accionSugerida` del 409 apunta a un endpoint sin contrato definido.
2. Definir la rectificación como hecho correctivo obliga a modelar cómo se referencia el hecho original y cómo se resuelve en las consultas.
3. La decisión cruza con 02 (especificación funcional): el intake indica que se cierra allí, no solo en 05.

## 7. Implementación

Qué falta para decidir: (a) definir si la rectificación es una ruta nueva o un reenvío con referencia; (b) especificar el cuerpo, las cabeceras de idempotencia y la referencia al hecho corregido; (c) alinear con la reatribución temporal (ADR-05) y la inmutabilidad (ADR-04). Se coordina con la especificación funcional (02) y se materializa en el `contratos-rest` de esta categoría cuando cierre.

## 8. Métricas de validación

- Al cerrar: una corrección tras un 409 produce un hecho correctivo trazable, sin update sobre el hecho original.
- La idempotencia (I-19, RN-09) se mantiene: ninguna rectificación duplica ni sobrescribe en silencio.

## 9. Referencias

- Intake §17 P.3 (endpoint de rectificación, PENDIENTE), P.11 (Sprint 0); pendiente P-05; CL-21.
- RN-09 Idempotencia de la ingesta; CU-11 Ingesta automatizada de intervenciones; NB-08; F-20.
- ADR relacionadas: ADR-04, ADR-05, ADR-17.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Decisión abierta (P-05). |
