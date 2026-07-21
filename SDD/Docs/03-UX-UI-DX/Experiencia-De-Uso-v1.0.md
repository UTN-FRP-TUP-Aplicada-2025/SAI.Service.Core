# Experiencia de uso — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Experiencia-De-Uso-v1.0.md
**Versión:** 1.2
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-03)
**Variante:** UX/UI

> Marco de experiencia del panel de control web de Sai-Service-Core: un servicio que monitorea un SAI, decide y ejecuta el apagado ordenado con reencendido garantizado, administra el ciclo de vida de los equipos y expone una API REST. Instancia propia, un solo administrador, acceso por LAN. Este documento es maqueta-aware: sus flujos clave son las rutas de navegación que la maqueta de la Fase B2 va a materializar, y cada superficie citada declara su nombre canónico en su wireframe.

---

## 1. Audiencia y contexto de uso

Persona objetivo primaria: administrador único del host, definido en 00 (Visión §2). Es una sola persona que combina los roles de propietario, implementador y beneficiario. Perfil técnico alto (administra el host, la instancia desplegada y el mecanismo de acceso al equipo), sin ser especialista en UX ni en electroquímica de baterías. No hay segundas personas con acceso: no existe gestión de usuarios ni de roles (exclusión E-05).

Contexto físico y emocional:

- Uso cotidiano (el 80 % del tiempo, CU-04): consulta rápida del estado en vivo desde cualquier equipo de la LAN (escritorio o portátil, navegador Chromium o Firefox). Tono tranquilo, mirada de comprobación. Duración típica: segundos a un par de minutos.
- Uso de alta consecuencia y baja frecuencia (CU-10, ventana de mantenimiento): con presencia física junto al servidor, ejecutando una prueba destructiva que corta la energía. Tensión alta, cero tolerancia a la ambigüedad: una acción mal entendida deja un host sin respaldo apagado hasta que alguien apriete el botón.
- Uso analítico ocasional (CU-06, CU-07, CU-12): preparando una decisión de compra o evaluando la calidad del suministro. Lectura pausada de series y veredictos, con atención a la procedencia de cada número.
- Uso de puesta en marcha, una sola vez (CU-01 alta inicial, CU-02 alta de equipos): arranque de una instancia recién desplegada que todavía no es utilizable.

Frecuencia y duración: consulta diaria breve; intervenciones de configuración e inventario esporádicas; ventana de mantenimiento una vez por período de vigencia (180 o 365 días según el supuesto). El sistema corre en el mismo host que protege y se opera desde la LAN, nunca desde internet.

Contexto adverso declarado: durante un corte de energía la red también puede caer (T-05, escenario E-4). El panel remoto puede quedar inalcanzable justo cuando más importa; por eso la fuente primaria de verdad es el histórico local, y el panel es la ventana de lectura, no el mecanismo de alerta primario.

---

## 2. Principios de diseño

### 2.1 Heurísticas de Nielsen aplicadas

| Heurística de Nielsen | Aplicación en el producto | Verificación |
| --- | --- | --- |
| Visibilidad del estado del sistema | El panel de estado en vivo muestra permanentemente estado del SAI, tensiones, conectividad y "n de m supuestos verificados"; la degradación a solo aviso se anuncia en la pantalla principal, no enterrada en configuración (CU-04 paso 7). El sello de identidad de versión declara qué instancia está corriendo. | Inspección heurística; test de visualización de supuestos (CA-05 de CU-04) |
| Coincidencia entre el sistema y el mundo real | Vocabulario del dominio del operador: "supuesto verificado", "días sin protección", "salud de batería como tendencia", no jerga interna. Cada número declara su procedencia en palabras (medido / derivado / imputado). | Glosario-UX + Glosario del dominio de 02 |
| Control y libertad del usuario | Toda configuración de política se previsualiza en modo simulación antes de confirmar; el usuario prueba el efecto antes de comprometerlo. Excepción deliberada: el primer arranque y la ventana de mantenimiento son actos sin "cancelar" a la mitad, por diseño. | Revisión de la frontera PropuestaDeConfiguracion |
| Prevención de errores | El formulario de política rechaza `tiempoReservadoApagadoSeg > 540` en el control (RN-04, techo duro I-10). La política nueva es una versión inmutable; no se edita la vigente, así que no se corrompe la explicación de decisiones pasadas (CU-03). | Test de rechazo por techo de 540 s |
| Reconocer antes que recordar | Los descriptores de cada parámetro traen su leyenda, su default y sus límites a la vista; el operador no memoriza rangos. La ayuda contextual cuelga del campo. | Config dirigida por esquema |
| Visibilidad de restricciones y estado degradado | El banner de bloqueo por verificación es persistente mientras la política esté degradada; no se puede pasar por alto. | RN-01, RN-02 |
| Ayudar a reconocer, diagnosticar y recuperarse de errores | El detalle de diagnóstico del sello de versión se copia en un solo gesto para adjuntarlo a un reporte. Los eventos de conectividad y de tensión fuera de rango aparecen entre los eventos recientes con causa y acción. | Identidad de versión; taxonomía de errores §8 |
| Estética y diseño minimalista | Una superficie, una tarea. El primer arranque se dibuja sin chrome de navegación: nada compite con el único acto disponible. | Shell partido |

