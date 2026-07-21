# Matriz de sensado de deriva — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Matriz-Sensado-Deriva-v1.0.md
**Versión:** 1.0
**Estado:** Vigente
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

---

## 0. Propósito y método

Esta matriz convierte la línea de base visual (`03-UX-UI-DX/Linea-Base-Visual-v1.0.md`) y el contrato de datos de la maqueta (`03-UX-UI-DX/Contrato-Datos-Maqueta-v1.0.md`) en una lista de comprobaciones `SD-XX` que el humano o un agente pueden correr en cualquier momento de la codificación para sensar deriva contra lo aprobado en la Fase B2 (Deriva-Rules §2.3). La emitió AG-03M con todo en `Sin verificar`; AG-08 la incorpora a la estrategia de testing **resolviendo el método de verificación de cada fila**: qué se cubre con test automatizado de 08 (referenciando el `TC-XX`) y qué queda como inspección visual o de esquema (Deriva-Rules §4, punto «Al cerrar la Fase E»).

Hay una fila `SD-XX` por cada elemento de la línea de base: las 11 superficies (`SUP-XX`), los 19 componentes (`CMP-XX`), los 79 estados (`EST-XX`), las 9 rutas (`NAV-XX`) y los 24 campos del contrato de datos (`DM-XX`). Total: 142 filas.

Estado inicial: todas las filas en `Sin verificar` (el sistema aún no está construido, R-10). Fecha de la última verificación: pendiente hasta el primer sensado por sprint.

### 0.1 Umbrales de deriva por dimensión (Deriva-Rules §3)

Cada fila declara su umbral por referencia a la dimensión que le corresponde. La expansión menor/mayor:

| Código | Dimensión | Deriva menor (se registra, no bloquea) | Deriva mayor (bloquea y exige decisión) |
| --- | --- | --- | --- |
| U-SUP | Superficies | Cambia el nombre o se reordena el contenido | Falta una superficie aprobada, aparece una no aprobada, o se fusionan dos |
| U-CMP | Componentes | Cambia el espaciado, el orden de columnas o el texto de una etiqueta | Falta el componente, cambia el tipo de presentación (tabla por tarjetas) o desaparece una acción |
| U-EST | Estados | Cambia el texto del mensaje o el tipo de indicador de carga | Falta un estado aprobado, o un estado cambia de condición disparadora |
| U-NAV | Navegación | Cambia el disparador visual de una ruta | Falta una ruta, aparece un callejón sin salida, o se pierde lo que debía preservarse al volver |
| U-DM | Modelo de datos | Cambia el orden de los campos en la presentación | Falta un campo, cambia el tipo, la obligatoriedad o el formato de presentación acordado |

### 0.2 Convención de método y evidencia

- **Test automatizado:** la fila se cubre con un `TC-XX` del catálogo `Casos-Prueba-Referenciales-v1.0.md` (render bUnit de componentes/estados, recorrido Playwright de superficies/rutas, o test unitario/integración de dominio para los campos). La evidencia esperada es la ejecución del TC en verde.
- **Inspección:** la fila se comprueba por inspección visual contra la maqueta (`SDD/Maquetas/Sai-Service-Core/<archivo>`) o por inspección del esquema de datos (migración EF Core), con captura o extracto fechado. Se reserva para estados puramente presentacionales (cargando, vacío, esqueleto), para el instrumento de maqueta que no es producto, y para lo que no tiene condición de dominio testeable.

---

