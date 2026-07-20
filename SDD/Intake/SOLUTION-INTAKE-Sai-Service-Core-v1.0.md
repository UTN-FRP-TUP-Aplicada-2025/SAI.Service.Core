# SOLUTION-INTAKE — SAI.Service.Core

| Campo | Valor |
|---|---|
| Nombre de la solución | SAI.Service.Core |
| Cliente / Stakeholder principal | Administrador único del host Linux `i7infra` (proyecto interno, autopromovido) |
| Repositorio | `DEV/SAI.Service.Core` (workspace local; sin remoto declarado en las fuentes) |
| Lead técnico | Administrador único (rol combinado propietario / implementador / beneficiario) |
| Documento | `SOLUTION-INTAKE-Sai-Service-Core-v1.0.md` |
| Versión | 1.0 |
| Fecha | 2026-07-20 |
| Stack principal | .NET 10 + Blazor (interactive server) + Entity Framework Core + SQLite |
| Estado | Borrador |

> Este documento captura qué quiere el cliente, cómo se compone la solución y cómo se construye cada proyecto.
> El orquestador deriva de §13 el `SOLUTION-MANIFEST` canónico; no se completa el manifiesto a mano.

**Procedencia de este intake.** Todo el contenido de la Parte A y el modelo de dominio se derivan de `PROMPTs/Generar-SDD/Inputs/Planteo-Analisis-Unificado-Antecedente-SAI-Service.md` (4017 líneas, estado `draft`, 2026-07-19) del repositorio `SAI.Service.Core.Documentacion`. La Parte B y las decisiones de stack de la Parte C se derivan del tool-prompt `PROMPTs/Generar-SDD/Crear-SDD-Documento-Intake.md` y de los inputs `Topologia-Proyecto-Solucion.md` y `Entorno-Desarrollo.md`. Donde una afirmación no tiene respaldo en esas fuentes, se marca explícitamente como **[derivado]** (inferencia trazable a un dato de las fuentes) o **PENDIENTE** (sin respaldo, requiere respuesta del stakeholder). No se inventó información.

**Advertencia heredada de la fuente.** El documento antecedente se autodeclara: *"No es un diseño cerrado ni una especificación"*, *"No hay código, ni esquema SQL definitivo, ni contratos de API cerrados"*. Los flujos de usuario de §6 son **propuesta de diseño, no comportamiento verificado**. Lo que sí está verificado es el relevamiento del equipo físico (2026-07-19).

---

# Parte A — Negocio de la solución

## §1 Idea y problema

Un servidor Linux doméstico/de laboratorio (`i7infra`, criticidad alta, entre 3 y 8 contenedores según la semana) está respaldado por un SAI que se comunica por USB. El monitoreo básico y el apagado ordenado **ya están resueltos** por NUT (`upsmon` + `upssched`), y la fuente es taxativa al respecto: no hace falta reconstruir eso. El dolor está en todo lo que NUT no hace y ninguna herramienta —libre ni comercial— resuelve para este equipo:

- **No hay histórico de salud de batería.** El equipo relevado no expone ningún indicador de salud y su autotest *no devuelve veredicto*: 51 muestras consecutivas sin `TEST`, sin `RB` y sin `ups.alarm`. El estándar de facto del monitoreo de SAI es confiar en la bandera `RB` («replace battery») del firmware, y **en este equipo esa bandera nunca va a llegar**. Un monitoreo convencional acá no alerta nunca. La salud solo puede obtenerse midiendo la caída de tensión durante el autotest y guardando la serie temporal — y ningún proyecto de software libre lo hace: todos retransmiten la bandera del firmware.
- **No hay modelo de ciclo de vida.** Altas, recambios de batería, reparaciones, sustitución del SAI, asociación de cada métrica al período de la batería que estaba montada: NUT no tiene modelo de inventario. `upslog` produce texto plano.
- **No hay verificación viva de los supuestos.** Que el host reencienda solo tras un corte es un supuesto que puede volverse falso en silencio (pila CMOS agotada, un *clear CMOS*, alguien que cambió el ajuste sin documentarlo). Ninguna herramienta lo vigila.
- **No hay panel remoto ni API.** NUT expone variables, no una interfaz de administración; el software del fabricante (ViewPower) tiene un RCE sin parche y no es una alternativa.

Debajo de todo eso hay un **problema crítico y bien delimitado: garantizar el reencendido**. La BIOS solo dispara el autoencendido cuando detecta una *transición* de ausencia a presencia de energía; por lo tanto el SAI **debe cortar su salida aunque el host ya esté apagado**. Si vuelve la energía durante la cuenta regresiva y el SAI cancela su apagado, no hubo transición, la BIOS no tiene nada que detectar y —textual— *"el host queda apagado hasta que alguien apriete el botón"*. La fuente lo califica: *"Es el peor resultado posible: el sistema se protegió correctamente de un corte que resultó ser breve, y a cambio quedó fuera de servicio indefinidamente."*

**Qué pasa si NO se construye.** El host **no tiene backups** — es lo que da urgencia al apagado ordenado y lo que hace grave equivocarse. Sin esta solución: (a) un corte prolongado corrompe un servidor sin respaldo; (b) la batería se degrada sin que nada avise, porque la única bandera que el monitoreo convencional mira nunca se enciende; (c) no hay forma de saber cuánto duró de verdad una batería ni de decidir la próxima compra con datos.

**Por qué ahora.** El relevamiento del equipo ya está hecho y verificado (2026-07-19) y la línea base de salud de batería ya fue tomada (caída de −0,47 V, mínimo 12,94 V, recuperación a 13,24 V en ~35 s con carga 13 %). La reserva **O-M6** advierte que esa línea base *"llega tarde"*: la batería está en servicio desde 2024, así que cada trimestre que pasa sin registrar la tendencia es un punto de datos perdido.

---

## §2 Audiencia y stakeholders

La fuente no usa la taxonomía propietario/implementador/beneficiario; la clasificación de la columna «Categoría» es **[derivado]** a partir de los roles que sí describe.

| Rol | Nombre o cargo | Categoría | Responsabilidad principal |
|---|---|---|---|
| Administrador único | `usr-admin` — *"Un único usuario administrador. Autenticación mínima."* | Propietario **y** Implementador **y** Beneficiario (una sola persona) | Aprueba este intake. Da de alta el parque (UF-1), configura políticas (UF-2), monitorea (UF-3), consulta históricos (UF-4), dispara pruebas manuales (UF-5), carga intervenciones (UF-6, UF-7), ejecuta la ventana de mantenimiento con presencia física (UF-8) y emite informes (UF-9). |
| Técnico externo / Proveedor | `Proveedor` (razón social, contacto, especialidad); en el escenario E-6 `"ejecutadaPor": "técnico externo"`, proveedor `prov-taller-electronica-sur` (ficticio) | Ejecutor externo (beneficiario indirecto) | Ejecuta recambios de batería, reparaciones e inspecciones preventivas. Retira las baterías agotadas y consta como `disposicionFinal.receptor` para trazabilidad ambiental. |
| Sistema externo GMAO | `fd-gmao-externo` — *"GMAO Corporativo v4"*, confianza base `media` | Integrador / consumidor de la API | Empuja intervenciones sin intervención humana vía `POST /api/v1/intervenciones` con `X-Idempotency-Key`. Es el actor de UF-10. |
| Host protegido | `i7infra`, criticidad `alta` | Beneficiario (sistema) | Es el objeto de la protección: el apagado ordenado y el reencendido automático son sobre él. Carga variable (de 3 a 8 contenedores en menos de una semana; `ups.load` de 13 % a 30 % al sumar dos contenedores de IA — observación **O-U8**). |
| Poller local | `fd-poller-local` — *"sai-service poller v1"*, confianza base `alta` | Componente del sistema | Sondeo y persistencia de métricas medidas. Es la fuente de datos de máxima confianza del sistema. |

**Gestión de usuarios y roles: fuera de alcance por decisión explícita** — *"Un solo administrador"* (§3.2 de la fuente). No hay jerarquía de usuarios que modelar más allá del administrador único.

---

## §3 Propuesta de valor y diferenciación

**Qué hace hoy el cliente y por qué no le alcanza.** Hoy corre NUT (`upsmon` + `upssched`) sobre el host. Eso monitorea y apaga ordenadamente, y nada más: no tiene inventario, expone variables en vez de una interfaz de administración, y su `upslog` produce texto plano. Las alternativas evaluadas y descartadas con fundamento son ViewPower (RCE sin parche), y el conjunto Grafana / Home Assistant / exportadores Prometheus, sobre los que la fuente concluye (**O-M8**): *"Ningún proyecto de software libre calcula salud de batería a partir de datos de NUT; todos retransmiten la bandera del firmware."* El propio espacio de nombres de NUT *"no tiene ninguna variable de estado de salud"*.

**La promesa central.** Un servicio propio que (1) garantiza que el host vuelva a encenderse solo tras un corte, y se **niega a apagarlo mientras no pueda probarlo**; y (2) construye el histórico de salud, ciclo de vida y costos que ninguna herramienta existente construye para este equipo.

Diferenciadores, en orden de defendibilidad:

1. **Cálculo propio de salud de batería, con procedencia y límites declarados.** Textual: *"Calcular salud propia no es sobre-ingeniería: es la única opción."* El veredicto lo emite el servicio porque el equipo no emite ninguno, y viaja con su nivel de confianza y su advertencia.
2. **Regla de bloqueo de seguridad.** El sistema no habilita el apagado si algún supuesto del que depende está en `NuncaVerificado`, `Vencido` o `Refutado`: la acción queda `BloqueadaPorVerificacion` y la modalidad efectiva degrada a `SoloAlerta`. Es una garantía que ninguna alternativa ofrece porque ninguna modela los supuestos.
3. **Verificación continua por evidencia.** El servicio cruza sus propios eventos de corte con `wtmp` del host para probar que el reencendido automático sigue funcionando, sin repetir la prueba destructiva. La ventana de mantenimiento se hace una vez; la verificación sigue viva sola.
4. **Trazabilidad total del origen de cada valor.** Cada número declara si fue `medido`, `derivado`, `estimadoPorDriver`, `declarado`, `imputado` o `noCalculable`. Responde sin leer código la pregunta *«¿este número lo midió el aparato o lo calculó el software?»*.
5. **Comparación de marcas por desempeño real observado**, con el costo por año de servicio normalizado a USD — necesario porque comparar 52.000 ARS de 2026 con 180.000 ARS de 2024 no significa nada.

---

## §4 Alcance funcional pretendido (MoSCoW)

La fuente **no trae etiquetas MoSCoW**. La columna MoSCoW de esta tabla es **[derivado]** a partir de la separación explícita que sí trae: «Primera entrega» (§3.3) ⇒ Must Have; «Dentro del alcance» no incluido en la primera entrega ⇒ Should/Could; «Fuera del alcance» (§3.2) ⇒ Won't Have v1. La columna «Origen» cita la sección de respaldo.

