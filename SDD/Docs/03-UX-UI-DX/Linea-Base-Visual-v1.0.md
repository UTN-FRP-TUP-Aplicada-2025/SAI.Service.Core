# Línea de base visual — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Linea-Base-Visual-v1.0.md
**Versión:** 1.0
**Estado:** Vigente
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03M)
**Variante:** UX/UI

**Trazabilidad:**
- 03-UX-UI-DX: `Experiencia-De-Uso-v1.0.md`, `Glosario-UX-v1.0.md` y los 11 `Wireframes-<superficie>-v1.0.md` de esta misma carpeta.
- 02-Especificacion-Funcional: `Modelo-Datos/Modelo-Conceptual-v1.0.md` (correspondencia de entidades) y los CU de origen.
- Maqueta aprobada: `SDD/Maquetas/Sai-Service-Core/` (index.html, las 11 superficies .html, `assets/js/Datos-Maqueta.js`, `assets/js/Maqueta.js`, README.md).
- Regla que define este artefacto: `IA/IA.SDD/SDD/Devs/Rules/Deriva-Rules.md` §2.1.
- Validación humana: `Bitacora-Validacion-Maqueta-v1.0.md` (misma carpeta), aprobación explícita 2026-07-20.

---

## 0. Propósito y método

Este documento es el inventario identificado de lo que el humano aprobó al mirar la maqueta de la Fase B2. No es prosa descriptiva: cada elemento tiene un identificador estable de dos dígitos (`SUP-XX`, `CMP-XX`, `EST-XX`, `NAV-XX`) contra el cual se puede sensar deriva en cualquier momento posterior de la codificación. La línea de base se cita, no se reinterpreta (Deriva-Rules §5).

Todo lo inventariado es observable en la maqueta aprobada. La fuente de cada elemento es un artefacto concreto del directorio de la maqueta, citado como evidencia según Deriva-Rules §1.

**Evidencia de origen del inventario:**
- `[EV-01 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Datos-Maqueta.js | D.superficies (líneas 401-546) y D.contratoCampos (líneas 370-395) | 2026-07-20]`
- `[EV-02 | artefacto | SDD/Maquetas/Sai-Service-Core/assets/js/Maqueta.js | objeto V por superficie y funciones de shell/barra (líneas 50-878) | 2026-07-20]`
- `[EV-03 | artefacto | SDD/Maquetas/Sai-Service-Core/index.html | catálogo de superficies y contrato de campos (líneas 21-65) | 2026-07-20]`
- `[EV-04 | humano | Bitacora-Validacion-Maqueta-v1.0.md | sección «Aprobación» | 2026-07-20]`

---

## 1. Superficies (`SUP-XX`)

Las 11 superficies de la maqueta aprobada. Nombre canónico, archivo de la maqueta, wireframe de 03 que la especifica, CU de 02 que la origina y propósito en una línea.