## 1. Superficies (`SD-01`..`SD-11` sobre `SUP-XX`)

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-01 | SUP-01 Alta-Inicial-Administrador | La superficie de alta inicial existe, sin chrome ni cancelar, y crea la única identidad | Test automatizado (Playwright, TC-30/TC-38) + inspección visual | Ejecución TC-38 en verde; captura contra `Alta-Inicial-Administrador.html` | U-SUP | Sin verificar | — |
| SD-02 | SUP-02 Acceso-Login | La superficie de ingreso existe, sin registro ni recuperación, con sello de versión | Test automatizado (Playwright, TC-30/TC-38) + inspección visual | Ejecución TC-30 en verde; captura contra `Acceso-Login.html` | U-SUP | Sin verificar | — |
| SD-03 | SUP-03 Panel-Estado-En-Vivo | El home del shell muestra estado del SAI, batería, conectividad, supuestos y eventos | Test automatizado (Playwright/bUnit, TC-33/TC-39) + inspección visual | Ejecución TC-33 en verde; captura contra `Panel-Estado-En-Vivo.html` | U-SUP | Sin verificar | — |
| SD-04 | SUP-04 Alta-De-Equipos | La superficie de alta descubre el dispositivo y siembra las verificaciones | Test automatizado (Playwright, TC-31/TC-38) + inspección visual | Ejecución TC-31 en verde; captura contra `Alta-De-Equipos.html` | U-SUP | Sin verificar | — |
| SD-05 | SUP-05 Configuracion-De-Politicas | La configuración dirigida por esquema con presets, simulación y previsualización existe | Test automatizado (Playwright, TC-32) + inspección visual | Ejecución TC-32 en verde; captura contra `Configuracion-De-Politicas.html` | U-SUP | Sin verificar | — |
| SD-06 | SUP-06 Prueba-De-Bateria | La prueba densa a 1 Hz con veredicto, confianza y reserva existe | Test automatizado (Playwright/bUnit, TC-28) + inspección visual | Ejecución TC-28 en verde; captura contra `Prueba-De-Bateria.html` | U-SUP | Sin verificar | — |
| SD-07 | SUP-07 Historicos-Y-Graficas | La evolución por período distingue muestras de agregados con cobertura | Test automatizado (Playwright, TC-34) + inspección visual | Ejecución TC-34 en verde; captura contra `Historicos-Y-Graficas.html` | U-SUP | Sin verificar | — |
| SD-08 | SUP-08 Panel-De-Verificaciones | El estado de los 4 supuestos y la ventana de mantenimiento (stepper) existen | Test automatizado (Playwright/bUnit, TC-36) + inspección visual | Ejecución TC-36 en verde; captura contra `Panel-De-Verificaciones.html` | U-SUP | Sin verificar | — |
| SD-09 | SUP-09 Registro-De-Intervenciones | La intervención con costos, cuadre, disposición y ficha de vida útil existe | Test automatizado (Playwright, TC-29) + inspección visual | Ejecución TC-29 en verde; captura contra `Registro-De-Intervenciones.html` | U-SUP | Sin verificar | — |
| SD-10 | SUP-10 Sustitucion-Del-SAI | La cobertura vigente, la sucesión y los días sin protección existen | Test automatizado (Playwright, TC-35) + inspección visual | Ejecución TC-35 en verde; captura contra `Sustitucion-Del-SAI.html` | U-SUP | Sin verificar | — |
| SD-11 | SUP-11 Informe-De-Periodo | El informe por período y la comparación de marcas por costo por año en USD existen | Test automatizado (Playwright, TC-37) + inspección visual | Ejecución TC-37 en verde; captura contra `Informe-De-Periodo.html` | U-SUP | Sin verificar | — |

---

## 2. Componentes (`SD-12`..`SD-30` sobre `CMP-XX`)

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-12 | CMP-01 Shell partido (chrome + main) | El shell de dos zonas aparece en SUP-03..SUP-11 | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-13 | CMP-02 Barra de navegación de módulos | La nav lateral con las 9 superficies marca la actual con `aria-current` | Test automatizado (bUnit render + TC-38 nav) | Snapshot + ejecución TC-38 | U-CMP | Sin verificar | — |
| SD-14 | CMP-03 Barra superior con identidad y acciones | Muestra el nombre del admin y las acciones Cambiar Contraseña / Cerrar Sesión | Test automatizado (bUnit render + TC-30) | Snapshot + ejecución TC-30 | U-CMP | Sin verificar | — |
| SD-15 | CMP-04 Sello de versión + diagnóstico | El sello con `versionLegible`, chip preliminar y modal de diagnóstico aparece en todas | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-16 | CMP-05 Banda de estado/alerta | Usa `role="alert"` para error y `role="status"` para aviso, con color y texto | Test automatizado (bUnit render, accesibilidad) | Snapshot con roles ARIA verificados | U-CMP | Sin verificar | — |
| SD-17 | CMP-06 Tarjeta de contenido | El contenedor de sección con título opcional aparece en SUP-03..SUP-11 | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-18 | CMP-07 Tarjetas de orientación | Las tarjetas de pasos sugeridos enlazan a su destino (orientación, no asistente) | Test automatizado (bUnit render + TC-38 NAV-05..08) | Snapshot + ejecución TC-38 | U-CMP | Sin verificar | — |
| SD-19 | CMP-08 Lista clave-valor | Presenta pares etiqueta/valor de solo lectura en las superficies declaradas | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-20 | CMP-09 Grilla/tabla de datos (ABM) | Tabla con desplazamiento horizontal y encabezados con `scope` | Test automatizado (bUnit render + TC-34/TC-37) | Snapshot + ejecución de TC de datos | U-CMP | Sin verificar | — |
| SD-21 | CMP-10 Insignia de estado (badge) | El estado se comunica con color más texto, nunca solo color | Test automatizado (bUnit render, accesibilidad) | Snapshot con texto presente verificado | U-CMP | Sin verificar | — |
| SD-22 | CMP-11 Marca de procedencia | Marca el origen (medido/derivado/declarado/imputado) junto al dato (RC-01) | Test automatizado (bUnit render + TC-08/TC-33) | Snapshot + ejecución TC-33 | U-CMP | Sin verificar | — |
| SD-23 | CMP-12 Estado vacío con acción | Ofrece la salida (descubrir, configurar) en lugar de una lista vacía | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-24 | CMP-13 Cargando / esqueleto | Muestra placeholder mientras se resuelve el contenido | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-25 | CMP-14 Stepper de verificaciones | Los 4 pasos de la ventana avanzan por efecto observado (SUP-08) | Test automatizado (bUnit render + TC-36) | Snapshot + ejecución TC-36 | U-CMP | Sin verificar | — |
| SD-26 | CMP-15 Formulario de acceso con sello | Formulario de acto único, sin chrome, con sello (SUP-01, SUP-02) | Test automatizado (bUnit render + TC-30) | Snapshot + ejecución TC-30 | U-CMP | Sin verificar | — |
| SD-27 | CMP-16 Pie con sello de versión | El pie fijo con proyecto, modelo UX-UI, fecha y sello aparece en todas | Test automatizado (bUnit render) | Snapshot de componente en verde | U-CMP | Sin verificar | — |
| SD-28 | CMP-17 Barra de validación de maqueta | Instrumento de la maqueta: NO debe trasladarse al sistema construido | Inspección visual del producto construido | Ausencia del componente en el producto, verificada por inspección fechada | U-CMP (mayor: aparece en el producto) | Sin verificar | — |
| SD-29 | CMP-18 Enlace de salto al contenido | El skip link de accesibilidad por teclado (WCAG 2.2 AA) aparece en todas | Test automatizado (bUnit render, accesibilidad) | Snapshot con skip link verificado | U-CMP | Sin verificar | — |
| SD-30 | CMP-19 Presets y chips de configuración | Presets de política y chips de verificaciones requeridas (SUP-05) | Test automatizado (bUnit render + TC-32) | Snapshot + ejecución TC-32 | U-CMP | Sin verificar | — |