### 2.2 Leyes UX relevantes

- Ley de Hick: en el perfil de operador único, la decisión óptima es la que no hay que tomar. La superficie de acceso no ofrece registro, "recordarme", selector de cuenta ni recuperación; cada una sería una puerta a un lugar que no existe. En configuración, la divulgación progresiva oculta lo avanzado hasta pedirlo.
- Ley de Jakob: el shell (barra lateral de navegación + área de contenido), los patrones de formulario, badges de estado y grillas de listado se comportan igual en toda la solución, heredados del catálogo de diseño. Un mismo concepto se ve y actúa igual en todas las superficies.
- Ley de Fitts: las acciones primarias son de ancho completo o de destino amplio (acción de aprovisionamiento, confirmar política, iniciar ventana de mantenimiento); el cierre de sesión está siempre a un clic en la barra de identidad.
- Ley de Miller: no más de 5 a 7 ítems de primer nivel por agrupación; la navegación de módulos y el panel en vivo agrupan la información en bloques escaneables (estado, batería, conectividad, supuestos, eventos).

### 2.3 Frontera entre configuración de aplicación y de entorno

El sistema tiene dos clases de configuración y confundirlas produce superficies que prometen lo que no cumplen.

| Clase | Parámetros | En la UI |
| --- | --- | --- |
| Configuración de aplicación (la gobierna el administrador, con efecto en caliente) | modalidad de apagado; `umbralDisparoSegundos`; `tiempoReservadoApagadoSeg`; verificaciones requeridas por la política; intervalo de sondeo; cadencia de prueba de batería programada | Tiene descriptor y superficie (CU-03; ver `Wireframes-Configuracion-De-Politicas`) |
| Configuración de entorno (la fija quien despliega, al arrancar) | cadena de conexión del almacenamiento; credenciales del mecanismo de acceso al equipo; anclaje del dispositivo USB por su ruta física; ubicación del adaptador de conexión con el equipo (en el host o en el contenedor); TLS / terminación segura en el borde (P-04); presupuesto de gracia de apagado del contenedor (150 s); techo duro del retardo de corte del SAI (540 s, límite del equipo) | No se dibuja, ni deshabilitada; se documenta como configuración de entorno (categoría 09). El techo de 540 s vive como límite `max` del descriptor de `tiempoReservadoApagadoSeg`, no como parámetro editable |

Cuando un parámetro de entorno condiciona el efecto de la superficie de aplicación (por ejemplo, el techo de 540 s que acota `tiempoReservadoApagadoSeg`), la superficie lo declara como información, sin ofrecer cambiarlo.

---

## 3. Flujos clave

Cada flujo es una ruta de navegación que la maqueta de la Fase B2 va a materializar. Derivan de las historias UF-1..UF-10 del intake §6 y de los CU de 02.

### 3.1 Puesta en marcha de la instancia (UF-1 / CU-01 + CU-02)

Disparador: primer acceso a una instancia recién desplegada, sin administrador y sin equipos.

