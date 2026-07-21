# Backlog Técnico — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Backlog-Tecnico-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Técnica de estimación:** Fibonacci (1, 2, 3, 5, 8, 13, 21)
**Trazabilidad upstream:** 05 ADR-01..ADR-22, Arquitectura-Solucion, Modelo-Datos-Logico, Contratos-REST, Extensibilidad; 02 CU/RN; 01 NB
**Documento hermano:** `Product-Backlog-v1.0.md`

Este backlog tiene **30 tareas técnicas (≤ 30)**, por lo que las BT viven **inline** en este documento (§2), con criterios de aceptación, fuente upstream, dependencias y tipo. Cada BT declara al menos una US consumidora o se justifica como infraestructura compartida con ADR explícita.

---

## 1. Épicas técnicas

Las épicas técnicas reutilizan la numeración `EP-XX` del Product Backlog para mantener coherencia entre las dos vistas.

| Épica | Objetivo técnico | Alcance | Fuente upstream | BT contenidas |
|---|---|---|---|---|
| EP-01 | Cerrar las decisiones de arranque de Sprint 0 antes de codificar infraestructura | Cuatro spikes acotados que producen ADR de cierre | ADR-19, ADR-20, ADR-21, ADR-22 (Propuestos) | BT-01, BT-02, BT-03, BT-04 |
| EP-02 | Levantar el esqueleto caminante que compila, corre y se navega | Estructura en cinco assemblies, scripts, devcontainer y panel base | ADR-15, Intake §16, 03 Experiencia-De-Uso | BT-05, BT-06 |
| EP-03 | Dar persistencia, procedencia y seguridad al esqueleto | Persistencia con migraciones, `Valor<T>`, append-only y autenticación | ADR-18, ADR-06, ADR-04, ADR-16 | BT-07, BT-08, BT-09, BT-10 |
| EP-04 | Modelar el dominio de equipos y el acceso al SAI | Catálogo/inventario/historia, vigencia temporal, anclaje USB, adaptador y políticas | ADR-07, ADR-05, ADR-03, ADR-02, ADR-01, NB-07 | BT-11, BT-12, BT-13, BT-14, BT-15, BT-16 |
| EP-05 | Construir el monitoreo, el motor de eventos y la salud | Planificador, muestras/agregados, reglas de derivación, disparo y salud | Componente Planificador, ADR-08, ADR-12, ADR-13, F-08 | BT-17, BT-18, BT-19, BT-20, BT-21 |
| EP-06 | Ejecutar el apagado seguro y modelar el ciclo de vida | Ciclo forzado, bloqueo por verificación, efecto observado, BIOS por comportamiento, intervenciones y ficha | ADR-09, ADR-10, ADR-11, ADR-14, RN-07, RN-08 | BT-22, BT-23, BT-24, BT-25, BT-26, BT-27 |
| EP-07 | Abrir la integración externa y los informes | Endpoint idempotente, adaptador simulado, informes y comparación de marcas | ADR-17, Contratos-REST, F-24, F-22, F-23 | BT-28, BT-29, BT-30 |

## 2. BT por épica

Criterios de aceptación en clave técnica: la BT compila, sus pruebas pasan, el contrato o invariante se respeta. Spikes con caja temporal explícita.

### EP-01 — Fundaciones y decisiones de arranque

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-01 | Spike: ubicación de la herramienta de acceso al SAI (contenedor vs host) | Spike | Must | 3 (caja 2 d) | ADR-19 (P-03) | — | Informe con las dos opciones evaluadas; recomendación con trade-offs; se eleva como ADR de cierre; garantiza un único consumidor del nodo USB anclado |
| BT-02 | Spike: terminación TLS del panel y la API en la LAN | Spike | Must | 2 (caja 1 d) | ADR-20 (P-04) | — | Informe con opciones (autofirmado vs reverse proxy); recomendación; ADR de cierre; el panel y la API quedan accesibles por la LAN con cifrado |
| BT-03 | Spike: contrato del endpoint de rectificación del 409 | Spike | Should | 3 (caja 2 d) | ADR-21 (P-05) | BT-28 | Informe con el contrato propuesto para rectificar un hecho ya ingresado; se cierra en la especificación funcional; la `accionSugerida` del 409 apunta a un contrato definido |
| BT-04 | Spike: firma del puerto del adaptador de conexión | Spike | Must | 3 (caja 2 d) | ADR-22 (P-06) | BT-01 | Firma del puerto documentada; permite validación por efecto observado (ADR-11) e implementación por la herramienta de acceso al SAI y por el adaptador simulado; sin decisión, BT-14 queda bloqueada |