| ID | Capacidad | MoSCoW | Origen |
|---|---|---|---|
| F-01 | Servicio web dockerizado, exclusivamente Linux, corriendo dentro de un host Linux | Must Have | §3.1, §3.3 |
| F-02 | Diálogo con el SAI a través de NUT (adaptador de conexión con implementación NUT) | Must Have | §3.3, §5.2 |
| F-03 | Identificación del dispositivo USB desde el panel, con prueba de conexión | Must Have | §3.1, UF-1 |
| F-04 | Alta de catálogo (fabricante, modelos) e inventario (host, SAI, batería) con ciclo de vida y baja lógica | Must Have | §3.1, §7, UF-1 |
| F-05 | Vínculos temporales `MontajeBateria` y `CoberturaHost`, resueltos por `ResolutorTemporal` | Must Have | §7, UF-4, UF-6, UF-7 |
| F-06 | Sondeo periódico con frecuencia configurable y persistencia de `Muestra` con calidad (`completa`/`parcial`/`perdida`) | Must Have | §3.1, §5.3, UF-3 |
| F-07 | Procedencia por valor (`Origen`) en todo dato almacenado, sin excepción | Must Have | §7 P-3, I-7 |
| F-08 | Derivación de eventos (`Microcorte`, `CorteSuministro`, `RetornoRed`, `DesconexionUsb`, `TensionFueraDeRango`) por `ReglaDerivacion` versionada | Must Have | §7, UF-3 |
| F-09 | Detección de pérdida de comunicación con el equipo (3 sondeos fallidos ⇒ `DesconexionUsb`) y alerta visual | Must Have | §5.3, O-U11 |
| F-10 | Políticas de apagado versionadas (`Politica` / `VersionPolitica`) con modalidades `SoloAlerta`, `SoloHost`, `HostLuegoUpsConRetorno`, `CicloForzado` | Must Have | §3.1, UF-2 |
| F-11 | Entidad `Verificacion` de supuestos, con evidencia, método, vigencia y estados `NuncaVerificado`/`Verificado`/`Vencido`/`Refutado`; degradación forzada a `SoloAlerta` | Must Have | §4.7, UF-8 |
| F-12 | Planificador interno: rondas de evaluación de políticas, temporizadores con cancelación, ejecución de acciones y registro del resultado | Must Have | §5.3 |
| F-13 | Panel web con estado en vivo, conectividad, panel de supuestos y eventos recientes | Must Have | §3.3, UF-3 |
| F-14 | Registro de intervenciones de servicio técnico con costos (`Costos.cuadra()`) y efectos aplicados (`Efectos`) | Must Have | §3.1, UF-6 |
| F-15 | Autenticación mínima de administrador único | Must Have | §3.1 |
| F-16 | Prueba de batería programada (trimestral) y manual, con cadencia densa a 1 Hz y congelado del `montajeBateriaId` | Must Have | UF-5, §6.7 |
| F-17 | Veredicto de salud calculado por el servicio, con confianza explícita y comparación contra línea base a carga igualada | Must Have | UF-5, §6.7 |
| F-18 | Históricos y gráficas de evolución (voltajes, carga, microcortes), individuales o superpuestas, con marcas de eventos | Must Have | §3.1, UF-4 |
| F-19 | Agregación y retención (resolución completa `P30D`, agregados `PT1H` durante `P10Y`, eventos indefinidos), con `cobertura` obligatoria | Must Have | §7, UF-4, I-20 |
| F-20 | API REST de ingesta idempotente para fuentes externas (`X-Idempotency-Key`, 201/200/409/422) | Must Have | §3.1, UF-10 |
| F-21 | Ciclo de vida del SAI: reparación, sustitución y cobertura suplente; días sin protección registrados | Should Have | UF-7 |
| F-22 | Informe de período (dispositivos activos, cobertura, baterías intervinientes, intervenciones, costos en USD, eventos, calidad de suministro, pruebas) | Should Have | UF-9 |
| F-23 | Comparación de marcas y modelos por `costoPorAnioDeServicio` normalizado a USD, agrupando `FichaVidaUtil` por `ModeloBateria` | Should Have | UF-9 |
| F-24 | Adaptador de conexión simulado, para probar políticas sin hardware ni riesgo y cubrir el flujo F-3 en pruebas automatizadas | Should Have | §5.2 |
| F-25 | Renovación automática de verificaciones por evidencia (un corte real que muestre `OB` renueva `ver-flag-ob`; un corte seguido de arranque automático renueva `ver-bios-autoencendido`) | Should Have | §4.7, UF-8 |
| F-26 | Capa de add-ons de dialecto de protocolo — **diseñada pero no implementada** en la primera entrega | Could Have | §3.3, §8 |
| F-27 | Adaptador de conexión directo (sin NUT) para equipos que NUT no cubra | Could Have | §5.2 |
| F-28 | Apagado de otros equipos de la red | Won't Have v1 | §3.2 |
| F-29 | Múltiples SAI simultáneos (*"El modelo los contempla, la implementación no"*) | Won't Have v1 | §3.2 |
| F-30 | Notificaciones externas (mail, SMS) como mecanismo primario de alerta | Won't Have v1 | §3.2 |
| F-31 | Gestión de usuarios y roles | Won't Have v1 | §3.2 |
| F-32 | Lectura del ajuste de BIOS «Restore on AC Power Loss» por software | Won't Have v1 | §3.2, §4.6 |
| F-33 | Escritura del traductor de protocolo del equipo actual (ya resuelto por NUT y verificado) | Won't Have v1 | §3.2 |

El conjunto Must Have es el mínimo razonable: sin F-01 a F-20 el servicio no cumple ninguno de sus dos propósitos (garantizar el reencendido y construir el histórico de salud).

---

## §5 Historias de usuario / experiencias deseadas

La fuente **no trae historias redactadas en formato Como/quiero/para**. Las siguientes son **[derivado]**: cada una se construye sobre una cita literal del requisito original (§7.15) o sobre el «Quién» explícito de un flujo de §9, indicado en la columna de trazabilidad.

| ID | Historia | Respaldo en la fuente |
|---|---|---|
| US-01 | Como **administrador**, quiero **dar de alta el SAI y su batería descubriendo el dispositivo USB desde el panel**, para **empezar a registrar historia sin editar archivos de configuración a mano**. | UF-1: *"el administrador único, la primera vez que abre el panel"* |
| US-02 | Como **administrador**, quiero **ver en el panel cuántos de los supuestos de la política de apagado están verificados**, para **saber si el sistema realmente va a apagar el host o solo va a avisar**. | UF-1 / UF-3: *«la política de apagado se apoya en 5 supuestos; 3 verificados, 1 vencido, 1 nunca probado»* |
| US-03 | Como **administrador**, quiero **configurar la política de apagado creando una versión nueva en vez de editar la vigente**, para **que las decisiones pasadas sigan explicándose con la configuración que las produjo**. | UF-2 / §7.15: *«conviene saber con qué configuración se tomó una decisión»* |
| US-04 | Como **administrador**, quiero **ver el estado del SAI en vivo desde cualquier equipo de la LAN**, para **enterarme de un problema sin estar sentado frente al servidor**. | UF-3: *"desde cualquier equipo de la LAN"*; objetivo 5 |
| US-05 | Como **administrador**, quiero **saber de qué origen viene cada número que veo**, para **no construir una conclusión sobre un valor que el driver interpoló**. | §7.15: *«la trazabilidad de los valores, cuál fue su origen»*; objetivo 6 |
| US-06 | Como **administrador**, quiero **graficar voltajes y carga superpuestos en un período con las marcas de eventos encima**, para **evaluar la calidad del suministro durante la vida del host**. | §7.15: *«Calidad de servicio de red durante la vida del host»*; UF-4 |
| US-07 | Como **administrador**, quiero **que el servicio pruebe la batería trimestralmente y me diga si se está degradando**, para **planificar el recambio antes de que falle**. | §7.15: *«Planificar recambios»*; UF-5 |
| US-08 | Como **administrador**, quiero **registrar el recambio de batería con su costo, sus hallazgos y su destino final**, para **que un solo acto cierre la vigencia vieja, abra la nueva y proyecte la ficha de vida útil**. | UF-6; §7.15: *«qué servicios técnicos y de qué tipo»* |
| US-09 | Como **administrador**, quiero **registrar que un SAI se fue a reparación y otro lo cubrió**, para **que el histórico diga qué equipo protegía al host en cada tramo y cuántos días quedó sin protección**. | UF-7; §7.15: *«en ese período de vida dónde intervino qué UPS, qué batería, qué eventos intervinieron»* |
| US-10 | Como **administrador con presencia física**, quiero **una ventana de mantenimiento guiada que verifique los cuatro supuestos uno por uno**, para **desbloquear el apagado automático con evidencia y no con optimismo**. | UF-8: *"con presencia física, en una ventana planificada"* |
| US-11 | Como **administrador**, quiero **un informe de período y una comparación de modelos por costo por año de servicio en USD**, para **decidir qué marca comprar la próxima vez**. | §7.15: *«decidir en pos a marcas o productos con mejores desempeños»*; *«Estimar costos de mantenimiento»*; UF-9 |
| US-12 | Como **sistema externo de gestión de mantenimiento**, quiero **empujar intervenciones por API con una clave de idempotencia**, para **que un reintento de red no duplique el hecho ni corrompa el histórico**. | §7.15: *«esta información podría ser capturada por servicios externos de forma automatizada»*; UF-10 |

Roles cubiertos: administrador (US-01 a US-11) y sistema externo (US-12).

---

## §6 Flujos típicos

Los diez flujos siguientes están descriptos en §9 de la fuente. Se transcriben los tres que la propia fuente marca como de mayor frecuencia o criticidad; los diez completos se enumeran después con su objetivo y su escenario de respaldo. **Advertencia de la fuente:** *"Los flujos son propuesta de diseño, no comportamiento verificado… Lo que no es propuesta son los datos que atraviesan cada flujo: cada uno se ancla a un escenario del Anexo A con su JSON completo."*

### Flujo del 80 % del tiempo — UF-3 · Monitoreo en vivo

1. El administrador abre el panel desde cualquier equipo de la LAN.
2. El planificador, cada `intervaloSegundos` (5 s por defecto), le pide el estado al SAI a través del adaptador NUT.
3. Si la respuesta llega completa, persiste una `Muestra` con `calidad = completa`; si llega incompleta, `parcial` (se conserva: descartarla entera perdería las variables que sí llegaron); si no llega, `perdida` e incrementa el contador de fallidos.
4. Evalúa las reglas de derivación sobre la ventana reciente y genera los eventos que correspondan.
5. Empuja estado y eventos nuevos al panel, que muestra: estado y tensiones, batería (con `battery.charge` **marcado como derivado**), conectividad (alerta a los 3 sondeos fallidos consecutivos), *n* de *m* supuestos verificados, y los eventos recientes con su regla y versión.
6. Si falta algún supuesto, el panel avisa en la pantalla principal —no enterrado en configuración— que la política está degradada a `SoloAlerta`.

### Flujo crítico que rara vez pasa y no puede fallar — UF-8 · Ventana de mantenimiento

Es el flujo que desbloquea el objetivo 1. Sin él, el servicio nunca sale de `SoloAlerta`. Requiere presencia física y es **destructivo por naturaleza**: implica cortar la energía al host.

1. El administrador inicia la ventana de verificación desde el panel; el panel muestra el checklist de los cuatro supuestos.
2. Con los contenedores detenidos, se cronometra el apagado completo del host bajo carga y se registra el tiempo ⇒ `ver-presupuesto-apagado` pasa a `Verificado`, con vigencia corta a propósito (**180 días**: la carga del host cambia).
3. Se corta la alimentación de red al SAI y se observa `ups.status = OB` ⇒ `ver-flag-ob` pasa a `Verificado` (vigencia 365 días).
4. Se ejecuta un `shutdown.return` controlado; el SAI corta la salida al host.
5. Se restaura la energía; el SAI restablece la salida.
6. Si el host arranca solo, sin tocar el botón: `ver-bios-autoencendido` y `ver-shutdown-return` pasan a `Verificado`, y la modalidad `HostLuegoUpsConRetorno` ya es efectiva. Si no arranca: `ver-bios-autoencendido` pasa a **`Refutado`**, que bloquea permanentemente hasta que alguien lo resuelva (`Refutado` no es `Vencido`: una prueba fallida bloquea, una vencida solo pide repetirla).

### Onboarding — UF-1 · Alta del parque y puesta en marcha

1. El panel lista los candidatos USB con sus descriptores; el adaptador identifica VID:PID y devuelve `0665:5161 · INNO TECH · iSerial vacío` — candidato encontrado, **sin marca ni modelo**.
2. El administrador declara a mano marca, modelo y potencia nominal, que quedan con procedencia `declarado`. Si la potencia nominal se desconoce, queda `null` con procedencia `imputado` — **nunca un número inventado**.
3. Alta de catálogo (fabricante, `ModeloDispositivo`, `ModeloBateria`) e inventario (host, SAI, batería).
4. Apertura de los vínculos `MontajeBateria` y `CoberturaHost` con `hasta = null`.
5. Prueba de conexión y creación de la `SesionSondeo` con su mapa variable→origen.
6. Siembra de las cuatro verificaciones en `NuncaVerificado`, lo que fuerza `SoloAlerta`. El panel muestra un aviso permanente: «operativo · 0 de 4 supuestos verificados», con enlace a UF-8.

### Los diez flujos y sus dependencias

| Flujo | Quién | Objetivo que sirve | Escenario de respaldo |
|---|---|---|---|
| UF-1 · Alta del parque y puesta en marcha | Administrador, primera vez | 2, 5 | E-1 |
| UF-2 · Configuración de políticas | Administrador, tras semanas de histórico | 1 | E-1, E-4 |
| UF-3 · Monitoreo en vivo | Administrador, desde la LAN | 3, 4 | E-2, E-3, E-4 |
| UF-4 · Históricos y gráficas | Administrador, preparando una decisión | 3, 6 | E-2, E-7 |
| UF-5 · Prueba de batería y salud | Planificador (programada) o administrador (manual) | 4 | E-5 |
| UF-6 · Servicio técnico: recambio de batería | Administrador, tras la intervención | 2, 7 | E-6 |
| UF-7 · Reparación o sustitución del SAI | Administrador, cuando el SAI falla | 2 | E-1 parcial (`ups-02` en stock) |
| UF-8 · Ventana de mantenimiento | Administrador con presencia física | 1 | E-4 |
| UF-9 · Informe de período y comparación de marcas | Administrador, al cerrar un año o comprar | 2, 7 | E-7 |
| UF-10 · Ingesta automatizada | Sistema externo, sin intervención humana | 8 | E-8 |

Grafo de dependencias entre flujos: UF-1 → UF-2, UF-3; UF-2 → UF-3; UF-3 → UF-4, UF-5; UF-5 → UF-6; UF-6 → UF-9; UF-7 → UF-9; UF-8 → UF-2; UF-4 → UF-9; UF-10 → UF-9.

---

## §7 Casos límite y "qué pasa si"

Todos los casos siguientes están planteados en la fuente. La columna «Respuesta» recoge la resolución que la fuente ya da; donde la fuente no resuelve, se marca PENDIENTE.

