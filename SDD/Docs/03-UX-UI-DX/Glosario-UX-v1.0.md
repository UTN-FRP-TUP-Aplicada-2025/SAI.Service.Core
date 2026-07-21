# Glosario UX — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Glosario-UX-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

> Terminología canónica de la categoría 03. Fija el vocabulario de experiencia, superficie y estado que usan `Experiencia-De-Uso` y los wireframes. Los términos del dominio (SAI, apagado ordenado, supuesto y verificación, procedencia, ciclo forzado, salud de batería, vínculo temporal, días sin protección) viven en el glosario del dominio de 02 (Modelo-Conceptual y Visión §9) y no se redefinen acá: se referencian. Este glosario agrega solo los términos propios de la sección de experiencia.

## 1. Términos de superficie y navegación

| Término | Definición en esta sección |
| --- | --- |
| Superficie | Unidad maquetable con estados propios: una pantalla, un modal con flujo propio o una pestaña. Cada superficie tiene un nombre canónico estable y un archivo `Wireframes-<Superficie>`. |
| Nombre canónico de superficie | Identificador estable de una superficie (por ejemplo, Panel-Estado-En-Vivo). Lo reusa la maqueta de la Fase B2 para nombrar su archivo y la línea de base para su `SUP-XX`. |
| Pantalla | Uso coloquial de superficie de nivel de página. Se prefiere "superficie" en la documentación normativa. |
| Vista | Render concreto de una superficie en un estado dado (vista vacía, vista con datos). |
| Panel | Superficie de trabajo compuesta por bloques de información (por ejemplo, el panel de estado en vivo). No confundir con "panel de control", que es el producto entero. |
| Shell | Marco persistente de la aplicación: barra lateral de navegación de módulos más el área de contenido. |
| Shell partido | Diseño de dos shells separados por la sesión: el shell de acceso (sin sesión, sin navegación) y el shell de trabajo (con sesión, con navegación). La transición entre ellos es una navegación completa. |
| Shell de acceso | Shell sin sesión: lienzo vacío con una tarjeta centrada, sin navegación. Lo usan Acceso-Login y Alta-Inicial-Administrador. |
| Shell de trabajo | Shell con sesión: navegación de módulos, barra superior con la identidad y las acciones de identidad, y área de contenido. |
| Barra de identidad | Zona de la barra superior del shell de trabajo con la identidad activa y las acciones de cambio de contraseña y cierre de sesión. |
| Área de contenido | Región donde vive el contenido de cada superficie, dentro del shell de trabajo. |

## 2. Términos de estado y feedback

| Término | Definición en esta sección |
| --- | --- |
| Estado (de superficie) | Condición declarada que una superficie puede presentar: vacío, cargando, con datos, error, y los propios de la superficie. La maqueta debe demostrar todos los estados declarados. |
| Estado vacío | Vista cuando no hay datos aún; es una invitación a actuar, no un adorno. En el panel, el estado vacío hospeda la orientación posterior. |
| Estado cargando | Vista de operación asíncrona en curso; skeleton por encima de ~400 ms o spinner en acciones puntuales. |
| Estado con datos | Vista normal con contenido. |
| Estado de error | Vista de falla recuperable; dice qué pasó, por qué y qué hacer. |
| Skeleton | Placeholder de la estructura del contenido mientras carga, en vez de una pantalla en blanco. |
| Toast | Confirmación efímera y no bloqueante de una acción completada (por ejemplo, "versión creada"). |
| Banda de resultado | Banda dentro de una tarjeta de acceso o aprovisionamiento que acusa error (`role="alert"`) o confirmación (`role="status"`); su texto se resuelve desde el catálogo de códigos, no se compone en la vista. |
| Banner de bloqueo | Aviso persistente en la pantalla principal del panel que declara que la política está degradada a solo aviso por un supuesto no verificado. No se entierra en configuración. |
| Badge / chip de estado | Etiqueta breve con texto y color que comunica un estado (calidad de muestra, modalidad efectiva, estado de un supuesto). El texto es obligatorio; el color nunca es el único canal. |
| Alerta de conectividad | Señal en el panel cuando el sistema pierde comunicación con el SAI (3 sondeos consecutivos sin respuesta). |
| Región activa (`aria-live`) | Zona cuyo cambio se anuncia a tecnologías asistivas sin que el usuario la busque; usada para el estado en vivo y las alertas. |
| Catálogo de códigos de resultado | Fuente única de los textos de resultado de acceso e identidad; cada código tiene un texto único, no repetido por superficie. |
| Rechazo indiferenciado | Mensaje de credenciales inválidas que no distingue "usuario inexistente" de "contraseña incorrecta", para no confirmar la existencia de la identidad. |

