# Arquitectura de solución — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Arquitectura-Solucion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

---

## 1. Objetivo

Este documento es el artefacto maestro de arquitectura técnica de **Sai-Service-Core**, un servicio web `web-monolith` que monitorea un SAI a través de NUT, decide y ejecuta el apagado ordenado del host con garantía de reencendido, administra el ciclo de vida de los equipos y expone un panel Blazor más una API REST de ingesta. Está dirigido a quien implementa el servicio (el administrador único, que es a la vez desarrollador) y a los revisores de las categorías 06 a 11. Describe las cuatro vistas mínimas (lógica, procesos, despliegue, datos), los cross-cutting concerns, los atributos de calidad con objetivo numérico, los riesgos y la trazabilidad hacia los casos de uso, las reglas de negocio y los 22 ADR de la categoría. El detalle físico del esquema vive en `Modelo-Datos-Logico-v1.0.md`; el cuerpo de cada decisión vive en `Adrs/`; el índice de decisiones en `Decisiones-Arquitectura-v1.0.md`.

## 2. Estilo arquitectónico

**Estilo elegido: Clean Architecture en cinco assemblies con la dirección de dependencias apuntando siempre hacia el dominio** (`Web → Api → Infrastructure → Application → Domain`; el dominio no depende de nada). Es la estructura fijada por `Topologia-Proyecto-Solucion.md`, formalizada en **ADR-15**. Los cinco assemblies (`SAI.Service.Core.Domain`, `.Application`, `.Infrastructure`, `.Api`, `.Web`) son capas internas de un **único** proceso y un **único** despliegue, no proyectos de la solución (§13, §14 del intake): la solución es un caso degenerado de un solo proyecto.

Sobre ese esqueleto se apoyan tres decisiones estructurales propias del dominio:

- **Adaptador de conexión** (puerto en `Application`, implementaciones en `Infrastructure`), que aísla al dominio del *cómo* se dialoga con el equipo — ADR-02, ADR-22, detalle en `Extensibilidad-v1.0.md`.
- **Planificador interno** como *hosted service* en `Application` — ADR-09 a ADR-12, detalle en `Flujo-Ejecucion-v1.0.md`.
- **Historia append-only** en las tablas de hechos — ADR-04.

**Justificación contra alternativas descartadas.** La elección se evalúa con los criterios de la tabla de estilos del §4.6 de las reglas (tamaño de equipo 1, un solo dominio de negocio, sin necesidad de deploy independiente, complejidad operativa que debe ser baja, camino crítico irreversible que debe ser testeable sin infraestructura):

| Criterio de decisión | Clean Architecture 5 assemblies (elegido) | Capas tradicionales con acceso a datos desde la UI (descartada) | Event-driven con event store y CQRS (descartada) |
| --- | --- | --- | --- |
| Testabilidad del dominio sin infraestructura | Alta: el dominio no referencia EF Core, Blazor ni NUT; los invariantes I-1 a I-21 se prueban puros | Baja: acopla la decisión de apagado al ORM y al framework web | Media: exige infraestructura de proyección para probar el estado |
| Complejidad operativa | Baja: un proceso, un contenedor, un archivo | Baja | Alta: event store, proyecciones, consistencia eventual |
| Aislamiento del camino crítico irreversible (apagado) | Alto: la lógica de decisión vive en `Application`/`Domain`, desacoplada del transporte por el adaptador | Bajo: la decisión queda mezclada con acceso a datos | Medio, con un modo de falla nuevo por asincronía en el peor lugar |
| Costo de documentación y mantenimiento (1 desarrollador) | Bajo | Bajo | Desproporcionado para 1 usuario, 1 SAI, 1 host |
| Ajuste al mandato del tool-prompt y de la fuente | Total (monolito impuesto; append-only pedido explícitamente) | Parcial | Rechazado explícitamente por la fuente (E-09) |

