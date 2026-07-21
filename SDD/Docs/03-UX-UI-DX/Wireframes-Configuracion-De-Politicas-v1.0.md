# Wireframes — Configuración de políticas de apagado

**Proyecto:** Sai-Service-Core
**Documento:** Wireframes-Configuracion-De-Politicas-v1.0.md
**Versión:** 1.2
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

---

## 1. Pantalla y propósito

Nombre canónico de la superficie: **Configuracion-De-Politicas**.

Superficie de configuración dirigida por esquema. Permite crear una versión nueva de la política de apagado (nunca editar la vigente), fijando la modalidad, el umbral de disparo, el tiempo reservado para el apagado y las verificaciones requeridas. Cada parámetro se presenta por su descriptor; los cambios se previsualizan en modo simulación y se confirman como una PropuestaDeConfiguracion antes de aplicarse: la UI propone, el humano confirma, el sistema valida. Origen: CU-03 (configuración de políticas de apagado), UF-2.

## 2. Layout

Shell de trabajo. Área de contenido con: cabecera con el chip de modo simulación; selector de presets; campos configurables dirigidos por descriptor (comunes visibles, avanzados en divulgación progresiva); bloque de explicación "en palabras"; ranura del asistente reservada y deshabilitada; y el pie de propuesta (previsualización + confirmar).

```text
+----------------+-----------------------------------------------------------------+
| navegacion     |  Configuracion de politicas   [chip: Modo simulacion]  <sello>  |  <h1> + §4.6 + sello
|  · ...         |  Presets: ( Solo aviso )( Apagado con retorno )( Ciclo forzado ) |  §4.4
|  · Politicas<- |                                                                 |
|                |  Modalidad            [ select v ]        (i)                   |  §4.1 campo + ayuda
|                |    por defecto: Solo aviso                                      |  hint del descriptor
|                |  Umbral de disparo    [   300  ] s        (i)                   |
|                |    por defecto 300; entre <min> y <max>                        |
|                |  Tiempo reservado     [   ---  ] s        (i)                   |
|                |    por defecto <d>; entre 12 y 540 (techo duro del equipo)      |  frontera entorno declarada
|                |  Verificaciones requeridas  [ 4 chips seleccionables ]  (i)     |
|                |  > Opciones avanzadas                                           |  §4.3 divulgacion progresiva
|                |                                                                 |
|                |  +--------- En palabras -----------------------------------+   |  §4.5
|                |  | "Cuando el corte supere 300 s, el sistema apagara el   |   |
|                |  |  host y cortara la salida del SAI con retorno; el corte |   |
|                |  |  no se cancela aunque vuelva la red."                   |   |
|                |  +---------------------------------------------------------+   |
|                |                                                                 |
|                |  +--------- Asistente (proximamente) [deshabilitado] -----+   |  §4.7 ranura reservada
|                |  +---------------------------------------------------------+   |
|                |                                                                 |
|                |  Alcance afectado: politica vigente -> nueva version           |  previsualizacion
|                |  [ Descartar ]                    [==== Confirmar version ====]  |  §6 frontera propuesta
+----------------+-----------------------------------------------------------------+
```

## 3. Componentes principales

