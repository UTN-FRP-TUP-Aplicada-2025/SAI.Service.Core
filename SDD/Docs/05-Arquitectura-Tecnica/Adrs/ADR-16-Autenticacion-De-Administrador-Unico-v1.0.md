# ADR-16 — Autenticación de administrador único

**Proyecto:** Sai-Service-Core
**Documento:** ADR-16-Autenticacion-De-Administrador-Unico-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

El sistema tiene un único administrador que es propietario, implementador y beneficiario. La fuente pide *"Autenticación mínima"*, sin gestión de usuarios ni roles (exclusión E-05). El panel y la API se exponen en la LAN. El sistema, al iniciar por primera vez, debe dar de alta al administrador y, a partir de ahí, exigir login. Este es uno de los cinco ADR obligatorios de web-monolith (autenticación), que ningún pre-ADR PA cubre explícitamente. Motivan la decisión el caso de uso CU-01 (autenticación del administrador) y la capacidad F-15.

## 2. Decisión

Se adopta ASP.NET Core Identity con un solo rol, `administrador`. La contraseña se almacena con el hash por defecto de Identity (PBKDF2), la sesión se sostiene por cookie, y las acciones de cerrar sesión y cambiar contraseña están disponibles desde la barra superior. La pantalla de alta inicial es accesible únicamente mientras no exista ningún administrador; el endpoint de salud queda sin autenticación. Todo otro endpoint y toda otra página requieren autenticación.

## 3. Estado

Aceptado el 2026-07-20. ADR obligatorio de web-monolith (autenticación), derivado de CU-01 y F-15. No proviene de un PA.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| ASP.NET Core Identity, rol único, PBKDF2, cookie | Estándar del stack; hash robusto por defecto; login/logout/cambio de contraseña listos | Trae más superficie de la que un solo usuario necesita (gestión de usuarios no usada) |
| Contraseña única en variable de entorno | Trivial | Sin hash gestionado, sin cambio de contraseña, sin sesión; frágil |
| Proveedor de identidad externo (OIDC) | Delegación de credenciales | Desproporción: un solo usuario en LAN sin internet; dependencia externa innecesaria |

## 5. Consecuencias positivas

1. Login, logout y cambio de contraseña resueltos con el mecanismo estándar del stack (CU-01, F-15).
2. El hash PBKDF2 por defecto protege la contraseña sin implementar criptografía a mano.
3. El alta inicial autolimitada (solo mientras no exista administrador) evita un endpoint de registro abierto permanente.

## 6. Consecuencias negativas y trade-offs

1. Identity incorpora tablas y superficie (gestión de usuarios/roles) que este sistema no usa (E-05): se acepta a cambio de no reimplementar autenticación.
2. La cookie de sesión sobre LAN sin TLS resuelto (P-04, ADR-20) deja el tránsito sin cifrar hasta que se decida el TLS: dependencia con ADR-20.
3. No hay recuperación de contraseña por correo (la red cae en un corte); el reseteo es manual/administrativo.

## 7. Implementación

ASP.NET Core Identity en `Api`/`Web` con un rol `administrador`. Migración inicial de las tablas de Identity junto al resto del esquema (ADR-18). Middleware de autorización que exige login en todo endpoint y página salvo el alta inicial (condicionada a la inexistencia de administrador) y el endpoint de salud. Barra superior con logout y cambio de contraseña (etapa 4 de §15). Secretos (cadena de conexión) por variables de entorno (P.5).

## 8. Métricas de validación

- CU-01: alta inicial disponible solo sin administrador; luego login obligatorio.
- Toda página del panel y todo endpoint (salvo salud) rechazan el acceso no autenticado.
- La contraseña se persiste hasheada (PBKDF2), nunca en claro.

## 9. Referencias

- Intake §17 P.5, P.1; exclusión E-05; §15 etapas 3 y 4.
- CU-01 Autenticación del administrador; F-15 Autenticación mínima de administrador único.
- ADR relacionadas: ADR-18 (persistencia/migraciones), ADR-20 (TLS, pendiente P-04), ADR-17.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. ADR obligatorio de web-monolith (autenticación). |