- **Capas tradicionales con acceso a datos desde la UI** — descartada porque acopla la única lógica con consecuencias irreversibles (la decisión del apagado) al ORM y al framework web, y encarece hasta lo impracticable las pruebas de los invariantes que la fuente exige escribir *antes* de codificar (R-10). Referencia: §17 P.2 del intake, ADR-15.
- **Arquitectura orientada a eventos con event store y CQRS** — descartada explícitamente por la fuente: el volumen y la concurrencia (un usuario, un dispositivo) no la justifican, y la garantía buscada —inmutabilidad de los hechos— se obtiene con disciplina de escritura append-only, no con infraestructura (*"Es una disciplina, no una tecnología"*, exclusión E-09). Referencia: ADR-04, ADR-15.

## 3. Vista lógica

Componente = módulo con responsabilidad cohesiva, no clase. Los cinco assemblies y los dos componentes transversales del dominio (adaptador y planificador):

| Componente | Responsabilidad | Entradas | Salidas | Dependencias (unidireccionales hacia el dominio) |
| --- | --- | --- | --- | --- |
| `SAI.Service.Core.Domain` | Entidades, objetos de valor (`Valor<T>` con `Origen`, `Dinero`), invariantes I-1 a I-21, `ResolutorTemporal`, máquina de estados de `UnidadFisica`, lógica de derivados y veredicto de salud, degradación de modalidad | DTOs internos, valores con procedencia | Entidades e hechos de dominio; decisiones de modalidad efectiva | Ninguna (núcleo puro; sin frameworks) |
| `SAI.Service.Core.Application` | Casos de uso, **puerto** del adaptador de conexión, **planificador** (hosted service), evaluación de políticas, orquestación de CU | Comandos validados, resultados del adaptador, temporizadores | Órdenes al adaptador; hechos a persistir; estado para el panel | `Domain` |
| `SAI.Service.Core.Infrastructure` | EF Core + SQLite (repositorios, migraciones), **implementaciones** del adaptador (NUT, simulada; directo diseñada), logging estructurado, `ResolutorTemporal` sobre datos persistidos | Entidades; variables NUT clave-valor; configuración por entorno | Filas en SQLite; llamadas a `upsd`/`upsc`; invocación de `shutdown` | `Application`, `Domain` |
| `SAI.Service.Core.Api` | Endpoints REST `/api/v1/` (ingesta idempotente), serialización, mapeo de errores a problem+json | HTTP + JSON; cabeceras `X-Idempotency-Key`, `X-Fuente-Datos` | Respuestas 201/200/409/422; DTOs | `Infrastructure` (composición), `Application`, `Domain` |
| `SAI.Service.Core.Web` | Host del proceso; panel Blazor interactive server (circuito por WebSocket); autenticación; composición de la raíz de dependencias | HTTP + WebSocket; interacción del administrador | Páginas del panel; arranque de los endpoints y del hosted service | `Api` (y todo lo anterior por transitividad) |
| Adaptador de conexión (puerto) | Contrato mínimo hacia el equipo: leer estado, probar conectividad, ordenar apagado con retorno, lanzar test de batería | — (define la interfaz) | — (define la interfaz) | Declarado en `Application`; implementado en `Infrastructure` (ADR-02, ADR-22) |
| Planificador (hosted service) | Rondas de sondeo, evaluación de políticas, temporizadores con cancelación, detección de pérdida de comunicación, elevación de cadencia durante pruebas de batería | Estado del adaptador; versión de política vigente; verificaciones | `Muestra`, `Evento`, `Accion`; alertas al panel | `Application` sobre `Domain`; usa el puerto del adaptador |

Regla de dependencia (verificada como test de arquitectura): ninguna flecha apunta hacia afuera del dominio. `Web → Api → Infrastructure → Application → Domain`. El panel y la API nunca acceden a SQLite ni a NUT salvo a través de `Application` y del puerto del adaptador.

## 4. Vista de procesos

El servicio corre como **un solo proceso** con **un único hilo escritor** de la historia (concurrencia de escritura = 1, T-01). Dentro de ese proceso conviven:

- **Planificador interno como hosted service.** Ejecuta **rondas de sondeo** en el `intervaloSegundos` configurado (**5 s** por defecto; **1 Hz** durante una prueba de batería, restaurado al terminar). Cada ronda: pide el estado por el adaptador, persiste una `Muestra` (`completa`/`parcial`/`perdida`), evalúa reglas de derivación, genera eventos y empuja estado al panel. La **latencia de decisión de una ronda debe ser < 1 s** para no desplazar la siguiente (§17 P.10, [derivado] del intervalo de 5 s).
- **Temporizadores con cancelación.** Una condición de disparo (equipo en batería) debe **sostenerse N segundos** (`umbralDisparoSegundos`, 300 s de partida) antes de generar la `Accion`, para no actuar ante microcortes. El temporizador se cancela si la condición cesa antes del umbral (FA-1 de CU-05). Una vez iniciada la secuencia de apagado en modalidad `CicloForzado`, el corte del SAI **no se cancela** aunque vuelva la red (ADR-09).
- **Circuito Blazor Server.** Cada sesión del panel mantiene un circuito por WebSocket sobre el que el servidor empuja el estado en vivo sin polling desde el navegador. El estado en memoria del circuito es efímero y reconstruible; la verdad vive en SQLite.
- **Detección de pérdida de comunicación.** Tres sondeos consecutivos sin respuesta ⇒ evento `DesconexionUsb` y alerta visual (ADR-11 por efecto observado; CL-14).

Manejo de estado en memoria: mínimo y reconstruible. La historia es la fuente de verdad; el planificador no conserva estado crítico que se pierda al reiniciar. Toda acción sobre el equipo o el host se confirma **por efecto observado**, nunca por ausencia de error (ADR-11). El detalle paso a paso del camino crítico está en `Flujo-Ejecucion-v1.0.md`.

## 5. Vista de despliegue

| Unidad de despliegue | Runtime objetivo | Dependencias de infraestructura |
| --- | --- | --- |
| Imagen de contenedor de producción (runtime-only) | Docker sobre Linux x86-64 (`linux/amd64`), .NET 10, corriendo en el host `i7infra` | Archivo SQLite en volumen persistente; acceso a NUT; dispositivo USB anclado por ruta física |
| Dev Container (spec containers.dev) | Docker en la máquina del desarrollador; SDK de .NET dentro del contenedor | Único requisito del host: Docker. Depuración por `.vscode/launch.json` (F5), nunca por los scripts |

- **Anclaje del USB por ruta física de puerto** con regla `udev` (`ID_PATH=pci-…-usb-0:3`), porque el equipo no expone `iSerial` y el nodo `hidraw` es volátil — ADR-03. Efecto lateral favorable: *«el SAI que esté enchufado ahí»*, de modo que un reemplazo de equipo no rompe el binding (CL-27).
- **Ubicación de NUT**: dentro del contenedor o en el host, con el servicio como cliente TCP de `upsd`. Decisión abierta de Sprint 0 — **ADR-19 [Propuesto]** (R-05, R-08).
- **Ambientes: dos, sin staging** — *Desarrollo* (Dev Container) y *Producción* (contenedor en `i7infra`). No hay staging porque no habría a qué SAI conectarlo (§17 P.8).
- **Terminación TLS del panel y de la API en la LAN**: decisión abierta de Sprint 0 — **ADR-20 [Propuesto]** (autofirmado o reverse proxy del host).
- **Rollback**: volver a la etiqueta de imagen anterior y reiniciar; la historia append-only no pierde hechos ante un rollback de versión.

## 6. Vista de datos

