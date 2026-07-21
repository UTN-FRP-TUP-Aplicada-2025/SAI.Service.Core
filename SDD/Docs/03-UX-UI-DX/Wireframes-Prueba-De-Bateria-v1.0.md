# Wireframes — Prueba de batería y veredicto de salud

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Prueba-De-Bateria-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Prueba-De-Bateria**.

Superficie de prueba de batería y veredicto de salud. Permite lanzar una prueba manual o consultar el resultado de la prueba programada trimestral: mide la caída de tensión durante el autotest con muestreo denso (1 Hz), congela el montaje de batería y emite un veredicto propio con confianza explícita y su reserva, comparado contra la línea base a carga igualada. El veredicto lo calcula el servicio porque el equipo no lo da. Origen: CU-07 (prueba de batería y veredicto de salud), UF-5.

## 2. Layout

Shell de trabajo. Área con: cabecera con la precondición y la acción de lanzar; bloque de prueba en curso (cadencia densa); bloque de veredicto con confianza y reserva; y el historial de pruebas con su comparabilidad.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Prueba de bateria                                    <sello>   |  <h1> + sello
|  · Pruebas <-  |  Precondicion: tiempo minimo en flotacion cumplido  [ok/no]     |  §gate de dominio
|                |                                    [====== Lanzar prueba ======] |
|                |  +--------- Prueba en curso (1 Hz) -----------------------+     |  estado en curso
|                |  | progreso ............  muestras: 42   perdidas: 3       |     |
|                |  +---------------------------------------------------------+     |
|                |                                                                 |
|                |  +--------- Veredicto -------------------------------------+     |
|                |  | Tendencia: se comporta peor que la linea base           |     |
|                |  | Confianza: [badge Baja]   Comparable: [badge Si]        |     |
|                |  | Caida vs linea base: -0,47 V -> -0,58 V                 |     |
|                |  | Reserva: sin correccion por temperatura (sin sensor)    |     |  R-09 declarada
|                |  +---------------------------------------------------------+     |
|                |                                                                 |
|                |  +--------- Historial de pruebas --------------------------+     |  grilla de listado
|                |  | fecha        caida     carga   comparable   confianza   |     |
|                |  | 2026-07-19   -0,47 V    13 %   Si           baja        |     |
|                |  +---------------------------------------------------------+     |
+----------------+-----------------------------------------------------------------+
```

Valores ilustrativos de tipo de dato (línea base E-5, relevamiento 2026-07-19), no maqueta ni valores fijos.

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Indicador de precondición | Declarar si se puede probar | tiempo mínimo en flotación cumplido | Bloquea el lanzamiento si no se cumple; validado por el dominio |
| Acción "Lanzar prueba" | Disparar la prueba manual | — | Eleva la cadencia a 1 Hz y congela el `montajeBateriaId` |
| Bloque de prueba en curso | Mostrar el avance de la prueba densa | progreso, muestras tomadas, muestras perdidas | Las muestras perdidas en la conmutación se declaran, no se ocultan |
| Bloque de veredicto | Comunicar la tendencia con su confianza | tendencia, confianza (badge), comparable (badge), caída vs línea base, reserva | El veredicto solo afirma "se comporta peor que antes"; nunca un porcentaje de salud |
| Reserva declarada | Explicitar el límite del método | sin corrección por temperatura (R-09) | Texto siempre presente junto al veredicto |
| Historial de pruebas (patrón §4.3 base) | Listar pruebas con su comparabilidad | fecha, caída, carga, comparable, confianza | Una prueba no comparable no entra en la tendencia (CL-26) |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Lanzar prueba | Activar "Lanzar prueba" | Comienza la prueba a 1 Hz; el montaje se congela; el panel en vivo eleva su cadencia | Precondición de flotación cumplida |
| Ver prueba programada | El planificador dispara la trimestral | El resultado aparece con su veredicto y confianza | Cadencia trimestral |
| Consultar historial | Abrir la superficie | Lista de pruebas con comparabilidad y confianza | Hay pruebas registradas |
| Marcar no comparable | La carga concurrente excede la tolerancia | La prueba se registra pero se marca no comparable y no entra en la tendencia | `deltaCargaConcurrente` fuera de tolerancia |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin pruebas) | Aún no hay pruebas registradas | Estado vacío con la línea base y la acción de lanzar |
| Cargando (prueba en curso) | Prueba a 1 Hz en ejecución | Bloque de progreso con muestras tomadas y perdidas |
| Con datos (veredicto emitido) | Prueba finalizada | Bloque de veredicto con confianza y reserva; historial actualizado |
| Error (precondición no cumplida) | Tiempo mínimo en flotación no alcanzado (por ejemplo, tras un corte reciente) | Mensaje que explica por qué no se puede probar todavía; acción deshabilitada con motivo |
| Error (muestras perdidas en conmutación) | El equipo deja de atender consultas mientras conmuta | Se declara el conteo de muestras perdidas; los derivados toleran nulos por variable, no rompen |
| Prueba no comparable | La carga concurrente cambió más allá de la tolerancia | Badge "No comparable"; queda fuera de la tendencia |

## 6. Versión móvil o responsive

La grilla del historial se vuelve desplazable horizontalmente dentro de su contenedor (no la página). El bloque de veredicto y la reserva se apilan. La barra lateral colapsa a drawer. Legible sin scroll horizontal de página a 320px.

## 7. Notas de implementación

- Accesibilidad: la prueba en curso se anuncia por `aria-live`; badges de confianza y comparabilidad con texto además de color; la reserva por temperatura es texto, no un ícono suelto; foco visible; números tabulares en la grilla.
- Procedencia y honestidad del dato: el veredicto viaja con su confianza (arranca en baja con menos de 4 pruebas comparables) y su reserva; no se presenta como conforme a norma (IEEE 1188 no adquirida). Los valores medidos y derivados se distinguen (RN-05, RN-06).
- Performance percibida: durante la prueba, la cadencia sube a 1 Hz y se restaura al terminar; la superficie refleja el progreso sin bloquear la navegación.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-07 (prueba de batería y veredicto de salud); UF-5 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.5 (prueba de batería) |
| Reglas de negocio relevantes | RN-05 (procedencia), RN-06 (aptitud para tendencia de salud); congelado del montaje (modelo conceptual) |
| US a generar en 06 | US-07 (prueba trimestral con aviso de degradación) |
| Tests previstos en 08 | Prueba comparable vs no comparable; muestra perdida que no rompe derivados; veredicto con confianza; congelado del montaje |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A (la cadencia trimestral vive como descriptor en Configuracion-De-Politicas) |
| Primer arranque aplicado | N/A |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Prueba-De-Bateria: precondición de flotación, prueba densa a 1 Hz con congelado del montaje, veredicto con confianza y reserva declarada por temperatura, historial con comparabilidad. Tabla de estados (vacío/cargando/con datos/error + no comparable), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: validación de maqueta aprobada (ruta SDD/Maquetas/Sai-Service-Core/). |