| ID | Nombre canónico | Archivo de la maqueta | Wireframe de 03 | CU de origen | Propósito |
| --- | --- | --- | --- | --- | --- |
| SUP-01 | Alta-Inicial-Administrador | `Alta-Inicial-Administrador.html` | `Wireframes-Alta-Inicial-Administrador-v1.0.md` | CU-01 (alta inicial) | Aprovisionamiento del primer arranque: crear la única identidad. Sin chrome ni cancelar. |
| SUP-02 | Acceso-Login | `Acceso-Login.html` | `Wireframes-Acceso-Login-v1.0.md` | CU-01 (ingreso) | Ingreso del operador único: sin registro, sin selector, sin recuperación; con sello de versión. |
| SUP-03 | Panel-Estado-En-Vivo | `Panel-Estado-En-Vivo.html` | `Wireframes-Panel-Estado-En-Vivo-v1.0.md` | CU-04 + CU-05 | Home del shell: estado del SAI, batería (carga derivada), conectividad, supuestos y eventos. |
| SUP-04 | Alta-De-Equipos | `Alta-De-Equipos.html` | `Wireframes-Alta-De-Equipos-v1.0.md` | CU-02 (alta de equipos) | Descubrimiento del dispositivo y datos declarados del SAI, batería y host; siembra las verificaciones. |
| SUP-05 | Configuracion-De-Politicas | `Configuracion-De-Politicas.html` | `Wireframes-Configuracion-De-Politicas-v1.0.md` | CU-03 (políticas) | Configuración dirigida por esquema: descriptores, presets, simulación y propuesta en previsualización. |
| SUP-06 | Prueba-De-Bateria | `Prueba-De-Bateria.html` | `Wireframes-Prueba-De-Bateria-v1.0.md` | CU-07 (prueba de batería) | Prueba densa a 1 Hz, veredicto con confianza y reserva, historial con comparabilidad. |
| SUP-07 | Historicos-Y-Graficas | `Historicos-Y-Graficas.html` | `Wireframes-Historicos-Y-Graficas-v1.0.md` | CU-06 (históricos) | Evolución de variables por período; distingue muestras de agregados con su cobertura. |
| SUP-08 | Panel-De-Verificaciones | `Panel-De-Verificaciones.html` | `Wireframes-Panel-De-Verificaciones-v1.0.md` | CU-10 + CU-05 | Estado de los 4 supuestos y ventana de mantenimiento (stepper) por efecto observado. |
| SUP-09 | Registro-De-Intervenciones | `Registro-De-Intervenciones.html` | `Wireframes-Registro-De-Intervenciones-v1.0.md` | CU-08 (recambio) | Intervención con costos y cuadre, disposición final, ficha de vida útil, fuente local o externa. |
| SUP-10 | Sustitucion-Del-SAI | `Sustitucion-Del-SAI.html` | `Wireframes-Sustitucion-Del-SAI-v1.0.md` | CU-09 (sustitución) | Cobertura vigente, sucesión de coberturas, días sin protección, aviso de caracterización (datos R-11 reconstruidos). |
| SUP-11 | Informe-De-Periodo | `Informe-De-Periodo.html` | `Wireframes-Informe-De-Periodo-v1.0.md` | CU-12 (informe) | Informe por período y comparación de marcas por costo por año normalizado a USD. |

Evidencia: `EV-01` (D.superficies), `EV-03` (catálogo del index). El `index.html` de la maqueta es la portada del instrumento (catálogo + contrato), no una superficie del producto; por eso no recibe `SUP-XX`.

---

## 2. Componentes reutilizables (`CMP-XX`)

Componentes observados en la maqueta, construidos por `Maqueta.js` y compartidos entre superficies. Nombre, superficies en que aparece, datos que muestra, comportamiento y patrón del catálogo de diseño que materializa (Catálogo base + Blazor-MudBlazor + 4 extensiones, aproximado con Bootstrap 5).