- **Motor: SQLite, archivo único**, con EF Core y **migraciones versionadas** aplicadas al arranque (no hay generación automática de esquema en producción) — ADR-18.
- **Cuatro capas del modelo**: catálogo (qué es), inventario (cuál es, con baja lógica), vínculos temporales (`MontajeBateria`, `CoberturaHost`) e historia append-only — ADR-07, ADR-04, ADR-05.
- **Patrón de acceso decisivo**: la historia **no** guarda a qué batería pertenece una métrica; guarda **dispositivo e instante**, y la batería se resuelve por `MontajeBateria` vía `ResolutorTemporal` — ADR-05, RC-07. Corregir la fecha de un recambio reatribuye el histórico sin migrar datos (CL-18).
- **Procedencia obligatoria en todo valor** (`Valor<T>` con `Origen`), optimizada declarando la procedencia una vez por `SesionSondeo` — ADR-06, I-7.
- **`Agregado` no hereda de `Muestra`**; para `input.voltage` los agregados conservan mínimo y máximo además del promedio, porque el promedio horario oculta los microcortes — ADR-08, I-20.
- **Retención y agregación**:

| Dato | Resolución completa | Después | Retención total |
| --- | --- | --- | --- |
| `Muestra` | `P30D` | se agrega a `PT1H` y se descarta | 30 días |
| `Agregado` | `PT1H` | — | `P10Y` |
| `Evento` | — | — | indefinido |

Volumen dimensionado: ~6,3 millones de filas/año a 5 s de intervalo. Multi-tenant: no aplica (un administrador, un host, un SAI activo). El modelo lógico completo, con tipos físicos, índices, restricciones y migración inicial, está en **`Modelo-Datos-Logico-v1.0.md`**, que referencia entidad por entidad el `Modelo-Conceptual-v1.0.md` de 02.

## 7. Cross-cutting concerns

Sección única que centraliza las decisiones transversales (anti-patrón «cross-cutting disperso», §4.7 de las reglas):

- **Logging**: estructurado y **local**, dentro del contenedor. Se loguea cada acción sobre el equipo con su **resultado observado**, cada sondeo fallido, cada cambio de estado de una `Verificacion` y cada versión de política creada (§17 P.10). No hay tracing distribuido ni exportación de métricas a un backend externo: un solo proceso, sin salida a internet.
- **Manejo de errores**: la API de ingesta responde con **problem+json (RFC 7807)** y códigos estables 201/200/409/422 — ADR-17, detalle en `Contratos-REST-v1.0.md`. El dominio expresa las violaciones como invariantes (`validacion`, `coherencia_temporal`). En el camino crítico, todo error de ejecución se resuelve por **efecto observado**: `EFECTO_NO_CONFIRMADO` mantiene el estado seguro y no reporta como ejecutado lo no observado — ADR-11, RN-03.
- **Configuración**: por **variables de entorno** del contenedor (cadena de conexión a SQLite, credenciales de `upsd` si NUT corre en el host, intervalo de sondeo).
- **Gestión de contraseñas y secretos**: en desarrollo, *user-secrets* del SDK dentro del Dev Container; en producción, **variables de entorno**, nunca en el repositorio. Sin credenciales de publicación (no hay despliegue automatizado a registros externos en el alcance) — §17 P.5.
- **Seguridad operativa (transversal al camino crítico)**: arranque forzado en `SoloAlerta`; **bloqueo por verificación** (una `Accion` con un supuesto en `NuncaVerificado`/`Vencido`/`Refutado` queda `BloqueadaPorVerificacion` y la modalidad efectiva degrada a `SoloAlerta`) — ADR-10, RN-01, RN-02.
- **Validación por efecto observado** como disciplina transversal a toda orden sobre equipo y host — ADR-11.
- **Autenticación**: ASP.NET Core Identity, administrador único, cookie de sesión, hash PBKDF2 por defecto — ADR-16.

## 8. Quality attributes (NFR)

Cada NFR con objetivo numérico, mecanismo de medición y ADR relacionada. Fuente: §17 P.10 del intake. Los marcados **PENDIENTE** no tienen respaldo numérico en las fuentes y se cierran en 08.