| Componente | Propósito | Datos que muestra | Comportamiento |
| --- | --- | --- | --- |
| Chip de modo simulación (patrón §4.6 Config-Esquema) | Señalar que los cambios se prueban y no se aplicaron | Etiqueta "Modo simulación" | Estado `warning` con texto; visible mientras se edita |
| Selector de presets (patrón §4.4) | Cargar un conjunto coherente de valores | Solo aviso / Apagado con retorno / Ciclo forzado | Compone los valores desde `default` y `ejemplos` de los descriptores; aterriza en simulación, no aplica directo |
| Campo Modalidad (patrón §4.1) | Elegir la modalidad de apagado | enum: SoloAlerta, SoloHost, HostLuegoUpsConRetorno, CicloForzado; default SoloAlerta | Control derivado de `tipo=selección`; ayuda contextual con ejemplos |
| Campo Umbral de disparo | Segundos de corte sostenido antes de disparar | `umbralDisparoSegundos`, default 300 s | Validación inline por `min`/`max` del descriptor |
| Campo Tiempo reservado | Segundos reservados para completar el apagado | `tiempoReservadoApagadoSeg`, entre 12 y 540 | El control rechaza `> 540` (techo duro del equipo, I-10/RN-04) |
| Verificaciones requeridas | Qué supuestos exige la política | los cuatro supuestos como chips | La selección condiciona el bloqueo (RN-02) |
| Divulgación progresiva (patrón §4.3) | Ocultar lo avanzado (por ejemplo, intervalo de sondeo) | expander "Opciones avanzadas" | Colapsado por defecto; `aria-expanded` |
| Explicación en palabras (patrón §4.5) | Describir en prosa la política propuesta | texto generado por plantilla de descriptores + valores | Se regenera al cambiar un valor; no se escribe a mano |
| Ranura del asistente (patrón §4.7) | Reservar el lugar del futuro asistente de IA | contenedor con borde discontinuo + badge "próximamente" | Deshabilitado; no realiza ninguna acción; anunciado a lectores de pantalla |
| Ayuda contextual (patrón §4.2) | Explicar cada parámetro | `leyenda` + `ejemplos` del descriptor | Se abre desde el ícono de info; `aria-describedby` |
| Pie de propuesta (frontera §6) | Previsualizar y confirmar | "en palabras" + alcance afectado | La UI nunca aplica directo; el sistema valida al confirmar |

Descriptores (fuente única; la pantalla los lee, no hardcodea):

| Parámetro | etiqueta | tipo | unidad | default | límites / enum | ejemplos (valor -> consecuencia) |
| --- | --- | --- | --- | --- | --- | --- |
| modalidad | Modalidad de apagado | selección | — | SoloAlerta | SoloAlerta, SoloHost, HostLuegoUpsConRetorno, CicloForzado | CicloForzado -> el corte no se cancela aunque vuelva la red |
| umbralDisparoSegundos | Umbral de disparo | numérico | segundos | 300 | min/max de política | 300 -> dispara si el corte se sostiene 5 min |
| tiempoReservadoApagadoSeg | Tiempo reservado para el apagado | numérico | segundos | [derivado] | 12–540 (max = techo duro del equipo) | 540 -> usa todo el presupuesto del equipo |
| intervaloSondeoSegundos | Intervalo de sondeo | numérico | segundos | 5 | rango de sondeo | 5 -> una lectura cada 5 s |

## 4. Interacciones

| Acción | Disparador | Resultado esperado | Precondición |
| --- | --- | --- | --- |
| Elegir preset | Activar un preset | Precarga valores coherentes en simulación; regenera la explicación en palabras | — |
| Cambiar un valor | Editar un campo | Validación inline por descriptor; regenera "en palabras"; mantiene modo simulación | Valor dentro de límites |
| Abrir ayuda contextual | Activar el ícono de info | Tarjeta `info` con leyenda y ejemplos del descriptor | — |
| Expandir avanzadas | Activar "Opciones avanzadas" | Muestra los parámetros avanzados; `aria-expanded=true` | — |
| Confirmar versión | Activar "Confirmar versión" | Se arma la PropuestaDeConfiguracion; el sistema valida; crea una versión nueva inmutable; la vigente no se edita | Previsualización revisada |
| Descartar | Activar "Descartar" | Vuelve a los valores de la política vigente; sale de simulación | Hay cambios |
| Rechazo por límite | Confirmar con un valor fuera de rango | El sistema rechaza; el campo declara el rango admitido; la propuesta no se aplica | — |

## 5. Estados

| Estado | Condición que lo produce | Representación esperada |
| --- | --- | --- |
| Vacío (sin política previa) | No hay versión de política todavía | Campos precargados con los `default` de los descriptores; texto orientativo |
| Cargando (descriptores) | Se están resolviendo los descriptores | Skeleton de los campos |
| Con datos | Política vigente cargada / edición en curso | Campos poblados; explicación en palabras generada |
| Error (valor fuera de límites) | Un campo viola `min`/`max`/`enum` (por ejemplo `tiempoReservado > 540`) | Borde de error + mensaje inline con el límite violado y el rango admitido |
| Error (propuesta rechazada por el sistema) | La validación del motor rechaza la propuesta al confirmar | Banda de error con el invariante violado; la propuesta no se aplica |
| Modo simulación | Hay cambios sin aplicar | Chip "Modo simulación" en la cabecera |
| Propuesta en previsualización | Se va a confirmar | Explicación en palabras + alcance afectado (vigente -> nueva versión) |
| Ranura del asistente deshabilitada | La IA no está conectada (forward-compat) | Contenedor con borde discontinuo + badge "próximamente"; anunciado a lectores de pantalla |