| ID | Componente | Aparece en | Datos que muestra | Comportamiento | Patrón del catálogo |
| --- | --- | --- | --- | --- | --- |
| CMP-01 | Shell partido (chrome + main) | SUP-03 a SUP-11 (superficies de trabajo) | Marca del producto, contenido de la superficie | Estructura fija de dos zonas: cabecera de módulos y área de contenido | Layout de aplicación (shell MudBlazor) |
| CMP-02 | Barra de navegación de módulos | SUP-03 a SUP-11 | Título e ícono de cada una de las 9 superficies de trabajo | Enlaces persistentes; marca la superficie actual con `aria-current="page"` | Nav lateral / NavMenu |
| CMP-03 | Barra superior con identidad y acciones | SUP-03 a SUP-11 | Nombre del administrador; acciones «Cambiar Contraseña» y «Cerrar Sesión» | «Cerrar Sesión» enlaza a Acceso-Login; «Cambiar Contraseña» es acción de la superficie | AppBar con acciones de usuario |
| CMP-04 | Sello de versión + diagnóstico | Todas (SUP-01 a SUP-11) y pie | `versionLegible`, chip «preliminar», detalle de diagnóstico | Botón que abre modal de diagnóstico; chip amarillo si `esPreliminar` | Identidad-De-Version (extensión) |
| CMP-05 | Banda de estado/alerta | Todas | Mensaje de estado, error o advertencia según la condición | `role="alert"` para error, `role="status"` para aviso; color + texto | Alert / Snackbar |
| CMP-06 | Tarjeta de contenido | SUP-03 a SUP-11 | Bloques de datos con título opcional | Contenedor de sección | Card |
| CMP-07 | Tarjetas de orientación | SUP-03 (estado vacío), index | Título, descripción y destino de cada paso sugerido | Enlace a la superficie de destino; orientación, no asistente obligatorio | Card de acción / navegación |
| CMP-08 | Lista clave-valor | SUP-03, SUP-04, SUP-08, SUP-09, SUP-10, SUP-11 | Pares etiqueta/valor de estado, medición o parámetro | Presentación de datos de solo lectura | Descripción / SimpleList |
| CMP-09 | Grilla/tabla de datos (ABM) | SUP-03, SUP-06, SUP-07, SUP-08, SUP-09, SUP-10, SUP-11, index | Filas de eventos, historial, supuestos, inventario, comparación | Tabla con desplazamiento horizontal (`mq-scroll-x`); encabezados con `scope` | DataGrid / Table |
| CMP-10 | Insignia de estado (badge) | SUP-03, SUP-04, SUP-05, SUP-06, SUP-08, SUP-09, SUP-10, SUP-11 | Texto de estado (En línea, Flotación, Verificado, etc.) | Color + texto siempre presente (no solo color) | Chip / Badge |
| CMP-11 | Marca de procedencia | SUP-03, SUP-04, SUP-09 | Origen del valor (medido, derivado, declarado, imputado) | Marca textual junto al dato; materializa RC-01 (Valor con Origen) | Etiqueta de metadato |
| CMP-12 | Estado vacío con acción | SUP-03, SUP-04, SUP-05, SUP-06, SUP-08, SUP-09, SUP-10, SUP-11 | Ícono, título, texto orientador y acción primaria | Ofrece la salida (descubrir, configurar) en lugar de una lista vacía | Empty state |
| CMP-13 | Cargando / esqueleto | Todas | Bloques de carga (skeleton) | Placeholder mientras se resuelve el contenido | Skeleton / Progress |
| CMP-14 | Stepper de verificaciones | SUP-08 | Los 4 pasos de la ventana de mantenimiento y el paso actual | Progreso por efecto observado; marca completado, actual y pendiente | Stepper |
| CMP-15 | Formulario de acceso con sello | SUP-01, SUP-02 | Usuario, contraseña, requisitos; línea de omisiones explícitas | Formulario de acto único; sin chrome; con sello de versión | Form de autenticación (extensión Acceso-Monousuario) |
| CMP-16 | Pie con sello de versión | Todas | Proyecto, modelo UX-UI, fecha de iteración, sello | Pie fijo de la vista | Footer |
| CMP-17 | Barra de validación de maqueta | Todas | Selector de estado, interruptor de recarga automática, rótulo | Instrumento de la maqueta; conmuta estados sin recargar. **No forma parte del producto** | Instrumento de maqueta (no producto) |
| CMP-18 | Enlace de salto al contenido | Todas | «Saltar al contenido» | Accesibilidad por teclado (WCAG 2.2 AA) | Skip link |
| CMP-19 | Presets y chips de configuración | SUP-05 | Presets de política y chips de verificaciones requeridas | Botones tipo píldora y chips de selección múltiple | ToggleButton / Chips |