### EP-02 — Esqueleto caminante y panel base

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-05 | Andamiaje en cinco assemblies con scripts y devcontainer | Devops | Must | 8 | ADR-15, Intake §16 | BT-01, BT-02 | Cinco assemblies con dependencias hacia el dominio; `build.sh`/`run.sh`/`build-all.sh`/`run-all.sh` funcionan; devcontainer y launch/tasks presentes; la solución compila y corre |
| BT-06 | Panel base: menú lateral y barra superior | Feature | Must | 5 | 03 Experiencia-De-Uso, Intake §15 etapa 2 | BT-05 | El layout del panel cumple la maqueta aprobada; navegación entre secciones; validado en el navegador |

### EP-03 — Persistencia, procedencia y seguridad

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-07 | Persistencia con migraciones versionadas aplicadas al arranque | Feature | Must | 5 | ADR-18 | BT-05 | Esquema versionado por migraciones; se aplican al arranque sin generación automática en producción; un archivo, un proceso escritor; pruebas de arranque en limpio pasan |
| BT-08 | Objeto de valor `Valor<T>` con `Origen` obligatorio | Feature | Must | 5 | ADR-06 | BT-07 | Ningún valor de dominio se persiste sin `Origen` declarado (invariante I-7); prueba de invariante en el pipeline con cero excepciones |
| BT-09 | Historia append-only como disciplina de escritura | Feature | Must | 3 | ADR-04 | BT-07 | Las tablas de hechos no admiten update ni delete; corrección por reatribución, no por migración; prueba que rechaza escrituras destructivas |
| BT-10 | Autenticación de administrador único y endpoint de salud público | Feature | Must | 5 | ADR-16 | BT-07 | Rol único con el algoritmo de hash de contraseña por defecto del proveedor de identidad y la sesión de usuario; alta inicial solo si no hay admin; `/health` sin auth y el resto autenticado; pruebas de autorización pasan |

### EP-04 — Dominio de equipos, adaptador y políticas

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-11 | Modelo de catálogo, inventario e historia con baja lógica | Feature | Must | 8 | ADR-07 | BT-08, BT-09 | Cuatro capas de modelo separadas; baja lógica con estado, fecha y motivo; sin borrado físico; una operación fechada después de la baja se rechaza por coherencia temporal |
| BT-12 | Vigencia como entidad con intervalo y `ResolutorTemporal` | Feature | Must | 8 | ADR-05 | BT-11 | `MontajeBateria` y `CoberturaHost` con `desde`/`hasta`; el resolutor devuelve la batería/dispositivo vigente en cada instante; corrección de fecha reatribuye el histórico sin migrar datos |
| BT-13 | Anclaje del USB por ruta física de puerto | Devops | Must | 3 | ADR-03 | BT-01 | El dispositivo se ancla por su ruta física de puerto; el binding sobrevive a reconexiones y no depende del serial; documentado para reproducir en el host |
| BT-14 | Puerto del adaptador de conexión (contrato mínimo) | Feature | Must | 5 | ADR-02, ADR-22 | BT-04 | Puerto en Application con las operaciones mínimas (leer estado, probar conexión, ordenar apagado con retorno, lanzar prueba de batería); permite tres implementaciones; pruebas contra el simulado pasan |
| BT-15 | Implementación de la herramienta de acceso al SAI en el adaptador de conexión | Feature | Must | 8 | ADR-01 | BT-14, BT-13 | El adaptador de conexión dialoga con el SAI a través de la herramienta de acceso al SAI; no se escribe traductor propio; la prueba de conexión real desde el panel devuelve descriptores |
| BT-16 | Políticas de apagado versionadas con techo duro de 540 s | Feature | Must | 5 | NB-07, RN-04, F-10 | BT-11 | Cada cambio crea una versión inmutable sin editar la vigente; se valida el techo de 540 s (RN-04); toda acción posterior queda ligada a su versión de política |

