# Wireframes — Alta inicial del administrador

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Alta-Inicial-Administrador-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Alta-Inicial-Administrador**.

Superficie de aprovisionamiento del primer arranque. La primera vez que se abre una instancia recién desplegada, cuando todavía no existe ningún administrador, el sistema pide crear la única identidad de operación (usuario y secreto). Es un acto explícito, indivisible e irreversible desde la UI, dibujado sin chrome de navegación y sin acción de cancelar, porque no hay estado previo al que volver. Al completarse, el sistema queda aprovisionado y esta superficie deja de existir para esa instancia. Origen: CU-01 (alta inicial del administrador), UF-1.

## 2. Layout

Shell partido: shell vacío, solo el lienzo y una tarjeta de aprovisionamiento anclada a la franja superior (patrón "Shell partido" §4.1 y "Tarjeta de aprovisionamiento" §4.2 de Design-Rules-Primer-Arranque). Ancho de tarjeta acotado (~380px). Sin barra lateral, sin barra superior, sin navegación de módulos.

```text
+---------------------------- lienzo, sin chrome ----------------------------+
|                                                                            |
|                 +--------------- ~380px ----------------+                  |
|                 |  Crear la cuenta de administrador     |  <h1> titulo     |
|                 |  Es la unica cuenta del sistema.      |  subtitulo       |
|                 |  [ banda de resultado   role=alert ]  |  condicional     |
|                 |  Usuario                              |  label           |
|                 |  [ campo identificador             ]  |                  |
|                 |  Secreto                              |  label           |
|                 |  [ campo secreto                   ]  |                  |
|                 |  <requisito de politica del secreto>  |  §4.5 declarado  |
|                 |  Repetir secreto                      |  label           |
|                 |  [ campo confirmacion              ]  |                  |
|                 |  [====== Crear administrador ======]  |  ancho completo  |
|                 |             <sello de version>        |  al pie, sutil   |
|                 +---------------------------------------+                  |
|                                                                            |
+----------------------------------------------------------------------------+
```

No se dibuja registro, ni selector de cuenta, ni "recordarme", ni recuperación: en el primer arranque no existen y no se muestran ni deshabilitados.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Encabezado de tarea (`<h1>`) | Nombrar el acto único | "Crear la cuenta de administrador" | Presente aunque no haya navegación (semántica AA) |
| Subtítulo de alcance | Declarar la unicidad de la identidad | "Es la única cuenta del sistema" | Estático; explica por qué faltan las opciones habituales |
| Banda de resultado (patrón §4.4 Primer-Arranque / §4.2 Acceso-Monousuario) | Acusar error o confirmación | Texto resuelto desde el catálogo de códigos | `role="alert"` en error; no se compone en la vista |
| Campo identificador | Capturar el usuario | Valor tecleado | Propósito declarado para el gestor de credenciales |
| Campo secreto | Capturar el secreto | Enmascarado | — |
| Requisito de política del secreto (patrón §4.5) | Declarar la regla antes del intento, en positivo | Regla de la política de secreto (derivada del sistema, no literal en la vista) | Asociado por `aria-describedby` |
| Campo confirmación | Evitar el error de tipeo del secreto | Enmascarado | Valida coincidencia antes de enviar |
| Acción primaria | Ejecutar el aprovisionamiento | "Crear administrador" | Ancho completo; sin acción secundaria ni escape |
| Sello de versión (patrón §4.1 Identidad-De-Version) | Identificar la instancia antes de autenticarse | `versionLegible` (+ distintivo si preliminar, + marcador si origen indeterminado) | Abre el detalle de diagnóstico; derivado de la construcción, no compuesto |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Resolver destino | Entrada a la instancia | Si el predicado `estaAprovisionado` es falso, se muestra esta superficie; si es verdadero, redirige a Acceso-Login | Guard de ruteo consulta el predicado único |
| Enviar el alta | Activar "Crear administrador" | Se crea la identidad; el sistema queda aprovisionado; redirige a `destinoAlCompletar` (Acceso-Login) con banda de confirmación "identidad creada" | Secreto cumple la política y coincide con la confirmación |
| Abrir detalle de diagnóstico | Activar el sello de versión | Panel con el contrato de versión completo y copiado en un solo gesto | — |
| Intento fuera de tiempo | Enviar cuando el sistema ya se aprovisionó entre la carga y el envío | Guard de la acción redirige de forma neutra a Acceso-Login, sin exponer el motivo | El predicado pasó a verdadero |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío | No aplica: es un acto único, no una lista | Se documenta como no aplicable; la superficie siempre presenta el formulario |
| Cargando (resolviendo destino) | El predicado todavía no respondió | Indicador de progreso indeterminado; nunca queda en blanco ni parpadea contenido que luego se retira |
| Con datos (formulario listo) | Predicado falso; sistema sin administrador | Tarjeta de aprovisionamiento sobre shell vacío; foco inicial en el primer campo |
| Error (requisito no cumplido) | El secreto viola la política declarada | Borde de error en el campo + banda de error con la regla violada, enunciada igual que en el requisito |
| Error (confirmación no coincidente) | El secreto y su repetición difieren | Banda de error con la discrepancia y qué hacer |
| Enviando | Se activó la acción primaria | Acción en estado de envío; sin doble envío |
| Envío fuera de tiempo | El sistema se aprovisionó entre carga y envío | Redirección neutra a Acceso-Login; sin mensaje en la superficie abandonada |
| Aprovisionado (redirige) | Predicado verdadero al entrar | No se muestra la superficie; guard redirige a Acceso-Login |