| # | NFR | Objetivo numérico | Mecanismo de medición | ADR relacionada |
| --- | --- | --- | --- | --- |
| N-01 | Retardo de corte del SAI (`ups.delay.shutdown`) | ≤ 540 s (techo duro; el formulario rechaza `tiempoReservadoApagadoSeg > 540`) | Test de invariante I-10 en `Domain`; límite del equipo verificado | ADR-09 (RN-04) |
| N-02 | Presupuesto del `shutdown-timeout` del demonio Docker | 150 s de los 540 (el grace de 15 s es insuficiente) | Medición cronometrada del grace del contenedor con VM huésped | ADR-09 |
| N-03 | Resto del apagado del sistema operativo | **PENDIENTE de medición**; se cronometra en la ventana de mantenimiento | `ver-presupuesto-apagado` (evidencia registrada en CU-10) | ADR-10, ADR-14 |
| N-04 | Retardo de reencendido (`ups.delay.start`) | 180 s (rango 60–599940 s) | Lectura de la variable del equipo | ADR-09 |
| N-05 | Umbral de disparo del apagado | 300 s de partida, ajustable por versión de política | Config de `VersionPolitica`; test de la lógica de temporizador | ADR-09, ADR-10 |
| N-06 | Latencia de decisión del planificador | < 1 s por ronda (intervalo de 5 s) | Métrica de duración de ronda logueada por el hosted service | ADR-15 |
| N-07 | Intervalo de sondeo normal | 5 s (configurable) | Configuración por entorno; medición de cadencia real | ADR-01, ADR-11 |
| N-08 | Cadencia durante prueba de batería | 1 Hz, restaurada al valor normal al terminar | Densidad de muestreo registrada en la `PruebaBateria` | ADR-13 |
| N-09 | Detección de pérdida de comunicación con el SAI | 3 sondeos consecutivos sin respuesta ⇒ `DesconexionUsb` + alerta | Contador de sondeos fallidos; test de integración | ADR-11 |
| N-10 | Rango aceptable de `input.voltage` | [198, 242] V; sostenido fuera 30 s ⇒ `TensionFueraDeRango` | `ReglaDerivacion` versionada; test de derivación | ADR-13 |
| N-11 | Umbral de microcorte | < 60 s entre OL→OB y OB→OL (regla v2) | `ReglaDerivacion` con versión; normalización por versión (CL-15) | ADR-04 (RC-09) |
| N-12 | Alarma dura de batería | `battery.voltage` < 13,3 V ⇒ celda en corto; > 14,5 V ⇒ celda abierta | Regla de alarma sobre la muestra; test de umbrales | ADR-13 |
| N-13 | Vigencia de `ver-presupuesto-apagado` | 180 días | Caducidad de la `Verificacion`; test de vencimiento | ADR-10 |
| N-14 | Vigencia de `ver-bios-autoencendido` y `ver-flag-ob` | 365 días | Caducidad de la `Verificacion` | ADR-10, ADR-14 |
| N-15 | Vigencia de `ver-shutdown-return` | Sin caducidad | `Verificacion` sin vencimiento | ADR-10 |
| N-16 | Puntos mínimos para una tendencia de salud | ≥ 4 pruebas comparables; con menos, confianza `baja` | Conteo de `PruebaBateria` con `comparable = true` | ADR-13 |
| N-17 | Cadencia de prueba de batería programada | Trimestral | Programación del planificador | ADR-13 |
| N-18 | Volumen de escritura | ~6,3 millones de filas/año (720 muestras/hora) | Conteo de filas; validación de agregación y retención | ADR-18, ADR-04 |
| N-19 | Retención (Muestra `P30D`, Agregado `PT1H` durante `P10Y`, Evento indefinido) | Según tabla de §6 | Job de agregación y descarte; test de retención | ADR-04, ADR-08, ADR-18 |
| N-20 | Tamaño máximo del archivo SQLite tras la agregación | **PENDIENTE** — no dimensionado en la fuente (R-07); se valida antes de producción | Medición del archivo bajo carga simulada | ADR-18 |
| N-21 | Cobertura de tests, conjunto de la solución | ≥ 80 % líneas / 70 % ramas (bloqueante en el pipeline) | Quality gate de cobertura en CI (stage 4) | ADR-15 |
| N-22 | Cobertura de tests, `SAI.Service.Core.Domain` | ≥ 90 % líneas / 85 % ramas (bloqueante) | Quality gate de cobertura en CI (stage 3) | ADR-15 |
| N-23 | Idempotencia de la ingesta | 100 % de operaciones repetidas (misma clave + mismo cuerpo ⇒ mismo id) | Test de los cuatro caminos 201/200/409/422; I-19 | ADR-17, ADR-21 |
| N-24 | Procedencia declarada en todo valor | 0 valores sin `Origen` (I-7, sin excepción) | Test de invariante I-7 en el pipeline | ADR-06 |
| N-25 | SLO de disponibilidad del propio servicio | **PENDIENTE** — no está en la fuente; se propone «rondas completadas / esperadas ≥ 0,99 mensual» [derivado] | Ratio de rondas de sondeo completadas | — |

