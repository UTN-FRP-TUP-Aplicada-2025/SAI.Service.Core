# Matriz de cobertura de pruebas — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Matriz-Cobertura-Pruebas-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

---

## 1. Propósito y alcance

Documento bisagra de la categoría 08: relaciona en tablas explícitas los casos de uso (CU-01..CU-12), los requerimientos no funcionales (N-01..N-25), las reglas de negocio (RN-01..RN-13) y los invariantes de dominio (I-1..I-21) con los casos de prueba `TC-XX` del catálogo `Casos-Prueba-Referenciales-v1.0.md`. Su objetivo es responder, para cada requisito, qué prueba lo valida y con qué herramienta; y para cada capa de la arquitectura, qué cobertura mínima se exige.

Alcance: la totalidad del backlog funcional (12 CU) y no funcional (25 NFR) de Sai-Service-Core, más las 13 reglas de negocio y los 21 invariantes bloqueantes del dominio. El estado de cada test es **pendiente** en esta emisión, porque el dominio aún no está implementado (riesgo R-10). La matriz se actualiza al cierre de cada sprint.

Referencias upstream: 02 (`Casos-De-Uso/`, `Reglas-De-Negocio/`), 05 (`Arquitectura-Solucion-v1.0.md` §8, `Extensibilidad-v1.0.md`), intake §17.P.6, §17.P.10, §20, §21.

---

## 2. Trazabilidad CU↔Tests

Cada CU con el criterio Given-When-Then que ejercita el test, el `TC-XX` que lo cubre, el tipo y el estado. Los criterios completos viven en `02-Especificacion-Funcional/Casos-De-Uso/CU-XX...§8`.

| CU | Criterio Given-When-Then (resumen del §8 del CU) | Test ID | Tipo | Estado |
| --- | --- | --- | --- | --- |
| CU-01 | Given primer arranque sin cuenta, When el admin se da de alta, Then crea la cuenta única y abre sesión; alta repetida ⇒ `ADMIN_YA_EXISTE`; credencial errónea ⇒ `CREDENCIAL_INVALIDA` | TC-30, TC-38 | Integration / E2E | Pendiente |
| CU-02 | Given dispositivo descubierto sin serie y modelo de batería sin temperatura de referencia, When confirma el alta, Then registra sin serie, siembra 4 verificaciones en `NuncaVerificado` y rechaza el modelo sin temperatura (`VIDA_FLOTACION_SIN_TEMPERATURA`) | TC-31, TC-21, TC-38 | Integration / Unit / E2E | Pendiente |
| CU-03 | Given política vigente 1, When crea versión 2 con `tiempoReservado 240`, Then versiona sin editar; `600` ⇒ `TIEMPO_APAGADO_EXCEDE_TECHO` | TC-32, TC-10, TC-13 | Integration / Unit | Pendiente |
| CU-04 | Given sondeo a 5 s con equipo en línea a 232,9 V, When corre la ronda, Then persiste muestra `completa`, `battery.charge` `derivado`, y a 3 sondeos fallidos ⇒ `PERDIDA_COMUNICACION` | TC-33, TC-07, TC-08, TC-14, TC-39 | Integration / Unit / E2E | Pendiente |
| CU-05 | Given modalidad con retorno y 3 de 4 supuestos sin verificar, When decide a los 300 s en batería, Then `BloqueadaPorVerificacion`, modalidad efectiva `SoloAlerta`, host no se apaga; con 4 de 4 verificados, apaga con retorno por efecto observado | TC-26, TC-27, TC-11, TC-13, TC-39 | Integration / Unit / E2E | Pendiente |
| CU-06 | Given período con muestras/agregados, When grafica, Then distingue muestra de agregado con `cobertura 0,987` y advertencia; período sin datos ⇒ `PERIODO_SIN_DATOS` | TC-34, TC-20, TC-14 | Integration / Unit | Pendiente |
| CU-07 | Given prueba comparable con 2 puntos, When emite veredicto, Then `SinDegradacionDetectable`, `confianza baja`, `calculadoPor sai-service`, `caidaV -0,47 V`; carga distinta ⇒ `comparable false` | TC-28, TC-09, TC-15, TC-16, TC-17 | Integration / Unit | Pendiente |
| CU-08 | Given `mnt-001` vigente, When registra el recambio, Then cierra y abre sin hueco, baja lógica de `bat-2024-a` consultable, ficha con costo por año en USD; costos que no cuadran ⇒ `COSTOS_NO_CUADRAN` | TC-29, TC-03, TC-05, TC-18 | Integration / Unit | Pendiente |
| CU-09 | Given `cob-001` vigente, When `ups-01` sale a reparación y `ups-02` cubre, Then sucesión sin solape, días sin protección calculados, verificaciones reiniciadas por cambio de modelo | TC-35, TC-04 | Integration / Unit | Pendiente |
| CU-10 | Given 4 supuestos sin verificar, When completa la ventana y el host arranca solo, Then 4 de 4 `Verificado` y modalidad efectiva; si no arranca ⇒ `SUPUESTO_REFUTADO` (bloqueo permanente); vencido ⇒ repetir | TC-36, TC-11, TC-12 | Integration / Unit | Pendiente |
| CU-11 | Given intervención con clave nueva, When se envía, Then `201 creado media`; reintento ⇒ `200 creado:false`; cuerpo distinto ⇒ `409`; invariante roto ⇒ `422` | TC-22, TC-23, TC-24, TC-25, TC-19, TC-18 | Integration / Unit | Pendiente |
| CU-12 | Given período 2026 con recambio, When pide el informe, Then intervalos recortados suman 365 días sin solape, baja lógica visible, `cobertura 0,987` con advertencia, costo por año a `29,50 USD` derivado | TC-37, TC-05, TC-20 | Integration / Unit | Pendiente |