1. Resolución del destino: el guard de ruteo consulta el predicado único de aprovisionamiento (`estaAprovisionado` = existe un administrador). Como es falso, redirige a la superficie `Alta-Inicial-Administrador`.
2. El administrador crea la única identidad del sistema (usuario y contraseña) en una superficie sin chrome de navegación y sin acción de cancelar. Acto explícito, indivisible e irreversible desde la UI.
3. `destinoAlCompletar` declarado: la superficie de acceso (`Acceso-Login`), que acusa recibo con la banda de confirmación "identidad creada".
4. Primer ingreso. El panel de estado en vivo abre en su estado vacío-orientación: todavía no hay equipos. La orientación posterior sugiere los pasos siguientes sin bloquear (dar de alta los equipos, configurar la política, planificar la ventana de mantenimiento).
5. El administrador da de alta los equipos (`Alta-De-Equipos`): descubre el dispositivo USB, declara marca/modelo/potencia con procedencia `declarado`, abre los vínculos temporales y siembra las cuatro verificaciones en "nunca verificado", lo que fuerza el modo solo aviso.
6. Salida: el panel muestra "operativo · 0 de 4 supuestos verificados" con el banner de bloqueo y el enlace a la ventana de mantenimiento.

Puntos de fricción anticipados: el operador puede esperar un asistente multipaso; se le da, en cambio, un acto único por superficie (evita ceremonias abandonables). El dispositivo se descubre sin marca ni modelo (`0665:5161 · INNO TECH · iSerial vacío`): el formulario declara antes del intento que esos campos se cargan a mano y que un serial vacío es válido.

### 3.2 Monitoreo en vivo (UF-3 / CU-04) — el 80 % del tiempo

Disparador: el administrador abre el panel desde la LAN.

1. Ingreso autenticado (si no hay sesión, el guard devuelve a `Acceso-Login`).
2. El panel `Panel-Estado-En-Vivo` muestra estado y tensiones, batería (con la carga de batería marcada como derivada), conectividad, "n de m supuestos verificados" y los eventos recientes con su regla y versión. El estado se empuja desde el servidor cada 5 s; el navegador no sondea.
3. Si faltan supuestos, el banner de bloqueo declara la degradación a solo aviso en la pantalla principal.
4. Si se pierde la comunicación (3 sondeos consecutivos sin respuesta), la alerta de conectividad aparece y el evento de desconexión se registra.

Puntos de fricción: la carga de batería es un valor que el driver interpola; mostrarla sin marca induciría una conclusión falsa. Se marca siempre como derivada (CA-03). El panel no inventa un veredicto de "batería OK": no hay bandera de reemplazo en este equipo.

### 3.3 Desbloqueo del apagado real (UF-8 / CU-10) — crítico, raro, no puede fallar

Disparador: el administrador, con presencia física, inicia la ventana de mantenimiento desde `Panel-De-Verificaciones`.

1. El panel muestra el checklist de los cuatro supuestos y advierte que el procedimiento es destructivo (corta la energía real).
2. Verificación uno por uno: presupuesto de apagado cronometrado (vigencia 180 días), flag OB observado (365 días), corte con retorno y autoencendido de BIOS por comportamiento.
3. Cada supuesto pasa a verificado con su evidencia y su vigencia. Si el host no arranca solo, el supuesto de autoencendido pasa a refutado, que bloquea permanentemente hasta resolverlo (distinto de vencido, que solo pide repetir).
4. Salida: con 4 de 4 verificados, la modalidad efectiva deja de degradar y el banner de bloqueo desaparece.

Puntos de fricción: es el único flujo con consecuencias físicas irreversibles. La superficie usa confirmaciones explícitas por paso y valida cada acción por efecto observado, nunca por ausencia de error (RN-03).

### 3.4 Configuración de la política de apagado (UF-2 / CU-03)

Disparador: tras semanas de histórico, el administrador ajusta la política.

1. En `Configuracion-De-Politicas`, cada parámetro se presenta por su descriptor (etiqueta, leyenda, default, límites, ejemplos). Puede partir de un preset.
2. Los cambios entran en modo simulación: la explicación "en palabras" describe qué hará la nueva política y a qué afecta.
3. La UI arma una `PropuestaDeConfiguracion`; el administrador la confirma; el sistema la valida (rechaza `tiempoReservadoApagadoSeg > 540`). Se crea una versión nueva de política, inmutable; la vigente no se edita.