| # | Qué pasa si… | Respuesta según la fuente |
|---|---|---|
| CL-01 | **Vuelve la energía durante la cuenta regresiva del apagado.** El SAI cancela su corte, no hay transición de energía y la BIOS no dispara el autoencendido. | Resuelto: modalidad `CicloForzado`. *"Una vez iniciada la secuencia de apagado, el corte del SAI no debe cancelarse aunque vuelva la red. Es preferible un apagón controlado de tres minutos a un servidor apagado hasta la mañana siguiente."* No usar `shutdown.stop`. |
| CL-02 | **El firmware del SAI no soporta el comando de apagado con retorno.** Nota del protocolo Megatec: *«S01R0001 and S01R0002 may not work on early firmware versions. The failure mode is that the UPS turns off and never returns.»* | No resuelto por diseño: se **verifica** en UF-8 antes de habilitar el apagado (`ver-shutdown-return`, sin caducidad). Riesgo S-1, severidad crítica. |
| CL-03 | **El ajuste de BIOS «Restore on AC Power Loss» se vuelve falso en silencio** (pila CMOS agotada, *clear CMOS*, cambio no documentado). | No es legible por software y se descarta intentarlo (§4.6). Se verifica por comportamiento (`ver-bios-autoencendido`, vigencia 365 d) y se renueva sola con cada corte real seguido de arranque automático, cruzando eventos propios contra `wtmp`. Riesgo S-3. |
| CL-04 | **El apagado del host no cabe en los 540 s del techo duro** del `ups.delay.shutdown`. | El SAI corta con el host a medio bajar. Mitigación: `ver-presupuesto-apagado` con vigencia de solo 180 días, y el panel debe mostrar cuánto de ese presupuesto ya consume el apagado de contenedores. Riesgo S-2. |
| CL-05 | **Crece la carga del host** (de 3 a 8 contenedores en una semana; `ups.load` de 13 % a 30 %). | La verificación del presupuesto de apagado caduca. Es la razón de la vigencia de 180 días. Observación O-U8. |
| CL-06 | **El flag `LB` (low battery) nunca llega.** No fue observado en el relevamiento. | La política **no debe depender de él**: usar tiempo en `OB` + `battery.voltage`. Riesgo S-4. |
| CL-07 | **Un comando no llega al equipo y no produce ningún error.** Ocurrió durante el relevamiento; se detectó comparando datos. | Regla dura: *"toda acción debe validarse por efecto observado, no por ausencia de error"*. Un servicio que asuma que «no hubo excepción» equivale a «se ejecutó» **va a mentir**. |
| CL-08 | **Hay un corte de energía y la red también cae**, así que las notificaciones remotas no salen (E-4: correo con `connect: network is unreachable`, webhook con `connect: no route to host`). | **Esperable**, no un fallo: el router también está sin energía. Por eso el histórico local es la fuente primaria y las notificaciones un extra. |
| CL-09 | **Dos cortes seguidos.** | La batería no está llena en el segundo: ~79 min de recarga tras 6 min de descarga. |
| CL-10 | **Un microcorte de 3 s con sondeo cada 5 s.** | *"No es detectable de forma confiable"* (O-M4). La duración lleva **incertidumbre estructural**: en E-3 se registra `duracionSegundos: 5` con `incertidumbreDuracionSeg: 10`. Un informe que sume duraciones sin propagar la incertidumbre produce un total con error del 100 %. |
| CL-11 | **Una sola muestra atrapa el corte.** | Es el caso realista, no el excepcional. Una regla que exija dos muestras consecutivas **no detectaría nada**. |
| CL-12 | **El driver responde incompleto o no responde.** | Muestra `parcial` (se conserva; los cálculos deben tolerar nulos por variable, no solo por muestra) o `perdida` con `null`. Un parser que exija `battery.voltage` no nulo falla contra datos reales. |
| CL-13 | **Se pierden muestras durante la prueba de batería a 1 Hz.** | Sistemático, no mala suerte: *"el equipo deja de atender consultas mientras conmuta"*, y las muestras perdidas caen justo en el instante más informativo. Con sondeo cada 30 s el evento pasa desapercibido. |
| CL-14 | **El nodo USB desaparece del bus** (O-U11, O-U12). | El servicio **debe vigilar su propia conectividad**: 3 sondeos consecutivos sin respuesta ⇒ evento `DesconexionUsb` y alerta. Riesgo S-6. |
| CL-15 | **Cambia la versión de una regla de derivación** (el umbral de microcorte pasó de 30 s en v1 a 60 s en v2). | El evento guarda `reglaDerivacionId` y `reglaVersion`. Una consulta de tendencia que mezcle versiones sin normalizar **produce una serie corrupta**; el histórico deja de ser comparable y nada más lo indicaría. |
| CL-16 | **El promedio horario borra los microcortes** que el sistema quiere estudiar. | Se conservan mínimo y máximo además del promedio, y el conteo de microcortes sale de `Evento`, **nunca** de la serie agregada. |
| CL-17 | **Una batería se retira, se prueba en banco y se reinstala**, o se mueve a otro SAI, o un SAI se va a reparación y otro cubre el host. | Resuelto por el vínculo temporal: son dos períodos, no uno. Con la vigencia como atributo de la batería (modelo descartado C-1) el tercer caso *"ni siquiera es representable"*. |
| CL-18 | **Hay que corregir la fecha de un recambio ya cargado.** | La corrección **reatribuye automáticamente** todo el histórico afectado, sin migrar datos, porque la historia guarda dispositivo e instante y la batería se resuelve consultando `MontajeBateria`. |
| CL-19 | **Llega un dato obligatorio vacío o mal formado**: batería sin número de serie, SAI sin `iSerial`, `fechaFabricacion` anterior a `fechaCompra`, potencia nominal desconocida. | Ninguno es un error: el número de serie es **anulable a propósito** (muchas SLA de gama baja no lo traen), `fechaFabricacion < fechaCompra` es lo normal y la edad real se cuenta desde ahí, y la potencia nominal desconocida queda `null` con procedencia `imputado`. |
| CL-20 | **Alguien quiere borrar una entidad** (batería agotada, SAI dado de baja). | El borrado físico **no existe en este dominio**. Baja lógica siempre (`estado` + `fechaBaja` + `motivoBaja`). Una entidad dada de baja se puede consultar con todo su historial pero **no operar**: una intervención fechada después de la baja se rechaza con 422 `coherencia_temporal`. |
| CL-21 | **Dos veces el mismo hecho por la API externa** (la red falla y el emisor reintenta). | Es el caso normal, no el excepcional. Misma clave + mismo cuerpo ⇒ 200 con `creado=false` y el mismo id. Misma clave + cuerpo distinto ⇒ **409**, no 200: *"Devolver 200 sería peor que duplicar: el emisor creería que su corrección se aplicó."* |
| CL-22 | **Los costos de una intervención externa no cuadran.** | 422. *"Es el invariante que la ingesta externa rompe primero. Sin él, los costos agregados quedan mal en silencio."* |
| CL-23 | **Una batería al 20 % de su capacidad flota igual que una nueva**, y una batería con tensiones y valores óhmicos normales puede tener 6 % de capacidad (caso Battcon 2017). | Reconocido como límite duro del método: *"ninguna de estas señales indirectas certifica que una batería sirva"*. Lo único que el método adoptado puede afirmar es *«Esta batería se comporta peor que antes; conviene revisarla»*. |
| CL-24 | **La temperatura del gabinete oscila con la estación** y confunde la tendencia de salud. | Sin solución con este equipo: no hay sensor de temperatura (`temperaturaAmbienteC` es siempre `null`). *"La oscilación estacional puede rivalizar con la señal de degradación en un año."* Toda conclusión lleva esa reserva declarada. Riesgo S-9. |
| CL-25 | **Se hace una prueba de batería poco después de un corte.** | Mide otra cosa. Precondición: tiempo mínimo en flotación cumplido, validado por el dominio antes de disparar. |
| CL-26 | **La carga concurrente cambió entre dos pruebas.** | Si `deltaCargaConcurrente` excede la tolerancia, la prueba se registra pero se marca `comparable: false` y **no entra en la tendencia** (I-16). |
| CL-27 | **Se sustituye el SAI por otro modelo.** | El dialecto hay que relevarlo otra vez, el panel debe disparar el procedimiento de caracterización, y **todas las verificaciones de firmware vuelven a `NuncaVerificado`** porque fueron probadas contra otro equipo. El anclaje del USB por ruta física de puerto hace que el binding no se rompa. |
| CL-28 | **NUT compite por el nodo USB** con el contenedor, con NUT en el host (O-U1) o con otro contenedor (O-U2). | Decisión abierta: NUT dentro del contenedor (un artefacto desplegable, más limpio) o en el host con el servicio como cliente TCP. Riesgo S-5, S-8. **PENDIENTE de decisión en el Sprint 0.** |

---

## §8 Métricas de éxito desde el negocio

La fuente **no formula métricas de éxito** con target y plazo. Las métricas siguientes son **[derivado]**: la magnitud y la línea base salen del documento; el target y el plazo son propuestos por este intake sobre esos números y deben ser ratificados por el stakeholder.

| ID | Criterio | Métrica | Línea base (fuente) | Target | Plazo |
|---|---|---|---|---|---|
| M-01 | El servicio sale del modo degradado y puede apagar el host | Supuestos verificados sobre supuestos requeridos | **0 de 4** al alta (UF-1) | **4 de 4** verificados, con modalidad efectiva `HostLuegoUpsConRetorno` | 1 mes desde la puesta en marcha (una ventana de mantenimiento UF-8) |
| M-02 | La tendencia de salud de batería es estadísticamente utilizable | Cantidad de pruebas de batería **comparables** acumuladas (carga concurrente dentro de tolerancia) | 1 prueba (línea base 2026-07-19, caída −0,47 V, carga 13 %) | **≥ 4 pruebas comparables** — umbral declarado por la fuente para pasar de confianza `baja` a una tendencia legible | 12 meses (cadencia trimestral) |
| M-03 | El host queda protegido de forma continua | `disponibilidadRespaldo` = días con cobertura / días del período | 1,0 en el período de ejemplo E-7 (365 con protección, 0 sin) | **≥ 0,98** de días con `CoberturaHost` vigente, y **0 arranques `crash`** en `wtmp` atribuibles a un corte no gestionado | Primer año calendario completo de operación |
| M-04 | El histórico es apto para decidir una compra | Modelos de batería con `FichaVidaUtil` cerrada y `costoPorAnioDeServicio` normalizado a USD | 1 ficha proyectable (batería en servicio desde 2024; 37.430 ARS/año ≈ **29,50 USD/año**) | **≥ 2 modelos** comparables, con `cumplioExpectativa` y `desvio` calculados contra `vidaFlotacionEsperada` | Al cierre del primer recambio de batería posterior a la puesta en marcha |
| M-05 | El servicio no miente sobre el origen de sus datos | Valores almacenados sin `Origen` declarado (invariante I-7) | — | **0** — cero excepciones; verificado como test de invariante en el pipeline | Desde el Sprint que introduce persistencia |

Estas son métricas de resultado de negocio. Las métricas técnicas (latencias, cadencias, retención) están en §17 P.10.

---

## §9 Lo que NO es esta solución (exclusiones)

Las seis primeras son exclusiones declaradas textualmente en §3.2 de la fuente, con su justificación original.

| # | Exclusión | Justificación (fuente) | ¿Cuándo podría incorporarse? |
|---|---|---|---|
| E-01 | **Apagado de otros equipos de la red** | *"Decisión del usuario: excede el alcance. Implica un protocolo de coordinación cuyo modo de falla es corrupción simultánea en varias máquinas."* | No previsto |
| E-02 | **Múltiples SAI simultáneos** | *"El modelo los contempla, la implementación no."* | Versión futura: el modelo de datos ya lo soporta (`MontajeBateria` lleva `posicion`; `CoberturaHost` es por dispositivo) |
| E-03 | **Notificaciones externas (mail, SMS) como mecanismo primario** | *"Alertas visuales en el panel por ahora. Y hay una razón de fondo: en un corte de energía la red también cae."* Verificado en E-4. | Como extra, no como primario, una vez que el histórico local esté consolidado |
| E-04 | **Escribir el traductor de protocolo del equipo actual** | *"Ya resuelto y verificado"* por `nutdrv_qx` con dialecto `Voltronic-QS-Hex 0.10`. Reescribirlo reintroduce riesgos que el driver ya conoce. | Nunca para este equipo |
| E-05 | **Gestión de usuarios y roles** | *"Un solo administrador."* | Si la solución se usara en un contexto multiusuario |
| E-06 | **Lectura del ajuste de BIOS por software** | *"Posible pero inútil"*: frágil por versión de firmware, peligroso al escribir (riesgo de NVRAM corrupta, recuperación por programador SPI, placa muerta), verifica lo que no importa y tiene coste desproporcionado. Se descarta con fundamento en §4.6. | Nunca; se sustituye por verificación por comportamiento |
| E-07 | **Implementación de la capa de add-ons de dialecto de protocolo** | *"Queda diseñada pero no implementada"* en la primera entrega; su interfaz *"no tiene sentido especificarla antes de tener el servicio"*. | Cuando aparezca un equipo que NUT no soporte, y solo sobre un SAI de banco con verdad de referencia instrumental |
| E-08 | **Afirmaciones cuantitativas de salud de batería**: resistencia interna absoluta, SoH en porcentaje, capacidad remanente en Ah, autonomía, y nada conforme a IEEE 1188 | El método adoptado es *"tendencia relativa en unidades arbitrarias"*. Los miliohmios no son recuperables con los datos disponibles; IEEE 1188 es de pago y no se pudo leer el texto normativo. | Solo comprando la norma y con instrumental adicional |
| E-09 | **Event store completo y CQRS** | *"No hace falta… Alcanza con que las tablas de historia sean append-only. Es una disciplina, no una tecnología."* | No previsto |

