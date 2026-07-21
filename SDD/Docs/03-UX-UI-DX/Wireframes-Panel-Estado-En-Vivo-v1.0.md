# Wireframes — Panel de estado en vivo

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Panel-Estado-En-Vivo-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Panel-Estado-En-Vivo**.

Home del shell de trabajo y superficie central del uso cotidiano (el 80 % del tiempo). Muestra el estado en vivo del SAI (estado y tensiones, batería con la carga marcada como derivada), la conectividad, cuántos de cuántos supuestos están verificados, la modalidad efectiva y los eventos recientes con su regla y versión. Cuando la política está degradada, lo declara en la pantalla principal con un banner de bloqueo, no enterrado en configuración. Es el destino tras el ingreso y, con el parque aún sin dar de alta, hospeda la orientación posterior al primer arranque. Origen: CU-04 (monitoreo en vivo), con la visibilidad del bloqueo de CU-05.

## 2. Layout

Shell de trabajo completo (patrón §3.1 del base): barra lateral de navegación de módulos + barra superior con la identidad activa y las acciones de identidad + área de contenido sobre el lienzo. Área de contenido en bloques escaneables (ley de Miller).

```text
+----------------------------------------------------------------------------------+
| [=] Sai-Service-Core        <identidad>  [Cambiar secreto]  [Salir]   v: <sello>  |  barra superior + sello
+----------------+-----------------------------------------------------------------+
| navegacion     |  [ banner de bloqueo por verificacion   role=alert ]            |  condicional, persistente
|  · Estado      |                                                                 |
|  · Verificac.  |  +--------- Estado del SAI ---------+  +---- Conectividad ----+  |
|  · Parque      |  | estado: [badge En linea]         |  | SAI: [badge OK]      |  |
|  · Politicas   |  | input.voltage   232,9 V          |  | ultimo sondeo: 2 s   |  |
|  · Pruebas     |  | output.voltage  230,1 V          |  | calidad: completa    |  |
|  · Historicos  |  | ups.load        13 %             |  +----------------------+  |
|  · Informes    |  +----------------------------------+                            |
|                |                                                                 |
|                |  +--------- Bateria ----------------+  +---- Supuestos -------+  |
|                |  | battery.voltage 13,24 V [medido] |  | 0 de 4 verificados   |  |
|                |  | battery.charge  ~ [derivado]     |  | modalidad efectiva:  |  |
|                |  | estado: [badge Flotacion]        |  | [badge Solo aviso]   |  |
|                |  +----------------------------------+  | [Ir a verificar ->]  |  |
|                |                                        +----------------------+  |
|                |                                                                 |
|                |  +------------- Eventos recientes -----------------------------+ |
|                |  | 12:03  Microcorte   5 s (+-10 s)   regla v2                 | |
|                |  | 11:40  RetornoRed                  regla v2                 | |
|                |  +-------------------------------------------------------------+ |
+----------------+-----------------------------------------------------------------+
```

Los valores del ASCII son ilustrativos del tipo de dato (tomados de los escenarios E-2/E-3), no una maqueta ni valores fijos.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Barra de identidad (patrón §4.3 Acceso-Monousuario) | Mostrar la identidad activa y las acciones de identidad | Usuario; "Cambiar secreto"; "Salir" | Cierre de sesión a un clic; navega a shell de acceso |
| Sello de versión (patrón §4.1 Identidad-De-Version) | Identificar la instancia en el sistema en funcionamiento | `versionLegible` + distintivo/marcador | Segunda ubicación obligatoria del sello; abre el detalle de diagnóstico |
| Banner de bloqueo por verificación | Declarar la política degradada en la pantalla principal | Motivo del bloqueo; modalidad efectiva | Persistente mientras haya bloqueo; `role="alert"`; enlaza a Panel-De-Verificaciones |
| Tarjeta Estado del SAI | Estado y tensiones en vivo | estado (badge), `input.voltage`, `output.voltage`, `ups.load` | Se actualiza por empuje del servidor; números tabulares |
| Tarjeta Batería | Estado de batería con procedencia | `battery.voltage` [medido], `battery.charge` [derivado], estado | La carga de batería siempre marcada como derivada (CA-03) |
| Tarjeta Conectividad | Salud de la comunicación con el equipo | último sondeo, calidad de la última muestra, badge de conexión | Alerta a los 3 sondeos fallidos consecutivos |
| Tarjeta Supuestos | Estado de verificación y modalidad | "n de m verificados", badge de modalidad efectiva | Enlace a la ventana de mantenimiento |
| Lista de eventos recientes | Traza de lo derivado | evento, hora, duración con incertidumbre, regla y versión | Cada evento con su `reglaDerivacion` y `reglaVersion` |
| Orientación posterior (patrón §4.6 Primer-Arranque) | Sugerir próximos pasos cuando no hay parque | tarjetas de acceso: dar de alta el parque, configurar política, ventana de mantenimiento | Solo en estado vacío; orienta sin bloquear |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Recibir estado en vivo | Empuje del servidor cada ronda de sondeo | Las tarjetas y la lista de eventos se actualizan | Sesión de sondeo activa |
| Ir a verificar | Activar el enlace del banner o de la tarjeta de supuestos | Navega a Panel-De-Verificaciones | Hay supuestos sin verificar |
| Cerrar sesión | Activar "Salir" | Navegación completa al shell de acceso | Sesión activa |
| Abrir diagnóstico de versión | Activar el sello | Detalle de versión completo, copiable en un gesto | — |
| Dar de alta el parque | Activar la tarjeta de orientación | Navega a Alta-Del-Parque | Estado vacío (sin parque) |
| Alerta de desconexión | 3 sondeos consecutivos sin respuesta | Aparece la alerta de conectividad y el evento de desconexión; se anuncia por `aria-live` | — |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin parque, orientación) | Aprovisionado y con sesión, pero sin parque dado de alta | Grilla de tarjetas de acceso de orientación posterior (dar de alta el parque, configurar política, ventana de mantenimiento); sin tarjetas de estado |
| Cargando | Esperando el primer empuje de estado tras abrir | Skeleton de las tarjetas de estado (por encima de ~400 ms) |
| Con datos | Sesión de sondeo activa y respondiendo | Tarjetas de estado, batería, conectividad y supuestos pobladas; eventos recientes listados |
| Error (sin conexión con el SAI) | 3 sondeos consecutivos sin respuesta / nodo USB ausente | Alerta de conectividad en la tarjeta correspondiente; evento de desconexión en la lista; el resto del panel sigue mostrando el último estado conocido con su marca de antigüedad |
| Error (circuito del panel caído) | Se corta el transporte del panel con el servidor | Aviso de reconexión no bloqueante; el histórico local no se pierde; reconexión automática |
| Política degradada a solo aviso | Un supuesto requerido en nunca verificado, vencido o refutado | Banner de bloqueo persistente + badge de modalidad efectiva "Solo aviso" |
| Tensión fuera de rango | `input.voltage` fuera de [198, 242] V sostenido 30 s | Evento de tensión fuera de rango entre los recientes; sin acción requerida del operador |