Puntos de fricción: el operador podría querer editar la política vigente. El sistema lo redirige al modelo de versiones para preservar la explicabilidad de decisiones pasadas (US-03).

### 3.5 Prueba de batería y veredicto de salud (UF-5 / CU-07)

Disparador: prueba programada trimestral o a pedido.

1. Desde `Prueba-De-Bateria`, el administrador lanza la prueba manual (o la observa cuando la dispara el planificador).
2. El sistema eleva la cadencia a 1 Hz, congela el montaje de batería y mide la caída de tensión durante el autotest.
3. Emite un veredicto propio con confianza explícita y su reserva, comparado contra la línea base a carga igualada. Si la carga concurrente cambió más allá de la tolerancia, la prueba se marca no comparable y no entra en la tendencia.

Puntos de fricción: el operador podría leer el veredicto como certeza. La superficie declara la reserva (sin corrección por temperatura, R-09) y muestra la confianza; el veredicto solo afirma "se comporta peor que antes".

### 3.6 Consulta de históricos (UF-4 / CU-06) e informe (UF-9 / CU-12)

Disparador: preparar una decisión o cerrar un período.

1. En `Historicos-Y-Graficas`, el administrador grafica tensiones y carga superpuestas en un período, con las marcas de eventos encima.
2. La superficie distingue siempre una serie de muestras a resolución completa de una serie de agregados; esta última viaja con su cobertura y su advertencia (RN-10).

---

## 4. Estados y feedback

Mapa de estados por superficie clave. El detalle por superficie vive en cada wireframe; acá se fija el marco.

| Superficie (nombre canónico) | Vacío | Cargando | Con datos | Error | Estados propios |
| --- | --- | --- | --- | --- | --- |
| Alta-Inicial-Administrador | No aplica (acto único) | Resolviendo destino; enviando | Formulario listo | Requisito no cumplido; confirmación no coincidente; envío fuera de tiempo | Aprovisionado (redirige) |
| Acceso-Login | No aplica | Enviando | Formulario listo | Credenciales rechazadas (indiferenciado); acceso restringido temporalmente; formulario vencido | Identidad recién creada; contraseña actualizada; sesión expirada |
| Panel-Estado-En-Vivo | Sin equipos (orientación posterior) | Esperando primer estado en vivo | Estado en vivo con eventos | Sin conexión con el SAI (desconexión USB); circuito del panel caído | Política degradada a solo aviso; tensión fuera de rango |
| Alta-De-Equipos | Sin dispositivos descubiertos | Descubriendo dispositivos USB; probando conexión | Candidato encontrado / inventario cargado | Prueba de conexión fallida; dato obligatorio inválido | Descubierto sin marca ni modelo |
| Configuracion-De-Politicas | Sin política previa (usa defaults del descriptor) | Cargando descriptores | Política vigente + edición | Valor fuera de límites; propuesta rechazada por el sistema | Modo simulación; propuesta en previsualización; ranura del asistente deshabilitada |
| Prueba-De-Bateria | Sin pruebas registradas | Prueba en curso (cadencia 1 Hz) | Veredicto emitido | Precondición no cumplida (flotación insuficiente); muestras perdidas en conmutación | Prueba no comparable |
| Historicos-Y-Graficas | Sin datos en el período | Cargando serie | Serie de muestras / serie de agregados | Período sin cobertura suficiente | Serie agregada con advertencia de cobertura |
| Panel-De-Verificaciones | Cuatro supuestos nunca verificados | Ejecutando paso de verificación | Estado de los cuatro supuestos | Paso fallido (validación por efecto observado); supuesto refutado (bloqueo permanente) | Ventana en curso; vencido vs refutado |

Reglas de feedback (heredadas del catálogo, §5): voz activa; una acción produce una confirmación que la nombra; los errores dicen qué pasó, por qué y qué hacer; la pantalla vacía es una invitación a actuar. Los mensajes de acceso se resuelven desde un catálogo de códigos de resultado (no se componen por vista) y el rechazo de credenciales es indiferenciado. Skeletons por encima de ~400 ms; el estado en vivo del SAI se empuja desde el servidor y se anima solo en la transición de estado.

---

## 5. Accesibilidad

Compromiso: WCAG 2.2 nivel AA como piso, no como extra. Criterios prioritarios verificables:

