# Wireframes — Registro de intervenciones y ficha de vida útil

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Registro-De-Intervenciones-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Registro-De-Intervenciones**.

Superficie de registro de una intervención de servicio técnico (típicamente el recambio de batería) con sus costos, sus hallazgos, sus mediciones de antes y después y su disposición final. Un solo acto cierra la vigencia del montaje viejo, abre la del nuevo sin hueco, da de baja lógica la unidad retirada, pone en servicio la nueva y proyecta la ficha de vida útil de la batería retirada con su costo por año de servicio normalizado. La superficie valida que los costos cuadren y que todo importe lleve moneda y fecha antes de aplicar ningún efecto. Muestra también las intervenciones que llegaron por fuente externa, con su confianza declarada menor que la del dato local. Origen: CU-08 (recambio de batería y ficha de vida útil); UF-6.

## 2. Layout

Shell de trabajo (barra lateral + barra superior). Área de contenido con: un formulario de intervención arriba (patrón "ABM — formulario de edición" §4.4 del base), un bloque de costos con su cuadre, un bloque de disposición final y, tras aplicar, la ficha de vida útil proyectada y el historial de intervenciones.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Registrar intervencion                              <sello>    |  <h1> + sello
|  · Equipos     |  Tipo [ select: recambio v ]  Instante [ fecha/hora ]           |
|  · Intervenc.<-|  Dispositivo [ select ]   Bateria saliente [ select ]           |
|                |  Bateria entrante [ select / alta en el mismo acto ]            |
|                |  Proveedor [ select ]   Ejecutada por [ campo ]                 |
|                |  +--------- Costos ---------------------------------------+     |
|                |  | Repuestos [   ] (moneda,fecha)  Mano de obra [   ]      |     |
|                |  | Total declarado [   ]        Cuadre: [badge Cuadra/No] |     |  Costos.cuadra()
|                |  +---------------------------------------------------------+     |
|                |  Hallazgos [ texto ]   Mediciones antes/despues [ campos ]       |
|                |  +--------- Disposicion final -----------------------------+     |
|                |  | Destino [ campo ]   Receptor [ campo ]                  |     |  trazabilidad ambiental
|                |  +---------------------------------------------------------+     |
|                |  [ Previsualizar efectos ]         [==== Registrar acto ====]    |  pie
|                |                                                                 |
|                |  --- tras aplicar ---                                           |
|                |  +--------- Ficha de vida util (bateria retirada) --------+     |
|                |  | Dias en servicio 654   Cumplio expectativa [badge No]  |     |
|                |  | Costo por anio: 37.430 (pesos) -> 29,50 USD [derivado] |     |  RN-07
|                |  +---------------------------------------------------------+     |
|                |  +--------- Historial de intervenciones ------------------+     |
|                |  | fecha  tipo   costo   fuente [badge local/externa]     |     |  confianza por fuente
|                |  +---------------------------------------------------------+     |
+----------------+-----------------------------------------------------------------+
```

Los valores del ASCII son ilustrativos del tipo de dato (escenario E-6), no una maqueta ni valores fijos.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Formulario de intervención | Capturar el acto | tipo, instante, dispositivo, baterías afectadas, proveedor, ejecutada por | Distingue instante en que ocurrió del instante en que se registra (carga diferida) |
| Bloque de costos con cuadre | Validar que el total iguala repuestos + mano de obra | repuestos, mano de obra, total declarado, badge de cuadre | Cada importe con moneda y fecha; el cuadre se verifica antes de aplicar |
| Bloque de mediciones y hallazgos | Registrar antes/después y observaciones | mediciones, texto de hallazgos | Las mediciones conservan su procedencia |
| Bloque de disposición final | Trazabilidad ambiental de la batería retirada | destino, receptor | Se registra siempre que haya una unidad retirada |
| Previsualización de efectos | Mostrar qué va a pasar antes de aplicar | cierre y apertura de montajes, cambios de estado, ficha a proyectar | El acto no se aplica hasta confirmar |
| Ficha de vida útil (patrón §4.3 base, tarjeta) | Proyectar el desempeño de la batería retirada | días en servicio, cumplió expectativa (badge), costo por año normalizado a USD (derivado) | El equivalente USD viaja marcado como derivado con su fuente de cotización |
| Historial de intervenciones | Listar intervenciones previas | fecha, tipo, costo, fuente (badge local/externa) | La fuente externa se muestra con confianza declarada menor que la local |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Cargar costos | Editar repuestos / mano de obra / total | Recalcula el cuadre en vivo; badge Cuadra / No cuadra | Importes con moneda y fecha |
| Previsualizar efectos | Activar "Previsualizar efectos" | Muestra el cierre/apertura de montajes y los cambios de estado sin aplicarlos | Formulario completo |
| Registrar acto | Activar "Registrar acto" | Aplica los efectos en un solo acto: cierra montaje viejo, abre nuevo sin hueco, da de baja la retirada, pone en servicio la nueva, registra disposición y proyecta la ficha | Costos cuadran; todo importe con moneda y fecha; coherencia temporal |
| Corregir la fecha del recambio | Editar la fecha de un recambio ya cargado | El histórico afectado se reatribuye automáticamente, sin migrar datos | La intervención existe |
| Consultar ficha proyectada | Tras aplicar | Muestra días en servicio, cumplimiento de expectativa y costo por año normalizado | Recambio aplicado |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin intervenciones) | Aún no se cargó ninguna intervención | Estado vacío con texto orientativo y la acción de registrar |
| Cargando | Validando o aplicando los efectos | Spinner en la acción; el resto del formulario sigue accesible |
| Con datos | Formulario completado / historial poblado | Formulario con costos y cuadre; historial y ficha visibles tras aplicar |
| Error (costos no cuadran) | El total no iguala la suma de repuestos y mano de obra | Badge "No cuadra" + mensaje que declara el motivo de cuadre de costos; no se aplica ningún efecto (validación tipo 422) |
| Error (importe sin moneda o fecha) | Un importe llega sin moneda o sin fecha | Error inline en el campo con la regla en positivo; no se aplica ningún efecto |
| Error (coherencia temporal) | La intervención se fecha después de la baja de una unidad afectada | Mensaje de coherencia temporal; la unidad dada de baja se consulta pero no se opera |
| Efecto aplicado | El acto se registró correctamente | Confirmación con el cierre/apertura de montajes y la ficha proyectada; próxima acción sugerida |
| Intervención por fuente externa (confianza media) | La intervención llegó por ingesta externa | En el historial, badge "externa" con confianza declarada menor que la del dato local |

## 6. Versión móvil o responsive

La grilla de campos pasa a una columna bajo ~768px; el bloque de costos mantiene juntos repuestos, mano de obra y total para leer el cuadre. La barra lateral colapsa a drawer. El historial se desplaza horizontalmente dentro de su contenedor, nunca la página. La ficha de vida útil se apila bajo el formulario. Legible sin scroll horizontal de página a 320px.

## 7. Notas de implementación

- Accesibilidad: el badge de cuadre lleva texto además de color; los errores de validación se asocian al campo por `aria-describedby` con la regla admitida; el foco va al primer campo inválido tras un intento rechazado; la confirmación del acto se anuncia por región activa; números tabulares en los importes.
- Honestidad del dato: el equivalente en USD viaja marcado como derivado con su fuente de cotización; comparar importes de años distintos sin normalizar no significa nada, por eso todo importe lleva moneda y fecha. La fuente externa se distingue de la local por su confianza, no se mezcla en silencio.
- Sin optimistic UI en la aplicación del acto: los efectos se muestran como aplicados recién cuando el sistema los confirma; la previsualización es explícita y previa.
- Sin CSS ni colores: layout, jerarquía y comportamiento solamente.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-08 (recambio de batería y ficha de vida útil); UF-6 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §6 (moneda y fecha), §8 (ingesta externa y su procedencia) |
| Reglas de negocio relevantes | RN-07 (importe con moneda y fecha), RN-08 (cuadre de costos), RN-12 (baja lógica y coherencia temporal); RN-05 (procedencia) |
| US a generar en 06 | US-08 (un acto cierra la vigencia vieja, abre la nueva y proyecta la ficha) |
| Tests previstos en 08 | Cierre y apertura de montaje sin hueco; baja lógica consultable; rechazo por cuadre de costos y por dinero incompleto; coherencia temporal; ficha con costo por año normalizado |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A (registro de hechos, no parámetros de configuración) |
| Primer arranque aplicado | N/A |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Registro-De-Intervenciones: formulario de intervención con carga diferida, bloque de costos con cuadre, disposición final, previsualización de efectos, ficha de vida útil proyectada y distinción de fuente local vs externa. Tabla de estados (vacío/cargando/con datos/error de cuadre, de importe y de coherencia temporal + efecto aplicado, fuente externa), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