Evidencia: `EV-02` (funciones `banda`, `badge`, `card`, `kv`, `tabla`, `vacio`, `cargando`, `selloHtml`, `navHtml`, `construir`, `barraValidacionHtml` de `Maqueta.js`). CMP-17 se inventaría por completitud pero está rotulado en la propia maqueta como «no forma parte del producto» y no debe trasladarse al sistema construido.

---

## 3. Estados por superficie (`EST-XX`)

Los estados que cada superficie demuestra desde su barra de validación. Cada fila: superficie, estado, condición que lo produce y representación aprobada. Cubren como mínimo vacío, cargando, con datos y error, más los estados propios de cada flujo.

### SUP-01 · Alta-Inicial-Administrador

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-01 | Cargando (resolviendo destino) | Se resuelve si el sistema ya está aprovisionado | Esqueleto con leyenda «Resolviendo si el sistema ya está aprovisionado…» |
| EST-02 | Con datos (formulario listo) | Primer arranque sin identidad creada | Formulario de alta: usuario, contraseña, repetir contraseña; requisito visible |
| EST-03 | Error (requisito de contraseña no cumplido) | Contraseña que no cumple la política (mín. 12 caracteres, letras y números) | Banda de error: «Contraseña insuficiente.» |
| EST-04 | Error (confirmación no coincide) | La confirmación difiere de la contraseña | Banda de error: «Las contraseñas no coinciden.» |
| EST-05 | Enviando | Envío del alta en curso | Botón deshabilitado; formulario en envío |
| EST-06 | Envío fuera de tiempo (redirige) | El sistema se aprovisionó mientras se cargaba | Banda informativa: redirección neutra a Acceso-Login |
| EST-07 | Aprovisionado (redirige a login) | El sistema ya estaba aprovisionado | Banda informativa: redirección a Acceso-Login |
| EST-08 | Vacío (N/A: acto único) | No aplica: es un acto, no una lista | Banda: «vacío: no aplica»; siempre presenta el formulario |

### SUP-02 · Acceso-Login

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-09 | Con datos (listo para ingresar) | Formulario de ingreso presentado | Campos usuario/contraseña; omisiones explícitas; sello de versión |
| EST-10 | Cargando (enviando) | Envío del ingreso en curso | Botón deshabilitado |
| EST-11 | Error (credenciales rechazadas) | Código ACC-RECHAZO | Banda de error con texto único indiferenciado |
| EST-12 | Error (acceso restringido temporalmente) | Código ACC-RESTRINGIDO | Banda de error: esperar antes de reintentar |
| EST-13 | Error (formulario vencido) | Código ACC-FORM-VENCIDO | Banda de error: recargar la página |
| EST-14 | Identidad recién creada | Código ACC-IDENTIDAD-CREADA | Banda de éxito: ya puede ingresar |
| EST-15 | Contraseña actualizada | Código ACC-SECRETO-ACTUALIZADO | Banda de éxito: la sesión anterior se cerró; ingresar con la contraseña nueva |
| EST-16 | Sesión expirada | Código ACC-SESION-VENCIDA | Banda de advertencia: volver a ingresar |
| EST-17 | Vacío (N/A: formulario de acto) | No aplica: formulario de acto | Banda: «vacío: no aplica»; siempre presenta los campos |

### SUP-03 · Panel-Estado-En-Vivo

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-18 | Con datos | Equipos dados de alta y sondeo activo | Tarjetas de SAI, conectividad, batería (carga derivada) y supuestos; tabla de eventos recientes |
| EST-19 | Vacío (sin equipos, orientación) | Sistema con sesión pero sin equipos | Banda informativa + tarjetas de orientación posterior (3 pasos) |
| EST-20 | Cargando (skeleton) | Carga inicial del panel | Esqueleto de tarjetas |
| EST-21 | Error (sin conexión con el SAI) | 3 sondeos consecutivos sin respuesta | Banda de error en Conectividad; resto muestra el último estado conocido con antigüedad |
| EST-22 | Error (circuito del panel caído) | Corte del transporte del panel con el servidor | Banda informativa no bloqueante; reconexión automática en curso |
| EST-23 | Política degradada a solo aviso | 3 de 4 supuestos sin verificar | Banda de advertencia: apagado automático degradado; enlace a verificar |
| EST-24 | Tensión fuera de rango | Transición OL→OB con `input.voltage` 0,0 V sostenido | Evento de tensión fuera de rango |