### EP-05 — Monitoreo, planificador y salud

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-17 | Planificador (hosted service) con cadencia configurable | Feature | Must | 8 | Componente Planificador (Arquitectura) | BT-15 | Rondas de sondeo a intervalo configurable (5 s por defecto); temporizadores con cancelación; eleva la cadencia a 1 Hz durante la prueba de batería; latencia de decisión < 1 s |
| BT-18 | Persistencia de `Muestra` con calidad y agregación con min/max | Feature | Must | 8 | ADR-08, F-19 | BT-17, BT-08 | Muestra con calidad completa/parcial/perdida; agregado horario conserva mínimo y máximo además del promedio y viaja con cobertura (I-20); retención 30 d muestra / 10 a agregado / eventos indefinidos |
| BT-19 | Reglas de derivación versionadas de eventos | Feature | Must | 5 | CU-04 | BT-18 | Los eventos (microcorte, corte, retorno, desconexión USB, tensión fuera de rango) se derivan por regla versionada; cada evento guarda `reglaDerivacionId` y versión; una sola muestra puede disparar un evento |
| BT-20 | Disparo del apagado por tiempo en batería y tensión, sin flag LB | Feature | Must | 5 | ADR-12 | BT-19 | El disparo no depende del flag LB ni del runtime; decide por tiempo en batería más `battery.voltage`; prueba con serie sintética que nunca enciende LB dispara igual |
| BT-21 | Método de salud por tendencia de la caída de tensión | Feature | Must | 8 | ADR-13 | BT-18 | Salud como tendencia relativa de la caída a carga igualada vs. línea base; sin magnitudes cuantitativas; veredicto con confianza (baja hasta ≥4 pruebas comparables) y reserva por falta de sensor de temperatura |

### EP-06 — Apagado, verificación y ciclo de vida

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-22 | Modalidad CicloForzado (no cancelar el corte aunque vuelva la red) | Feature | Must | 5 | ADR-09 | BT-16, BT-20 | Iniciada la secuencia, el corte del SAI no se cancela aunque vuelva la red; no se usa `shutdown.stop`; prueba con retorno de energía durante la cuenta regresiva no aborta el ciclo |
| BT-23 | Arranque seguro y bloqueo por verificación | Feature | Must | 8 | ADR-10 | BT-12, BT-16 | Arranca forzado en SoloAlerta; con un supuesto en NuncaVerificado/Vencido/Refutado la acción queda BloqueadaPorVerificacion y degrada a SoloAlerta; el panel lo muestra en la pantalla principal |
| BT-24 | Validación por efecto observado y detección de pérdida de comunicación | Feature | Must | 5 | ADR-11 | BT-17 | Ninguna acción se da por ejecutada por ausencia de excepción; se confirma por efecto observado; 3 sondeos sin respuesta generan DesconexionUsb y alerta visual |
| BT-25 | Verificación del ajuste de BIOS por comportamiento (cruce con los registros de arranque del sistema operativo) | Feature | Must | 5 | ADR-14 | BT-24, BT-23 | El autoencendido no se lee por software; se verifica por comportamiento en la ventana de mantenimiento y se renueva cruzando eventos propios contra los registros de arranque del sistema operativo; una prueba fallida deja el supuesto Refutado |
| BT-26 | Registro de intervenciones con cuadre de costos y `Dinero` con moneda y fecha | Feature | Must | 8 | RN-07, RN-08, F-14 | BT-12 | `Costos.cuadra()` valida total = repuestos + mano de obra (RN-08); todo importe lleva moneda y fecha (RN-07); un recambio cierra la vigencia vieja y abre la nueva en un solo acto |
| BT-27 | Ficha de vida útil con costo por año normalizado a USD | Feature | Should | 5 | NB-06, F-23 | BT-26 | La ficha proyecta días en servicio, cumplimiento de expectativa y costo por año de servicio normalizado a USD marcado como derivado con su fuente de cotización |

### EP-07 — Integración, informes y extensibilidad

| BT | Título | Tipo | Prioridad | Estim. | Fuente | Dependencias | Criterios de aceptación |
|---|---|---|---|---|---|---|---|
| BT-28 | Endpoint `POST /api/v1/intervenciones` idempotente | Feature | Must | 8 | ADR-17, Contratos-REST, RN-09 | BT-26 | Cabeceras `X-Idempotency-Key` y `X-Fuente-Datos`; cuatro caminos 201/200/409/422 con el formato de error estándar del contrato REST; misma clave y cuerpo devuelve el mismo id; clave repetida con cuerpo distinto devuelve 409, nunca 200 |
| BT-29 | Adaptador de conexión simulado para pruebas | Devops | Should | 5 | F-24, ADR-02 | BT-14 | Implementación simulada del puerto que permite ejercitar políticas y apagado sin hardware ni riesgo; cubre el camino de prueba de conexión en las pruebas de integración automatizadas |
| BT-30 | Informe de período y comparación de marcas | Feature | Should | 8 | F-22, F-23, CU-12 | BT-12, BT-27 | Interseca intervalos y devuelve dispositivos, cobertura, días con y sin protección, costos por tipo, eventos y calidad de suministro; agrupa fichas cerradas por modelo y calcula costo por año normalizado; los agregados adjuntan cobertura y advertencia |

