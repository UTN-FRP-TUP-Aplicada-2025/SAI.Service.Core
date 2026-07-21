# Wireframes — Históricos y gráficas de evolución

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Historicos-Y-Graficas-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Historicos-Y-Graficas**.

Superficie de consulta y graficación de la evolución de las variables del equipo (tensiones, carga, microcortes) en un período, individuales o superpuestas, con las marcas de eventos encima. Distingue siempre una serie de muestras a resolución completa de una serie de agregados; la serie de agregados viaja con su cobertura y su advertencia. Sirve para evaluar la calidad del suministro durante la vida del host. Origen: CU-06 (históricos y gráficas de evolución), UF-4.

## 2. Layout

Shell de trabajo. Área con: barra de controles (período, variables a superponer, resolución); lienzo del gráfico con marcas de eventos; y una banda de advertencia de cobertura cuando la serie es agregada.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Historicos y graficas                               <sello>    |  <h1> + sello
|  · Historicos<-|  Periodo [ desde ][ hasta ]  Variables [x input][x load]        |  controles / filtros
|                |  Resolucion: ( Muestras P30D ) ( Agregados PT1H )               |  distincion explicita
|                |  +--------- Grafico -------------------------------------+       |
|                |  |            .                                          |       |
|                |  |  V   .   .   .  |evento|    .    .                    |       |  serie + marcas de evento
|                |  |    .           .           .                        |       |
|                |  +-------------------------------------------------------+       |
|                |  [ banda: serie agregada -> cobertura 0,98; min/max conservados ]|  §advertencia RN-10
+----------------+-----------------------------------------------------------------+
```

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Selector de período | Acotar la ventana temporal | desde / hasta | Determina si hay muestras (P30D) o solo agregados |
| Selector de variables | Superponer series | input.voltage, output.voltage, ups.load, microcortes | Individuales o superpuestas |
| Selector de resolución | Distinguir muestra de agregado | Muestras P30D / Agregados PT1H | Distinción explícita; un agregado nunca se sirve como si fuera muestra |
| Lienzo del gráfico | Mostrar la evolución con eventos | serie temporal + marcas de evento | Las marcas de evento salen de `Evento`, no de la serie agregada |
| Banda de advertencia de cobertura | Declarar la cobertura y la advertencia de la serie agregada | `cobertura` + advertencia (RN-10) | Presente siempre que la serie sea agregada; conserva mínimo y máximo además del promedio |
| Conteo de microcortes | Informar microcortes del período | conteo desde `Evento` | Nunca se calcula desde la serie agregada (CL-16) |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Elegir período | Cambiar desde/hasta | Se carga la serie del período; se resuelve resolución disponible | — |
| Superponer variables | Marcar variables | Se dibujan las series superpuestas con leyenda | Hay datos en el período |
| Cambiar resolución | Alternar muestras/agregados | Se redibuja distinguiendo la fuente; aparece la advertencia si es agregada | Período soporta la resolución elegida |
| Leer marcas de evento | Pasar por una marca | Detalle del evento con su regla y versión | Hay eventos en el período |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin datos en el período) | El período seleccionado no tiene datos | Estado vacío con acción "Probar con otro período" |
| Cargando (serie) | Consulta de la serie en curso | Skeleton del lienzo del gráfico |
| Con datos (serie de muestras) | Período dentro de los 30 días con resolución completa | Serie de muestras con marcas de evento |
| Con datos (serie de agregados) | Período fuera de los 30 días | Serie de agregados con la banda de advertencia de cobertura y min/max |
| Error (cobertura insuficiente) | La cobertura del período está por debajo de lo utilizable | Banda que declara la cobertura baja; no se oculta el hueco |
| Serie agregada con advertencia | Se sirve una serie agregada | Banda de cobertura + advertencia siempre visible (RN-10) |

## 6. Versión móvil o responsive

El lienzo del gráfico se ajusta al ancho; los controles de período y variables se apilan. En viewport chico, la leyenda de series pasa debajo del gráfico. El gráfico ancho scrollea dentro de su propio contenedor, nunca la página. La barra lateral colapsa a drawer. Legible a 320px.

## 7. Notas de implementación

- Accesibilidad: el gráfico no porta información crítica que no esté también en texto (la cobertura y la advertencia son texto, no solo la banda); series distinguibles por más de un canal (no solo color); foco por teclado en controles y en las marcas de evento; números tabulares en ejes y tooltips.
- Honestidad del dato: la distinción muestra/agregado es normativa (PA-08, RN-10); el conteo de microcortes sale de eventos, no del promedio horario que los borra; el min/max se conserva en el agregado.
- Performance percibida: por defecto se consultan agregados; solo se baja a resolución completa dentro de la ventana de 30 días, para no traer millones de filas; skeleton mientras carga la serie.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-06 (históricos y gráficas de evolución); UF-4 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.6 (históricos), §7 (performance percibida) |
| Reglas de negocio relevantes | RN-05 (procedencia), RN-10 (agregado con cobertura y advertencia) |
| US a generar en 06 | US-06 (graficar voltajes y carga superpuestos con marcas de eventos) |
| Tests previstos en 08 | Serie de muestras vs serie de agregados; obligatoriedad de cobertura y advertencia; conteo de microcortes desde eventos |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A |
| Primer arranque aplicado | N/A |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación visual de maqueta | N/A (pendiente Fase B2) |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Historicos-Y-Graficas: selección de período/variables/resolución, distinción explícita muestra/agregado, marcas de evento, banda de advertencia de cobertura, conteo de microcortes desde eventos. Tabla de estados (vacío/cargando/con datos muestras y agregados/error + serie con advertencia), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