---

## §10 Restricciones del cliente

**Presupuesto y fecha objetivo: no hay ninguno de los dos, y la ausencia es deliberada.** Es un proyecto interno de un único desarrollador que es a la vez propietario, implementador y beneficiario; no hay presupuesto asignado ni cliente externo que imponga una fecha. El ritmo lo marcan las etapas de validación humana definidas en §15: cada etapa cierra con una verificación visual o funcional del administrador, y la siguiente no arranca hasta que esa validación pasa. Los únicos importes que aparecen en la fuente son costos de repuestos y equipos dentro de los escenarios de prueba, explícitamente marcados como ficticios o reconstruidos (180.000 ARS el `ups-01`, 240.000 ARS el `ups-02`, 52.000 ARS la batería, 15.000 ARS de mano de obra), y no constituyen presupuesto de proyecto.

**Restricciones legales y regulatorias.** Dos, ambas acotadas:

- **Normas de pago no adquiridas.** *"IEEE 1188 es de pago y no se pudo leer el texto normativo."* La consecuencia es normativa para el producto: *"Si alguna de estas cifras va a fundamentar una decisión de compra, hay que comprar la norma."* El sistema no puede presentar sus veredictos como conformes a IEEE 1188.
- **Trazabilidad ambiental de baterías.** La `disposicionFinal` de cada batería retirada (destino, receptor) se registra *"para trazabilidad ambiental"*. La fuente no cita ninguna normativa concreta que lo exija. PENDIENTE si hay una regulación local aplicable.

Sin requisitos de privacidad, GDPR, retención legal ni auditoría regulatoria: no aparecen en la fuente y el sistema no maneja datos personales más allá del contacto de un proveedor.

**Contexto macroeconómico como restricción de diseño.** *"En un país con la inflación de Argentina, un costo «normalizado» sin marcar es el mismo error que `battery.charge`."* Todo importe lleva moneda y fecha, y el equivalente en USD viaja marcado como `derivado`, con su fuente de cotización (`BNA-divisa-venta`).

**Integraciones obligatorias y restricciones técnicas duras impuestas por el equipamiento existente** (no negociables, verificadas en el relevamiento):

| Restricción | Valor | Estado |
|---|---|---|
| `ups.delay.shutdown` — techo duro del retardo de corte | **máximo 540 s (9 min)**; actual 30 s; rango 12–540 s | Verificado, no negociable. Invariante I-10 |
| `ups.delay.start` — retardo de reencendido | actual 180 s; rango 60–599940 s | Medido |
| Presupuesto consumido por la corrección del riesgo R-1 (grace de Docker) | **150 de los 540 s** — *"Las dos decisiones están acopladas: no se pueden tomar por separado"* | Verificado |
| NUT como capa de acceso al equipo | Obligatorio en la primera entrega | Decidido |
| BIOS del host: «Restore on AC Power Loss» | Debe estar en **Power On**, no en *Last State* | No verificable por software; se verifica por comportamiento |
| Placa del host | B75M-D3H (2012), AMI, BIOS F15 de 2013-10-23; sin soporte LVFS ni utilidad de configuración para Linux | Verificado |
| Plataforma | Exclusivamente Linux, dentro de un host Linux, dockerizado | Decidido |
| Alcance físico | Un solo SAI activo, el conectado al host | Decidido |
| Acceso | Por LAN, sin exposición a internet | Decidido |
| Entorno de desarrollo | Dev Container (spec containers.dev); único requisito del host es Docker, sin SDK .NET en el host | Impuesto por `Entorno-Desarrollo.md` |

---

## §11 Riesgos detectados desde el negocio

Los once primeros son la tabla S-1 a S-11 de §10 de la fuente, transcripta con su severidad original. Los tres últimos son riesgos de negocio que la fuente declara fuera de esa tabla.

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|---|
| R-01 (S-1) | El ciclo de apagado y reencendido **no está verificado** en este equipo; la trampa de firmware podría dejar el SAI apagado para siempre | Media | **Crítico** | Prueba física en ventana de mantenimiento (UF-8) antes de habilitar cualquier modalidad distinta de `SoloAlerta`; `ver-shutdown-return` sin caducidad |
| R-02 (S-2) | El presupuesto de 540 s **no está medido** contra el apagado real del host, y la corrección de R-1 consume 150 s de ese techo | Alta | **Alto** | Medir cronometrado en UF-8 antes de habilitar el apagado; `ver-presupuesto-apagado` con vigencia de 180 días |
| R-03 (S-3) | La BIOS debe tener *Restore on AC Power Loss* = Power On; sin verificar y **no legible por software** | Media | **Alto** | Verificar por comportamiento en UF-8; después se mantiene solo por evidencia acumulada (cruce de eventos propios contra `wtmp`) |
| R-04 (S-4) | El flag `LB` **no fue observado**: la política no debería depender de él | Alta | Medio | La política usa tiempo en `OB` + `battery.voltage`, nunca `LB` ni `battery.runtime` |
| R-05 (S-5) | Competencia por el nodo USB entre el contenedor, NUT en el host (O-U1) y otro contenedor (O-U2) | Media | Medio | Decidir en Sprint 0 dónde vive NUT; anclaje por ruta física de puerto con regla `udev` |
| R-06 (S-6) | Desapariciones del bus USB documentadas en este modelo (O-U11) | Media | Medio | Vigilancia de conectividad obligatoria: 3 sondeos fallidos ⇒ `DesconexionUsb` |
| R-07 (S-7) | Retención de métricas definida en el modelo pero **no probada** contra el volumen real (~6,3 millones de filas/año) | Media | Bajo | Validar la agregación y la retención antes de producción |
| R-08 (S-8) | Decisión abierta: ¿NUT dentro del contenedor o en el host? | Alta | Bajo | ADR en Sprint 0 |
| R-09 (S-9) | **Sin sensor de temperatura**: el confusor de la tendencia de salud no tiene solución con este equipo (O-M5) | Cierta | **Alto** para la salud de batería | Declarado; toda conclusión de salud lleva esa reserva explícita en su advertencia |
| R-10 (S-10) | El modelo de datos **no se implementó**: los invariantes son hipótesis de diseño hasta que existan como pruebas que corran | Cierta | Medio | Escribir los invariantes I-1 a I-21 como pruebas antes de codificar el dominio |
| R-11 (S-11) | La secuencia completa de sustitución de SAI (UF-7) **no tiene escenario de datos** que la ejercite | Cierta | Bajo | Agregar un escenario E-9 cuando se implemente el flujo |
| R-12 | **Riesgo de expectativa — el principal del proyecto según la fuente:** *"el servicio va a tomar la decisión de apagar un servidor sin backups. Si falla, falla de noche y sin testigos."* | Media | **Crítico** | Regla de bloqueo por verificación: el sistema arranca forzado en `SoloAlerta` y no apaga nada hasta poder probar que el host vuelve a encenderse |
| R-13 | **Riesgo de conclusión falsa (O-M18, severidad Alta):** guardar `battery.charge` sin marcar que es una interpolación del driver es *"el modo de falla más probable del sistema: no un error de código, sino una conclusión falsa sobre datos que parecían medidos"* | Alta | **Alto** | Procedencia obligatoria en todo valor (I-7); `aptoParaTendenciaDeSalud()` devuelve `false` para `derivado`, `estimadoPorDriver` e `imputado`; el panel marca en pantalla todo valor derivado o estimado |
| R-14 | **Riesgo de estar haciendo algo que nadie hace.** Ningún proyecto libre calcula salud de batería desde NUT; *"obliga a ser conservador con las conclusiones"* | Cierta | Medio | Confianza explícita en cada veredicto, arrancando en `baja`; lenguaje del veredicto limitado a *«se comporta peor que antes»* |

---

## §12 Glosario del dominio del cliente

| Término | Definición | Sinónimos / notas |
|---|---|---|
| **SAI** | Sistema de Alimentación Ininterrumpida: equipo que sostiene la alimentación del host cuando falla la red eléctrica. El relevado es de topología *offline / line interactive* | UPS (*Uninterruptible Power Supply*) |
| **Apagado ordenado** | Apagar el sistema operativo deteniendo contenedores y sincronizando discos **antes** de perder la alimentación, en vez de sufrir un corte abrupto | — |
| **Reencendido automático** | Que el host vuelva a arrancar solo al restablecerse la energía. Depende de que la BIOS detecte una **transición** de ausencia a presencia de energía, y por eso exige que el SAI corte su salida aunque el host ya esté apagado | Autoencendido; ajuste de BIOS «Restore on AC Power Loss» |
| **Ciclo forzado** | Modalidad en la que, una vez iniciada la secuencia de apagado, el corte del SAI **no se cancela aunque vuelva la red**. Existe para evitar que el host quede apagado indefinidamente | «Mandatory Power Cycle» en la terminología de CyberPower PowerPanel |
| **Microcorte** | Parpadeo breve de la red eléctrica. Regla vigente (v2): transición OL→OB seguida de OB→OL en **menos de 60 s** | La v1 usaba 30 s; mezclar ambas sin normalizar corrompe la serie |
| **Corte de suministro** | OL→OB→OL con duración **≥ 60 s** | — |
| **Flotación** | Estado de carga permanente en el que el cargador del SAI mantiene la batería. Medida en este equipo: **13,41 V** (2,235 V/celda). Informa sobre el cargador, no sobre la batería | *Float* |
| **Autonomía** | Cuánto tiempo puede sostener el SAI la carga con la batería. **No se mide en este equipo**: `battery.runtime` no existe y queda `noCalculable` con motivo. No puede usarse como umbral de disparo | *Runtime*, *backup time* |
| **Salud de batería** | Grado de degradación de la batería respecto de su estado nuevo. Acá **no** es un porcentaje ni un SoH: es una **tendencia relativa en unidades arbitrarias**, derivada de la caída de tensión durante el autotest a carga igualada | SoH (*State of Health*) — término evitado a propósito |
| **Línea base** | Primera medición de referencia contra la que se comparan las pruebas posteriores. La del equipo: caída −0,47 V, mínimo 12,94 V, recuperación a 13,24 V en ~35 s, con carga 13 % (2026-07-19) | *Baseline* |
| **Procedencia** | El origen declarado de cada valor almacenado: `medido`, `derivado`, `estimadoPorDriver`, `declarado`, `imputado` o `noCalculable`. Responde *«¿este número lo midió el aparato o lo calculó el software?»* | Origen; *provenance* |
| **Supuesto / Verificación** | Una afirmación de la que depende la política de apagado (por ejemplo, «la BIOS reenciende el host»), con su evidencia, su método, su fecha y su vigencia. Estados: `NuncaVerificado`, `Verificado`, `Vencido`, `Refutado` | *Refutado* ≠ *Vencido*: el primero bloquea, el segundo solo pide repetir |
| **Modalidad** | Qué hace el servicio cuando se cumple la condición de disparo: `SoloAlerta` (solo avisa), `SoloHost` (apaga el host), `HostLuegoUpsConRetorno` (apaga el host y luego el SAI, que vuelve al restablecerse la energía), `CicloForzado` (idem sin cancelación) | Se distingue `modalidadSolicitada` de `modalidadEfectiva` |
| **Baja lógica** | Marcar una unidad como retirada (`estado` + `fechaBaja` + `motivoBaja`) sin borrarla. Sigue consultable con todo su historial, pero no operable. **El borrado físico no existe en este dominio** | *Soft delete* |
| **Vínculo temporal** | La relación «qué estuvo con qué, cuándo», modelada como entidad con intervalo (`MontajeBateria`, `CoberturaHost`) en vez de como atributo. Es lo que permite representar que una batería se retiró, se probó y se reinstaló | Patrón de *tenencia* o *asignación* |
| **Bitemporalidad** | Guardar dos tiempos por hecho: `tiempoValido` (cuándo ocurrió) y `tiempoRegistrado` (cuándo lo supo el sistema). Cargar una intervención tres días después con la factura en la mano es el caso corriente | — |
| **Idempotencia de ingesta** | Que reenviar el mismo hecho no lo duplique, gracias a una clave provista por el emisor. El reintento es el caso normal, no el excepcional | `X-Idempotency-Key` |
| **Agregado** | Resumen de una ventana de tiempo (`PT1H`, `P1D`) con su función, cantidad de muestras y **cobertura**. Nunca se sirve por el mismo canal que una muestra sin decir que lo es: el promedio horario borra los microcortes | — |
| **Ficha de vida útil** | Proyección que cierra la historia de una batería: cuánto duró de verdad, cuántos eventos soportó, si cumplió la expectativa del catálogo y cuánto costó por año de servicio | — |
| **NUT** | *Network UPS Tools*: el ecosistema de software libre que ya dialoga con el equipo. Componentes usados: `upsmon`, `upssched`, `upsc`, `upsd`, `nutdrv_qx` | — |
| **Dialecto de protocolo** | La variante concreta del protocolo que habla un equipo. El de este SAI es `Voltronic-QS-Hex 0.10`, de la familia Megatec/Qx, sobre un puente serie-sobre-HID. **El firmware manda**: dos equipos de la misma marca y modelo pueden hablar dialectos distintos | Subdriver |
| **AGM / VRLA** | Tecnología de batería de plomo-ácido con electrolito absorbido y regulada por válvula. Es la del equipo. El enum cerrado del modelo es `AGM`, `GEL`, `Inundada` | — |
| **Capacidad C20** | Capacidad de la batería medida al régimen de descarga de 20 horas. Es la magnitud comparable entre modelos | — |
| **Días sin protección** | Los días en que el host no tuvo ningún SAI cubriéndolo, medidos como el hueco entre dos `CoberturaHost`. Es lo que reporta el informe de período como `diasSinProteccion` | — |