### SUP-04 · Alta-De-Equipos

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-25 | Con datos (candidato / inventario) | Descubrimiento con candidato USB | Descubrimiento del dispositivo; datos declarados de SAI, batería y host |
| EST-26 | Vacío (sin dispositivos) | Sin candidatos USB conectados | Estado vacío con acción «Descubrir» |
| EST-27 | Cargando (descubriendo / probando) | Descubrimiento o prueba de conexión en curso | Esqueleto + «Descubriendo dispositivos USB / probando conexión…» |
| EST-28 | Error (prueba de conexión fallida) | El equipo no responde a la prueba | Banda de error; alta deshabilitada hasta pasar la prueba (RN-03) |
| EST-29 | Error (dato obligatorio inválido) | Vida de flotación sin temperatura de referencia (RN-13) o dato mal formado | Banda de error; campo marcado |
| EST-30 | Descubierto sin marca ni modelo | El descriptor USB no expone marca ni modelo | Badge «sin marca ni modelo»; se piden a mano con procedencia declarado |

### SUP-05 · Configuracion-De-Politicas

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-31 | Con datos | Política vigente cargada (vp-001) | Descriptores por esquema, presets, valores vigentes, «en palabras» |
| EST-32 | Vacío (sin política previa) | No hay política configurada | Estado vacío |
| EST-33 | Cargando (descriptores) | Carga de descriptores | Esqueleto |
| EST-34 | Error (valor fuera de límites) | Parámetro fuera de su rango (min/max) | Banda de error de límites |
| EST-35 | Error (propuesta rechazada por el sistema) | Propuesta inválida | Banda de error de propuesta |
| EST-36 | Modo simulación | Edición en simulación | Badge «Modo simulación»; cabecera de estado de edición |
| EST-37 | Propuesta en previsualización | Propuesta lista para revisar (vp-003) | Previsualización de la nueva versión inmutable |

### SUP-06 · Prueba-De-Bateria

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-38 | Con datos (veredicto emitido) | Prueba concluida con veredicto | Veredicto con confianza y reserva; historial con comparabilidad |
| EST-39 | Vacío (sin pruebas) | No hay pruebas registradas | Estado vacío |
| EST-40 | Cargando (prueba en curso) | Prueba densa a 1 Hz en ejecución | Progreso, muestras, cadencia |
| EST-41 | Error (precondición no cumplida) | Tiempo mínimo en flotación no cumplido | Banda de error de precondición |
| EST-42 | Error (muestras perdidas en conmutación) | El equipo deja de atender consultas al conmutar a batería | Banda de error; muestras perdidas en el instante más informativo |
| EST-43 | Prueba no comparable | Carga concurrente fuera de tolerancia | Prueba marcada no comparable con su motivo |

### SUP-07 · Historicos-Y-Graficas

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-44 | Con datos (serie de muestras) | Serie P30D disponible | Serie de muestras con resumen (promedio, mínimo, máximo, p95) y eventos |
| EST-45 | Con datos (serie de agregados) | Serie PT1H disponible | Serie de agregados con cobertura y advertencia (RN-10) |
| EST-46 | Vacío (sin datos en el período) | El período elegido no tiene datos | Estado vacío |
| EST-47 | Cargando (serie) | Carga de la serie | Esqueleto |
| EST-48 | Error (cobertura insuficiente) | Cobertura por debajo de lo utilizable | Banda de error; no se oculta el hueco |