Los 12 CU tienen al menos un TC. Ningún CU queda huérfano.

---

## 3. Trazabilidad NFR↔Tests

Cada NFR con objetivo numérico, el test o mecanismo de medición que lo valida y su tooling. Los NFR marcados **PENDIENTE** (N-03, N-20, N-25) no tienen objetivo numérico cerrado en las fuentes y se detallan en §6 (gaps). Todos los NFR con objetivo numérico cerrado tienen test o mecanismo de medición asociado.

| NFR | SLA / objetivo numérico | Test / mecanismo | Tooling de medición |
| --- | --- | --- | --- |
| N-01 | `ups.delay.shutdown` ≤ 540 s (techo duro) | TC-10 (I-10); TC-32 (rechazo en el formulario) | xUnit + FluentAssertions |
| N-02 | `shutdown-timeout` del demonio Docker = 150 s de los 540 | Medición cronometrada del grace del contenedor con VM huésped; evidencia registrada en CU-10 (`ver-presupuesto-apagado`) | Cronómetro + script `ver-presupuesto-apagado` (stage de medición en 09) |
| N-03 | Resto del apagado del SO — **PENDIENTE de medición** | Cronometrado en la ventana de mantenimiento (CU-10); ver §6 | `ver-presupuesto-apagado` (evidencia de `Verificacion`) |
| N-04 | `ups.delay.start` = 180 s (rango 60–599940 s) | TC-27 (lectura/uso de la variable contra el adaptador simulado) | xUnit + WebApplicationFactory (adaptador simulado) |
| N-05 | Umbral de disparo = 300 s de partida, ajustable por política | TC-26 (lógica de temporizador a 300 s); TC-32 (config por versión) | xUnit + reloj inyectado |
| N-06 | Latencia de decisión < 1 s por ronda (intervalo 5 s) | TC-33 (mide la duración de la ronda) + métrica de duración logueada por el hosted service | Stopwatch en test de integración + log estructurado |
| N-07 | Intervalo de sondeo normal = 5 s (configurable) | TC-33 (cadencia real de la ronda) | xUnit + WebApplicationFactory + reloj inyectado |
| N-08 | Cadencia durante prueba de batería = 1 Hz, restaurada al terminar | TC-28 (densidad de muestreo de la `PruebaBateria`) | xUnit + adaptador simulado |
| N-09 | 3 sondeos consecutivos sin respuesta ⇒ `DesconexionUsb` + alerta | TC-33 (contador de sondeos fallidos) | xUnit + WebApplicationFactory |
| N-10 | `input.voltage` en [198, 242] V; fuera 30 s ⇒ `TensionFueraDeRango` | TC-14 (`ReglaDerivacion` versionada) + test unitario de derivación de tensión | xUnit + FluentAssertions |
| N-11 | Microcorte < 60 s entre OL→OB y OB→OL (regla v2) | TC-14 (normalización de umbral por versión de regla) | xUnit + FluentAssertions |
| N-12 | `battery.voltage` < 13,3 V ⇒ celda en corto; > 14,5 V ⇒ celda abierta | Test unitario de la regla de alarma sobre la muestra (umbrales); fixture `§20.E-6` (12,71 V < 13,3) | xUnit + FluentAssertions |
| N-13 | Vigencia de `ver-presupuesto-apagado` = 180 días | TC-12 (vencimiento por vigencia); TC-36 | xUnit + reloj inyectado |
| N-14 | Vigencia de `ver-bios-autoencendido` y `ver-flag-ob` = 365 días | TC-12; TC-36 | xUnit + reloj inyectado |
| N-15 | Vigencia de `ver-shutdown-return` = sin caducidad | TC-12 (nunca vence); TC-36 | xUnit + reloj inyectado |
| N-16 | Tendencia de salud: ≥ 4 pruebas comparables; con menos, confianza `baja` | TC-28 (confianza `baja` con 2 puntos); TC-16 | xUnit + FluentAssertions |
| N-17 | Cadencia de prueba de batería programada = trimestral | TC-28 (disparo `programado`) + test de programación del planificador | xUnit + reloj inyectado |
| N-18 | Volumen ≈ 6,3 M filas/año (720 muestras/hora) | TC-34 (agregación y retención) + medición de conteo de filas bajo carga simulada | xUnit + SQLite físico + carga simulada |
| N-19 | Retención: Muestra `P30D`, Agregado `PT1H` durante `P10Y`, Evento indefinido | TC-34 (job de agregación y descarte) | xUnit + SQLite físico + reloj inyectado |
| N-20 | Tamaño máximo del archivo SQLite tras la agregación — **PENDIENTE** | Medición del archivo bajo carga simulada antes de producción; ver §6 | Medición de tamaño de archivo (stage de medición en 09) |
| N-21 | Cobertura de la solución ≥ 80 % líneas / 70 % ramas (bloqueante) | Quality gate de cobertura en CI (stage 4) sobre la suite completa | Coverlet + reportgenerator (gate en 09) |
| N-22 | Cobertura de `SAI.Service.Core.Domain` ≥ 90 % líneas / 85 % ramas (bloqueante) | Quality gate de cobertura en CI (stage 3) sobre el dominio | Coverlet + reportgenerator (gate en 09) |
| N-23 | Idempotencia de la ingesta = 100 % (misma clave + mismo cuerpo ⇒ mismo id) | TC-22, TC-23, TC-24, TC-25 (cuatro caminos) + TC-19 (I-19) | xUnit + WebApplicationFactory |
| N-24 | Procedencia declarada en todo valor = 0 valores sin `Origen` | TC-07 (I-7) | xUnit + FluentAssertions |
| N-25 | SLO de disponibilidad del servicio — **PENDIENTE** (propuesto ≥ 0,99 mensual) | Monitoreo SLO en 09 (ratio de rondas completadas/esperadas); no es test unitario; ver §6 | Métrica observada (monitoreo, no test) |

