# US-01 — Alta inicial del administrador único

**Proyecto:** Sai-Service-Core
**Documento:** US-01-Alta-Inicial-Del-Administrador-Unico-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Épica:** EP-03 Persistencia, alta de administrador y sesión
**Prioridad MoSCoW:** Must
**Estimación:** 3 SP (Fibonacci)

## 1. Historia
Como administrador, quiero dar de alta mi cuenta la primera vez que abro el panel definiendo usuario y contraseña, para acceder al sistema sin que nadie más pueda apropiarse de la instalación.

## 2. Contexto
NB-05 exige seguridad operativa y ancla la autenticación mínima del administrador único (F-15). CU-01 describe que, al iniciar por primera vez, el servicio detecta que no existe cuenta y pide credenciales para crearla, redirigiendo a la página principal. Es la etapa 3 de §15 y la puerta de entrada de todos los demás CU.

## 3. Criterios de aceptación
- Given un servicio recién instalado sin ninguna cuenta, When el administrador abre el panel y define usuario y contraseña, Then el sistema crea la cuenta, abre sesión y redirige a la página principal.
- Given una instalación donde ya existe la cuenta de administrador, When alguien intenta acceder a la pantalla de alta inicial, Then el sistema rechaza el segundo alta (ADMIN_YA_EXISTE) y ofrece el ingreso normal.
- Given un intento de alta con contraseña vacía o que no cumple el mínimo, When el administrador confirma, Then el sistema no crea la cuenta y explica el requisito sin revelar detalles internos.

## 4. Trazabilidad
| Dimensión | Referencia |
| --- | --- |
| NB upstream | NB-05 |
| CU cubiertos | CU-01 |
| BT derivadas | BT-07, BT-10 |
| Tests previstos | acceptance/AT-01-alta-inicial-administrador |

## 5. Prioridad y estimación
Must: sin cuenta de administrador no hay acceso a ninguna función del sistema. 3 SP por analogía con altas simples de una sola pantalla; técnica Fibonacci.

## 6. DoR check
- [x] Valor explícito para el rol
- [x] Trazabilidad a CU-01
- [x] NB de origen (NB-05) identificada
- [x] Criterios en Given/When/Then con happy path y edge case
- [x] Estimada en SP (Fibonacci)
- [x] Sin dependencias bloqueantes (depende de BT-07 y BT-10, planificadas antes en EP-03)

## 7. Notas y supuestos
El algoritmo de hash y la sesión los fija ADR-16 (no son criterios de esta US). La gestión de múltiples usuarios y roles está fuera de alcance (F-31, Won't v1).