### SUP-08 · Panel-De-Verificaciones

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-49 | Vacío (4 nunca verificados) | Estado inicial tras el alta | Los 4 supuestos en «NuncaVerificado» |
| EST-50 | Con datos (estado de supuestos) | Mezcla de estados de los supuestos | Lista de supuestos con estado, método y vigencia |
| EST-51 | Cargando (paso en ejecución) | Un paso del stepper en curso | Esqueleto de paso |
| EST-52 | Error (paso fallido) | Un paso de verificación falla | Banda de error del paso |
| EST-53 | Supuesto refutado (bloqueo permanente) | Supuesto refutado por prueba física | Bloqueo permanente hasta resolverlo |
| EST-54 | Supuesto vencido | Vigencia caducada | Supuesto en «Vencido» |
| EST-55 | Ventana en curso (stepper) | Ventana de mantenimiento abierta | Stepper de 4 pasos con paso actual |
| EST-56 | Desbloqueado (4 de 4) | Los 4 supuestos verificados | Apagado automático habilitado |

### SUP-09 · Registro-De-Intervenciones

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-57 | Con datos | Intervención registrada con costos que cuadran | Formulario, costos, disposición, ficha de vida útil |
| EST-58 | Vacío (sin intervenciones) | Sin intervenciones registradas | Estado vacío |
| EST-59 | Cargando (validando / aplicando) | Validación o aplicación en curso | Esqueleto |
| EST-60 | Error (costos no cuadran) | total ≠ Σ repuestos + mano de obra (invariante Costos.cuadra) | Banda de error de cuadre |
| EST-61 | Error (importe sin moneda o fecha) | Dinero sin moneda o sin fecha (RN-07, I-18) | Banda de error de importe |
| EST-62 | Error (coherencia temporal) | Intervención sobre una unidad ya dada de baja (I-5) | Banda de error de coherencia temporal |
| EST-63 | Efecto aplicado (ficha proyectada) | Intervención aplicada | Ficha de vida útil proyectada |
| EST-64 | Intervención por fuente externa | Registro de origen ApiExterna | Fuente externa con confianza media, sin verificación cruzada |

### SUP-10 · Sustitucion-Del-SAI

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-65 | Con datos | Cobertura vigente y sucesión (datos R-11 reconstruidos) | Cobertura vigente, sucesión de coberturas, equipos disponibles |
| EST-66 | Vacío (sin sucesión registrada) | Sin sucesión de coberturas | Estado vacío |
| EST-67 | Cargando (validando / aplicando) | Validación o aplicación en curso | Esqueleto |
| EST-68 | Error (cobertura solapada) | Solapamiento de coberturas (RC-02) | Banda de error de solapamiento |
| EST-69 | Error (coherencia temporal) | Intervención incoherente en el tiempo | Banda de error de coherencia temporal |
| EST-70 | Host sin cobertura (alerta) | Hueco de días sin protección | Alerta de host sin cobertura, con los días |
| EST-71 | Cobertura suplente activa | Suplente cubriendo al host | Cobertura del suplente activa |
| EST-72 | SAI en reparación | Equipo en reparación | Estado en reparación; aviso de caracterización del suplente |

### SUP-11 · Informe-De-Periodo

| ID | Estado | Condición disparadora | Representación aprobada |
| --- | --- | --- | --- |
| EST-73 | Con datos | Período con actividad registrada | Cobertura, intervenciones y costos, eventos, calidad, comparación |
| EST-74 | Vacío (sin selección) | Sin parámetros de consulta | Estado vacío |
| EST-75 | Cargando (intersecando intervalos) | Cálculo de intersección de intervalos | Esqueleto |
| EST-76 | Error (período sin datos suficientes) | El período elegido no tiene actividad | No se arma un informe vacío como si fuera real |
| EST-77 | Error (agregado sin cobertura) | Calidad sin cobertura ni advertencia | La sección no se sirve sin esos campos |
| EST-78 | Informe con advertencia de cobertura | Calidad construida sobre agregados horarios | Informe con advertencia (RN-10) |
| EST-79 | Comparación con confianza baja | 1 ficha cerrada; se necesitan ≥ 2 modelos | Comparación de marcas con aviso de confianza baja |