## 6. Versión móvil o responsive

La barra lateral colapsa a navegación superior o drawer bajo ~768px. La grilla de tarjetas de estado pasa de varias columnas a una sola, en orden de prioridad: estado del SAI, supuestos/banner de bloqueo, batería, conectividad, eventos. El banner de bloqueo y la tarjeta de supuestos se mantienen por encima del pliegue: son la información de mayor consecuencia. Legible sin scroll horizontal a 320px.

## 7. Notas de implementación

- Accesibilidad: landmarks `nav` y `main`; `<h1>` de la vista; el estado en vivo y las alertas de conectividad se anuncian por `aria-live` para no depender de mirar en el instante; badges de estado con texto además de color (calidad de muestra, modalidad, estado del SAI); foco visible; navegación por teclado en orden lógico.
- Performance percibida: el estado se empuja desde el servidor, sin polling desde el navegador; skeleton en la primera carga; transición breve al cambiar de estado, desactivable con `prefers-reduced-motion`; el panel refleja el estado con no más de un intervalo de retraso.
- Procedencia visible: `battery.charge` siempre marcado como derivado; los valores medidos y derivados se distinguen en pantalla (RN-05, R-13), para que el operador no construya una conclusión sobre un valor interpolado.
- Identidad de versión: sello en el sistema en funcionamiento (segunda ubicación obligatoria).
- Acceso monousuario: barra de identidad con cierre de sesión siempre a un clic; sin roles visibles.
- Primer arranque: el estado vacío realiza la orientación posterior; el sistema ya es operable, las tarjetas sugieren el camino sin ser un wizard obligatorio.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-04 (monitoreo en vivo); CU-05 (visibilidad del bloqueo por verificación) |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.2 (monitoreo), §4 (estados), §7 (performance percibida) |
| Reglas de negocio relevantes | RN-05 (procedencia obligatoria), RN-06 (aptitud para tendencia), RN-02 (bloqueo por verificación), RN-03 (efecto observado) |
| US a generar en 06 | US-02 (supuestos verificados en el panel), US-04 (estado en vivo desde la LAN), US-05 (origen de cada número) |
| Tests previstos en 08 | Persistencia y visualización de muestra completa/parcial/perdida; marca de derivado en carga de batería; alerta a los 3 sondeos fallidos; visualización de "n de m supuestos" y banner de bloqueo; snapshot del estado vacío de orientación |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A (solo lectura de estado) |
| Primer arranque aplicado | sí (estado vacío = orientación posterior; destino al completar el primer login) |
| Acceso de operador único aplicado | sí (shell de trabajo con barra de identidad; sin roles) |
| Identidad de versión aplicada | sí (sello en el sistema en funcionamiento; detalle de diagnóstico) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación visual de maqueta | N/A (pendiente Fase B2) |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie central Panel-Estado-En-Vivo: shell de trabajo, tarjetas de estado/batería/conectividad/supuestos, eventos recientes con regla y versión, banner de bloqueo por verificación, sello de versión en funcionamiento, orientación posterior en el estado vacío. Tabla de estados (vacío/cargando/con datos/error + degradación, tensión fuera de rango, circuito caído), responsive, accesibilidad AA con aria-live, trazabilidad. Maqueta-aware. |
