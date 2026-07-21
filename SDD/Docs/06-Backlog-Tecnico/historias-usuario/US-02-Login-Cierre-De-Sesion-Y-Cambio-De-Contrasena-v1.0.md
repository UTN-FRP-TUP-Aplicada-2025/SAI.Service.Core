# US-02 — Login, cierre de sesión y cambio de contraseña

**Proyecto:** Sai-Service-Core
**Documento:** US-02-Login-Cierre-De-Sesion-Y-Cambio-De-Contrasena-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-03 Persistencia, alta de administrador y sesión
**Prioridad MoSCoW:** Must
**Estimación:** 3 SP (Fibonacci)

## 1. Historia
Como administrador, quiero ingresar con mis credenciales, cerrar sesión y cambiar mi contraseña desde la barra superior, para controlar quién opera el sistema y rotar la clave sin depender de nadie.

## 2. Contexto
NB-05 fija la autenticación mínima. CU-01 describe el ingreso contra la cuenta almacenada y las acciones de sesión disponibles en la barra superior. Es la etapa 4 de §15; a partir de aquí toda función exige sesión activa (SESION_REQUERIDA).

## 3. Criterios de aceptación
- Given una cuenta de administrador existente, When ingresa el usuario y la contraseña correctos, Then el sistema abre la sesión y muestra las acciones de cerrar sesión y cambiar contraseña en la barra superior.
- Given un ingreso con credenciales incorrectas, When el administrador confirma, Then el sistema rechaza el acceso (CREDENCIAL_INVALIDA) sin revelar qué campo falló.
- Given una sesión activa, When el administrador cambia la contraseña con la clave actual válida y una nueva que cumple el mínimo, Then el sistema actualiza el hash y mantiene la sesión.
- Given un intento de acceder a una función de administración sin sesión, When se solicita la página, Then el sistema redirige al ingreso.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-05 |
| CU cubiertos | CU-01 |
| BT derivadas | BT-10 |
| Tests previstos | acceptance/AT-02-sesion-administrador |

## 5. Prioridad y estimación
Must: sin ingreso ni cierre de sesión el sistema no es operable de forma segura. 3 SP; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-01
- [x] NB de origen (NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Sin dependencias bloqueantes (US-01 y BT-10 previas)

## 7. Notas y supuestos
El endpoint `/health` queda público sin autenticación (ADR-16); todo lo demás exige sesión. La recuperación de contraseña olvidada no está en alcance: al ser una sola persona con acceso físico al host, se resuelve por reinicialización operativa.
