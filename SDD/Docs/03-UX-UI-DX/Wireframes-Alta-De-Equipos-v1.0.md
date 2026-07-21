# Wireframes — Alta de equipos y puesta en marcha

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Alta-De-Equipos-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Alta-De-Equipos**.

Superficie de catálogo e inventario para la puesta en marcha. Permite descubrir el dispositivo por su conexión física, declarar a mano marca, modelo y potencia nominal (con procedencia `declarado`), dar de alta el host y la batería, abrir los vínculos temporales de montaje y cobertura, probar la conexión y sembrar las cuatro verificaciones en "nunca verificado" (lo que fuerza el modo solo aviso). Deja el sistema listo para registrar historia. Origen: CU-02 (alta de equipos y puesta en marcha), UF-1.

## 2. Layout

Shell de trabajo (barra lateral + barra superior). Área de contenido con un bloque de descubrimiento arriba y un formulario de inventario debajo. Usa el patrón "ABM — formulario de edición" §4.4 del base para la grilla de campos y la barra superior de la sección.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Alta de equipos                                    <sello v>   |  <h1> + sello (shell)
|  · Estado      |  +--------- Descubrimiento del dispositivo ------------------+  |
|  · Verificac.  |  | Candidatos USB:                              [Descubrir]  |  |
|  · Equipos <-  |  | 0665:5161 · INNO TECH · iSerial vacio   [Seleccionar]     |  |
|  · ...         |  +-----------------------------------------------------------+  |
|                |                                                                 |
|                |  +--------- Datos declarados del SAI -----------------------+  |
|                |  | Marca            [ campo ]     Modelo      [ campo ]       |  |
|                |  | Potencia nominal [ campo ] (VA)                           |  |
|                |  | <requisito: serial anulable; potencia desconocida = null> |  |
|                |  +-----------------------------------------------------------+  |
|                |                                                                 |
|                |  +--------- Bateria montada --------------------------------+  |
|                |  | N. de serie [ campo, anulable ]  Modelo [ campo ]         |  |
|                |  | Fecha compra [ campo ]  Fecha fabricacion [ campo ]       |  |
|                |  +-----------------------------------------------------------+  |
|                |                                                                 |
|                |  +--------- Host protegido ---------------------------------+  |
|                |  | Nombre [ campo ]   Criticidad [ select ]                  |  |
|                |  +-----------------------------------------------------------+  |
|                |                                                                 |
|                |  [ Probar conexion ]              [====== Dar de alta ======]  |  pie
+----------------+-----------------------------------------------------------------+
```

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Bloque de descubrimiento | Identificar el dispositivo por su conexión física | Candidatos USB con descriptor VID:PID, fabricante, serial | El adaptador identifica el candidato; puede venir sin marca ni modelo |
| Formulario datos del SAI | Declarar lo que el equipo no expone | marca, modelo, potencia nominal (VA) | Los valores quedan con procedencia `declarado`; potencia desconocida = `null` con procedencia `imputado`, nunca un número inventado |
| Requisito declarado (patrón §4.5 Primer-Arranque) | Enunciar en positivo qué es válido antes del intento | Serial anulable; fecha de fabricación anterior a compra es normal | Asociado por `aria-describedby` |
| Formulario batería | Alta de la batería montada | serie (anulable), modelo, fechas | Valida coherencia de fechas sin tratarla como error cuando fabricación < compra |
| Formulario host | Alta del host protegido | nombre, criticidad | — |
| Acción "Probar conexión" | Verificar la comunicación antes de comprometer el alta | resultado de la prueba | Valida por efecto observado (RN-03) |
| Acción primaria "Dar de alta" | Crear catálogo, inventario, vínculos y sembrar verificaciones | — | Abre `MontajeBateria` y `CoberturaHost` con `hasta = null`; siembra 4 verificaciones en nunca verificado |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Descubrir | Activar "Descubrir" | Lista de candidatos USB con sus descriptores | Adaptador disponible |
| Seleccionar candidato | Activar "Seleccionar" | Precarga VID:PID; deja marca/modelo a declarar | Hay un candidato |
| Probar conexión | Activar "Probar conexión" | Resultado por efecto observado; habilita el alta si responde | Candidato seleccionado |
| Dar de alta | Activar "Dar de alta" | Crea catálogo + inventario + vínculos; siembra verificaciones; el sistema queda en solo aviso; navega al panel con "0 de 4 supuestos" | Datos válidos; conexión probada |
| Corregir dato inválido | Enviar con un campo mal formado | Error inline asociado al campo con la regla en positivo | — |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin dispositivos) | Aún no se ejecutó el descubrimiento o no hay candidatos | Estado vacío con acción "Descubrir" y texto orientativo |
| Cargando (descubriendo / probando) | Descubrimiento USB o prueba de conexión en curso | Spinner en la acción; el resto del formulario sigue accesible |
| Con datos (candidato / inventario) | Candidato encontrado o formularios completados | Formularios poblados; acción primaria disponible |
| Error (prueba de conexión fallida) | El equipo no responde a la prueba | Mensaje con causa y acción; validado por efecto observado, no por ausencia de error |
| Error (dato obligatorio inválido) | Vida de flotación sin temperatura de referencia (RN-13); dato mal formado | Error inline en el campo con la regla enunciada en positivo |
| Descubierto sin marca ni modelo | El adaptador devuelve `0665:5161 · INNO TECH · iSerial vacío` | El bloque de descubrimiento marca "sin marca ni modelo"; se piden a mano con procedencia `declarado` |

## 6. Versión móvil o responsive

La grilla de campos (`minmax`) pasa a una columna bajo ~768px; la barra lateral colapsa a drawer. El bloque de descubrimiento y las acciones del pie se apilan. Los pares fecha compra / fecha fabricación se mantienen próximos para leer su relación. Legible sin scroll horizontal a 320px.

## 7. Notas de implementación

- Accesibilidad: labels asociados a cada control; requisito declarado por `aria-describedby` antes del intento; errores inline asociados al campo (`aria-describedby`) con el rango o la regla admitida; foco al primer campo inválido tras el envío; ningún estado solo por color.
- Procedencia como concepto de UI: los campos declarados se distinguen de los medidos; la potencia desconocida se representa como ausencia con motivo (`imputado`), no como cero ni como valor inventado (RN-05, R-13).
- Primer arranque: al completarse, el destino es el panel con la orientación posterior ya consumida; el alta fuerza el modo solo aviso sembrando las verificaciones en nunca verificado.
- Validación por efecto observado (RN-03): la prueba de conexión no se da por buena porque no haya excepción.
- Sin CSS ni colores: layout, jerarquía y comportamiento solamente.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-02 (alta de equipos y puesta en marcha); UF-1 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.1 (puesta en marcha) |
| Reglas de negocio relevantes | RN-01 (arranque seguro), RN-05 (procedencia), RN-13 (vida de flotación con temperatura); RN-06 indirecta |
| US a generar en 06 | US-01 (alta del SAI y la batería descubriendo el USB), US-05 (base: procedencia de cada valor) |
| Tests previstos en 08 | Dispositivo sin número de serie válido; rechazo de vida de flotación sin temperatura; apertura de vínculos sin hueco; siembra de verificaciones y forzado de solo aviso; prueba de conexión por efecto observado |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A (inventario, no parámetros de configuración) |
| Primer arranque aplicado | sí (paso de la orientación posterior; deja el sistema operable en solo aviso; destino al completar declarado) |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Alta-De-Equipos: descubrimiento del dispositivo por conexión física, formularios de SAI/batería/host con procedencia declarada, requisito declarado antes del intento, prueba de conexión por efecto observado, siembra de verificaciones que fuerza solo aviso. Tabla de estados (vacío/cargando/con datos/error + descubierto sin marca), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