Nota: CMP-17 es el único componente que se verifica por su ausencia. En la maqueta está rotulado «no forma parte del producto»; su aparición en el sistema construido es deriva mayor.

---

## 3. Estados (`SD-31`..`SD-109` sobre `EST-XX`)

### 3.1 SUP-01 Alta-Inicial-Administrador

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-31 | EST-01 Cargando (resolviendo destino) | Muestra el esqueleto mientras resuelve si ya hay aprovisionamiento | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-32 | EST-02 Con datos (formulario listo) | Presenta el formulario de alta con el requisito de contraseña visible | Test automatizado (TC-30/TC-38) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-33 | EST-03 Error (contraseña insuficiente) | Banda de error si la contraseña no cumple la política (mín. 12, letras y números) | Test automatizado (TC-30) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-34 | EST-04 Error (confirmación no coincide) | Banda de error si la confirmación difiere de la contraseña | Test automatizado (TC-30) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-35 | EST-05 Enviando | Botón deshabilitado y formulario en envío | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-36 | EST-06 Envío fuera de tiempo (redirige) | Banda informativa y redirección neutra a Acceso-Login | Test automatizado (TC-30, ADMIN_YA_EXISTE) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-37 | EST-07 Aprovisionado (redirige a login) | Banda informativa y redirección si el sistema ya estaba aprovisionado | Test automatizado (TC-30) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-38 | EST-08 Vacío (N/A: acto único) | Banda «vacío: no aplica»; siempre presenta el formulario | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |

### 3.2 SUP-02 Acceso-Login

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-39 | EST-09 Con datos (listo para ingresar) | Campos usuario/contraseña, omisiones explícitas y sello de versión | Test automatizado (TC-30/TC-38) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-40 | EST-10 Cargando (enviando) | Botón deshabilitado durante el envío | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-41 | EST-11 Error (credenciales rechazadas) | Banda de error con texto único indiferenciado (ACC-RECHAZO) | Test automatizado (TC-30, CREDENCIAL_INVALIDA) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-42 | EST-12 Error (acceso restringido temporalmente) | Banda de error: esperar antes de reintentar (ACC-RESTRINGIDO) | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-43 | EST-13 Error (formulario vencido) | Banda de error: recargar la página (ACC-FORM-VENCIDO) | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-44 | EST-14 Identidad recién creada | Banda de éxito: ya puede ingresar (ACC-IDENTIDAD-CREADA) | Test automatizado (TC-30) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-45 | EST-15 Contraseña actualizada | Banda de éxito: la sesión anterior se cerró; ingresar con la nueva | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-46 | EST-16 Sesión expirada | Banda de advertencia: volver a ingresar (ACC-SESION-VENCIDA) | Test automatizado (TC-30, SESION_REQUERIDA) | Ejecución TC-30 en verde | U-EST | Sin verificar | — |
| SD-47 | EST-17 Vacío (N/A: formulario de acto) | Banda «vacío: no aplica»; siempre presenta los campos | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |

### 3.3 SUP-03 Panel-Estado-En-Vivo

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-48 | EST-18 Con datos | Tarjetas de SAI, conectividad, batería derivada y supuestos, con tabla de eventos | Test automatizado (TC-33) | Ejecución TC-33 en verde | U-EST | Sin verificar | — |
| SD-49 | EST-19 Vacío (sin equipos, orientación) | Banda informativa y tarjetas de orientación (3 pasos) | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-50 | EST-20 Cargando (skeleton) | Esqueleto de tarjetas en la carga inicial | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-51 | EST-21 Error (sin conexión con el SAI) | Banda de error en Conectividad a los 3 sondeos fallidos; resto con antigüedad | Test automatizado (TC-33, N-09) | Ejecución TC-33 en verde | U-EST | Sin verificar | — |
| SD-52 | EST-22 Error (circuito del panel caído) | Banda informativa no bloqueante; reconexión automática en curso | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-53 | EST-23 Política degradada a solo aviso | Banda de advertencia con 3 de 4 supuestos sin verificar y enlace a verificar | Test automatizado (TC-33/TC-39) | Ejecución TC-39 en verde | U-EST | Sin verificar | — |
| SD-54 | EST-24 Tensión fuera de rango | Evento de tensión fuera de rango en transición OL→OB con `input.voltage` 0,0 V | Test automatizado (TC-14/TC-33, N-10) | Ejecución TC-14 en verde | U-EST | Sin verificar | — |

### 3.4 SUP-04 Alta-De-Equipos

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-55 | EST-25 Con datos (candidato / inventario) | Descubrimiento con candidato USB y datos declarados de SAI, batería y host | Test automatizado (TC-31) | Ejecución TC-31 en verde | U-EST | Sin verificar | — |
| SD-56 | EST-26 Vacío (sin dispositivos) | Estado vacío con acción «Descubrir» | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-57 | EST-27 Cargando (descubriendo / probando) | Esqueleto con leyenda de descubrimiento o prueba de conexión | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-58 | EST-28 Error (prueba de conexión fallida) | Banda de error; alta deshabilitada hasta pasar la prueba (RN-03) | Test automatizado (TC-31/TC-40) | Ejecución TC-40 en verde | U-EST | Sin verificar | — |
| SD-59 | EST-29 Error (dato obligatorio inválido) | Banda de error por vida de flotación sin temperatura de referencia (RN-13) | Test automatizado (TC-21/TC-31) | Ejecución TC-21 en verde | U-EST | Sin verificar | — |
| SD-60 | EST-30 Descubierto sin marca ni modelo | Badge «sin marca ni modelo»; se piden a mano con procedencia declarado | Test automatizado (TC-31) | Ejecución TC-31 en verde | U-EST | Sin verificar | — |

### 3.5 SUP-05 Configuracion-De-Politicas

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-61 | EST-31 Con datos | Descriptores por esquema, presets, valores vigentes (vp-001) y «en palabras» | Test automatizado (TC-32) | Ejecución TC-32 en verde | U-EST | Sin verificar | — |
| SD-62 | EST-32 Vacío (sin política previa) | Estado vacío si no hay política configurada | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-63 | EST-33 Cargando (descriptores) | Esqueleto durante la carga de descriptores | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-64 | EST-34 Error (valor fuera de límites) | Banda de error de límites al superar el rango (min/max), p. ej. techo 540 s | Test automatizado (TC-32/TC-10) | Ejecución TC-32 en verde | U-EST | Sin verificar | — |
| SD-65 | EST-35 Error (propuesta rechazada por el sistema) | Banda de error de propuesta inválida | Test automatizado (TC-32) | Ejecución TC-32 en verde | U-EST | Sin verificar | — |
| SD-66 | EST-36 Modo simulación | Badge «Modo simulación» y cabecera de estado de edición | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-67 | EST-37 Propuesta en previsualización | Previsualización de la nueva versión inmutable (vp-003) | Test automatizado (TC-32) | Ejecución TC-32 en verde | U-EST | Sin verificar | — |

### 3.6 SUP-06 Prueba-De-Bateria

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-68 | EST-38 Con datos (veredicto emitido) | Veredicto con confianza y reserva; historial con comparabilidad | Test automatizado (TC-28) | Ejecución TC-28 en verde | U-EST | Sin verificar | — |
| SD-69 | EST-39 Vacío (sin pruebas) | Estado vacío si no hay pruebas registradas | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-70 | EST-40 Cargando (prueba en curso) | Progreso, muestras y cadencia de la prueba densa a 1 Hz | Test automatizado (TC-28, N-08) | Ejecución TC-28 en verde | U-EST | Sin verificar | — |
| SD-71 | EST-41 Error (precondición no cumplida) | Banda de error de precondición (FLOTACION_INSUFICIENTE) | Test automatizado (TC-28) | Ejecución TC-28 en verde | U-EST | Sin verificar | — |
| SD-72 | EST-42 Error (muestras perdidas en conmutación) | Banda de error; muestras perdidas en el instante más informativo | Test automatizado (TC-17/TC-28) | Ejecución TC-17 en verde | U-EST | Sin verificar | — |
| SD-73 | EST-43 Prueba no comparable | Prueba marcada no comparable con su motivo (carga concurrente fuera de tolerancia) | Test automatizado (TC-16/TC-28) | Ejecución TC-16 en verde | U-EST | Sin verificar | — |