- Contraste de texto 4.5:1 (3:1 para texto grande) y 3:1 en componentes y estados de foco. El sello de versión, pese a su jerarquía baja, cumple el piso: información secundaria no es información ilegible.
- Foco visible en todo elemento interactivo, con anillo de ≥2px que no dependa solo del color; navegación completa por teclado en orden lógico, sin trampas de foco; objetivos de toque ≥24×24px.
- El color nunca es el único canal: cada estado combina color con etiqueta textual e ícono. El estado de la política (activa, degradada, bloqueada), la calidad de una muestra (completa / parcial / perdida), el estado de un supuesto (verificado / vencido / refutado) y la modalidad efectiva llevan siempre texto.
- Semántica: un `<h1>` por vista (incluida la superficie sin chrome del primer arranque, que mantiene encabezado aunque no tenga navegación); landmarks `nav` y `main`; labels asociados a cada control; `aria-label` en icon-buttons.
- Anuncio de cambios dinámicos: la banda de error con `role="alert"` y la de confirmación con `role="status"`; el estado en vivo del SAI y las alertas de conectividad se anuncian por región activa `aria-live`, para que el operador no dependa de mirar la pantalla en el momento exacto. El mensaje de error de un campo se asocia por `aria-describedby` e indica el rango admitido, no solo "valor inválido".
- Disclosure por teclado con `aria-expanded` en la ayuda contextual, la divulgación progresiva de configuración y el detalle de diagnóstico del sello; la confirmación de copiado del diagnóstico se anuncia como región activa.
- `prefers-reduced-motion` respetado: las animaciones no esenciales (incluida la transición del estado en vivo) se desactivan.

Idioma de la interfaz: español rioplatense. La accesibilidad idiomática incluye no abreviar los estados críticos y escribir los mensajes desde el lado de lo que el operador controla.

---

## 6. Internacionalización

- Idiomas soportados: español, un único idioma. No hay conmutador de idioma ni contenido multilingüe; declararlo evita construir infraestructura de traducción que no tiene consumidor (coherente con el alcance de un único administrador).
- Dirección de lectura: izquierda a derecha.
- Expansión de texto: no aplica cambio de idioma, pero los textos de estado y los mensajes del catálogo de resultados se escriben sin depender de un ancho fijo; los descriptores de parámetros y las leyendas pueden crecer sin romper el layout (grillas fluidas del catálogo).
- Formatos de fecha, número y moneda: fecha y hora locales del host; números tabulares en columnas numéricas. Todo importe se muestra con moneda y fecha explícitas, y su equivalente en USD viaja marcado como derivado con su fuente de cotización (`BNA-divisa-venta`); es una exigencia del contexto de alta inflación, no una preferencia de formato. Las duraciones de eventos cortos se muestran con su incertidumbre estructural (por ejemplo, "5 s ± 10 s"), nunca como un número seco.

---

## 7. Performance percibida

- Estado en vivo del SAI: se empuja desde el servidor (circuito del panel), sin polling desde el navegador. La ronda de sondeo del planificador es de 5 s; el objetivo de percepción es que el estado visible nunca esté más de un intervalo por detrás del real, y que un cambio de estado (por ejemplo, paso a batería) se refleje en la transición siguiente. La latencia de decisión del planificador es < 1 s por ronda (para no desplazar la ronda siguiente); esa cota es técnica y vive en 05, pero fija la expectativa de frescura del panel.
- Skeletons por encima de ~400 ms de espera en listas y series; spinner solo para acciones puntuales. La primera carga del panel muestra el skeleton del bloque de estado hasta recibir el primer empujón.
- Sin optimistic UI en el camino crítico: ninguna acción sobre el equipo se muestra como hecha antes de confirmarse por efecto observado (RN-03). El optimistic UI se admite solo en operaciones reversibles de inventario y configuración en simulación.
- Series históricas de gran volumen (hasta ~6,3 millones de filas/año): la superficie de históricos consulta agregados por defecto y solo baja a resolución completa dentro de la ventana de 30 días; la distinción muestra/agregado es también una decisión de performance, no solo de honestidad del dato.
- Animación: transiciones breves al servicio del cambio de estado y de la orientación espacial, nunca movimiento ambiental permanente; `prefers-reduced-motion` desactiva lo no esencial.