Confirmación: cada NFR con objetivo numérico cerrado (todos salvo N-03, N-20 y N-25) tiene un test o mecanismo de medición asociado con su tooling. Los tres PENDIENTE se registran como gaps en §6, según lo previsto por las fuentes (§17.P.10 los declara sin dimensionar).

---

## 4. Trazabilidad RN↔Tests e invariantes↔Tests

### 4.1 Reglas de negocio (RN-01..RN-13)

| RN | Enunciado | TC | Tipo |
| --- | --- | --- | --- |
| RN-01 | Arranque seguro en solo aviso | TC-31, TC-36, TC-38 | Integration / E2E |
| RN-02 | Bloqueo por verificación y degradación de modalidad | TC-11, TC-26, TC-36, TC-39 | Unit / Integration / E2E |
| RN-03 | Validación por efecto observado | TC-27, TC-33, TC-40 | Integration / Contract |
| RN-04 | Techo duro del tiempo reservado de apagado (540 s) | TC-10, TC-26, TC-32 | Unit / Integration |
| RN-05 | Procedencia obligatoria y origen declarado de todo valor | TC-07, TC-08, TC-17, TC-33 | Unit / Integration |
| RN-06 | Aptitud de datos para la tendencia de salud | TC-09, TC-16, TC-28 | Unit / Integration |
| RN-07 | Todo importe con moneda y fecha | TC-18, TC-25, TC-37 | Unit / Integration |
| RN-08 | Cuadre de costos de una intervención | TC-25 | Integration |
| RN-09 | Idempotencia de la ingesta externa | TC-19, TC-23, TC-24 | Unit / Integration |
| RN-10 | Agregado con cobertura y advertencia obligatorias | TC-20, TC-34, TC-37 | Unit / Integration |
| RN-11 | Acción referida a una versión de política | TC-13, TC-26, TC-32 | Unit / Integration |
| RN-12 | Baja lógica y coherencia temporal de las intervenciones | TC-05, TC-25, TC-35 | Unit / Integration |
| RN-13 | Vida de flotación esperada con temperatura de referencia | TC-21, TC-31 | Unit / Integration |