### 3.7 SUP-07 Historicos-Y-Graficas

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-74 | EST-44 Con datos (serie de muestras) | Serie P30D con resumen (promedio, mínimo, máximo, p95) y eventos | Test automatizado (TC-34) | Ejecución TC-34 en verde | U-EST | Sin verificar | — |
| SD-75 | EST-45 Con datos (serie de agregados) | Serie PT1H con cobertura y advertencia (RN-10) | Test automatizado (TC-20/TC-34) | Ejecución TC-20 en verde | U-EST | Sin verificar | — |
| SD-76 | EST-46 Vacío (sin datos en el período) | Estado vacío si el período no tiene datos (PERIODO_SIN_DATOS) | Test automatizado (TC-34) | Ejecución TC-34 en verde | U-EST | Sin verificar | — |
| SD-77 | EST-47 Cargando (serie) | Esqueleto durante la carga de la serie | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-78 | EST-48 Error (cobertura insuficiente) | Banda de error; no se oculta el hueco de cobertura | Test automatizado (TC-34) | Ejecución TC-34 en verde | U-EST | Sin verificar | — |

### 3.8 SUP-08 Panel-De-Verificaciones

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-79 | EST-49 Vacío (4 nunca verificados) | Los 4 supuestos en «NuncaVerificado» tras el alta | Test automatizado (TC-31/TC-36) | Ejecución TC-31 en verde | U-EST | Sin verificar | — |
| SD-80 | EST-50 Con datos (estado de supuestos) | Lista de supuestos con estado, método y vigencia | Test automatizado (TC-36) | Ejecución TC-36 en verde | U-EST | Sin verificar | — |
| SD-81 | EST-51 Cargando (paso en ejecución) | Esqueleto de un paso del stepper en curso | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-82 | EST-52 Error (paso fallido) | Banda de error del paso de verificación | Test automatizado (TC-36) | Ejecución TC-36 en verde | U-EST | Sin verificar | — |
| SD-83 | EST-53 Supuesto refutado (bloqueo permanente) | Bloqueo permanente hasta resolverlo (SUPUESTO_REFUTADO) | Test automatizado (TC-36) | Ejecución TC-36 en verde | U-EST | Sin verificar | — |
| SD-84 | EST-54 Supuesto vencido | Supuesto en «Vencido» por vigencia caducada | Test automatizado (TC-12/TC-36) | Ejecución TC-12 en verde | U-EST | Sin verificar | — |
| SD-85 | EST-55 Ventana en curso (stepper) | Stepper de 4 pasos con el paso actual | Test automatizado (TC-36) | Ejecución TC-36 en verde | U-EST | Sin verificar | — |
| SD-86 | EST-56 Desbloqueado (4 de 4) | Apagado automático habilitado con los 4 supuestos verificados | Test automatizado (TC-36) | Ejecución TC-36 en verde | U-EST | Sin verificar | — |

### 3.9 SUP-09 Registro-De-Intervenciones

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-87 | EST-57 Con datos | Formulario, costos, disposición y ficha de vida útil con costos que cuadran | Test automatizado (TC-29) | Ejecución TC-29 en verde | U-EST | Sin verificar | — |
| SD-88 | EST-58 Vacío (sin intervenciones) | Estado vacío si no hay intervenciones | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-89 | EST-59 Cargando (validando / aplicando) | Esqueleto durante la validación o aplicación | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-90 | EST-60 Error (costos no cuadran) | Banda de error de cuadre (total ≠ Σ repuestos + mano de obra) | Test automatizado (TC-25) | Ejecución TC-25 en verde | U-EST | Sin verificar | — |
| SD-91 | EST-61 Error (importe sin moneda o fecha) | Banda de error de importe (RN-07, I-18) | Test automatizado (TC-18/TC-25) | Ejecución TC-18 en verde | U-EST | Sin verificar | — |
| SD-92 | EST-62 Error (coherencia temporal) | Banda de error por intervención sobre una unidad ya dada de baja (I-5) | Test automatizado (TC-25) | Ejecución TC-25 en verde | U-EST | Sin verificar | — |
| SD-93 | EST-63 Efecto aplicado (ficha proyectada) | Ficha de vida útil proyectada tras aplicar la intervención | Test automatizado (TC-29) | Ejecución TC-29 en verde | U-EST | Sin verificar | — |
| SD-94 | EST-64 Intervención por fuente externa | Registro de origen ApiExterna con confianza media, sin verificación cruzada | Test automatizado (TC-22) | Ejecución TC-22 en verde | U-EST | Sin verificar | — |