## 6. Versión móvil o responsive

La grilla de campos pasa a una columna bajo ~768px; la barra lateral colapsa a drawer. Los presets se apilan o pasan a una fila desplazable. El bloque "en palabras" y el pie de propuesta se mantienen visibles al confirmar. La ranura del asistente queda al final para no competir con la configuración manual. Legible sin scroll horizontal a 320px.

## 7. Notas de implementación

- Configuración dirigida por esquema: cada campo toma etiqueta, leyenda, default, límites y ejemplos del descriptor (fuente única, no hardcodeado); ayuda contextual y "en palabras" derivadas del descriptor; presets compuestos desde `default` + `ejemplos`; modo simulación como red de seguridad; ranura del asistente reservada y deshabilitada; frontera PropuestaDeConfiguracion (la UI propone, el humano confirma, el sistema valida).
- Frontera aplicación/entorno: esta superficie gobierna modalidad, umbral, tiempo reservado, verificaciones requeridas e intervalo de sondeo. No dibuja (ni deshabilitada) la configuración de entorno: la cadena de conexión del almacenamiento, las credenciales del mecanismo de acceso al equipo, la ubicación del adaptador de conexión con el equipo, TLS/terminación segura en el borde, el presupuesto de gracia de apagado del contenedor, ni el techo duro de 540 s (que vive como límite `max` del descriptor de tiempo reservado, declarado como información).
- Prevención de errores: el modelo de versiones inmutables evita editar la vigente y preserva la explicabilidad de decisiones pasadas (US-03, CU-03).
- Accesibilidad: disclosure por teclado con `aria-expanded` en ayuda y avanzadas; error inline por `aria-describedby` con el rango admitido; chip de simulación con texto además de color; ranura deshabilitada anunciada; `prefers-reduced-motion` en la apertura de ayuda.

## 8. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-03 (configuración de políticas de apagado); UF-2 |
| Marco de experiencia aplicado | Experiencia-De-Uso-v1.0 §2.3 (frontera aplicación/entorno), §3.4 (configuración) |
| Reglas de negocio relevantes | RN-04 (techo duro de 540 s), RN-11 (acción referida a versión de política); RN-02 aplicable a la ejecución posterior |
| US a generar en 06 | US-03 (crear una versión nueva en vez de editar la vigente) |
| Tests previstos en 08 | Creación de versión inmutable; rechazo por techo de 540 s; conservación del historial; presets derivados de descriptores; explicación en palabras regenerada; propuesta validada antes de aplicar |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 |
| Configuración dirigida por esquema aplicada | sí (descriptores, ayuda contextual, presets, explicación en palabras, modo simulación, ranura del asistente, frontera PropuestaDeConfiguracion, frontera aplicación/entorno) |
| Primer arranque aplicado | N/A (se configura con el sistema en marcha) |
| Acceso de operador único aplicado | sí (shell de trabajo) |
| Identidad de versión aplicada | sí (sello heredado del shell de trabajo) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

## 9. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Superficie de configuración dirigida por esquema Configuracion-De-Politicas: descriptores de modalidad/umbral/tiempo reservado/intervalo, presets, ayuda contextual, divulgación progresiva, explicación en palabras, modo simulación, ranura del asistente reservada, frontera PropuestaDeConfiguracion y frontera aplicación/entorno. Tabla de estados (vacío/cargando/con datos/error + simulación, previsualización, ranura deshabilitada), responsive, accesibilidad AA, trazabilidad. Maqueta-aware. |
| 1.1 | 2026-07-20 | Corrección de conformidad D7: reemplazo de nombres de stack por vocabulario de dominio tras audit de Fase B. En la nota de frontera aplicación/entorno (§7) se sustituyeron los nombres de implementación por términos agnósticos (almacenamiento, mecanismo de acceso al equipo, adaptador de conexión, terminación segura en el borde, contenedor); sin cambios de semántica. |
| 1.2 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: validación de maqueta aprobada (ruta SDD/Maquetas/Sai-Service-Core/). |