### 4.2 Invariantes de dominio (I-1..I-21)

Cada invariante tiene exactamente un TC unitario que lo verifica en `SAI.Service.Core.Domain` (escrito antes de codificar, R-10). La regla conceptual y el escenario de respaldo se citan para la trazabilidad.

| Invariante | TC | Escenario | RN/RC relacionada |
| --- | --- | --- | --- |
| I-1 Montajes sin solape por (dispositivo, posición) | TC-01 | E-1, E-6 | RC-02, RC-03 |
| I-2 A lo sumo un montaje vigente | TC-02 | E-1 | RC-02 |
| I-3 Cierre y apertura sin hueco | TC-03 | E-6 | RC-03 |
| I-4 Ídem para `CoberturaHost` por host | TC-04 | E-1, E-7 | RC-02 |
| I-5 Entidad dada de baja sigue consultable | TC-05 | E-6, E-7 | RC-08, RN-12 |
| I-6 Transición de estado no salta pasos | TC-06 | E-6 | RC-08 |
| I-7 Todo valor tiene origen | TC-07 | E-2, E-5 | RC-01, RN-05 |
| I-8 Valor derivado declara `de` | TC-08 | E-2 | RC-01, RN-05 |
| I-9 Tendencia rechaza procedencia no medida | TC-09 | E-2 | RN-06 |
| I-10 `tiempoReservadoApagadoSeg ≤ 540` | TC-10 | E-4 | RN-04 |
| I-11 Incumplimiento ⇒ `SoloAlerta` + `BloqueadaPorVerificacion` | TC-11 | E-4 | RN-01, RN-02 |
| I-12 Verificación vence sola | TC-12 | E-4 | RN-02 |
| I-13 Acción referida a versión, no política | TC-13 | E-4 | RC-05, RN-11 |
| I-14 Evento referido a regla versionada | TC-14 | E-3, E-4 | RC-09 |
| I-15 Prueba congela `montajeBateriaId` | TC-15 | E-5 | RC-07 |
| I-16 Prueba no comparable no entra en tendencia | TC-16 | E-5 | RN-06 |
| I-17 Muestra perdida no rompe derivados | TC-17 | E-5 | RN-05 |
| I-18 Dinero con moneda y fecha | TC-18 | E-6, E-7, E-8 | RN-07 |
| I-19 Idempotencia por clave sin duplicar | TC-19 | E-8 | RN-09 |
| I-20 Agregado con cobertura y advertencia | TC-20 | E-7 | RC-04, RN-10 |
| I-21 Vida de flotación exige temperatura de referencia | TC-21 | E-1 | RN-13 |

Los 21 invariantes están cubiertos; las 13 RN tienen al menos un TC.

---

## 5. Cobertura por capa

Umbrales mínimos bloqueantes en el pipeline. **Fuente única de estos umbrales: `Estrategia-Testing-v1.0.md §2` (cobertura por capa)**; esta tabla los reproduce para la trazabilidad, pero no los redefine: ante cualquier discrepancia, prevalece la estrategia de testing. El `Actual` figura como pendiente porque el dominio aún no está implementado (R-10); los valores objetivo son piso, no techo.