### 3.10 SUP-10 Sustitucion-Del-SAI

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-95 | EST-65 Con datos | Cobertura vigente, sucesión de coberturas y equipos disponibles | Test automatizado (TC-35) | Ejecución TC-35 en verde | U-EST | Sin verificar | — |
| SD-96 | EST-66 Vacío (sin sucesión registrada) | Estado vacío si no hay sucesión de coberturas | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-97 | EST-67 Cargando (validando / aplicando) | Esqueleto durante la validación o aplicación | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-98 | EST-68 Error (cobertura solapada) | Banda de error de solapamiento de coberturas (RC-02) | Test automatizado (TC-04/TC-35) | Ejecución TC-04 en verde | U-EST | Sin verificar | — |
| SD-99 | EST-69 Error (coherencia temporal) | Banda de error de coherencia temporal | Test automatizado (TC-35) | Ejecución TC-35 en verde | U-EST | Sin verificar | — |
| SD-100 | EST-70 Host sin cobertura (alerta) | Alerta de host sin cobertura con los días sin protección | Test automatizado (TC-35) | Ejecución TC-35 en verde | U-EST | Sin verificar | — |
| SD-101 | EST-71 Cobertura suplente activa | Cobertura del suplente activa cubriendo al host | Test automatizado (TC-35) | Ejecución TC-35 en verde | U-EST | Sin verificar | — |
| SD-102 | EST-72 SAI en reparación | Estado en reparación con aviso de caracterización del suplente | Test automatizado (TC-35) | Ejecución TC-35 en verde | U-EST | Sin verificar | — |

### 3.11 SUP-11 Informe-De-Periodo

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-103 | EST-73 Con datos | Cobertura, intervenciones y costos, eventos, calidad y comparación del período | Test automatizado (TC-37) | Ejecución TC-37 en verde | U-EST | Sin verificar | — |
| SD-104 | EST-74 Vacío (sin selección) | Estado vacío sin parámetros de consulta | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-105 | EST-75 Cargando (intersecando intervalos) | Esqueleto durante el cálculo de intersección de intervalos | Inspección visual (bUnit render) | Captura del estado contra la maqueta | U-EST | Sin verificar | — |
| SD-106 | EST-76 Error (período sin datos suficientes) | No se arma un informe vacío como si fuera real | Test automatizado (TC-37) | Ejecución TC-37 en verde | U-EST | Sin verificar | — |
| SD-107 | EST-77 Error (agregado sin cobertura) | La sección de calidad no se sirve sin cobertura ni advertencia | Test automatizado (TC-20/TC-37) | Ejecución TC-20 en verde | U-EST | Sin verificar | — |
| SD-108 | EST-78 Informe con advertencia de cobertura | Informe con advertencia cuando la calidad se construye sobre agregados (RN-10) | Test automatizado (TC-20/TC-37) | Ejecución TC-37 en verde | U-EST | Sin verificar | — |
| SD-109 | EST-79 Comparación con confianza baja | Comparación de marcas con aviso de confianza baja (1 ficha; se necesitan ≥ 2) | Test automatizado (TC-37) | Ejecución TC-37 en verde | U-EST | Sin verificar | — |

---

## 4. Rutas de navegación (`SD-110`..`SD-118` sobre `NAV-XX`)

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-110 | NAV-01 Alta-Inicial → Acceso-Login | Tras crear la identidad (o si ya aprovisionado) redirige a login, sin preservar nada | Test automatizado (Playwright, TC-30/TC-38) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-111 | NAV-02 Acceso-Login → Panel-Estado-En-Vivo | Ingreso exitoso lleva al home del shell con la sesión iniciada | Test automatizado (Playwright, TC-38) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-112 | NAV-03 Barra de módulos → otra superficie de trabajo | La nav lateral lleva a cualquiera de las 8 superficies y marca la actual con `aria-current` | Test automatizado (Playwright, TC-38/TC-39) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-113 | NAV-04 Barra superior → Acceso-Login (Cerrar Sesión) | «Cerrar Sesión» cierra la sesión y vuelve a login, sin preservar nada | Test automatizado (Playwright, TC-30) | Ejecución TC-30 en verde | U-NAV | Sin verificar | — |
| SD-114 | NAV-05 Panel (vacío) → Alta-De-Equipos | La tarjeta «Dar de alta los equipos» lleva a SUP-04 preservando la sesión | Test automatizado (Playwright, TC-38) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-115 | NAV-06 Panel (vacío) → Configuracion-De-Politicas | La tarjeta «Configurar la política» lleva a SUP-05 preservando la sesión | Test automatizado (Playwright, TC-38) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-116 | NAV-07 Panel (vacío) → Panel-De-Verificaciones | La tarjeta «Ventana de mantenimiento» lleva a SUP-08 preservando la sesión | Test automatizado (Playwright, TC-38) | Ejecución TC-38 en verde | U-NAV | Sin verificar | — |
| SD-117 | NAV-08 Panel (degradado) → Panel-De-Verificaciones | El enlace «Ir a verificar» lleva a SUP-08 preservando la sesión | Test automatizado (Playwright, TC-39) | Ejecución TC-39 en verde | U-NAV | Sin verificar | — |
| SD-118 | NAV-09 Índice de la maqueta → superficie | Ruta del instrumento de maqueta (portada), NO del producto | Inspección (no aplica al producto construido) | Constancia fechada de que es ruta del instrumento, ausente del producto | U-NAV (no aplica al producto) | Sin verificar | — |

