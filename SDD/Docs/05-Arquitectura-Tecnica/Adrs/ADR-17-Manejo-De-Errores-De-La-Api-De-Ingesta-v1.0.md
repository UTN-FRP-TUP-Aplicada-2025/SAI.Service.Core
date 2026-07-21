# ADR-17 — Manejo de errores de la API de ingesta

**Proyecto:** Sai-Service-Core
**Documento:** ADR-17-Manejo-De-Errores-De-La-Api-De-Ingesta-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Comunicación

## 1. Contexto

La única superficie de la solución consumida por un tercero es la API de ingesta de intervenciones (`POST /api/v1/intervenciones`), usada por un GMAO externo sin intervención humana. La red puede fallar y el emisor reintenta: el reintento es el caso normal, no el excepcional (CL-21). Devolver el código equivocado corrompe el histórico en silencio: *"Devolver 200 sería peor que duplicar: el emisor creería que su corrección se aplicó"*. Los costos de una intervención externa son el invariante que la ingesta rompe primero (CL-22). Este es uno de los cinco ADR obligatorios de web-monolith (manejo de errores). Motivan la decisión el caso de uso CU-11 (ingesta automatizada) y las reglas RN-09 (idempotencia), RN-08 (cuadre de costos) y RN-07 (importe con moneda y fecha).

## 2. Decisión

La API de ingesta responde con un contrato de códigos explícito y errores en formato problem+json (RFC 7807). Cabeceras obligatorias `X-Idempotency-Key` y `X-Fuente-Datos`. Los cuatro caminos: `201` (cuerpo válido y clave nueva, devuelve id y confianza `media`); `200` con `creado=false` (clave ya procesada con el mismo cuerpo, devuelve el mismo id); `409 conflicto_idempotencia` (clave ya procesada con cuerpo distinto, devuelve huellas `sha256` original y recibida más una `accionSugerida`, nunca `200`); `422` (invariante roto: `validacion` para `Costos.cuadra()` o `Dinero` sin moneda/fecha, `coherencia_temporal` para una intervención fechada después de la baja).

## 3. Estado

Aceptado el 2026-07-20. ADR obligatorio de web-monolith (manejo de errores), derivado de CU-11 y RN-09. No proviene de un PA.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| 201/200/409/422 + problem+json, idempotencia por clave | Distingue creación, reintento idempotente, conflicto y violación de invariante; el emisor sabe qué pasó | Más lógica de servidor: huellas `sha256`, comparación de cuerpo, validación de invariantes |
| Responder siempre 200/201 sin distinguir conflicto | Cliente más simple | Un cuerpo distinto con la misma clave pasaría por aplicado (CL-21): corrompe el histórico en silencio |
| Errores como texto plano o códigos propietarios | Menos ceremonia | No interoperable; el GMAO externo no puede automatizar el manejo; rompe el estándar problem+json |

## 5. Consecuencias positivas

1. El reintento normal devuelve 200 idempotente con el mismo id, sin duplicar el hecho (CL-21, RN-09, I-19).
2. Un cuerpo distinto con la misma clave devuelve 409 con huellas y acción sugerida, no un 200 engañoso.
3. Los costos que no cuadran o el `Dinero` sin moneda/fecha se rechazan con 422 antes de entrar al histórico (CL-22, RN-08, RN-07, I-18).

## 6. Consecuencias negativas y trade-offs

1. El endpoint de rectificación que sugiere la respuesta 409 no está definido todavía (P-05): se trata en ADR-21.
2. El servidor debe calcular y comparar huellas `sha256` del cuerpo y evaluar los invariantes en la ruta de ingesta: más lógica y más pruebas.
3. La confianza `media` de la fuente externa (menor que la del poller local) debe propagarse a cada valor ingresado (ADR-06).

## 7. Implementación

Endpoint `POST /api/v1/intervenciones` en `Api`, con validación de las cabeceras `X-Idempotency-Key` y `X-Fuente-Datos`. La idempotencia se resuelve por `claveIdempotencia` (I-19) contra la historia append-only (ADR-04); el cuerpo se compara por huella `sha256`. Los invariantes `Costos.cuadra()`, `Dinero` con moneda y fecha (I-18) y coherencia temporal (RN-12) se evalúan en `Domain`. Los errores se serializan como problem+json. La confianza `media` se asigna a los valores de la fuente externa. El versionado de la API es por ruta (`/api/v1/`, P.3). El detalle del contrato se documenta en el `contratos-rest` de esta categoría y el sample `samples/ingesta-gmao/`.

## 8. Métricas de validación

- Los cuatro caminos 201/200/409/422 cubiertos por pruebas de integración (P.6, `WebApplicationFactory`).
- CL-21: misma clave + mismo cuerpo ⇒ 200 `creado=false`; misma clave + cuerpo distinto ⇒ 409.
- CL-22: costos que no cuadran ⇒ 422 `validacion`; intervención posterior a la baja ⇒ 422 `coherencia_temporal`.

## 9. Referencias

- Intake §17 P.3 (contrato de ingesta), P.10 (I-18, I-19), §18 (sample); CL-21, CL-22.
- RN-07 Todo importe con moneda y fecha; RN-08 Cuadre de costos de intervención; RN-09 Idempotencia de la ingesta; RN-12 Baja lógica y coherencia temporal.
- CU-11 Ingesta automatizada de intervenciones; NB-08; F-20. US-12.
- ADR relacionadas: ADR-04, ADR-06, ADR-07, ADR-21 (endpoint de rectificación, pendiente P-05).

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. ADR obligatorio de web-monolith (manejo de errores). |