| Capa | Líneas objetivo | Branches objetivo | Mutation score | Actual (líneas / branches) | Umbral mínimo |
| --- | --- | --- | --- | --- | --- |
| `SAI.Service.Core.Domain` | ≥ 90 % | ≥ 85 % | — (no exigido para web-monolith) | Pendiente / Pendiente | 90 / 85 |
| `SAI.Service.Core.Application` | ≥ 80 % | ≥ 70 % | — | Pendiente / Pendiente | 80 / 70 |
| `SAI.Service.Core.Infrastructure` | ≥ 70 % | ≥ 60 % | — | Pendiente / Pendiente | 70 / 60 |
| `SAI.Service.Core.Api` | ≥ 80 % | ≥ 70 % | — | Pendiente / Pendiente | 80 / 70 |
| `SAI.Service.Core.Web` (presentación) | ≥ 60 % | ≥ 50 % | — | Pendiente / Pendiente | 60 / 50 |

Notas:
- La asimetría del dominio (90/85) es deliberada: es donde viven las decisiones irreversibles (apagado). La capa `Api` lleva su propio piso 80/70 —no el de presentación— por ser frontera del contrato crítico de ingesta. El conjunto de la solución cae bajo el gate global 80/70 (N-21).
- La cobertura se reporta por capa, nunca como número global único (anti-patrón §4.10 de las reglas).
- Presentación: además del porcentaje, el panel se cubre con bUnit (componentes) y Playwright (recorridos UF-1, UF-3).

---

## 6. Gaps identificados y plan de remediación

| Gap | Descripción | Impacto | Plan de remediación |
| --- | --- | --- | --- |
| F-3 no automatizable | El flujo F-3 (ciclo físico completo de apagado y reencendido) corresponde a **CU-05 (ejecución del apagado ordenado)** y no se puede probar solo con software: exige cortar la energía real (T-08). Su verificación física ocurre en la **ventana de mantenimiento CU-10**. | Alto (camino crítico irreversible) | El adaptador simulado cubre la lógica de CU-05 (TC-26, TC-27, TC-36); el comportamiento real del firmware y la BIOS se verifica en la ventana de mantenimiento (CU-10) y se registra como evidencia de una `Verificacion`, no como test. Se documenta el límite en la Matriz-Sensado-Deriva y en la DoD de release. |
| N-03 sin medir | Resto del apagado del sistema operativo: no dimensionado en la fuente; se cronometra en la ventana de mantenimiento. | Medio (define el presupuesto de 540 s) | Cronometrar en CU-10 con la VM huésped; registrar en `ver-presupuesto-apagado` (vigencia 180 días). Sprint 0 / primera ventana de mantenimiento. |
| N-20 sin dimensionar | Tamaño máximo del archivo SQLite tras la agregación: no dimensionado (riesgo R-07). | Bajo | Medir el archivo bajo carga simulada de ~6,3 M filas/año antes de producción; validar la agregación/retención (TC-34). Antes del primer despliegue productivo. |
| N-25 SLO sin definir | SLO de disponibilidad del propio servicio: no está en la fuente; propuesto «rondas completadas / esperadas ≥ 0,99 mensual» [derivado]. | Bajo | Definir e instrumentar el ratio de rondas en 09 como monitoreo (no test unitario). El servicio corre en el mismo host que protege, así que su disponibilidad está acotada por la del host. |
| Mutation testing | No exigido para web-monolith (§2.2 de las reglas), pero el dominio irreversible se beneficiaría. | Bajo | Evaluar mutation testing sobre `Domain` como mejora opcional en un sprint posterior; no bloqueante. |
| Endpoint de rectificación (409) | La respuesta 409 sugiere un endpoint de rectificación cuyo contrato no está definido (pendiente P-05 de CU-11). | Bajo | Hasta cerrar el contrato, TC-24 valida el 409 y su `accionSugerida`; el flujo de rectificación se testeará cuando se defina el endpoint. |

---

## 7. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Matriz inicial con las cuatro tablas obligatorias: CU↔Tests (12 CU), NFR↔Tests (25 NFR, cada uno numérico con test o mecanismo), RN↔Tests (13 RN) e invariantes↔Tests (21), más cobertura por capa y 6 gaps con plan de remediación (F-3 no automatizable, N-03, N-20, N-25, mutation, endpoint de rectificación). |
| 1.1 | 2026-07-21 | Corrección de conformidad: unificación de la tabla de cobertura por capa como fuente única tras audit de Fase E. `SAI.Service.Core.Infrastructure` corregida de 80/70 a 70/60; `Api` se mantiene en 80/70 (capa de frontera con contrato crítico); se remite a `Estrategia-Testing-v1.0.md §2` como fuente única. F-3 atribuido a CU-05 con verificación física en CU-10, de forma consistente con la estrategia de testing. |