## 6. Versión móvil o responsive

La tarjeta de ancho acotado ya es la forma angosta: en viewport chico ocupa el ancho disponible con márgenes mínimos, anclada arriba (no centrada verticalmente, para no saltar al abrir el teclado en pantallas táctiles). Sin barra lateral que colapsar (no hay chrome). Contenido legible sin scroll horizontal a 320px. Aunque el uso previsto es de escritorio en la LAN, la superficie no se degrada en pantallas chicas.

## 7. Notas de implementación

- Accesibilidad: `<h1>` presente pese a la ausencia de navegación; banda de error `role="alert"` y banda de confirmación (en la superficie siguiente) `role="status"`; requisito de secreto asociado por `aria-describedby` y anunciado antes del intento; foco inicial en el primer campo y, tras un error, en la banda o el primer campo inválido; foco visible AA; ningún estado solo por color.
- Primer arranque: predicado único de aprovisionamiento (`estaAprovisionado` = existe administrador); artefacto mínimo = la cuenta de administrador; guard en tres capas (ruteo, superficie, acción) contra ese mismo predicado; sin cancelar; `destinoAlCompletar` explícito = Acceso-Login; el acto cierra el lazo con la banda de confirmación en la superficie siguiente.
- Acceso monousuario: esta superficie crea la identidad única; comparte composición con el shell de acceso para dar continuidad entre crear la identidad y usarla.
- Identidad de versión: el sello al pie es una de las dos ubicaciones obligatorias (superficie de acceso). Se deriva de la construcción; muestra el marcador de origen indeterminado en ejecución local y el distintivo de preliminar en artefactos `-alpha.N`.
- Performance percibida: la resolución del destino es breve por contrato; se muestra como estado del sistema, no como pantalla en blanco.
- Sin CSS ni colores: la capa visual vive en el catálogo y en 05.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-01 (alta inicial del administrador); UF-1 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.1 (puesta en marcha) |
| Reglas de negocio relevantes | RN-01 (arranque seguro, indirecta: el acceso no altera la modalidad) |
| US a generar en 06 | US de acceso — alta inicial del administrador (derivada de CU-01) |
| Tests previstos en 08 | Alta única e idempotencia del arranque; guard en tres capas; rechazo de secreto que viola la política; envío fuera de tiempo redirige; acuse de recibo en la superficie siguiente |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A |
| Primer arranque aplicado | sí (predicado, guard en tres capas, superficie sin chrome ni cancelar, destino al completar declarado, orientación posterior en el destino) |
| Acceso de operador único aplicado | sí (crea la identidad única; shell de acceso compartido) |
| Identidad de versión aplicada | sí (sello en la superficie de acceso; detalle de diagnóstico) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación visual de maqueta | N/A (pendiente Fase B2) |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie de aprovisionamiento del primer arranque con nombre canónico Alta-Inicial-Administrador: layout sin chrome, componentes, interacciones con guard en tres capas, tabla de estados (incluye vacío N/A, cargando, con datos, error), responsive, notas de accesibilidad y de las cuatro extensiones aplicables, trazabilidad. Maqueta-aware. |