---

## 8. Errores y recuperación

Taxonomía de los errores que el administrador verá, con tono y vía de recuperación. El tono es de voz activa, sin disculpas ni vaguedad, y sin exponer parámetros internos de política.

| Categoría | Ejemplos en este producto | Tono y mensaje | Vía de recuperación |
| --- | --- | --- | --- |
| Acceso rechazado | Par usuario/contraseña inválido | Rechazo indiferenciado, sin decir qué parte falló ni si el usuario existe | Reintentar; el foco vuelve a la banda de resultado |
| Acceso restringido temporalmente | Se superó el umbral de intentos | Declara la restricción y su carácter temporal, sin umbrales ni cuenta regresiva | Esperar y reintentar |
| Sesión vencida | La sesión expiró por inactividad o tope | Retorno al shell de acceso con estado "sesión vencida", sin culpar al usuario ni fallar en una acción arbitraria | Volver a ingresar |
| Envío fuera de tiempo (primer arranque) | El sistema se aprovisionó entre la carga y el envío | Redirección neutra a la superficie de acceso, sin exponer el motivo | Ingresar con la identidad ya creada |
| Pérdida de comunicación con el SAI | 3 sondeos consecutivos sin respuesta; el dispositivo desaparece del bus | Alerta de conectividad en el panel + evento de desconexión registrado; el sistema vigila su propia conectividad y no descarta el problema en silencio | Revisar el cableado / el anclaje físico del dispositivo; el panel se recupera solo al volver la comunicación |
| Circuito del panel caído | Se corta el transporte del panel con el servidor | Aviso de reconexión no bloqueante; el histórico local no se pierde | Reconexión automática; recarga manual si persiste |
| Tensión fuera de rango | `input.voltage` fuera de [198, 242] V sostenido 30 s | Evento entre los recientes, con causa | Informativo; no requiere acción del operador |
| Dato de inventario inválido | Vida de flotación sin temperatura de referencia (RN-13); intervención fechada tras la baja (coherencia temporal) | Error inline asociado al campo, con la regla enunciada en positivo | Corregir el campo señalado |
| Configuración fuera de límites | `tiempoReservadoApagadoSeg > 540` | El control rechaza el valor y declara el rango admitido; la propuesta no se aplica | Ajustar dentro del rango |
| Bloqueo por verificación | Un supuesto requerido está en nunca verificado, vencido o refutado | Banner de bloqueo persistente en la pantalla principal; la modalidad efectiva degrada a solo aviso; distingue vencido (repetir) de refutado (resolver) | Ejecutar la ventana de mantenimiento (CU-10) |
| Ingesta externa rechazada (API) | 409 conflicto de idempotencia; 422 invariante roto | No es superficie de UI del administrador, pero sus efectos (una intervención que no se aplicó) se leen en el histórico con su procedencia | Corrección desde el emisor externo |

Handoff humano: no hay soporte externo; el administrador es el único operador. La recuperación de credenciales no es una superficie (no hay segundo canal de confianza en una instancia propia): es un procedimiento de operación (categoría 09). El detalle de diagnóstico del sello de versión se copia en un solo gesto para documentar cualquier incidente contra la versión exacta que corre.

