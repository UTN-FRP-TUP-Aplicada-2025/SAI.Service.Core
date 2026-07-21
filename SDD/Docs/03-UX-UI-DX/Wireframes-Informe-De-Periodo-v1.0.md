# Wireframes — Informe de período y comparación de marcas

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Informe-De-Periodo-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Informe-De-Periodo**.

Superficie de informes. Arma el informe de un período (dispositivos activos con su cobertura, días con y sin protección, baterías intervinientes con intervalos recortados, intervenciones y costos por tipo, eventos por tipo y calidad de suministro) intersecando intervalos, y la comparación de modelos de batería por costo por año de servicio normalizado a USD. Todos los importes viajan con moneda y fecha, y el equivalente normalizado va marcado como derivado con su fuente de cotización. Incluye las baterías dadas de baja que intervinieron en el período. Origen: CU-12 (informe de período y comparación de marcas); UF-9.

## 2. Layout

Shell de trabajo. Área de contenido con: una barra de parámetros del informe (host, período, o conjunto de modelos a comparar); un cuerpo de informe en secciones (cobertura, intervenciones y costos, eventos y calidad de suministro); y un bloque de comparación de marcas con su advertencia de confianza.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Informe de periodo                                  <sello>    |  <h1> + sello
|  · Informes  <-|  Host [ select ]  Periodo [ desde ][ hasta ]  [ Generar ]       |  parametros
|                |  Modo ( Informe de periodo )( Comparacion de marcas )           |
|                |  +--------- Cobertura --------------------------------------+     |
|                |  | Dispositivos activos: ups-01   Dias con proteccion: 365 |     |
|                |  | Dias sin proteccion: 0                                  |     |
|                |  | Baterias intervinientes: bat-2024-a, bat-2026-a         |     |  incluye bajas
|                |  +---------------------------------------------------------+     |
|                |  +--------- Intervenciones y costos ----------------------+     |
|                |  | por tipo: recambio 1   costo total [ moneda,fecha ]     |     |
|                |  | equivalente USD: ... [badge derivado · BNA]             |     |  RN-07
|                |  +---------------------------------------------------------+     |
|                |  +--------- Eventos y calidad de suministro --------------+     |
|                |  | microcortes: N (desde eventos)                          |     |
|                |  | [ banda: calidad sobre agregados · cobertura 0,987 ]    |     |  RN-10 advertencia
|                |  +---------------------------------------------------------+     |
|                |  --- modo comparacion ---                                       |
|                |  +--------- Comparacion de marcas ------------------------+     |
|                |  | modelo   costo/anio (USD, derivado)   cumplio   desvio  |     |
|                |  | [ aviso: 1 ficha cerrada; se necesitan >= 2 modelos ]   |     |  confianza baja
|                |  +---------------------------------------------------------+     |
+----------------+-----------------------------------------------------------------+
```

Valores del ASCII ilustrativos del tipo de dato (escenarios E-6 y E-7), no una maqueta ni valores fijos.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Barra de parámetros | Elegir host y período, o conjunto de modelos | host, desde/hasta, modo | Determina si se arma un informe de período o una comparación |
| Sección de cobertura | Reportar quién protegió al host y cuántos días | dispositivos activos, días con y sin protección, baterías intervinientes | Interseca intervalos; recorta al período; incluye las bajas que intervinieron |
| Sección de intervenciones y costos | Agregar costos por tipo | intervenciones por tipo, costo total con moneda y fecha, equivalente USD | El equivalente USD lleva badge de derivado con su fuente de cotización |
| Sección de eventos y calidad de suministro | Reportar eventos y calidad | microcortes (desde eventos), calidad de suministro | El conteo de microcortes sale de eventos, no del promedio; la calidad sobre agregados lleva cobertura y advertencia |
| Banda de advertencia de cobertura | Declarar la cobertura y la advertencia cuando la calidad sale de agregados | cobertura + advertencia (RN-10) | Presente siempre que la sección se construya sobre agregados |
| Bloque de comparación de marcas | Comparar modelos por costo por año de servicio | modelo, costo/año normalizado (derivado), cumplió expectativa, desvío | Agrupa fichas de vida útil cerradas por modelo |
| Aviso de confianza de la comparación | Advertir cuando la comparación no es concluyente | mensaje de mínimo de modelos / de pruebas comparables | Con menos de 2 modelos con ficha cerrada o menos de 4 pruebas comparables, la confianza es baja |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Generar informe | Activar "Generar" con host y período | Se arma el informe intersecando intervalos por sección | Hay historia en el período |
| Cambiar de modo | Alternar informe / comparación | Cambia el cuerpo entre informe de período y comparación de marcas | — |
| Leer costos normalizados | Ver la sección de costos | Muestra el importe con moneda y fecha y el equivalente USD marcado como derivado | Hay intervenciones con costos |
| Comparar modelos | Elegir modo comparación | Agrupa fichas cerradas por modelo y calcula costo por año normalizado | Hay al menos una ficha cerrada |
| Leer advertencia de comparación | Comparación con datos insuficientes | Aparece el aviso de que se necesitan al menos dos modelos con ficha cerrada, o de confianza baja con menos de cuatro pruebas comparables | Menos de 2 modelos o menos de 4 pruebas |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin selección) | Aún no se generó ningún informe | Estado vacío con la barra de parámetros y texto orientativo |
| Cargando | Intersecando intervalos y agregando | Skeleton de las secciones del informe |
| Con datos | Informe o comparación armados | Secciones pobladas; importes con moneda y fecha; comparación por costo por año |
| Error (período sin datos suficientes) | El período no tiene actividad registrada | Mensaje de período sin datos; no se arma un informe vacío como si fuera real |
| Error (agregado sin cobertura) | La calidad de suministro se serviría sin cobertura ni advertencia | La sección no se sirve sin esos campos; se declara el faltante |
| Informe con advertencia de cobertura | La calidad sale de agregados | Banda de cobertura + advertencia de que los promedios no representan microcortes |
| Comparación con confianza baja | Menos de 2 modelos con ficha cerrada, o menos de 4 pruebas comparables | Aviso de confianza baja / de mínimo de modelos; la comparación se muestra pero no como concluyente |

## 6. Versión móvil o responsive

Las secciones del informe se apilan en una columna bajo ~768px. Las tablas de intervenciones y de comparación se desplazan horizontalmente dentro de su contenedor, nunca la página. La banda de advertencia de cobertura y el aviso de confianza se mantienen visibles junto a la sección que califican. La barra lateral colapsa a drawer. Legible sin scroll horizontal de página a 320px.

## 7. Notas de implementación

- Accesibilidad: el badge de derivado y el aviso de confianza llevan texto además de color; las tablas tienen encabezados asociados; los importes usan números tabulares; las advertencias de cobertura y de confianza se leen junto a su sección, no en un pie desconectado.
- Honestidad del dato: comparar importes de años distintos sin normalizar no significa nada; todo importe lleva moneda y fecha y el equivalente normalizado viaja como derivado con su fuente de cotización. El sistema no presenta sus veredictos de salud como conformes a ninguna norma no adquirida; toda cifra de comparación conserva su confianza y su reserva. Un período sin datos no produce un informe engañoso.
- Performance percibida: las secciones se arman sobre agregados por defecto y se bajan a resolución completa solo donde el período lo permite; skeleton mientras se intersecan los intervalos.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-12 (informe de período y comparación de marcas); UF-9 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.6 (informe), §6 (moneda y fecha), §7 (performance percibida) |
| Reglas de negocio relevantes | RN-05 (procedencia), RN-07 (importe con moneda y fecha), RN-10 (agregado con cobertura y advertencia), RN-12 (bajas incluidas en informes) |
| US a generar en 06 | US-11 (informe de período y comparación de modelos por costo por año en USD) |
| Tests previstos en 08 | Intersección de intervalos; inclusión de bajas en informes; obligatoriedad de cobertura y advertencia; comparación por costo por año normalizado; aviso de confianza con datos insuficientes |
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
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Informe-De-Periodo: parámetros de host/período/modo, secciones de cobertura, intervenciones y costos, eventos y calidad de suministro, comparación de marcas por costo por año normalizado con aviso de confianza. Tabla de estados (vacío/cargando/con datos/error de período sin datos y de agregado sin cobertura + informe con advertencia, comparación con confianza baja), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
