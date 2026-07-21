# CU-01 — Autenticación y gestión de la sesión del administrador

**Proyecto:** Sai-Service-Core
**Documento:** CU-01-Autenticacion-Del-Administrador-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que la única persona que opera el sistema obtenga acceso autenticado al panel y a la API de administración, y gestione sus propias credenciales. Cubre el alta inicial del administrador en el primer arranque (cuando todavía no existe ninguno), el ingreso posterior con usuario y contraseña, el cambio de contraseña y el cierre de sesión. Es la puerta de entrada de todos los demás casos de uso.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Da de alta la cuenta única en el primer arranque, ingresa con sus credenciales y las gestiona |
| Servicio de acceso | Sistema | Valida credenciales, crea y cierra la sesión, y aplica la regla de existencia de un único administrador |

## 3. Precondiciones

- El servicio está en ejecución y accesible desde la LAN.
- Para el alta inicial: no existe todavía ninguna cuenta de administrador.
- Para el ingreso, el cambio de contraseña y el cierre de sesión: existe una cuenta de administrador dada de alta.

## 4. Flujo principal (ingreso del administrador ya dado de alta)

1. El administrador abre el panel desde un equipo de la LAN.
2. El servicio de acceso detecta que existe una cuenta de administrador y presenta la solicitud de credenciales.
3. El administrador ingresa usuario y contraseña.
4. El servicio de acceso verifica las credenciales contra la cuenta almacenada.
5. El servicio de acceso abre la sesión y redirige a la página principal del panel.
6. El panel queda operativo con las acciones de sesión disponibles (cambiar contraseña, cerrar sesión).

## 5. Flujos alternativos

- FA-1 Alta inicial en el primer arranque. Disparador: no existe ninguna cuenta de administrador. En el paso 2, el servicio de acceso presenta el formulario de alta en lugar del de ingreso; el administrador declara usuario y contraseña; el servicio crea la cuenta única, abre la sesión y redirige a la página principal. Retorna al paso 6 del flujo principal. A partir de este momento el formulario de alta deja de estar accesible.
- FA-2 Cambio de contraseña. Disparador: el administrador, ya autenticado, elige cambiar su contraseña. Ingresa la contraseña actual y la nueva; el servicio verifica la actual, reemplaza el resguardo de la contraseña y mantiene la sesión. Retorna al paso 6.
- FA-3 Cierre de sesión. Disparador: el administrador elige cerrar sesión. El servicio invalida la sesión y presenta nuevamente la solicitud de credenciales. Retorna al paso 2.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| CREDENCIAL_INVALIDA | Usuario o contraseña incorrectos en el ingreso | Rechaza el ingreso, no abre sesión y solicita nuevamente las credenciales sin revelar cuál de los dos campos falló |
| ADMIN_YA_EXISTE | Intento de acceder al alta inicial cuando ya existe un administrador | Rechaza el alta y redirige a la solicitud de credenciales |
| CONTRASENA_ACTUAL_INVALIDA | La contraseña actual provista en el cambio no coincide | No modifica la contraseña y conserva la vigente |
| SESION_REQUERIDA | Acceso a una página o endpoint protegido sin sesión activa | Redirige a la solicitud de credenciales y no ejecuta la operación pedida |

## 7. Postcondiciones

- Éxito (ingreso o alta): existe una sesión activa del administrador y el panel es operable. Tras el alta inicial existe exactamente una cuenta de administrador.
- Éxito (cambio de contraseña): la contraseña vigente es la nueva; la sesión sigue activa.
- Éxito (cierre de sesión): no hay sesión activa; el panel exige credenciales.
- Fallo: no hay sesión nueva ni cambios en la cuenta; el estado previo se conserva.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | El servicio arranca por primera vez y no existe ninguna cuenta de administrador | El administrador declara usuario `administrador` y una contraseña en el formulario de alta | El sistema crea la cuenta única, abre la sesión y redirige a la página principal |
| CA-02 | Ya existe la cuenta `administrador` | El administrador ingresa `administrador` con la contraseña correcta | El sistema abre la sesión y muestra la página principal del panel |
| CA-03 | Ya existe la cuenta `administrador` | El administrador ingresa `administrador` con una contraseña incorrecta | El sistema rechaza el ingreso con el código CREDENCIAL_INVALIDA y no abre sesión |
| CA-04 | Ya existe una cuenta de administrador y hay una sesión activa | Se solicita nuevamente la pantalla de alta inicial | El sistema responde ADMIN_YA_EXISTE y redirige a la solicitud de credenciales |
| CA-05 | El administrador tiene una sesión activa | Elige cerrar sesión y luego intenta abrir la página principal | El sistema invalida la sesión y responde SESION_REQUERIDA, exigiendo credenciales |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-05 (Seguridad operativa) — ver nota de alcance en §11 |
| Reglas de negocio aplicables | RN-01 (arranque seguro) de forma indirecta: el acceso no altera la modalidad operativa |
| Historias de usuario a generar | US a derivar en 06 para alta inicial, ingreso, cambio de contraseña y cierre de sesión |
| Componentes esperados | Superficie de autenticación y sesión del servicio (referencia tentativa a 05-Arquitectura-Tecnica) |
| Tests previstos | Pruebas de alta única, ingreso con credencial válida e inválida, unicidad del administrador y expulsión sin sesión (referencia tentativa a 08-Calidad-Y-Pruebas) |

## 10. Notas y supuestos

- El intake declara la autenticación como capacidad Must Have (F-15, §17 P.5) pero no la asocia a ninguna necesidad de negocio del catálogo 01. Se traza a NB-05 por ser la necesidad de seguridad más próxima. Se marca como ambigüedad para el orquestador: falta una NB dedicada al control de acceso, o la confirmación explícita de que la autenticación es un habilitador transversal sin NB propia.
- Alcance de un único administrador: no hay gestión de usuarios ni de roles (exclusión E-05 del intake). El sistema impide crear un segundo administrador.
- El detalle de interfaz (disposición de campos, mensajes) corresponde a 03-UX-UI-DX; aquí solo se define el qué.
- El mecanismo concreto de resguardo de la contraseña y de transporte seguro (TLS) es materia de 05-Arquitectura-Tecnica; TLS en la LAN queda como pendiente P-04.

## 11. Interacción multiusuario y concurrencia

Trivial: el sistema es monousuario por diseño (un único administrador). No hay sesiones concurrentes de distintos usuarios que coordinar.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §15 (etapas 3 y 4), §17 P.5 y F-15 |