## 9. Riesgos arquitectónicos

De §11 del intake, con la severidad original. Impacto y probabilidad tal como los declara la fuente; mitigación con su anclaje arquitectónico.

| ID | Riesgo | Probabilidad | Impacto | Mitigación (anclaje) |
| --- | --- | --- | --- | --- |
| R-01 | El ciclo de apagado y reencendido no está verificado; la trampa de firmware podría dejar el SAI apagado para siempre | Media | Crítico | Prueba física en ventana de mantenimiento (CU-10) antes de habilitar cualquier modalidad distinta de `SoloAlerta`; `ver-shutdown-return` sin caducidad — ADR-10, ADR-14 |
| R-02 | El presupuesto de 540 s no está medido contra el apagado real; la corrección de R-01 consume 150 s | Alta | Alto | Medición cronometrada en CU-10; `ver-presupuesto-apagado` con vigencia 180 días — ADR-09, N-01/N-02/N-03 |
| R-03 | La BIOS *Restore on AC Power Loss* sin verificar y no legible por software | Media | Alto | Verificación por comportamiento; renovación por evidencia cruzando eventos propios contra `wtmp` — ADR-14 |
| R-04 | El flag `LB` no fue observado: la política no debe depender de él | Alta | Medio | Disparo por tiempo en `OB` + `battery.voltage`, nunca `LB` ni `battery.runtime` — ADR-12 |
| R-05 | Competencia por el nodo USB entre contenedor, NUT en el host y otro contenedor | Media | Medio | Ubicación de NUT a decidir en Sprint 0; anclaje por ruta física con `udev` — ADR-03, ADR-19 [Propuesto] |
| R-06 | Desapariciones del bus USB documentadas | Media | Medio | Vigilancia de conectividad: 3 sondeos fallidos ⇒ `DesconexionUsb` — ADR-11, N-09 |
| R-07 | Retención de métricas no probada contra el volumen real (~6,3 M filas/año) | Media | Bajo | Validar agregación y retención antes de producción — ADR-18, N-19/N-20 |
| R-08 | Decisión abierta: ¿NUT dentro del contenedor o en el host? | Alta | Bajo | ADR en Sprint 0 — ADR-19 [Propuesto] |
| R-09 | Sin sensor de temperatura: el confusor de la tendencia de salud no tiene solución con este equipo | Cierta | Alto (para salud de batería) | Declarado; toda conclusión de salud lleva la reserva explícita — ADR-13, T-04 |
| R-10 | El modelo de datos no se implementó: los invariantes son hipótesis hasta que existan como pruebas | Cierta | Medio | Escribir I-1 a I-21 como pruebas antes de codificar el dominio — ADR-15, N-21/N-22 |
| R-11 | La sustitución de SAI (CU-09) no tiene escenario de datos que la ejercite | Cierta | Bajo | Agregar un escenario E-9 al implementar el flujo — ADR-05 |
| R-12 | **Riesgo principal**: el servicio decide apagar un servidor sin backups; si falla, falla de noche y sin testigos | Media | Crítico | Bloqueo por verificación: arranque forzado en `SoloAlerta`, no apaga nada hasta probar el reencendido — ADR-10 |
| R-13 | Guardar `battery.charge` sin marcar que es interpolación del driver: conclusión falsa sobre datos que parecían medidos | Alta | Alto | Procedencia obligatoria (I-7); `aptoParaTendenciaDeSalud()` = `false` para `derivado`/`estimadoPorDriver`/`imputado`; el panel marca todo valor derivado — ADR-06, ADR-13, N-24 |
| R-14 | Hacer algo que nadie hace: ningún proyecto libre calcula salud desde NUT | Cierta | Medio | Confianza explícita en cada veredicto, arrancando en `baja`; lenguaje limitado a *«se comporta peor que antes»* — ADR-13, N-16 |

