# Wireframes — Acceso al panel (login)

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Acceso-Login-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Acceso-Login**.

Superficie de acceso del perfil de operador único. Con el sistema ya aprovisionado y sin sesión activa, el administrador ingresa con su usuario y su contraseña para alcanzar el panel de trabajo. Es también el destino al completar el primer arranque y el destino al que devuelve una sesión vencida. Lleva el sello de identidad de versión (ubicación obligatoria de acceso). Origen: CU-01 (ingreso, cierre de sesión).

## 2. Layout

Shell de acceso: lienzo vacío, sin navegación, con una tarjeta de acceso anclada a la franja superior (patrón "Tarjeta de acceso al sistema" §4.1 y "Banda de resultado por código" §4.2 de Design-Rules-Acceso-Monousuario). Comparte composición con Alta-Inicial-Administrador para dar continuidad.

```text
+---------------------------- lienzo, sin chrome ----------------------------+
|                                                                            |
|                 +--------------- ~380px ----------------+                  |
|                 |  Iniciar sesion                       |  <h1> titulo     |
|                 |  Es la unica cuenta del sistema.      |  subtitulo       |
|                 |  [ banda confirmacion  role=status ]  |  condicional     |
|                 |  [ banda de error      role=alert  ]  |  condicional     |
|                 |  Usuario                              |  label           |
|                 |  [ campo identificador             ]  |                  |
|                 |  Contraseña                           |  label           |
|                 |  [ campo contraseña                ]  |                  |
|                 |  [========== Ingresar ============]   |  ancho completo  |
|                 |             <sello de version>        |  al pie, sutil   |
|                 +---------------------------------------+                  |
|   sin registro · sin recordarme · sin recuperacion · sin selector          |
+----------------------------------------------------------------------------+
```

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Encabezado de tarea (`<h1>`) | Nombrar la acción | "Iniciar sesión" | Presente sin navegación |
| Subtítulo de alcance | Explicar la ausencia de opciones multiusuario | "Es la única cuenta del sistema" | Estático |
| Banda de resultado por código | Acusar rechazo, restricción, confirmación entrante o sesión vencida | Texto único resuelto desde el catálogo de códigos | Error `role="alert"`, confirmación `role="status"`; nunca el código crudo |
| Campo identificador | Capturar el usuario | Valor tecleado | Propósito declarado para el gestor de credenciales |
| Campo contraseña | Capturar la contraseña | Enmascarado | — |
| Acción primaria | Ingresar | "Ingresar" | Ancho completo; sin acciones secundarias |
| Sello de versión | Identificar la instancia antes de entrar | `versionLegible` + distintivo/marcador según contrato | Abre el detalle de diagnóstico con copiado en un gesto |

Omisiones declaradas (no se dibujan, ni deshabilitadas): registro de cuentas, selector o listado de cuentas, recuperación de la contraseña, persistencia opcional de sesión ("recordarme"), roles o permisos visibles.

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Ingresar | Activar "Ingresar" | Se crea la sesión; navegación completa al shell de trabajo (Panel-Estado-En-Vivo) | Par usuario/contraseña válido; sistema aprovisionado |
| Rechazo de credenciales | Par inválido | Banda de error con rechazo indiferenciado; el foco vuelve a la banda o al primer campo | — |
| Llegar desde el primer arranque | `destinoAlCompletar` tras crear la identidad | Banda de confirmación "identidad creada; ya podés ingresar" | Sistema recién aprovisionado |
| Llegar por sesión vencida | La sesión expiró por inactividad o tope | Banda con estado "sesión vencida", sin culpar al usuario | Había sesión y venció |
| Abrir diagnóstico de versión | Activar el sello | Detalle de versión completo, copiable | — |
| Entrar con sistema sin aprovisionar | Guard detecta predicado falso | Redirige a Alta-Inicial-Administrador | `estaAprovisionado` falso |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío | No aplica: es un formulario de acto, no una lista | Documentado como no aplicable; siempre presenta los campos |
| Cargando (enviando) | Se activó "Ingresar" | Acción en estado de envío; sin doble envío |
| Con datos (listo para ingresar) | Sin sesión, sistema aprovisionado | Tarjeta de acceso sobre shell vacío; foco inicial en el identificador |
| Error (credenciales rechazadas) | El par no valida | Banda de error, rechazo indiferenciado, sin decir qué parte falló |
| Error (acceso restringido temporalmente) | Se superó el umbral de intentos de la política | Banda de error que declara la restricción temporal, sin umbrales ni cuenta regresiva |
| Error (formulario vencido) | La protección del formulario expiró | Banda de error que pide reintentar, sin detalle técnico |
| Identidad recién creada | Se llega desde el primer arranque | Banda de confirmación con qué se creó y qué hacer ahora |
| Contraseña actualizada | Se llega desde el cambio de contraseña | Banda de confirmación con qué cambió y el efecto sobre la sesión |
| Sesión expirada | La sesión venció | Retorno a esta superficie con estado "sesión vencida" declarado |

## 6. Versión móvil o responsive

Igual que Alta-Inicial-Administrador: tarjeta angosta anclada arriba, sin chrome que colapsar, legible sin scroll horizontal a 320px. El sello de versión y la línea de omisiones se reordenan bajo la tarjeta sin perder legibilidad.

## 7. Notas de implementación

- Accesibilidad: `<h1>` presente sin navegación; banda de error `role="alert"` y confirmación `role="status"`; propósito de los campos declarado para gestores de credenciales y tecnologías asistivas; acción de ingreso con etiqueta explícita; tras rechazo, foco a la banda o al primer campo; ningún estado solo por color; contraste AA en el sello pese a su baja jerarquía.
- Acceso monousuario: catálogo de códigos de resultado con texto único por resultado (no se compone por vista); rechazo indiferenciado; ningún mensaje expone parámetros de política; la restricción temporal no muestra contador; duración de sesión única y declarada (su valor exacto es política de 05/09); el vencimiento devuelve acá con estado explícito, sin vencimiento silencioso.
- Identidad de versión: sello en la superficie de acceso (una de las dos ubicaciones obligatorias); la otra es Panel-Estado-En-Vivo.
- Shell partido: la transición de acceso a trabajo es una navegación completa; el cambio de shell es la señal de que la sesión cambió.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-01 (ingreso, cierre de sesión) |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.2 (ingreso), §8 (errores de acceso y sesión) |
| Reglas de negocio relevantes | RN-01 (indirecta: el acceso no altera la modalidad) |
| US a generar en 06 | US de acceso — ingreso, cierre de sesión (derivadas de CU-01) |
| Tests previstos en 08 | Ingreso con credencial válida e inválida; rechazo indiferenciado; expulsión sin sesión; retorno por sesión vencida; sello de versión presente |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A |
| Primer arranque aplicado | sí (es el `destinoAlCompletar`; guard que redirige a alta inicial si el predicado es falso) |
| Acceso de operador único aplicado | sí (shell de acceso, omisiones declaradas, catálogo de resultados, política de sesión) |
| Identidad de versión aplicada | sí (sello obligatorio de acceso; detalle de diagnóstico) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie de acceso Acceso-Login: shell partido, omisiones del perfil de operador único, banda de resultado por código, sello de versión obligatorio, tabla de estados (vacío N/A, cargando, con datos, error y estados propios de acceso/sesión), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