---

# Parte B — Composición de la solución

## §13 Proyectos de la solución

La solución es **de un solo proyecto** (caso degenerado): un servicio monolítico donde front, API REST y backend corren en el mismo proceso, según lo define el tool-prompt (*"Arquitectura del servicio: monolítica (Front, API Rest, etc todos corriendo en un mismo servicio)"*). Los cinco assemblies de `Topologia-Proyecto-Solucion.md` (`Domain`, `Application`, `Infrastructure`, `Api`, `Web`) son **capas internas de ese único proyecto**, no proyectos de la solución en el sentido D8: el propio input aclara que *"Domain, Application, Infrastructure, Api y Web son un solo proyecto"*. Su descomposición se documenta en §16 (estructura de repositorio) y en §17 P.2 (estilo arquitectónico interno).

Tabla de proyectos (fuente del manifiesto derivado):

| `Nombre-Proyecto` | `project_type` (D8) | Rol en la solución | Dependencias | `redistribuible` |
|---|---|---|---|---|
| Sai-Service-Core (principal) | `web-monolith` | Servicio web único que monitorea el SAI, decide y ejecuta el apagado ordenado, administra el ciclo de vida del parque y expone panel y API REST | (ninguna) | false |

Proyecto principal: **Sai-Service-Core**. Grafo de dependencias: trivialmente acíclico (un nodo, sin aristas). Sin colisión de nombres.

Perfil de convención de nombres de código:

| Parámetro | Valor | Notas |
|---|---|---|
| Forma del nombre de solución en código | PascalCase con segmentos separados por punto | `SAI.Service.Core` — el nombre legible ya es dotted PascalCase y se conserva literal, alineado con la referencia `MiEmpresa.Reservas.*` de `Topologia-Proyecto-Solucion.md` |
| Separador de segmentos | `.` | Separa la raíz de la solución del sufijo de capa |
| Prefijo de paquetes redistribuibles | `Aplicada` | No se usa: no hay redistribuibles en esta solución |

Nombre de código del proyecto: **`SAI.Service.Core`**, materializado como cinco assemblies `SAI.Service.Core.<Capa>` bajo `src/`.

---

## §14 Estilo arquitectónico de la solución

Con un único proyecto no hay contratos inter-proyecto que declarar: la solución **no expone ningún contrato hacia otros proyectos de la misma jerarquía**, porque no hay otros. Lo que sí expone hacia afuera de la solución son tres superficies, todas servidas por el mismo proceso:

| Superficie | Consumidor | Contrato |
|---|---|---|
| Panel web Blazor (interactive server) | El administrador único, desde la LAN | Interfaz de usuario; sin contrato versionado |
| API REST `/api/v1/` | Sistemas externos de gestión de mantenimiento (GMAO) | HTTP + JSON, versionada en la ruta, con idempotencia por cabecera. Es el contrato que UF-10 ejercita |
| Adaptador de conexión → NUT | El equipo SAI, vía `upsd` / `upsc` | Contrato **saliente**: el servicio es cliente de NUT, no al revés |

**Por qué esta descomposición y no otra.** Se evaluaron dos alternativas y se descartaron:

- **Microservicios (panel, poller, API y planificador como servicios separados).** Descartado: el sistema tiene un único usuario, un único SAI y corre en un único host sin backups. La complejidad operativa de coordinar varios procesos no compra nada, e introduce un modo de falla nuevo justo en el camino crítico —el apagado— que es el que no puede fallar. El propio tool-prompt fija la arquitectura monolítica.
- **Solución multi-proyecto con las cinco capas como proyectos D8 tipados.** Descartado: obligaría a declarar contratos versionados y políticas de *breaking changes* entre lo que en realidad son capas de un mismo binario que se despliegan siempre juntas, y multiplicaría la documentación por cinco sin agregar información. La descomposición en capas es real y se documenta, pero como estructura interna (§17 P.2), no como jerarquía de proyectos.

**Punto de entrada para el consumidor final:** el assembly `SAI.Service.Core.Web`, que arranca el proceso y hostea tanto el panel Blazor como los endpoints de la API. La dirección de dependencias interna es unidireccional hacia adentro: `Web → Api → Infrastructure → Application → Domain`; el dominio no depende de nada.

---

## §15 Esquema de descomposición y delivery

**Estrategia: walking skeleton seguido de vertical slicing por flujo de usuario.** La descomposición es **vertical** —cada etapa a partir de la 3 entrega una rebanada funcional que atraviesa las cinco capas hasta una pantalla usable— y no horizontal por capas. El primer sprint entrega valor demostrable end-to-end: una solución que compila, corre con un script y se abre en el navegador.

**Criterio transversal, impuesto por el tool-prompt:** cada etapa cierra con un **punto de validación del agente humano** con entregable tangible. El orquestador de codificación no arranca la etapa siguiente hasta que esa validación pase.

| Etapa | Contenido | Validación de cierre |
|---|---|---|
| **1 — Scaffolding** | Estructura de la solución con los cinco proyectos, `.devcontainer/devcontainer.json`, `.vscode/launch.json` y `tasks.json`, y el set único de scripts `build.sh <proyecto>` / `run.sh <proyecto>` más `build-all.sh` / `run-all.sh` | La solución **compila y corre** mediante los scripts. El administrador valida visualmente la estructura de la solución |
| **2 — Front** | Layout del panel: menú lateral y barra superior, con MudBlazor | El servicio compila y se lanza. El administrador **valida en el navegador** que el panel de control cumple con la maqueta aprobada en la especificación UX-UI (Fase B2) |
| **3 — Persistencia y alta de administrador** | Integración de SQLite y EF Core; entidades de autenticación y autorización; primera interfaz que pide usuario y contraseña para dar de alta el administrador, con redirección a la página principal | Idem etapa 2: validación visual en el navegador contra la maqueta |
| **4 — Sesión** | Login, cambio de contraseña, y las acciones de la barra superior del administrador: cerrar sesión y cambiar contraseña | Idem etapa 2 |
| **5 en adelante — Un flujo de usuario por etapa** | Una etapa por cada flujo de §6, en el orden topológico de su grafo de dependencias: **UF-1** (alta del parque) → **UF-2** (políticas) → **UF-3** (monitoreo en vivo) → **UF-5** (prueba de batería) → **UF-4** (históricos y gráficas) → **UF-8** (ventana de mantenimiento) → **UF-6** (recambio de batería) → **UF-7** (reparación/sustitución del SAI) → **UF-10** (ingesta automatizada) → **UF-9** (informe de período y comparación de marcas) | En cada etapa se implementa todo lo necesario para el flujo y se **verifica en el navegador que las pantallas funcionan** |

El orden de las etapas 5 en adelante respeta el grafo de dependencias de los flujos declarado en §6: ningún flujo se construye antes que aquellos de los que depende. UF-9 va último porque consume las salidas de UF-4, UF-6, UF-7 y UF-10.

**Nota sobre la etapa de UF-8.** Es el único flujo que no se puede validar solo con software: exige presencia física y pruebas destructivas sobre el equipo. Su etapa entrega la interfaz guiada y el registro de evidencias; la ejecución real de la ventana de mantenimiento es una actividad operativa posterior, y hasta que ocurra el sistema permanece forzado en `SoloAlerta`. El adaptador simulado (F-24) es lo que permite cubrir ese camino en pruebas automatizadas.

---

## §16 Estructura de repositorio de la solución

```text
SAI.Service.Core/
├── .devcontainer/
│   └── devcontainer.json               # orquestación declarativa del entorno de desarrollo
├── .vscode/
│   ├── launch.json                     # depuración coreclr (F5)
│   └── tasks.json
├── scripts/
│   ├── build.sh                        # build.sh <proyecto> — agnóstico al entorno, asume dotnet en el PATH
│   ├── run.sh                          # run.sh <proyecto>
│   ├── build-all.sh
│   ├── run-all.sh
│   └── dev.sh                          # (opcional) devcontainer up && devcontainer exec ./scripts/build-all.sh
├── src/
│   └── SAI.Service.Core/               # el único proyecto de §13, en cinco assemblies
│       ├── SAI.Service.Core.Domain/           # entidades, objetos de valor, invariantes, ResolutorTemporal
│       ├── SAI.Service.Core.Application/      # casos de uso, planificador, contrato del adaptador
│       ├── SAI.Service.Core.Infrastructure/   # EF Core + SQLite, adaptador NUT, adaptador simulado
│       ├── SAI.Service.Core.Api/              # endpoints REST /api/v1/  → Infrastructure
│       └── SAI.Service.Core.Web/              # panel Blazor + host del proceso → Api
├── tests/
│   ├── SAI.Service.Core.Domain.Tests/         # invariantes I-1 a I-21 como pruebas
│   ├── SAI.Service.Core.Application.Tests/
│   └── SAI.Service.Core.Integration.Tests/    # contra el adaptador simulado
├── samples/
│   └── ingesta-gmao/                   # cliente HTTP de referencia de la API de ingesta
├── SDD/
│   ├── Intake/                         # este documento y el SOLUTION-MANIFEST derivado
│   ├── Docs/                           # categorías 00 a 11 (layout aplanado: un solo proyecto)
│   └── Maquetas/                       # maqueta navegable de validación visual (Fase B2)
└── SAI.Service.Core.sln
```

Cada assembly de `/src` lleva el nombre de código `SAI.Service.Core.<Capa>` según el perfil de §13. No hay redistribuibles, así que ninguno usa el prefijo `Aplicada`. La estructura sigue la convención del ecosistema .NET (`src/`, `tests/`, `samples/`, solución en la raíz) y replica la referencia de `Topologia-Proyecto-Solucion.md`.

`SDD/Docs/` usa el **layout aplanado** del caso degenerado: las doce categorías `00-Contexto/` a `11-Examples/` cuelgan directamente de `SDD/Docs/`, sin el subnivel `Proyectos/<Nombre>/` ni la carpeta `Solucion/`.

### §16.1 Materialización de `/samples`

El proyecto es `web-monolith`, tipo para el que la tabla de adaptabilidad no exige samples. Se produce igualmente **un sample**, justificado por la única superficie de la solución consumida por terceros:

| Sample | Qué ilustra | Complejidad | Vínculo con `/src` |
|---|---|---|---|
| `ingesta-gmao/` | Cliente HTTP de referencia de `POST /api/v1/intervenciones`: ejercita los cuatro caminos del contrato (201 creado, 200 idempotente, 409 conflicto de idempotencia, 422 invariante roto) | Media | Consume `SAI.Service.Core.Api` sin referenciar código de la solución; se ejecuta contra una instancia levantada con `run.sh` |

El panel Blazor no produce sample: se demuestra ejecutando el propio servicio.

---

# Parte C — Técnica por proyecto

## §17 Bloque técnico — Sai-Service-Core

| Campo | Valor |
|---|---|
| `Nombre-Proyecto` | Sai-Service-Core |
| `nombre-proyecto-codigo` | `SAI.Service.Core` |
| `project_type` (D8) | `web-monolith` |
| Rol | Servicio web único que monitorea el SAI, decide y ejecuta el apagado ordenado, administra el ciclo de vida del parque y expone panel y API REST |
| `redistribuible` | false |

### §17.P.1 Stack tecnológico

| Componente | Elección | Justificación |
|---|---|---|
| Lenguaje y runtime | **C# sobre .NET 10** (versión mínima: .NET 10.0; sin *fallback* a versiones anteriores) | Impuesto por el tool-prompt §4.1 |
| Framework de UI | **Blazor con render mode *interactive server*** | Impuesto por el tool-prompt §4.1 y confirmado por la fuente (§3.3: *"Servicio .NET con Blazor (interactive server)"*). El modo server evita descargar el runtime al cliente y permite empujar el estado en vivo del SAI al panel sin polling desde el navegador |
| Librería de componentes | **MudBlazor** | Impuesto por el tool-prompt §4.2 |
| Acceso a datos | **Entity Framework Core** con proveedor SQLite | Impuesto por el tool-prompt §4.1 y §4.3 |
| Motor de base de datos | **SQLite** | Impuesto por el tool-prompt §4.3 y confirmado por la fuente (§3.3) |
| Acceso al SAI | **NUT** (`nutdrv_qx` + `upsd`), consumido por el adaptador de conexión como cliente de `upsd` o invocando `upsc` | Decidido en la fuente §5.2: *"Usar NUT en lugar de construir la trama a mano. El driver ya conoce esta clase de trampas"* |
| Empaquetado | **Contenedor Docker sobre Linux** | Fuente §3.1, §3.3 |
| Entorno de desarrollo | **Dev Container** (spec containers.dev); el SDK vive dentro del contenedor, el único requisito del host es Docker | Impuesto por `Entorno-Desarrollo.md` |