Nota: NAV-02 se verifica sobre el flujo de acceso, no sobre un enlace estático (en la maqueta el botón «Ingresar» no lleva `href`).

---

## 5. Campos del contrato de datos (`SD-119`..`SD-142` sobre `DM-XX`)

Método común: inspección del esquema de datos (migración EF Core: tipo y obligatoriedad) más test de dominio/integración que fija el valor y el formato de presentación acordado (§2 del contrato de datos).

| ID | Elemento | Afirmación a verificar | Método de verificación | Evidencia esperada | Umbral | Estado | Fecha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SD-119 | DM-01 `input.voltage` | Tensión (V) medida, obligatoria, formato `232,9 V` con coma decimal | Inspección de esquema + test (TC-07/TC-33) | Migración + ejecución TC-33 en verde | U-DM | Sin verificar | — |
| SD-120 | DM-02 `battery.voltage` | Tensión (V) medida, obligatoria, formato `13,41 V` | Inspección de esquema + test (TC-07/TC-33) | Migración + ejecución TC-33 en verde | U-DM | Sin verificar | — |
| SD-121 | DM-03 `battery.charge` | Porcentaje derivado, siempre marcado `[derivado]`, nunca como medido | Inspección de esquema + test (TC-08/TC-33) | Migración + ejecución TC-08 en verde | U-DM | Sin verificar | — |
| SD-122 | DM-04 `ups.load` | Porcentaje medido, obligatorio, formato `12 %` | Inspección de esquema + test (TC-33) | Migración + ejecución TC-33 en verde | U-DM | Sin verificar | — |
| SD-123 | DM-05 `ups.status` | Enum OL/OB medido, obligatorio; deriva eventos vía ReglaDerivacion | Inspección de esquema + test (TC-14/TC-33) | Migración + ejecución TC-14 en verde | U-DM | Sin verificar | — |
| SD-124 | DM-06 `cobertura` | Fracción [0..1] obligatoria en el agregado, formato `0,997` | Inspección de esquema + test (TC-20) | Migración + ejecución TC-20 en verde | U-DM | Sin verificar | — |
| SD-125 | DM-07 `duracionSegundos` | Entero con incertidumbre, obligatorio, formato `5 s (±10 s)` | Inspección de esquema + test (TC-14/TC-34) | Migración + ejecución TC-14 en verde | U-DM | Sin verificar | — |
| SD-126 | DM-08 `reglaVersion` | Entero obligatorio; evento referido a regla versionada | Inspección de esquema + test (TC-14) | Migración + ejecución TC-14 en verde | U-DM | Sin verificar | — |
| SD-127 | DM-09 `modalidad` | Enum obligatorio en su forma canónica (`SoloAlerta`, etc.) | Inspección de esquema + test (TC-26/TC-32) | Migración + ejecución TC-32 en verde | U-DM | Sin verificar | — |
| SD-128 | DM-10 `umbralDisparoSegundos` | Segundos, obligatorio, formato `300 s` | Inspección de esquema + test (TC-26/TC-32) | Migración + ejecución TC-32 en verde | U-DM | Sin verificar | — |
| SD-129 | DM-11 `tiempoReservadoApagadoSeg` | Segundos ≤ 540, anulable, formato `240 s`; techo duro (RN-04) | Inspección de esquema + test (TC-10/TC-32) | Migración + ejecución TC-10 en verde | U-DM | Sin verificar | — |
| SD-130 | DM-12 `modalidadSolicitada / efectiva` | Ambos enums obligatorios; se distingue solicitada de efectiva | Inspección de esquema + test (TC-11/TC-26) | Migración + ejecución TC-11 en verde | U-DM | Sin verificar | — |
| SD-131 | DM-13 `estado (verificacion)` | Enum obligatorio (`NuncaVerificado`/`Verificado`/`Vencido`/`Refutado`) | Inspección de esquema + test (TC-12/TC-36) | Migración + ejecución TC-36 en verde | U-DM | Sin verificar | — |
| SD-132 | DM-14 `vigenciaDias` | Entero o null (null = sin caducidad), formato `180 / 365 / sin caducidad` | Inspección de esquema + test (TC-12/TC-36) | Migración + ejecución TC-12 en verde | U-DM | Sin verificar | — |
| SD-133 | DM-15 `caidaV (prueba)` | Tensión (V) derivada, formato `-0,47 V` | Inspección de esquema + test (TC-17/TC-28) | Migración + ejecución TC-28 en verde | U-DM | Sin verificar | — |
| SD-134 | DM-16 `veredicto / confianza` | Ambos enums derivados, calculados por el servicio (no por el equipo) | Inspección de esquema + test (TC-28) | Migración + ejecución TC-28 en verde | U-DM | Sin verificar | — |
| SD-135 | DM-17 `Dinero (monto, moneda, fecha)` | Importe fechado obligatorio, formato `67.000 ARS @ 2026-09-05` (I-18) | Inspección de esquema + test (TC-18) | Migración + ejecución TC-18 en verde | U-DM | Sin verificar | — |
| SD-136 | DM-18 `equivalenteUsd` | Importe derivado con fuente, formato `52,80 USD [BNA]` | Inspección de esquema + test (TC-37) | Migración + ejecución TC-37 en verde | U-DM | Sin verificar | — |
| SD-137 | DM-19 `diasEnServicio` | Entero derivado, formato `654` | Inspección de esquema + test (TC-29) | Migración + ejecución TC-29 en verde | U-DM | Sin verificar | — |
| SD-138 | DM-20 `costoPorAnioDeServicio` | Importe derivado normalizado, formato `37.430 ARS → 29,50 USD` | Inspección de esquema + test (TC-29/TC-37) | Migración + ejecución TC-37 en verde | U-DM | Sin verificar | — |
| SD-139 | DM-21 `desde / hasta (montaje)` | Intervalo temporal obligatorio, formato `2024-11-20 → 2026-09-05` | Inspección de esquema + test (TC-03/TC-29) | Migración + ejecución TC-03 en verde | U-DM | Sin verificar | — |
| SD-140 | DM-22 `desde / hasta (cobertura)` | Intervalo con extremo abierto explícito, formato `2024-11-20 → abierto` | Inspección de esquema + test (TC-04/TC-35) | Migración + ejecución TC-04 en verde | U-DM | Sin verificar | — |
| SD-141 | DM-23 `estado (unidad) / motivoBaja` | Enum + texto; motivo si baja, formato `DadoDeBaja / FinDeVidaUtil` (baja lógica) | Inspección de esquema + test (TC-05/TC-29) | Migración + ejecución TC-05 en verde | U-DM | Sin verificar | — |
| SD-142 | DM-24 `confianza (fuente)` | Enum obligatorio, formato `alta (local) / media (externa)` | Inspección de esquema + test (TC-22) | Migración + ejecución TC-22 en verde | U-DM | Sin verificar | — |

