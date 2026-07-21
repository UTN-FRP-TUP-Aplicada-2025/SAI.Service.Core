# ADR-20 — TLS del panel y de la API en la LAN

**Proyecto:** Sai-Service-Core
**Documento:** ADR-20-Tls-Del-Panel-Y-La-Api-En-La-Lan-v1.0.md
**Versión:** 1.0
**Estado:** Superado por ADR-26
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

El panel (Blazor interactive server, con circuito WebSocket) y la API REST se exponen en la LAN, sin exposición a internet. La autenticación es por cookie de sesión (ADR-16), que viaja en cada request. La fuente no trata el TLS: es un pendiente a cerrar como ADR de Sprint 0 (P-04). Sin TLS, la cookie de sesión y las credenciales de login circulan en claro por la LAN. Es una decisión abierta: este ADR documenta el problema y las opciones, no cierra una elección. Motiva la decisión la superficie de exposición de §17 P.5 y la autenticación de CU-01.

## 2. Decisión

PENDIENTE (P-04, Sprint 0). Se documentan las opciones de terminación TLS para el panel y la API en la LAN. No se adopta ninguna todavía.

## 3. Estado

Propuesto el 2026-07-20. Decisión abierta de Sprint 0 (§17 P.5; pendiente P-04). Se convertirá en una ADR aceptada nueva al resolverse, y esta pasará a `Superado por ADR-YY`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| TLS con certificado autofirmado en el propio servicio (Kestrel) | Sin piezas extra; cifra el tránsito de la cookie en la LAN | Advertencia de certificado en el navegador; gestión y rotación del certificado manual |
| Terminación TLS en un reverse proxy del host | Centraliza el certificado; descarga al servicio del TLS | Añade una pieza a operar; el WebSocket de Blazor debe pasar por el proxy correctamente |
| Sin TLS (HTTP plano en LAN) | Cero configuración | Cookie de sesión y credenciales en claro; inaceptable aunque sea LAN, dado que hay autenticación |

## 5. Consecuencias positivas

1. (Esperadas al cerrar) La cookie de sesión y las credenciales viajan cifradas por la LAN.
2. El circuito WebSocket de Blazor y la API quedan bajo el mismo esquema de transporte.
3. Cerrar la decisión completa el modelo de seguridad de la exposición LAN junto con ADR-16.

## 6. Consecuencias negativas y trade-offs

1. Un certificado autofirmado genera advertencias en el navegador y requiere gestión manual de confianza.
2. Un reverse proxy añade una pieza de infraestructura a operar y a documentar en 09.
3. Mientras siga abierta, el despliegue de producción no puede fijar el esquema de transporte.

## 7. Implementación

Qué falta para decidir: (a) determinar si se acepta la advertencia de certificado autofirmado o se prefiere gestionar confianza; (b) evaluar si ya hay un reverse proxy en el host `i7infra` que pueda terminar TLS; (c) verificar que el transporte WebSocket de Blazor Server funciona a través de la opción elegida. Se cierra en Sprint 0 y se materializa en la vista de despliegue de 09. Los secretos asociados (clave del certificado) se inyectan por variables de entorno (P.5).

## 8. Métricas de validación

- Al cerrar: el panel y la API responden solo por HTTPS; el circuito WebSocket de Blazor funciona bajo TLS.
- La cookie de sesión no viaja en claro en ninguna request.

## 9. Referencias

- Intake §17 P.5 (superficie de exposición, TLS pendiente), P.11 (Sprint 0); pendiente P-04; §17 P.3 (WebSocket del panel).
- CU-01 Autenticación del administrador; CU-04 Monitoreo en vivo (acceso desde la LAN).
- ADR relacionadas: ADR-16.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Decisión abierta de Sprint 0 (P-04). |
| 1.1 | 2026-07-21 | Decisión cerrada en el Sprint 0; este ADR queda superado por ADR-26. Ver ese ADR para la decisión adoptada. |