Dependencias core sin las que no compila: el SDK de .NET 10, `Microsoft.EntityFrameworkCore.Sqlite`, `MudBlazor`. Dependencia de runtime sin la que el servicio no cumple su función: NUT accesible (dentro del contenedor o en el host, según el ADR de R-08).

### §17.P.2 Estilo arquitectónico del proyecto

**Estilo elegido: Clean Architecture en cuatro capas más el host**, con la dirección de dependencias apuntando siempre hacia el dominio: `Web → Api → Infrastructure → Application → Domain`. El dominio no depende de frameworks: ni de EF Core, ni de Blazor, ni de NUT. Es la estructura que fija `Topologia-Proyecto-Solucion.md`.

Dentro de ese esqueleto hay tres decisiones estructurales propias del dominio:

- **Adaptador de conexión** (puerto en `Application`, implementaciones en `Infrastructure`): aísla al dominio del *cómo* se habla con el equipo. Tres implementaciones previstas — NUT (primera entrega), directo con add-on de dialecto (diseñada, no implementada) y **simulada** (permite probar políticas sin hardware ni riesgo, y es lo que hace testeable el flujo de apagado). Contrato mínimo: leer estado, probar conectividad, ordenar apagado con retorno, lanzar test de batería.
- **Planificador interno** (`Application`) como *hosted service*: rondas de sondeo en el intervalo configurado, evaluación de políticas, temporizadores con cancelación (una condición debe sostenerse N segundos antes de disparar, para no actuar ante microcortes), detección de pérdida de comunicación, y elevación de la cadencia de sondeo mientras dura una prueba de batería.
- **Historia append-only**: las tablas de hechos no se actualizan ni se borran. *"No hace falta un event store completo ni CQRS. Alcanza con que las tablas de historia sean append-only… Es una disciplina, no una tecnología."*

**Alternativas descartadas:**

1. **Arquitectura en capas tradicional con acceso a datos desde la UI.** Descartada: acopla la lógica de decisión del apagado —lo único con consecuencias irreversibles— al ORM y al framework web, y encarece hasta lo impracticable las pruebas de los invariantes I-1 a I-21, que la fuente exige escribir como pruebas antes de codificar (R-10).
2. **Arquitectura orientada a eventos con *event store* y CQRS.** Descartada explícitamente por la fuente: el volumen y la concurrencia (un usuario, un dispositivo) no lo justifican, y la garantía que se busca —inmutabilidad de los hechos— se obtiene con disciplina de escritura, no con infraestructura.

Coherente con `web-monolith` y con §14: un solo proceso, un solo despliegue.

### §17.P.3 Comunicación e integración

| Dirección | Protocolo | Formato | Versionado | Política de *breaking changes* |
|---|---|---|---|---|
| **Entrante — API REST** | HTTP/1.1 sobre LAN | JSON (UTF-8) | Versión en la ruta: `/api/v1/` | Un cambio incompatible abre `/api/v2/`; `v1` se mantiene mientras haya un consumidor declarado. Los cambios aditivos (campos opcionales nuevos) no rompen versión |
| **Entrante — Panel** | HTTP + WebSocket (Blazor Server circuit) | — | No versionado (se despliega junto al servidor) | No aplica |
| **Saliente — SAI** | Cliente de `upsd` (TCP local) o invocación de `upsc`; NUT habla Megatec/Qx `Voltronic-QS-Hex 0.10` sobre USB con el equipo | Variables NUT clave-valor | El `driver` y su `driverVersion` se registran en cada `SesionSondeo`, junto con el `dialecto` y el mapa variable→origen | Un cambio de versión de driver abre una `SesionSondeo` nueva; las muestras viejas conservan la procedencia con la que fueron tomadas |
| **Saliente — Host** | Invocación de `shutdown` del sistema operativo | — | — | Toda acción se valida **por efecto observado**, nunca por ausencia de error |

**Contrato de la API de ingesta** (el único contrato formal hacia terceros):

- `POST /api/v1/intervenciones`, cabeceras obligatorias `X-Idempotency-Key` y `X-Fuente-Datos`.
- `201` — cuerpo válido y clave nueva: devuelve el id y confianza `media` (la del emisor externo, menor que la del poller local).
- `200` con `creado=false` — clave ya procesada con el mismo cuerpo: devuelve el mismo id. El reintento es el caso normal.
- `409 conflicto_idempotencia` — clave ya procesada con cuerpo distinto: devuelve las huellas `sha256` original y recibida más una `accionSugerida`. Nunca `200`.
- `422` — invariante roto: devuelve el campo y el invariante violado (`validacion` para `Costos.cuadra()` o `Dinero` sin moneda/fecha; `coherencia_temporal` para una intervención fechada después de la baja de la entidad).

El **endpoint de rectificación** que sugiere la respuesta 409 se menciona en la fuente pero su contrato no está definido: **PENDIENTE**, a cerrar en la especificación funcional.

### §17.P.4 Persistencia

**Motor: SQLite**, impuesto por el tool-prompt y confirmado por la fuente. Es adecuado porque hay un único proceso escritor, un único usuario y el despliegue es un contenedor sobre un host: no hay concurrencia de escritura que justifique un motor cliente-servidor, y un archivo único simplifica el respaldo del propio servicio.

**Versionado del esquema:** migraciones de EF Core versionadas en el repositorio, aplicadas al arranque del servicio. Cada migración es un archivo versionado y revisable; no hay generación automática de esquema en producción.

**Qué se guarda**, en las cuatro capas del modelo:

- **Catálogo** (qué es): `Fabricante`, `ModeloDispositivo`, `ModeloBateria`, `TipoIntervencion`, `Proveedor`.
- **Inventario** (cuál es, con baja lógica): `Host`, `Dispositivo`, `Bateria` — todas derivadas de `UnidadFisica` con `estado` (`EnStock`, `EnServicio`, `EnReparacion`, `DadoDeBaja`), `fechaBaja` y `motivoBaja`.
- **Vínculos temporales**: `MontajeBateria` (batería↔dispositivo, con `posicion`) y `CoberturaHost` (dispositivo↔host), ambos con intervalo `desde`/`hasta`.
- **Historia** (append-only): `Muestra`, `Agregado`, `Evento`, `PruebaBateria`, `Intervencion`, `Accion`, `Verificacion`, `SesionSondeo`, `ReglaDerivacion`, `Politica`/`VersionPolitica`, `FuenteDatos`.

**Patrón de acceso decisivo:** *"la historia no guarda a qué batería pertenece una métrica. Guarda el dispositivo y el instante. La batería se resuelve consultando `MontajeBateria`"* a través del `ResolutorTemporal`. Es lo que permite que corregir la fecha de un recambio reatribuya todo el histórico afectado sin migrar datos.

**Optimización de almacenamiento prevista:** la procedencia (`o`) y la lista de derivación (`de`) se declaran **una vez por `SesionSondeo`**, no por muestra, para no duplicar constantes 17.280 veces por día; se expanden solo en la API por legibilidad.

**Retención y agregación** (dimensionada sobre ~6,3 millones de filas/año a 5 s de intervalo):

| Dato | Resolución completa | Después | Retención total |
|---|---|---|---|
| `Muestra` | `P30D` | se agrega a `PT1H` y se descarta | 30 días |
| `Agregado` | `PT1H` | — | `P10Y` |
| `Evento` | — | — | indefinido |

Para `input.voltage` los agregados conservan **mínimo y máximo además del promedio**, porque el promedio horario oculta exactamente los microcortes que el sistema quiere estudiar. Todo agregado lleva su `cobertura` y su advertencia (I-20), y nunca se sirve por el mismo canal que una muestra sin declarar que lo es.

**Multi-tenant: no aplica.** Un solo administrador, un solo host, un solo SAI activo.

### §17.P.5 Seguridad y autenticación

**Autenticación: mínima, de administrador único.** El sistema, al iniciar por primera vez, pide nombre de usuario y contraseña para dar de alta al administrador (etapa 3 de §15); a partir de ahí exige login. La fuente no especifica más que *"Autenticación mínima"*, así que se fija lo mínimo defendible: ASP.NET Core Identity con hash de contraseña (el algoritmo por defecto de Identity, PBKDF2), cookie de sesión, y las acciones de cerrar sesión y cambiar contraseña disponibles desde la barra superior (etapa 4).

**Autorización:** un solo rol, `administrador`. No hay gestión de usuarios ni de roles (exclusión E-05). Todo endpoint y toda página del panel requieren autenticación, salvo la pantalla de alta inicial —accesible únicamente mientras no exista ningún administrador— y el endpoint de salud.

**Superficie de exposición:** LAN, sin exposición a internet. TLS: **PENDIENTE** — la fuente no lo trata. Recomendación a cerrar como ADR en Sprint 0: TLS con certificado autofirmado o terminación en un *reverse proxy* del host.

**Secretos en runtime:** la cadena de conexión a SQLite y las credenciales de `upsd` (si NUT corre en el host) se inyectan por variables de entorno del contenedor, nunca en el repositorio. La configuración de desarrollo usa el *user-secrets* del SDK dentro del Dev Container.

**Secretos en CI/CD:** no hay publicación a registros externos ni despliegue automatizado en el alcance, así que no hay credenciales de publicación que gestionar.

**Compliance:** no aplica ninguno. El sistema no maneja datos personales más allá del contacto de un proveedor, y no hay requisitos de privacidad, GDPR ni retención legal en las fuentes.

**Seguridad operativa** — es la que la fuente sí desarrolla en profundidad, y es un requisito de seguridad de primer orden aunque no sea de seguridad informática:

- **Regla de bloqueo:** la política de apagado no se habilita si algún supuesto del que depende está en `NuncaVerificado`, `Vencido` o `Refutado`. La `Accion` queda `BloqueadaPorVerificacion` con su motivo, y la `modalidadEfectiva` degrada a `SoloAlerta`.
- **Arranque seguro por defecto:** el sistema arranca forzado en `SoloAlerta`. No es una recomendación de puesta en marcha: es un estado impuesto por el sistema hasta que las verificaciones se cumplan.
- **Validación por efecto observado:** ninguna acción sobre el equipo se da por ejecutada porque no haya habido excepción.

### §17.P.6 Estrategia de testing

**Pirámide: 70 % unitarias / 25 % integración / 5 % end-to-end.** La base es más ancha de lo habitual porque el núcleo de valor del sistema es lógica de dominio pura (resolución temporal, invariantes, cálculo de derivados y veredictos) que se prueba sin infraestructura.

| Nivel | Alcance | Framework |
|---|---|---|
| Unitarias | `Domain` y `Application`: los 21 invariantes I-1 a I-21 escritos como pruebas, el `ResolutorTemporal`, `Costos.cuadra()`, `Intervencion.aplicar()` y sus `Efectos`, la máquina de estados de `UnidadFisica`, el cálculo de derivados de prueba de batería y la lógica de degradación de modalidad | xUnit + FluentAssertions |
| Integración | `Infrastructure`: EF Core contra SQLite en archivo temporal (migraciones incluidas), adaptador NUT contra el **adaptador simulado**, y la API de ingesta con sus cuatro caminos (201/200/409/422) | xUnit + `WebApplicationFactory` + SQLite físico |
| End-to-end | Los flujos de usuario críticos sobre el panel, contra el adaptador simulado: UF-1 (alta del parque), UF-3 (monitoreo en vivo) y el camino de apagado completo de UF-8 | bUnit para componentes Blazor; Playwright para los recorridos de panel |

**Cobertura mínima, bloqueante en el pipeline: 80 % de líneas y 70 % de ramas** sobre el conjunto de la solución, con un umbral elevado y separado para `SAI.Service.Core.Domain`: **90 % de líneas y 85 % de ramas**. La asimetría es deliberada: el dominio es donde viven las decisiones irreversibles.

**BDD/ATDD:** los criterios de aceptación de cada caso de uso se escriben en Given/When/Then y se materializan como pruebas de integración. No se adopta un framework de BDD con *gherkin* separado: con un solo desarrollador, el costo de mantener los archivos de features supera el beneficio.

**Prueba imposible de automatizar, declarada:** el flujo F-3 (ciclo completo de apagado y reencendido físico) no se puede probar solo con software. El adaptador simulado cubre la lógica de decisión; la verificación del comportamiento real del firmware y de la BIOS es la ventana de mantenimiento de UF-8, y su resultado se registra como evidencia de una `Verificacion`, no como un test.

**Tests de contrato hacia otros proyectos:** no aplica, no hay otros proyectos.

### §17.P.7 Estrategia de versionado y release