## 3. Términos de configuración dirigida por esquema

| Término | Definición en esta sección |
| --- | --- |
| Descriptor de parámetro | Contrato único de un parámetro configurable: etiqueta, leyenda, tipo, unidad, default, límites y ejemplos. Es la fuente de verdad; la pantalla lo lee, no lo hardcodea. |
| Ayuda contextual | Tarjeta informativa que cuelga de un campo y se arma con la leyenda y los ejemplos del descriptor. |
| Divulgación progresiva | Ocultamiento de los parámetros avanzados en un expander colapsado, para acotar las opciones simultáneas. |
| Preset | Conjunto coherente de valores listo para aplicar, compuesto desde el default y los ejemplos de los descriptores. Aterriza en simulación, no se aplica directo. |
| Explicación en palabras | Descripción en prosa de la configuración actual o propuesta, generada por plantilla desde descriptores y valores. |
| Modo simulación | Estado en el que los cambios se prueban sin aplicarse todavía; se señala con un chip explícito. |
| PropuestaDeConfiguracion | Frontera por la que pasa todo cambio de configuración: la UI propone, el humano confirma, el sistema valida. La UI nunca aplica directo. |
| Ranura del asistente | Hueco de UI reservado y deshabilitado para el futuro asistente de IA (forward-compat); hoy no realiza ninguna acción. |
| Configuración de aplicación | Parámetros que el administrador gobierna desde el sistema, con efecto en caliente; tienen descriptor y superficie. |
| Configuración de entorno | Parámetros que se fijan al desplegar la instancia; no se dibujan en la superficie, ni deshabilitados; se documentan. |

## 4. Términos de primer arranque y aprovisionamiento

| Término | Definición en esta sección |
| --- | --- |
| Predicado de aprovisionamiento | Booleano único que responde "¿está aprovisionado?" (acá: existe un administrador). Todas las superficies lo consultan; ninguna lo infiere. |
| Artefacto mínimo | Entidad indispensable que satisface el predicado (acá: la cuenta de administrador). |
| Superficie de aprovisionamiento | Superficie sin chrome de navegación y sin cancelar donde ocurre el acto único del primer arranque. |
| Guard en tres capas | Corte del primer arranque en ruteo, superficie y acción, los tres contra el mismo predicado. |
| Destino al completar | Ruta declarada a la que aterriza el usuario al terminar el aprovisionamiento (acá: la superficie de acceso). |
| Orientación posterior | Grilla de tarjetas de acceso que sugiere los próximos pasos tras el aprovisionamiento, sin bloquear. |
| Requisito declarado | Regla de admisión enunciada en positivo bajo el campo, antes del intento, derivada de la política del sistema. |

## 5. Términos de identidad de versión

| Término | Definición en esta sección |
| --- | --- |
| Sello de versión | Texto de baja jerarquía al pie de una superficie que muestra la versión legible de la instancia; se deriva de la construcción, no se compone en la vista. |
| Ubicación obligatoria del sello | Las dos superficies donde el sello debe aparecer: la de acceso y una del sistema en funcionamiento. |
| Distintivo de artefacto preliminar | Chip contiguo al sello que declara que la instancia no proviene de una línea de publicación estable. |
| Marcador de origen indeterminado | Marcador textual que reemplaza la versión cuando la identidad no pudo derivarse de la construcción. |
| Detalle de diagnóstico | Panel que expone el contrato de versión completo con copiado en un solo gesto, para reportes. |

## 6. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Glosario de dominio (no se duplica) | 02 Modelo-Conceptual; Visión §9 (SAI, apagado ordenado, supuesto y verificación, procedencia, ciclo forzado, salud de batería, vínculo temporal, días sin protección) |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 + Design-Rules-Blazor-Mudblazor-v1.0 y las cuatro extensiones de capacidad |
| Artefactos que consumen este glosario | Experiencia-De-Uso-v1.0 y los ocho Wireframes-<Superficie> de esta categoría |

## 7. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Vocabulario de superficie/navegación, estado/feedback, configuración por esquema, primer arranque e identidad de versión. Referencia al glosario del dominio de 02 sin duplicarlo. |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