---

## 6. Resumen de la matriz

| Tipo | Filas | Cubiertas por test automatizado | Cubiertas por inspección |
| --- | --- | --- | --- |
| Superficies (SUP) | 11 | 11 | 0 |
| Componentes (CMP) | 18 | 18 | 0 |
| Componentes (CMP, instrumento no producto) | 1 | 0 | 1 (CMP-17) |
| Estados (EST) | 79 | 54 | 25 |
| Rutas (NAV) | 9 | 8 | 1 (NAV-09, instrumento) |
| Campos de datos (DM) | 24 | 24 | 0 |
| **Total** | **142** | **115** | **27** |

Notas de resolución del método (Deriva-Rules §4, cierre de la Fase E):

- Las 27 filas de inspección corresponden a estados puramente presentacionales (cargando, enviando, esqueleto, vacío sin condición de dominio, errores de sesión/rate-limit no cubiertos por TC), al componente instrumento de maqueta que no es producto (CMP-17, SD-28) y a la ruta del instrumento de portada (NAV-09, SD-118).
- Las 115 filas cubiertas por test referencian el `TC-XX` que produce la evidencia; los campos de datos combinan test con inspección del esquema (migración EF Core) para el tipo y la obligatoriedad.
- El límite físico no automatizable (F-3, ciclo de apagado/reencendido real) no genera filas de esta matriz porque no es un elemento de la línea de base visual; se documenta como gap en `Matriz-Cobertura-Pruebas-v1.0.md` §6.
- Toda deriva mayor detectada en un sensado por sprint se resuelve por corrección del sistema o por actualización aprobada de la línea de base (Deriva-Rules §3), nunca por omisión.

---

## 7. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Matriz inicial con 142 filas `SD-XX`, una por cada elemento de la línea de base (11 SUP, 19 CMP, 79 EST, 9 NAV, 24 DM). AG-08 resuelve el método de verificación por fila (115 test automatizado con su `TC-XX`, 27 inspección), la evidencia esperada y el umbral menor/mayor por dimensión (Deriva-Rules §3). Todas las filas en `Sin verificar` a la espera del primer sensado por sprint. |
