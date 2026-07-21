# Wireframes — Reparación y sustitución del SAI

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Sustitucion-Del-SAI-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Sustitucion-Del-SAI**.

Superficie del ciclo de vida del equipo de alimentación: registrar que un SAI se fue a reparación o se sustituyó, poner a otro dispositivo a cubrir el host como cobertura suplente y dejar registrada la sucesión de coberturas sin solaparlas. De los huecos entre coberturas se calculan los días sin protección. Muestra en todo momento qué equipo protege al host y advierte cuando el host queda sin cobertura. Cuando la sustitución es por otro modelo, avisa que las verificaciones de firmware vuelven a estado sin verificar y que corresponde el procedimiento de caracterización. Origen: CU-09 (reparación y sustitución del SAI con cobertura suplente); UF-7.

## 2. Layout

Shell de trabajo. Área de contenido con: una línea de cobertura vigente arriba (qué equipo protege al host ahora), el formulario de intervención sobre el equipo en el medio y la sucesión de coberturas del período abajo, con los días sin protección resaltados.

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Reparacion y sustitucion del SAI                    <sello>    |  <h1> + sello
|  · Equipos     |  Cobertura vigente: [ ups-01 protege i7infra ]  [badge Activa]  |  estado de cobertura
|  · Sustituc. <-|                                                                 |
|                |  +--------- Intervencion sobre el equipo -----------------+     |
|                |  | Accion ( Reparacion )( Sustitucion )   Instante [ f/h ] |     |
|                |  | Equipo saliente [ select ]                              |     |
|                |  | Suplente [ select stock / alta sustituto ]              |     |
|                |  | Desde [ f/h ] (inicio de la cobertura suplente)         |     |
|                |  +---------------------------------------------------------+     |
|                |  [ Previsualizar efectos ]         [==== Registrar acto ====]    |
|                |                                                                 |
|                |  +--------- Sucesion de coberturas del host --------------+     |
|                |  | ups-01  desde ... hasta 09-05 [cerrada]                 |     |
|                |  | (( hueco: 2 dias sin proteccion ))            [alerta]  |     |  dias sin proteccion
|                |  | ups-02  desde 09-07 ... [vigente]                       |     |
|                |  +---------------------------------------------------------+     |
|                |  Nota: si el suplente es de otro modelo, las verificaciones     |
|                |  de firmware vuelven a "sin verificar" -> caracterizar          |  FA-2
+----------------+-----------------------------------------------------------------+
```

Valores del ASCII ilustrativos de la sucesión de coberturas (E-1 parcial). El escenario de datos completo de este flujo está pendiente (ver §7).

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Línea de cobertura vigente | Declarar qué equipo protege al host ahora | equipo cubriente, host, badge de estado | Siempre visible; si no hay cobertura, muestra la alerta de host sin protección |
| Formulario de intervención | Capturar reparación o sustitución | acción, instante, equipo saliente, suplente, inicio de cobertura suplente | Cambia el estado del equipo saliente a en reparación o dado de baja |
| Selector de suplente | Elegir el equipo que cubre | equipo en stock o alta de sustituto | Abre una cobertura nueva desde el instante en que empieza a cubrir |
| Previsualización de efectos | Mostrar el cierre y la apertura de coberturas antes de aplicar | cierre de cobertura vigente, apertura de la suplente, hueco resultante | No se aplica hasta confirmar |
| Sucesión de coberturas | Mostrar qué equipo protegió al host en cada tramo | coberturas con intervalo desde/hasta, estado | Los huecos entre coberturas se marcan como días sin protección |
| Contador de días sin protección | Cuantificar el tramo desprotegido | días del hueco | Se calcula de los huecos, no se declara a mano |
| Aviso de caracterización | Advertir el reinicio de verificaciones al sustituir por otro modelo | mensaje + enlace a Panel-De-Verificaciones | Aparece solo en sustitución por otro modelo |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Registrar reparación | Confirmar acción "Reparación" | Cierra la cobertura vigente en el instante; pasa el equipo a en reparación | Existe cobertura vigente por ese equipo |
| Registrar sustitución | Confirmar acción "Sustitución" | Cierra la cobertura vigente; da de baja o repara el saliente; si hay suplente, abre cobertura nueva sin solapamiento | Hay suplente en stock o se registra el sustituto |
| Activar cobertura suplente | Elegir suplente y su instante de inicio | Pasa el suplente a en servicio y abre la cobertura desde ese instante | El instante no solapa otra cobertura vigente |
| Registrar retorno de reparación | El equipo vuelve reparado | Lo pasa a en servicio y abre una cobertura nueva desde ese instante; la anterior no se reabre (son dos tramos) | El equipo estaba en reparación |
| Consultar días sin protección | Elegir un período | Informa los días sin cobertura calculados de los huecos | Hay historia de coberturas |
| Sustituir por otro modelo | Elegir un suplente de otro modelo | Aplica el reemplazo y avisa que las verificaciones de firmware vuelven a sin verificar; enlaza a caracterizar | La sustitución es por modelo distinto |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin sucesión registrada) | El host tiene una sola cobertura, sin intervenciones aún | Línea de cobertura vigente y estado vacío del historial con texto orientativo |
| Cargando | Validando o aplicando la intervención | Spinner en la acción; el resto del formulario accesible |
| Con datos | Sucesión de coberturas poblada | Línea de vigente + sucesión de tramos con sus intervalos y estados |
| Error (cobertura solapada) | Se intentaría abrir una cobertura que se solapa con otra vigente | Rechazo de la apertura; a lo sumo una cobertura vigente por host |
| Error (coherencia temporal) | La intervención se fecha después de la baja del equipo afectado | Mensaje de coherencia temporal; no se aplica el efecto |
| Host sin cobertura (alerta) | Hay un tramo en que ningún equipo cubre al host | Alerta destacada de host sin protección; los días del hueco se cuentan |
| Cobertura suplente activa | Un equipo suplente cubre al host | La línea de vigente muestra el suplente; badge de cobertura suplente |
| SAI en reparación | El equipo saliente está en reparación | Badge "en reparación" en el equipo; su cobertura aparece cerrada |

## 6. Versión móvil o responsive

La línea de cobertura vigente se mantiene por encima del pliegue. El formulario pasa a una columna bajo ~768px; la sucesión de coberturas se apila como tarjetas por tramo, con el hueco de días sin protección resaltado entre ellas. La barra lateral colapsa a drawer. Legible sin scroll horizontal de página a 320px.

## 7. Notas de implementación

- Accesibilidad: la alerta de host sin cobertura se anuncia por región activa (`role="alert"`); los badges de estado (activa, suplente, en reparación) llevan texto además de color; la sucesión de coberturas se lee como lista temporal navegable por teclado; foco al primer campo inválido tras un rechazo.
- Vigencia como entidad con intervalo: la sucesión de coberturas es el patrón que hace representable "un equipo se fue a reparación y otro cubrió el host"; con la vigencia como atributo del equipo no sería representable.
- Pendiente de datos declarado: el intake marca el riesgo R-11 — este flujo aún no tiene un escenario de datos completo; se propone un escenario E-9 cuando se implemente. La superficie se ancla en el escenario E-1 parcial (equipo suplente en stock) y no inventa datos de sustitución; la maqueta de la Fase B2 debe demostrar los estados con datos de ejemplo marcados como reconstruidos.
- Sin optimistic UI: el cierre y la apertura de coberturas se muestran aplicados recién cuando el sistema los confirma; la previsualización es explícita y previa.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-09 (reparación y sustitución del SAI con cobertura suplente); UF-7 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §3.6 (ciclo de vida), §10 (pendientes referidos) |
| Reglas de negocio relevantes | RN-12 (baja lógica y coherencia temporal); RN-01 y RN-02 aplicables a la reanudación de la modalidad tras la sustitución |
| US a generar en 06 | US-09 (qué equipo protegía al host en cada tramo y cuántos días quedó sin protección) |
| Tests previstos en 08 | Cierre y apertura de cobertura sin solapamiento; cálculo de días sin protección; reinicio de verificaciones al sustituir por otro modelo |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | N/A |
| Primer arranque aplicado | N/A |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie Sustitucion-Del-SAI: línea de cobertura vigente, formulario de reparación/sustitución, cobertura suplente, sucesión de coberturas con días sin protección y aviso de caracterización al sustituir por otro modelo. Tabla de estados (vacío/cargando/con datos/error de solapamiento y de coherencia temporal + host sin cobertura, suplente activa, SAI en reparación), responsive, accesibilidad AA, pendiente R-11 referido, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