- **SemVer 2.0.0 sin excepciones.** `MAJOR` para cambios incompatibles del contrato de la API `/api/v1/` o del esquema de datos que requiera intervención manual; `MINOR` para capacidades nuevas retrocompatibles (típicamente, cada etapa de §15 cerrada); `PATCH` para correcciones.
- **Conventional Commits sin excepciones**, con los tipos `feat`, `fix`, `docs`, `refactor`, `test`, `chore`, `build`, `ci`, `perf`. `BREAKING CHANGE` en el pie o `!` tras el tipo eleva `MAJOR`.
- **Herramienta de cálculo de versión: MinVer**, que deriva la versión del tag de git más cercano. Se elige por ser la de menor fricción en .NET (un paquete, sin archivo de configuración) y no requerir un paso de escritura en el repositorio durante el build.
- **Branching: trunk-based.** Rama `main` siempre desplegable; ramas de trabajo cortas por etapa (`etapa/NN-<slug>`), integradas por merge. Con un solo desarrollador, GitFlow es sobrecarga pura.
- **Canales:** único canal, la imagen de contenedor etiquetada con la versión SemVer y con `latest`. Las versiones de preestreno usan el sufijo `-alpha.N` que MinVer genera desde la distancia al tag.
- **Feed:** no se publica a ningún registro público. La imagen se construye localmente y, si en el futuro se publicara, sería a un registro privado. No hay paquetes NuGet redistribuibles (`redistribuible: false`).

### §17.P.8 Pipeline CI/CD

Plataforma: **GitHub Actions**. Se elige por ser la que no requiere infraestructura propia sobre un repositorio git y por tener soporte nativo de contenedores Linux, que es el único target.

| # | Stage | Qué hace | Quality gate (bloqueante para mergear) |
|---|---|---|---|
| 1 | Build | `dotnet build` en configuración Release sobre .NET 10 | Cero errores **y cero warnings** (`TreatWarningsAsErrors`) |
| 2 | Lint / formato | `dotnet format --verify-no-changes` y analizadores de .NET en nivel `recommended` | Sin diferencias de formato; cero diagnósticos de severidad *error* |
| 3 | Test unitario | xUnit sobre `Domain` y `Application` | 100 % de pruebas en verde. **Cobertura ≥ 90 % líneas / 85 % ramas en `Domain`** |
| 4 | Test de integración | EF Core contra SQLite físico + API + adaptador simulado | 100 % en verde. **Cobertura global ≥ 80 % líneas / 70 % ramas** (coincide con P.6) |
| 5 | Test end-to-end | bUnit + Playwright sobre los flujos críticos | 100 % en verde |
| 6 | SCA | `dotnet list package --vulnerable --include-transitive` | Cero vulnerabilidades de severidad **alta o crítica** |
| 7 | SBOM | Generación de SBOM en formato CycloneDX, adjunto como artefacto del build | El artefacto existe y es válido |
| 8 | Build de imagen | `docker build` de la imagen de producción (runtime-only, distinta de la del Dev Container) | La imagen construye y su *smoke test* arranca el servicio y responde el endpoint de salud |
| 9 | Firma | Firma de la imagen con `cosign` en modo *keyless* | Solo en builds de tag; la firma se verifica antes de publicar |
| 10 | Publicación | Push de la imagen etiquetada a registro privado | Solo desde `main` con tag SemVer, y solo si los stages 1 a 9 pasaron |

**Matriz:** un solo eje, porque el target es único — `ubuntu-latest` con .NET 10, contenedor Linux `linux/amd64`. No se prueba en Windows ni en macOS: el proyecto es exclusivamente Linux por decisión de alcance.

**Ambientes:** dos. *Desarrollo* es el Dev Container en la máquina del desarrollador, levantado con «Reopen in Container» o `devcontainer up`; la depuración va por `.vscode/launch.json` con F5, nunca por los scripts. *Producción* es el contenedor corriendo en el host `i7infra`, con el dispositivo USB compartido por ruta física de puerto vía regla `udev`. No hay ambiente de *staging*: no habría a qué SAI conectarlo.

**Rollback:** volver a la etiqueta de imagen anterior y reiniciar el contenedor. Las migraciones de EF Core se diseñan aditivas siempre que sea posible; una migración destructiva exige respaldo previo del archivo SQLite, que es una copia de un único archivo. El estado crítico —la historia— es append-only, así que un rollback de versión no pierde hechos registrados.

### §17.P.9 Compatibilidad y plataformas target

| Dimensión | Soportado | Versión mínima |
|---|---|---|
| Sistema operativo del host | Linux x86-64 | Kernel 6.1 (el host relevado corre 6.12.95+deb13-amd64) |
| Runtime | .NET | 10.0 |
| Contenedores | Docker Engine | 24.0 |
| Arquitectura de CPU | `linux/amd64` | — |
| Navegadores del panel | Chromium / Google Chrome; Mozilla Firefox | Chromium 120; Firefox 121 — mínimos que soportan de forma estable el transporte WebSocket que usa Blazor Server |
| Acceso al SAI | NUT | 2.8.0 (la versión relevada en el equipo es 2.8.1) |
| Equipo SAI | Cualquiera soportado por `nutdrv_qx`; verificado sobre `0665:5161`, dialecto `Voltronic-QS-Hex 0.10` | — |

**Toda combinación no listada se considera no soportada.** En particular y de forma explícita: no se soporta Windows ni macOS como host de producción, ni `linux/arm64`, ni Safari, ni el acceso al panel desde fuera de la LAN, ni equipos SAI que `nutdrv_qx` no cubra (para eso está la capa de add-ons, diseñada pero no implementada — E-07).

### §17.P.10 Requerimientos no funcionales (NFR)

**Tiempos del camino crítico** (los únicos con consecuencias irreversibles):

| NFR | Valor | Estado |
|---|---|---|
| Retardo de corte del SAI (`ups.delay.shutdown`) | **≤ 540 s**, techo duro del equipo; el formulario de políticas rechaza `tiempoReservadoApagadoSeg > 540` | Verificado (I-10) |
| Presupuesto consumido por el `shutdown-timeout` del demonio Docker | 150 s de los 540 (corrección de R-1; el grace actual de 15 s es insuficiente: el contenedor con VM huésped necesita ~120 s) | Medido |
| Resto del apagado del sistema operativo | **Sin medir** — se cronometra en UF-8 y es lo que verifica `ver-presupuesto-apagado` | PENDIENTE de medición |
| Retardo de reencendido (`ups.delay.start`) | 180 s (rango 60–599940 s) | Medido |
| Umbral de disparo del apagado (`umbralDisparoSegundos`) | 300 s como punto de partida, ajustable por versión de política | Propuesto en la fuente |
| Latencia de decisión del planificador | La evaluación de políticas de una ronda debe completarse en **< 1 s**, para no desplazar la ronda siguiente con un intervalo de 5 s | **[derivado]** del intervalo de sondeo |

**Frecuencia de muestreo y precisión temporal:**

| NFR | Valor |
|---|---|
| Intervalo de sondeo normal | **5 s** (configurable) |
| Cadencia durante una prueba de batería | **1 Hz**, restaurada al valor normal al terminar |
| Incertidumbre de duración de eventos cortos | Estructural: un evento derivado de una sola muestra lleva `incertidumbreDuracionSeg` igual al intervalo de sondeo. Todo informe que sume duraciones **debe propagar la incertidumbre** |
| Detección de microcortes | Un microcorte más corto que el intervalo de sondeo no es detectable de forma confiable; el sistema lo declara en vez de ocultarlo |

**Volumen, escalabilidad y retención:**

| NFR | Valor |
|---|---|
| Volumen de escritura | 720 muestras/hora ⇒ **~6,3 millones de filas/año** |
| Retención de muestras a resolución completa | `P30D` |
| Retención de agregados horarios | `P10Y` |
| Retención de eventos | Indefinida |
| Tamaño máximo del archivo SQLite tras la agregación | **PENDIENTE** — no está dimensionado en la fuente; se valida antes de producción (riesgo R-07) |
| Concurrencia | 1 usuario, 1 dispositivo, 1 proceso escritor. No hay requisito de escalado horizontal |

**Disponibilidad y observabilidad:**

| NFR | Valor |
|---|---|
| Detección de pérdida de comunicación con el SAI | **3 sondeos consecutivos sin respuesta** ⇒ evento `DesconexionUsb` + alerta visual en el panel |
| Rango aceptable de `input.voltage` | **[198, 242] V**; sostenido fuera durante 30 s ⇒ evento `TensionFueraDeRango` |
| Umbral de microcorte | **< 60 s** entre OL→OB y OB→OL (regla v2; la v1 usaba 30 s) |
| Alarma dura de batería | `battery.voltage` **< 13,3 V** en flotación ⇒ potencial celda en corto; **> 14,5 V** ⇒ potencial celda abierta. No se usa el umbral de 11 V, que corresponde a baterías inundadas |
| Vigencia de `ver-presupuesto-apagado` | **180 días** (la carga del host cambia) |
| Vigencia de `ver-bios-autoencendido` y `ver-flag-ob` | **365 días** |
| Vigencia de `ver-shutdown-return` | Sin caducidad |
| Puntos mínimos para una tendencia de salud | **≥ 4 pruebas comparables**; con menos, la confianza es `baja` |
| Cadencia de prueba de batería programada | **Trimestral** |
| SLO de disponibilidad del propio servicio | **PENDIENTE** — no está en la fuente. El servicio corre en el mismo host que protege, así que su disponibilidad está acotada por la del host; se propone medirla como «rondas de sondeo completadas / rondas esperadas ≥ 0,99 mensual» **[derivado]** |
| Qué se loguea | Cada acción sobre el equipo con su resultado observado; cada sondeo fallido; cada cambio de estado de una `Verificacion`; cada versión de política creada. Log local, estructurado, en el contenedor |

**Integridad y trazabilidad (invariantes bloqueantes del dominio):**

- **I-7:** todo valor almacenado tiene su `Origen`. **Sin excepción.**
- **I-8:** un valor `derivado` declara `de` con al menos una variable de la que deriva.
- **I-9:** `aptoParaTendenciaDeSalud()` devuelve `false` para `derivado`, `estimadoPorDriver` e `imputado`.
- **I-10:** `tiempoReservadoApagadoSeg ≤ 540`.
- **I-13:** toda `Accion` referencia una `VersionPolitica`, nunca una `Politica`.
- **I-18:** todo `Dinero` lleva moneda **y** fecha.
- **I-19:** idempotencia de ingesta por `claveIdempotencia`.
- **I-20:** todo `Agregado` servido declara su `cobertura` y su advertencia.
- **I-21:** una `vidaFlotacionEsperada` sin `temperaturaReferenciaC` es inválida.

**NFR ausentes en las fuentes, declarados como tales:** tiempos de respuesta objetivo del panel, accesibilidad, internacionalización, *rate limiting* de la API, límites de tamaño de request, y plan de recuperación ante desastre del propio servicio. **PENDIENTE**; se cierran en la categoría 08.

### §17.P.11 Decisiones técnicas pre-tomadas (pre-ADR)

Cada una se convierte en un ADR de `05-Arquitectura-Tecnica/`.

| # | Decisión | Justificación | Alternativas evaluadas |
|---|---|---|---|
| PA-01 | **Usar NUT en lugar de escribir el traductor de protocolo** | *"El driver ya conoce esta clase de trampas y modela el comando; escribirlo directamente es reintroducir el riesgo."* El dialecto del equipo no era evidente —el driver descartó `voltronic-qs` antes de acertar `voltronic-qs-hex`— y el espacio de comandos incluye letras sueltas que cortan la energía | Trama Megatec construida a mano (descartada por riesgo); ViewPower del fabricante (descartada por RCE sin parche) |
| PA-02 | **Adaptador de conexión con tres implementaciones** (NUT, directo+add-on, simulado); solo NUT en la primera entrega | Aísla el dominio del transporte y hace testeable el camino de apagado sin hardware | Acceso directo a NUT desde la capa de aplicación (descartado: haría el apagado imposible de probar) |
| PA-03 | **Anclar el USB por ruta física de puerto** (`ID_PATH=pci-0000:00:14.0-usb-0:3`) con regla `udev` | El equipo no tiene `iSerial`, así que no hay `/dev/serial/by-id`; el nodo `hidraw` es volátil. Efecto lateral favorable: anclar por puerto significa *«el SAI que esté enchufado ahí»*, así que un reemplazo de equipo no rompe el binding | Anclaje por serial (imposible); mapear `/dev/bus/usb` entero al contenedor (descartado: expondría otros periféricos del host) |
| PA-04 | **Historia append-only, sin event store ni CQRS** | *"Alcanza con que las tablas de historia sean append-only… Es una disciplina, no una tecnología"* | Event sourcing completo (descartado por desproporción) |
| PA-05 | **Vigencia como entidad con intervalo, no como atributo** (`MontajeBateria`, `CoberturaHost`) | Es la carencia C-1 del modelo descartado: con la vigencia en la batería, el caso «el SAI se fue a reparación y otro cubrió el host» *"ni siquiera es representable"* | Fechas `desde`/`hasta` en la batería (descartado, era el modelo anterior) |
| PA-06 | **Procedencia obligatoria en todo valor** (`Valor<T>` con `Origen`) | Carencia C-3, la más grave del modelo anterior. Mitiga el riesgo R-13, *"el modo de falla más probable del sistema"* | Guardar solo el número (descartado) |
| PA-07 | **Separar catálogo de inventario de historia** | Es lo que habilita comparar marcas: sin distinguir el modelo del producto de la unidad física, agrupar fichas de vida útil por modelo es imposible (carencia C-2) | Una sola tabla de dispositivos (descartado) |
| PA-08 | **`Agregado` no hereda de `Muestra`** | *"el compilador debe obligar a distinguirlos"*. Servir un agregado como si fuera una muestra es el modo de falla que borra los microcortes | Herencia con un flag (descartado) |
| PA-09 | **Modalidad `CicloForzado`: no cancelar el corte del SAI aunque vuelva la red** | *"Es preferible un apagón controlado de tres minutos a un servidor apagado hasta la mañana siguiente"* | Cancelar al retornar la energía (es el comportamiento por defecto, y es el que produce el bloqueo) |
| PA-10 | **Arranque forzado en `SoloAlerta` y bloqueo por verificación** | El servicio no apaga un servidor sin backups mientras no pueda probar que va a volver a encenderse. Mitiga R-12, el riesgo principal del proyecto | Confiar en los supuestos sin verificarlos (descartado explícitamente) |
| PA-11 | **Validar toda acción por efecto observado, no por ausencia de error** | Durante el relevamiento, un comando que nunca llegó al equipo no produjo ningún mensaje de error | Asumir éxito ante ausencia de excepción (descartado: *"va a mentir"*) |
| PA-12 | **No depender del flag `LB` ni de `battery.runtime`** para el disparo; usar tiempo en `OB` + `battery.voltage` | `LB` no fue observado nunca en este equipo; `battery.runtime` no existe | Umbral por `battery.charge` (descartado: es derivado por el driver) |
| PA-13 | **Método de salud: tendencia de la caída de tensión durante el autotest, a carga igualada** | *"Calcular salud propia no es sobre-ingeniería: es la única opción."* Es lo único que los datos disponibles permiten afirmar | Medición óhmica absoluta, coup de fouet, constante de tiempo de recuperación y tensión de flotación — las cuatro descartadas con fundamento en §6 de la fuente |
| PA-14 | **No leer el ajuste de BIOS por software; verificarlo por comportamiento** | *"Posible pero inútil"*: frágil por versión, peligroso al escribir, y verifica lo que no importa | Lectura de `efivars`/IFR (descartada con cuatro razones en §4.6) |
| PA-15 | **Clean Architecture en cinco assemblies con dependencias hacia el dominio** | Impuesto por `Topologia-Proyecto-Solucion.md`; hace testeables los invariantes sin infraestructura | Capas tradicionales; event-driven (ambas descartadas en P.2) |