Evidencia: `EV-01` (arreglo `estados` de cada superficie en `D.superficies`) y `EV-02` (render de cada estado en el objeto `V` de `Maqueta.js`).

---

## 4. Rutas de navegación (`NAV-XX`)

Rutas derivadas del `index.html` y de los flujos materializados en `Maqueta.js`. Cada fila: superficie origen, disparador, superficie destino y qué se preserva al volver.

| ID | Origen | Disparador | Destino | Qué se preserva |
| --- | --- | --- | --- | --- |
| NAV-01 | SUP-01 Alta-Inicial-Administrador | Identidad creada / sistema ya aprovisionado / envío fuera de tiempo | SUP-02 Acceso-Login | Nada: acto único; redirección neutra |
| NAV-02 | SUP-02 Acceso-Login | Ingreso exitoso | SUP-03 Panel-Estado-En-Vivo | Sesión iniciada; home del shell |
| NAV-03 | SUP-03 a SUP-11 (barra de módulos) | Clic en un módulo de la nav lateral | Cualquiera de las otras 8 superficies de trabajo | Sesión; superficie actual marcada con `aria-current` |
| NAV-04 | SUP-03 a SUP-11 (barra superior) | Acción «Cerrar Sesión» | SUP-02 Acceso-Login | Nada: cierra la sesión |
| NAV-05 | SUP-03 Panel-Estado-En-Vivo (vacío) | Tarjeta «Dar de alta los equipos» | SUP-04 Alta-De-Equipos | Sesión |
| NAV-06 | SUP-03 Panel-Estado-En-Vivo (vacío) | Tarjeta «Configurar la política» | SUP-05 Configuracion-De-Politicas | Sesión |
| NAV-07 | SUP-03 Panel-Estado-En-Vivo (vacío) | Tarjeta «Ventana de mantenimiento» | SUP-08 Panel-De-Verificaciones | Sesión |
| NAV-08 | SUP-03 Panel-Estado-En-Vivo (política degradada / supuestos) | Enlace «Ir a verificar» | SUP-08 Panel-De-Verificaciones | Sesión |
| NAV-09 | Índice de la maqueta (portada, instrumento) | Tarjeta de acceso a superficie | Cualquiera de las 11 superficies | Nada: instrumento de la maqueta, no del producto |

Notas: la acción «Cambiar Contraseña» de la barra superior (CMP-03) no tiene superficie destino en la maqueta (es acción de superficie, no ruta) y por eso no genera `NAV-XX`. NAV-09 es del instrumento de maqueta (portada `index.html`), no del producto; se registra por trazabilidad. NAV-02 se deriva del flujo de acceso descrito en 03 y en el README de la maqueta; en la maqueta estática el botón «Ingresar» no lleva `href`, por lo que su umbral de deriva se verifica sobre el flujo, no sobre un enlace.

Evidencia: `EV-02` (`navHtml`, barra superior con `Cerrar Sesión`, banderas y tarjetas de orientación en el objeto `V`) y `EV-03` (grilla de superficies del index).

---

## 5. Resumen de la línea de base

| Tipo | Prefijo | Cantidad |
| --- | --- | --- |
| Superficies | SUP-XX | 11 |
| Componentes | CMP-XX | 19 |
| Estados | EST-XX | 79 |
| Rutas de navegación | NAV-XX | 9 |

Todos los identificadores son estables. Un elemento que se retire no libera su número: su fila queda con estado `Retirado` y la fecha (Deriva-Rules §2.1). En esta emisión inicial ningún elemento está retirado.