---

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Persona objetivo | Administrador único (00, Visión §2) |
| CU origen | CU-01 a CU-12 (02); con foco en CU-01, CU-02, CU-03, CU-04, CU-05, CU-06, CU-07, CU-10 |
| Reglas de negocio relevantes | RN-01, RN-02, RN-03, RN-04, RN-05, RN-06, RN-10, RN-11, RN-13 |
| Wireframes que materializan el marco | Wireframes-Alta-Inicial-Administrador, Wireframes-Acceso-Login, Wireframes-Panel-Estado-En-Vivo, Wireframes-Alta-De-Equipos, Wireframes-Configuracion-De-Politicas, Wireframes-Prueba-De-Bateria, Wireframes-Historicos-Y-Graficas, Wireframes-Panel-De-Verificaciones |
| US a generar en 06 | US-01 a US-11 (administrador); US de acceso (alta inicial, ingreso, cambio de contraseña, cierre) a derivar de CU-01 |
| Tests previstos en 08 | Snapshot de estados por superficie; test de accesibilidad AA (contraste, foco, teclado, aria-live); visualización de supuestos y banner de bloqueo; marca de derivado en carga de batería; rechazo por techo de 540 s; distinción vencido/refutado |
| Catálogo de diseño aplicado | Design-Rules-Web-Generico-v1.0 (base) + Design-Rules-Blazor-Mudblazor-v1.0 (stack: Blazor Interactive Server + MudBlazor) |
| Configuración dirigida por esquema aplicada (descriptores, presets, modo simulación, ranura del asistente) | sí (Design-Rules-Config-Esquema; superficie de configuración de políticas y frontera aplicación/entorno declarada en §2.3) |
| Primer arranque aplicado (predicado de aprovisionamiento, guard en tres capas, destino al completar) | sí (Design-Rules-Primer-Arranque; predicado "existe administrador", guard en ruteo/superficie/acción, destino al completar = superficie de acceso) |
| Acceso de operador único aplicado (omisiones declaradas, shell partido, catálogo de resultados, política de sesión) | sí (Design-Rules-Acceso-Monousuario; omisiones en §2.2 y en Wireframes-Acceso-Login) |
| Identidad de versión aplicada (contrato, ubicaciones del sello, detalle de diagnóstico) | sí (Design-Rules-Identidad-De-Version; sello en acceso y en el sistema en funcionamiento) |
| Modelo UX-UI aplicado en la Fase B2 | catálogo base (no se eligió un modelo de Modelos-UX-UI aún) |
| Validación de maqueta | aprobada 2026-07-20, ruta SDD/Maquetas/Sai-Service-Core/ |
| Línea de base emitida | N/A (pendiente Fase B2) |

---

## 10. Notas y supuestos

- Pendientes trazados a P-0x del intake: TLS del panel y la API en la LAN (P-04, configuración de entorno, no se dibuja); contrato del endpoint de rectificación que sugiere el 409 (P-05, no es superficie del administrador); SLO de disponibilidad y tiempos de respuesta objetivo del panel, accesibilidad e i18n formal (P-09, se cierran en 08). Donde falta un dato se marca [derivado] o se refiere el pendiente.
- El intervalo de sondeo (5 s), la cadencia trimestral y las vigencias de verificación (180 / 365 días / sin caducidad) se toman de §17 P.10 del intake y viven como descriptores; sus valores son la fuente única y la superficie los lee, no los hardcodea.
- La ranura del asistente de IA en la superficie de configuración queda reservada y deshabilitada (forward-compat): hoy no realiza ninguna acción; el enganche futuro se hará contra la frontera PropuestaDeConfiguracion sin rediseñar el layout.
- El panel no es el mecanismo de alerta primario: en un corte, la red puede caer (T-05). La fuente de verdad es el histórico local; el panel es lectura.
- No se nombra el stack en el cuerpo de este documento salvo en la fila de trazabilidad "catálogo de diseño aplicado", conforme a las reglas de redacción.

---

## 11. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial. Marco de experiencia del panel Sai-Service-Core: audiencia y contexto, principios (heurísticas de Nielsen y leyes UX), frontera aplicación/entorno, seis flujos clave derivados de UF-1..UF-10, mapa de estados por superficie, accesibilidad WCAG 2.2 AA, i18n español único, performance percibida del estado en vivo, taxonomía de errores y recuperación, trazabilidad upstream/downstream. Aplica el catálogo de diseño base + especialización de stack y las cuatro extensiones de capacidad (config por esquema, primer arranque, acceso monousuario, identidad de versión). Maqueta-aware: los flujos son las rutas que materializará la Fase B2. |
| 1.1 | 2026-07-20 | Corrección de conformidad D7: reemplazo de nombres de stack por vocabulario de dominio tras audit de Fase B. En §1, §2.3 y §8 se sustituyeron menciones de implementación (mecanismo de acceso al equipo, almacenamiento, contenedor, anclaje físico del dispositivo) por términos agnósticos de implementación; sin cambios de semántica. |
| 1.2 | 2026-07-20 | Retroalimentación de la Fase B2 de validación de maqueta: unificación de 'parque' → 'equipos' y 'secreto' → 'contraseña'. |
