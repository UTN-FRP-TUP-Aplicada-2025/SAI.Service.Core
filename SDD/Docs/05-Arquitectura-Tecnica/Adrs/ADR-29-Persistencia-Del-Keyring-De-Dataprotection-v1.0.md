# ADR-29 — Persistencia del keyring de DataProtection

**Proyecto:** Sai-Service-Core
**Documento:** ADR-29-Persistencia-Del-Keyring-De-Dataprotection-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

ASP.NET Core Data Protection cifra la **cookie de sesión** del panel (ADR-16) y los **tokens antiforgery** de los formularios SSR de acceso (alta inicial, login, cambio de contraseña). Por defecto, el keyring se genera en memoria/efímero por proceso: en un contenedor sin volumen se escribe en `~/.aspnet/DataProtection-Keys`, que **no** persiste fuera del contenedor. Al reiniciar o redesplegar la imagen (algo normal en el modelo de despliegue de ADR-24), el keyring cambia y todo lo cifrado con el anterior deja de poder descifrarse: la sesión activa se invalida (obliga a re-loguear) y, peor, los tokens antiforgery de un formulario ya renderizado fallan la validación y el POST devuelve **HTTP 400** ("The antiforgery token could not be decrypted"). El síntoma se observó en la Etapa 1 al validar el alta inicial tras recrear el contenedor.

## 2. Decisión

El keyring de Data Protection se **persiste en un volumen** y la aplicación fija un **nombre de aplicación estable**:

- `AddDataProtection().SetApplicationName("SAI.Service.Core")` fija el discriminador de propósito, para que el keyring sea estable entre instancias de la misma app.
- `PersistKeysToFileSystem(<ruta>)` escribe las llaves en el directorio configurado por `DataProtection:RutaLlaves` (variable de entorno `DataProtection__RutaLlaves`). En producción esa ruta debe apuntar a un **volumen persistente** montado en el contenedor (p. ej. `/keys`). Si la ruta no se configura, el keyring queda efímero (aceptable solo en desarrollo).

Las llaves quedan **en claro** en el volumen (en Linux no hay un encryptor de reposo tipo DPAPI); su protección recae en los permisos del sistema de archivos del host y en el aislamiento del volumen, coherente con el modelo de secretos en la LAN de ADR-20. La rotación de llaves de Data Protection sigue su política por defecto (rotación ~90 días, con período de gracia), ahora efectiva porque el keyring sobrevive a los reinicios.

## 3. Estado

Aceptado — 2026-07-21. Decisión de la Etapa 1, derivada de ADR-16 (sesión por cookie) y ADR-20 (despliegue y secretos en la LAN). No supera a ningún ADR.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| Persistir el keyring en un volumen + `SetApplicationName` (elegida) | Sesión y antiforgery sobreviven a reinicios/redeploys; simple; sin dependencias extra | Requiere montar y respaldar un volumen; llaves en claro protegidas por permisos de FS |
| Keyring efímero por contenedor (default) | Cero configuración | Descartada: cada reinicio invalida sesiones y rompe los forms con HTTP 400 |
| Persistir el keyring en la base (`PersistKeysToDbContext`) | Un solo artefacto persistente (la base de ADR-18) | Descartada para la Etapa 1: agrega un paquete y una migración; el volumen es más simple y desacopla llaves de datos |
| Cifrar el keyring en reposo (certificado X.509) | Llaves cifradas en el volumen | Diferible: agrega gestión de un certificado dedicado; el aislamiento del volumen basta para la LAN (ADR-20) |

## 5. Consecuencias positivas

1. Reiniciar o redesplegar el contenedor ya no invalida la sesión del administrador ni rompe los formularios de acceso.
2. Los tokens antiforgery emitidos por una instancia se validan en la siguiente: se elimina la clase de fallo "HTTP 400 tras redeploy".
3. La rotación de llaves de Data Protection pasa a ser efectiva (antes se perdía en cada reinicio).

## 6. Consecuencias negativas y trade-offs

1. El despliegue debe montar y respaldar un volumen para `DataProtection:RutaLlaves`; perder ese volumen equivale a invalidar sesiones y tokens (no hay pérdida de datos de negocio, esos viven en la base de ADR-18).
2. Las llaves quedan en claro en el volumen; su confidencialidad depende de los permisos del host (mitigable a futuro con cifrado en reposo por certificado).

## 7. Implementación

- Composition root (`Web/Program.cs`): `AddDataProtection().SetApplicationName("SAI.Service.Core")` y, si `DataProtection:RutaLlaves` está definida, `PersistKeysToFileSystem(new DirectoryInfo(ruta))` (creando el directorio si falta).
- `appsettings.json` documenta la ranura `DataProtection:RutaLlaves` (vacía por defecto; en producción se inyecta por `DataProtection__RutaLlaves` apuntando al volumen).
- Nota de despliegue relacionada: los formularios SSR de acceso emiten **un único** token antiforgery (lo provee `EditForm`); un `<AntiforgeryToken/>` explícito adicional duplicaría el campo y también produciría HTTP 400 (corregido en la Etapa 1).

## 8. Métricas de validación

- Tras recrear el contenedor con el volumen de llaves montado, una sesión iniciada antes del reinicio sigue siendo válida y el POST del formulario de acceso no devuelve 400.
- El directorio de `DataProtection:RutaLlaves` contiene el archivo `key-*.xml` del keyring tras el primer arranque.

## 9. Referencias

- ADR-16 (sesión por cookie del administrador), ADR-20 (TLS y secretos en la LAN), ADR-24 (modelo de ambientes DEV/PROD), ADR-28 (autenticación de la API; la API es sin estado y no depende del keyring).
- CU-01 (acceso del administrador); Especificacion de despliegue de 09.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Decisión de Etapa 1: persistir el keyring de Data Protection en un volumen y fijar el nombre de aplicación, para que sesión y antiforgery sobrevivan a reinicios/redeploys. |