**Queda abierto para el Sprint 0** (a resolver como ADR antes de codificar la infraestructura):

- **¿NUT dentro del contenedor o en el host?** (riesgos R-05 y R-08). Dentro: un único artefacto desplegable, más limpio. En el host: el contenedor no necesita el dispositivo, y el servicio es cliente TCP de `upsd`. En ambos casos hay que resolver la competencia por el nodo USB (O-U1).
- **TLS del panel y de la API en la LAN** (P.5).
- **Contrato del endpoint de rectificación** que sugiere la respuesta 409 (P.3).
- **Forma exacta del contrato del adaptador de conexión** — la fuente declara sus cuatro operaciones pero deja la firma abierta.

### §17.P.12 Restricciones técnicas y trade-offs aceptados

| # | A qué se renuncia | Qué se gana | Justificación |
|---|---|---|---|
| T-01 | **Escalabilidad horizontal y concurrencia de escritura** (SQLite, un solo proceso) | Simplicidad operativa: un contenedor, un archivo, respaldo trivial | Un usuario, un SAI, un host. Un motor cliente-servidor sería infraestructura sin contrapartida |
| T-02 | **Detección confiable de microcortes más cortos que el intervalo de sondeo** | Un volumen de datos manejable (~6,3 M filas/año en vez de un orden de magnitud más) | La alternativa —sondear a 1 Hz de forma permanente— multiplicaría el volumen por cinco sin eliminar el problema, porque el equipo deja de atender consultas mientras conmuta |
| T-03 | **Cualquier afirmación cuantitativa de salud de batería** (SoH en %, capacidad en Ah, autonomía, resistencia interna absoluta) | Un indicador honesto y defendible: una tendencia relativa con su confianza y su reserva declaradas | El equipo no expone los datos necesarios y IEEE 1188 es de pago. Afirmar más sería inventar |
| T-04 | **Corrección de la tendencia de salud por temperatura** | Nada: es una renuncia forzada | No hay sensor de temperatura (`temperaturaAmbienteC` siempre `null`). La oscilación estacional puede rivalizar con la señal de degradación (R-09). Toda conclusión lleva esa reserva |
| T-05 | **Notificaciones remotas como mecanismo de alerta primario** | Que el sistema alerte cuando importa, aunque nadie lo esté mirando | En un corte de energía la red también cae: verificado en E-4 con `network is unreachable` y `no route to host`. El histórico local es la fuente primaria |
| T-06 | **Un apagón controlado de ~3 minutos cuando el corte resulta breve** (modalidad `CicloForzado`) | Que el host nunca quede apagado indefinidamente esperando que alguien apriete el botón | Es el trade-off central del proyecto y está explícitamente aceptado por la fuente |
| T-07 | **Independencia de NUT** | No reintroducir el riesgo de construir tramas a mano sobre un dialecto que no era evidente y un espacio de comandos que incluye letras que cortan la energía | El adaptador aísla la dependencia: cambiar de mecanismo más adelante no toca el dominio |
| T-08 | **Cobertura de test del flujo físico de apagado y reencendido** | — | No es automatizable: exige cortar la energía real. Se sustituye por el adaptador simulado para la lógica, y por la evidencia registrada de UF-8 para el comportamiento del firmware |
| T-09 | **Portabilidad a Windows, macOS y arm64** | Un solo target que probar, un solo pipeline, un solo conjunto de scripts | El sistema administra un SAI conectado a un host Linux concreto. La portabilidad no tendría consumidor |
| T-10 | **Precisión de `ups.load` como magnitud absoluta** | Poder usarlo igual como variable de comparabilidad entre pruebas | Es un entero en porcentaje, de una potencia nominal desconocida (`imputado`, `null`), estimado por el propio SAI, sin factor de potencia real. Sirve para igualar carga entre dos pruebas, no para calcular vatios |

---

## §18 Estrategia de demo / samples

Un solo sample, `samples/ingesta-gmao/`, correspondiente a la única superficie de la solución consumida por un tercero.

| Aspecto | Detalle |
|---|---|
| Qué ilustra | El contrato completo de `POST /api/v1/intervenciones`: los cuatro caminos de respuesta (201 creado, 200 idempotente con `creado=false`, 409 `conflicto_idempotencia` con huellas `sha256`, 422 con campo e invariante violado) |
| Proyecto que ilustra | Sai-Service-Core (su capa `SAI.Service.Core.Api`) |
| Nivel de complejidad | Media — cubre el camino feliz y los tres de error, incluido el reintento, que es el caso normal y no el excepcional |
| Vínculo con `/src` | Consume la API por HTTP, sin referenciar código de la solución. Se ejecuta contra una instancia levantada con `scripts/run.sh SAI.Service.Core.Web` |
| Reproducción | Cuatro pasos: (1) `devcontainer up`; (2) `./scripts/run.sh SAI.Service.Core.Web`; (3) `cd samples/ingesta-gmao`; (4) ejecutar el script del sample, que imprime las cuatro respuestas con sus códigos |
| Punto de extensión demostrado | La `FuenteDatos` externa con confianza `media`, distinta de la del poller local: el sample muestra que el dato ingresado queda registrado con menor confianza que el medido |

El panel Blazor no produce sample: se demuestra ejecutando el propio servicio, que es el entregable de cada etapa de §15.

---

## §19 Checklist de completitud del intake

**Negocio (Parte A):**
- [x] La cabecera tiene nombre de solución, cliente, fecha y estado.
- [x] §1 describe un problema concreto y qué pasa si no se construye.
- [x] §2 tiene al menos un stakeholder por categoría con rol explícito (la categoría es derivada: la fuente no usa la taxonomía).
- [x] §4 tiene al menos un ítem en cada categoría MoSCoW y el Must Have es el mínimo razonable (las etiquetas MoSCoW son derivadas de la separación «Primera entrega / Dentro / Fuera del alcance»).
- [x] §5 tiene al menos 3 historias en formato `Como/quiero/para`, cubriendo 2 roles (administrador y sistema externo). Las 12 historias son derivadas: la fuente no trae ninguna redactada.
- [x] §7 lista 28 casos límite con su respuesta.
- [x] §8 tiene 5 métricas SMART de negocio con target y plazo numéricos. **Los targets son propuestos y requieren ratificación del stakeholder.**
- [x] §9 lista 9 exclusiones con justificación.
- [x] §10 declara "sin presupuesto y sin fecha", justificado.
- [x] §11 lista 14 riesgos con probabilidad, impacto y mitigación.
- [x] §12 define 24 términos del dominio.

**Composición (Parte B):**
- [x] §13 enumera todos los proyectos (uno), con su valor D8 (`web-monolith`), señala el principal y el grafo es acíclico (trivialmente).
- [x] §13 declara el perfil de convención de nombres; no hay colisión de nombres.
- [x] §14 describe la composición y los contratos; no hay contratos inter-proyecto porque hay un solo proyecto, y se declara explícitamente.
- [x] §15 garantiza valor demostrable end-to-end en la primera etapa (solución que compila, corre y se valida visualmente).
- [x] §16 publica el árbol `tree` derivado de la jerarquía y de la convención de nombres, con §16.1.

**Técnica por proyecto (Parte C):**
- [x] §17 está completo para el único proyecto de §13 (identidad + P.1 a P.12).
- [x] P.6 declara cobertura mínima numérica (80/70 global, 90/85 en `Domain`); P.7 adopta SemVer 2.0.0 y Conventional Commits sin excepciones; P.8 enumera 10 quality gates bloqueantes; P.9 declara plataformas y versiones mínimas con cláusula de no-soporte; P.10 expresa NFR con métricas numéricas.

**General:**
- [x] No hay vocabulario del dominio fuente del bootstrap ni stacks hardcodeados en el texto normativo (D7).
- [x] El control de cambios refleja la versión y fecha del documento.

**Pendientes declarados** (ninguno bloqueante según Intake-Rules §2; todos son campos no marcados `(*)` o decisiones de Sprint 0):

| # | Pendiente | Sección | Nivel |
|---|---|---|---|
| P-01 | Ratificación de los targets de las métricas de éxito, que son propuestos sobre datos de la fuente | §8 | Recomendado |
| P-02 | ¿Existe normativa local aplicable a la disposición final de baterías? | §10 | Recomendado |
| P-03 | ¿NUT dentro del contenedor o en el host? | §17 P.11 | Recomendado — ADR de Sprint 0 |
| P-04 | TLS del panel y de la API en la LAN | §17 P.5 | Recomendado — ADR de Sprint 0 |
| P-05 | Contrato del endpoint de rectificación del 409 | §17 P.3 | Recomendado — se cierra en la categoría 02 |
| P-06 | Firma exacta del contrato del adaptador de conexión | §17 P.11 | Recomendado — se cierra en la categoría 05 |
| P-07 | Tiempo real de apagado completo del host bajo carga | §17 P.10 | Recomendado — se mide en UF-8, no bloquea la documentación |
| P-08 | Tamaño máximo del archivo SQLite tras la agregación | §17 P.10 | Recomendado — se valida antes de producción (R-07) |
| P-09 | SLO de disponibilidad del servicio, tiempos de respuesta del panel, accesibilidad, i18n y *rate limiting* | §17 P.10 | Recomendado — se cierran en la categoría 08 |

---

## Trazabilidad downstream

| Sección del intake | Destino | Documento downstream típico |
|---|---|---|
| §1 a §12 (negocio) | `00-Contexto/`, `01-Necesidades-Negocio/` | Visión de producto, alcance, NB-XX |
| §13 (proyectos) | `SOLUTION-MANIFEST` derivado; todas las categorías | Manifiesto canónico; selector de variante `web-monolith` |
| §14 estilo de solución | `05-Arquitectura-Tecnica/` | `Arquitectura-Solucion-v1.0.md` (sin vista de solución: un solo proyecto) |
| §15 descomposición | `06-Backlog-Tecnico/`, `07-Plan-Sprint/` | Backlog por etapa, plan de sprint |
| §16 estructura | `05-Arquitectura-Tecnica/`, `10-Developer-Guide/` | Árbol de repositorio, guía de onboarding |
| §17 P.1 a P.12 | `05`, `08`, `09`, `00` | ADRs (15 pre-ADR de P.11), estrategia de testing, pipeline, NFR, compatibilidad de plataformas |
| §18 samples | `11-Examples/` | `Ejemplo-01-Ingesta-Gmao-v1.0.md` |

---

## Control de cambios

| Versión | Fecha | Cambios | Autor |
|---|---|---|---|
| 1.0 | 2026-07-20 | Intake unificado inicial de la solución SAI.Service.Core, derivado de `Planteo-Analisis-Unificado-Antecedente-SAI-Service.md`, `Topologia-Proyecto-Solucion.md`, `Entorno-Desarrollo.md` y el tool-prompt `Crear-SDD-Documento-Intake.md`. | Orquestador SDD (Claude Code) |