## 10. Trazabilidad

Tabla CU/RN upstream, ADR que gobiernan y tests previstos en 08. Los CU están en `02-Especificacion-Funcional/Casos-De-Uso/`; las RN en `Reglas-De-Negocio/`; los ADR en `Adrs/`.

| CU upstream | RN upstream | ADR que gobiernan | Tests previstos en 08 |
| --- | --- | --- | --- |
| CU-01 Autenticación del administrador | RN-01 | ADR-16 | Login, alta inicial única, cambio de contraseña |
| CU-02 Alta de equipos y puesta en marcha | RN-01, RN-05, RN-12, RN-13 | ADR-02, ADR-03, ADR-06, ADR-07, ADR-10 | Descubrimiento USB, siembra de verificaciones en `NuncaVerificado`, procedencia `declarado`/`imputado` |
| CU-03 Configuración de políticas | RN-04, RN-11 | ADR-05, ADR-07 | Versión de política nueva (no edición), techo de 540 s |
| CU-04 Monitoreo en vivo | RN-03, RN-05 | ADR-01, ADR-08, ADR-11 | Calidad de muestra, `battery.charge` marcado derivado, alerta a los 3 sondeos fallidos |
| CU-05 Ejecución del apagado ordenado | RN-02, RN-03, RN-04, RN-11 | ADR-09, ADR-10, ADR-11, ADR-12 | Degradación a `SoloAlerta`, `CicloForzado`, efecto observado, acción referida a versión de política |
| CU-06 Históricos y gráficas | RN-10 | ADR-04, ADR-08 | Superposición de series, marcas de eventos, agregado con cobertura |
| CU-07 Prueba de batería y veredicto de salud | RN-06, RN-13 | ADR-13 | Caída de tensión, comparabilidad por carga, confianza `baja` con < 4 pruebas |
| CU-08 Recambio de batería y ficha de vida útil | RN-07, RN-08, RN-12 | ADR-05, ADR-07 | Cierre/apertura de `MontajeBateria`, `Costos.cuadra()`, costo por año en USD |
| CU-09 Reparación y sustitución del SAI | RN-12 | ADR-03, ADR-05, ADR-07 | Cobertura suplente, días sin protección, binding por ruta física |
| CU-10 Ventana de mantenimiento y verificación | RN-01, RN-02, RN-03 | ADR-10, ADR-14 | Transición de supuestos, refutado vs vencido, presupuesto de apagado |
| CU-11 Ingesta automatizada de intervenciones | RN-07, RN-08, RN-09, RN-12 | ADR-17, ADR-21 [Propuesto] | Cuatro caminos 201/200/409/422, idempotencia, coherencia temporal |
| CU-12 Informe de período y comparación de marcas | RN-06, RN-07, RN-10 | ADR-04, ADR-07, ADR-08 | Cobertura del período, `costoPorAnioDeServicio`, agregación con cobertura |

Trazabilidad NFR↔ADR: consolidada en la columna «ADR relacionada» de la tabla del §8. El estilo, la persistencia, la extensibilidad y la seguridad tienen además su ADR aceptada individual (ADR-15, ADR-18, ADR-02, ADR-10/16). Las cuatro decisiones abiertas de Sprint 0 viven como ADR-19 a ADR-22 en estado Propuesto.

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Documento maestro inicial: 10 secciones del §4.2, estilo justificado contra 2 alternativas, 25 NFR con objetivo numérico y mecanismo, 14 riesgos, trazabilidad de 12 CU a ADR y tests de 08. |
