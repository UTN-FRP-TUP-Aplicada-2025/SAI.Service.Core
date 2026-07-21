# ADR-28 — Autenticación de la API REST con Bearer JWT vía ROPC

**Proyecto:** Sai-Service-Core
**Documento:** ADR-28-Autenticacion-De-La-Api-Con-Bearer-Jwt-Via-Ropc-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

ADR-16 fijó la autenticación del **panel** con ASP.NET Core Identity y sesión por cookie, y estableció que "todo otro endpoint y toda otra página requieren autenticación", pero no fijó **cómo** se autentica un cliente no interactivo contra la API REST `/api/v1`. La cookie de Identity sirve a un navegador con circuito Blazor, no a un consumidor programático (un script, otro servicio, una prueba de integración) que no puede ni debe sostener la mecánica de cookie + antiforgery del panel. El único titular de credenciales del sistema es el administrador único de ADR-16 (no hay registro de clientes ni usuarios múltiples). La API queda dentro de la LAN, sobre el TLS de ADR-20. Esta decisión se toma al codificar la Etapa 1 (persistencia + acceso), cuando aparece el primer endpoint protegido de la API (`/api/v1/ping`).

## 2. Decisión

La API REST se autentica con **Bearer JWT**, y el token se obtiene por el flujo **ROPC** (Resource Owner Password Credentials): el cliente entrega usuario y contraseña del administrador (ADR-16) al endpoint `POST /api/v1/token` y recibe un JWT firmado. Se adopta un **esquema de autenticación dual** en el mismo host:

- La **cookie** de Identity queda como esquema por defecto y sirve al panel Blazor (sin cambios respecto de ADR-16).
- El **Bearer JWT** se exige **explícitamente** en los endpoints de la API mediante la policy de autorización `Api` (`AddPolicy("Api", …).AddAuthenticationSchemes(Bearer).RequireAuthenticatedUser()`). La cookie no autoriza la API y el Bearer no autoriza el panel.

Detalles del token:

- Firma **HS256** con clave simétrica leída de `Jwt:ClaveFirma`. La clave es un secreto que **no** se hornea en el repositorio: en producción se inyecta por la variable de entorno `Jwt__ClaveFirma` (≥ 32 bytes); en Development/pruebas hay una clave local (ADR-20). El arranque falla si la clave mide menos de 32 bytes.
- `issuer`, `audience` y vigencia (`Jwt:MinutosVigencia`, 60 min por defecto) se leen de configuración. La validación exige emisor, audiencia, firma y vigencia, con `ClockSkew` de 30 s.
- Los claims incluyen el identificador y el nombre del administrador y el rol `administrador`, de modo que la API comparte el modelo de identidad de ADR-16 (un solo titular, un solo rol).
- Origen único de la configuración JWT: tanto la **emisión** (al firmar) como la **validación** (al recibir) resuelven la clave/emisor/audiencia de forma **diferida** desde `IOptions<OpcionesJwt>`, para que firmar y validar usen siempre el mismo valor aunque la configuración se sobreescriba después del registro (p. ej. la clave de las pruebas de integración).

Las credenciales inválidas en `/api/v1/token` devuelven `400` sin distinguir "usuario inexistente" de "contraseña incorrecta"; los endpoints protegidos sin Bearer válido devuelven `401`.

## 3. Estado

Aceptado — 2026-07-21. Complementa a ADR-16 (autenticación del administrador único): reutiliza su titular y su rol, y añade el canal de autenticación no interactivo de la API. No supera a ningún ADR.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| Bearer JWT vía ROPC, esquema dual con la cookie (elegida) | Cliente programático sin cookie/antiforgery; reutiliza el administrador único de ADR-16; sin estado de servidor; testeable | ROPC entrega la contraseña al endpoint; sin refresh token (se re-emite con las credenciales) |
| Reusar la cookie de Identity también para la API | Un solo mecanismo | Descartada: obliga al cliente no interactivo a manejar cookie + antiforgery; acopla la API al circuito del panel |
| OAuth2 con `client_credentials` / servidor de identidad (p. ej. IdentityServer) | Estándar para máquina-a-máquina; refresh, scopes | Descartada: no hay más titular que el administrador único; sobredimensionado para un monolito en LAN (viola la simplicidad de ADR-15/ADR-16) |
| API key estática por cabecera | Trivial de emitir | Descartada: no ata la llamada al administrador de ADR-16; rotación y revocación manuales; sin vigencia |

## 5. Consecuencias positivas

1. Un consumidor programático se autentica con las mismas credenciales del administrador (ADR-16), sin duplicar el modelo de identidad.
2. La API es sin estado: el JWT se valida por firma; no hay sesión de servidor que sostener ni store de tokens.
3. El corte por policy (`Api`, esquema Bearer explícito) deja el panel y la API con superficies de autorización independientes sobre el mismo host.
4. Emisión y validación comparten un único origen de configuración diferido, lo que elimina la clase de fallo "firmo con una clave y valido con otra".

## 6. Consecuencias negativas y trade-offs

1. ROPC transmite la contraseña del administrador al endpoint de token: aceptable **solo** sobre el TLS de ADR-20 y en la LAN; quedaría desaconsejado en una exposición pública.
2. No hay refresh token: al vencer el JWT, el cliente vuelve a presentar las credenciales. Es un trade-off consciente a favor de la simplicidad (un único titular).
3. La revocación no es inmediata: un JWT emitido es válido hasta vencer (mitigado por la vigencia corta y por rotar `Jwt:ClaveFirma`, que invalida todos los tokens en circulación).
4. La clave de firma es un secreto operativo cuyo manejo recae en el despliegue (ADR-20).

## 7. Implementación

- `POST /api/v1/token` (ROPC) y el cierre de sesión del panel viven en `EndpointsAcceso` (`Web/Endpoints`); `GeneradorTokens` (`Web/Autenticacion`) firma el JWT desde `IOptions<OpcionesJwt>`.
- El composition root (`Web/Program.cs`) registra `AddAuthentication().AddJwtBearer(Bearer)` y configura sus `TokenValidationParameters` de forma diferida vía `AddOptions<JwtBearerOptions>(Bearer).Configure<IOptions<OpcionesJwt>>(…)`, más la policy `Api`. La cookie de Identity (ADR-16) permanece como esquema por defecto.
- Los endpoints de la API (`/api/v1/ping` y sucesores) aplican `.RequireAuthorization("Api")`. `/api/v1` informativo y `/health` quedan anónimos.

## 8. Métricas de validación

- `POST /api/v1/token` con credenciales válidas devuelve un JWT (`token_type: Bearer`); con credenciales inválidas devuelve `400`.
- `GET /api/v1/ping` sin Bearer devuelve `401`; con un Bearer emitido por `/api/v1/token` devuelve `200`. Cubierto por las pruebas de integración de acceso (Etapa 1).
- La cookie del panel no autoriza la API y el Bearer no autoriza el panel (esquemas separados por policy).

## 9. Referencias

- ADR-16 (autenticación del administrador único, cookie del panel), ADR-20 (TLS y manejo del secreto de firma en la LAN), ADR-15 (Clean Architecture en cinco assemblies; el composition root es `Web`).
- CU-01 (acceso del administrador), F-15; Especificacion-Api (contrato `/api/v1`).

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Decisión de Etapa 1: autenticación de la API REST con Bearer JWT obtenido por ROPC, esquema dual con la cookie del panel. Complementa a ADR-16. |
