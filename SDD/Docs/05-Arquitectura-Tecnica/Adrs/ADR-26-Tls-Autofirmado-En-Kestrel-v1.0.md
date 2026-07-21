# ADR-26 — TLS con certificado autofirmado en Kestrel

**Proyecto:** Sai-Service-Core
**Documento:** ADR-26-Tls-Autofirmado-En-Kestrel-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

ADR-20 dejó abierta, como decisión de Sprint 0 (pendiente P-04), la estrategia de cifrado del panel (Blazor interactive server, con circuito WebSocket) y de la API REST en la LAN. Sin TLS, la cookie de sesión (ADR-16) y las credenciales de login (CU-01) circulan en claro por la red local. Este ADR cierra la decisión.

## 2. Decisión

El servicio termina **TLS directamente en Kestrel con un certificado autofirmado**. No se introduce un reverse proxy: el propio proceso sirve HTTPS. El certificado se gestiona por instancia (generado en el primer arranque o provisto por configuración) y se renueva manualmente. El circuito WebSocket de Blazor viaja sobre la misma conexión TLS de Kestrel, sin intermediarios.

## 3. Estado

Aceptado — 2026-07-21. Supera a ADR-20, que pasa a estado `Superado por ADR-26`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| TLS autofirmado en Kestrel (elegida) | Sin piezas extra; cifra la cookie y el login en la LAN; el WebSocket de Blazor no cruza un proxy | Advertencia de certificado en el navegador; rotación manual del certificado |
| Terminación TLS en un reverse proxy del host | Centraliza el certificado; descarga el TLS del servicio | Una pieza más a operar; el WebSocket de Blazor debe atravesar el proxy correctamente |
| Sin TLS (HTTP plano) | — | Descartada en ADR-20: credenciales y cookie en claro, inaceptable con autenticación |

## 5. Consecuencias positivas

1. Cero infraestructura adicional: coherente con la simplicidad operativa de un solo host y un solo administrador.
2. La cookie de sesión y las credenciales viajan cifradas en la LAN.
3. El circuito WebSocket de Blazor Server no cruza un proxy, evitando una clase de fallas de configuración.

## 6. Consecuencias negativas y trade-offs

1. El navegador muestra una advertencia de certificado no confiable; el administrador la acepta una vez (o instala el certificado como confiable en su equipo). Trade-off aceptado en una LAN de un único usuario.
2. La rotación del certificado es manual; se documenta su vigencia y el procedimiento en 09.
3. Si en el futuro se agrega un reverse proxy (por otro motivo), la terminación TLS podría moverse allí con una ADR nueva que supere a esta.

## 7. Implementación

`Program.cs` del proyecto `Web` configura Kestrel con el endpoint HTTPS y el certificado autofirmado (en DEV, el certificado de desarrollo del SDK; en PROD, el certificado de la instancia por configuración). El `Entornos-Deploy` de 09 declara dónde vive el certificado y su rotación. HSTS y redirección HTTP→HTTPS quedan habilitados. La cookie de sesión de Identity se marca `Secure`.

## 8. Métricas de validación

- El panel y la API responden solo por HTTPS; una petición HTTP redirige a HTTPS.
- La cookie de sesión se emite con el atributo `Secure`.
- El circuito de Blazor Server se establece sobre TLS sin errores de WebSocket.

## 9. Referencias

- ADR-20 (decisión abierta que este ADR cierra), ADR-16 (autenticación por cookie), CU-01 (autenticación).
- Intake §17 P.5 (superficie de exposición), P-04.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Cierre de la decisión de Sprint 0 (P-04): TLS autofirmado terminado en Kestrel. Supera a ADR-20. |