**Nota sobre extensibilidad (F-26, F-27, US-25, US-26).** La capa de add-ons de dialecto de protocolo y el adaptador de conexión directo sin la herramienta de acceso al SAI quedan **diseñados pero no implementados** en v1. Su superficie técnica es el puerto del adaptador de tres implementaciones de **BT-14** (ADR-02): las historias Could US-25 y US-26 consumen ese contrato como punto de extensión, sin abrir una BT de implementación en la primera entrega.

## 3. Trazabilidad BT↔US↔CU

Para cada BT: las US que la consumen y los CU upstream. Las BT sin US consumidora se justifican como infraestructura compartida con su ADR.

| BT | US consumidoras | CU upstream | Justificación si no hay US |
|---|---|---|---|
| BT-01 | US-03, US-07 | CU-02, CU-04 | — |
| BT-02 | US-07 | CU-04 | Infra compartida (ADR-20): habilita todo acceso por la LAN |
| BT-03 | US-22 | CU-11 | — |
| BT-04 | US-03, US-14 | CU-02, CU-05 | Infra del adaptador (ADR-22): habilita BT-14 |
| BT-05 | US-01, US-02, US-07 | CU-01, CU-04 | Infra compartida (ADR-15): esqueleto de toda la solución |
| BT-06 | US-07 | CU-04 | — |
| BT-07 | US-01, US-04, US-08 | CU-01, CU-02, CU-04 | Infra compartida (ADR-18): persistencia de todo el sistema |
| BT-08 | US-08, US-10 | CU-04, CU-06 | — |
| BT-09 | US-08, US-11 | CU-04, CU-06 | — |
| BT-10 | US-01, US-02 | CU-01 | — |
| BT-11 | US-04, US-18, US-20 | CU-02, CU-08, CU-09 | — |
| BT-12 | US-04, US-18, US-20, US-23 | CU-02, CU-08, CU-09, CU-12 | — |
| BT-13 | US-03 | CU-02 | — |
| BT-14 | US-03, US-07, US-14, US-25, US-26 | CU-02, CU-04, CU-05 | — |
| BT-15 | US-03, US-07, US-08 | CU-02, CU-04 | — |
| BT-16 | US-06, US-14 | CU-03, CU-05 | — |
| BT-17 | US-07, US-08, US-09, US-12, US-14 | CU-04, CU-05, CU-07 | — |
| BT-18 | US-08, US-11 | CU-04, CU-06 | — |
| BT-19 | US-09, US-11 | CU-04, CU-06 | — |
| BT-20 | US-14 | CU-05 | — |
| BT-21 | US-13 | CU-07 | — |
| BT-22 | US-14 | CU-05 | — |
| BT-23 | US-05, US-15 | CU-02, CU-05, CU-10 | — |
| BT-24 | US-09, US-15 | CU-04, CU-05, CU-10 | — |
| BT-25 | US-16, US-17 | CU-10, CU-05 | — |
| BT-26 | US-18, US-20, US-21 | CU-08, CU-09, CU-11 | — |
| BT-27 | US-19, US-24 | CU-08, CU-12 | — |
| BT-28 | US-21, US-22 | CU-11 | — |
| BT-29 | US-12, US-14, US-15, US-16 | CU-05, CU-07, CU-10 | — |
| BT-30 | US-23, US-24 | CU-12 | — |

**Cobertura.** Las 26 US están cubiertas por al menos una BT y los 12 CU aparecen como upstream de al menos una BT. Ninguna BT queda sin US consumidora ni sin justificación de infraestructura.

---

## Control de cambios

| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial del backlog técnico (30 BT inline, matriz BT↔US↔CU). |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D (BT-01, BT-04, BT-10, BT-13, BT-15, BT-25, BT-28 y nota de extensibilidad). Corrección de fuente upstream de BT-06 (03 Experiencia-De-Uso) y BT-19 (CU-04). |
