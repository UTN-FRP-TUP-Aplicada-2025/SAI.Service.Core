# Casos de prueba referenciales — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Casos-Prueba-Referenciales-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

---

## 0. Propósito y alcance

Catálogo de casos de prueba referenciales `TC-XX` de Sai-Service-Core. Cada `TC` es un artefacto de software con trazabilidad explícita a un caso de uso (CU-01..CU-12), a una regla de negocio (RN-01..RN-13) o a un requerimiento no funcional (N-01..N-25), con setup determinista tomado de las fixtures de los escenarios `§20.E-1..E-8` del SOLUTION-INTAKE y con un expected verificable. Ningún dato de expected se inventa: sale verbatim de los escenarios o de la especificación funcional.

La pirámide objetivo del proyecto es **70 % unitarias / 25 % integración / 5 % end-to-end** (§17.P.6 del intake; `web-monolith` de §2.2 de las reglas). El tooling por nivel:

| Nivel | Alcance | Tooling |
| --- | --- | --- |
| Unitarias | `Domain` y `Application`: los 21 invariantes I-1..I-21, `ResolutorTemporal`, `Costos.cuadra()`, `Intervencion.aplicar()`, máquina de estados de `UnidadFisica`, derivados de prueba de batería, degradación de modalidad | xUnit + FluentAssertions |
| Integración | `Infrastructure`: EF Core contra SQLite en archivo temporal (migraciones incluidas), adaptador NUT contra el adaptador simulado, API de ingesta con sus cuatro caminos | xUnit + WebApplicationFactory + SQLite físico |
| End-to-end | Flujos de usuario críticos sobre el panel, contra el adaptador simulado: UF-1, UF-3 y el camino de apagado de UF-8 | bUnit (componentes Blazor); Playwright (recorridos de panel) |
| Contrato | Puerto del adaptador de conexión y sus implementaciones | xUnit + suite de contrato compartida (ver `Guia-Testing-Extensibilidad-v1.0.md`) |

**Convención de `status`:** en esta emisión, el modelo de datos y el dominio aún no están implementados (riesgo R-10: «los invariantes son hipótesis hasta que existan como pruebas»). Por eso el `Actual output` y el `Status` de todos los casos figuran como **pendiente**. La matriz se actualiza al cierre de cada sprint (§4.10 de las reglas, anti-patrón «matriz desactualizada»).

**Determinismo:** todos los casos son deterministas, reproducibles y sin dependencia del orden de ejecución. Las fixtures se cargan desde los escenarios `§20.E-x` (identificadores legibles `ups-01`, `bat-2024-a`, `mnt-001`). Los tiempos se inyectan (reloj controlado), nunca se leen del reloj de pared.

---

## 1. Casos de invariante (unit, `SAI.Service.Core.Domain`)

Los 21 invariantes I-1..I-21 escritos como prueba antes de la primera línea de implementación (§21.B.2 del intake, R-10). Tipo: **unit**. Tooling: xUnit + FluentAssertions.

### TC-01 · Montaje-Sin-Solape-Por-Dispositivo-Y-Posicion

- Tipo: unit
- Cubre: I-1; RC-02, RC-03; CU-08
- Setup: fixture de montajes de `§20.E-1`/`§20.E-6` para `(ups-01, posicion 1)`.
- Given dos `MontajeBateria` para el mismo `(dispositivoId, posicion)`, When se intenta persistir un intervalo que solapa a otro, Then el dominio rechaza la operación por violación de invariante `validacion`.
- Expected: para `mnt-001` [2024-11-20 → 2026-09-05] y un montaje que abra antes del cierre de `mnt-001`, la validación falla; los intervalos no solapados se aceptan.
- Actual: pendiente. Status: pendiente.

### TC-02 · A-Lo-Sumo-Un-Montaje-Vigente

- Tipo: unit
- Cubre: I-2; RC-02; CU-08
- Setup: `§20.E-1`, `mnt-001` con `hasta = null`.
- Given un montaje vigente (`hasta = null`) en `(ups-01, posicion 1)`, When se intenta abrir un segundo montaje vigente en la misma posición, Then el dominio lo rechaza: a lo sumo un montaje con `hasta = null` por `(dispositivoId, posicion)`.
- Expected: se permite exactamente un montaje abierto; el segundo abierto se rechaza.
- Actual: pendiente. Status: pendiente.

### TC-03 · Cierre-Y-Apertura-Sin-Hueco

- Tipo: unit
- Cubre: I-3; RC-03; CU-08
- Setup: `§20.E-6`, cierre de `mnt-001` y apertura de `mnt-002` por `int-20260905-001`.
- Given un montaje que se cierra en el instante `2026-09-05T10:30:00-03:00`, When se abre el siguiente en el mismo instante, Then `mnt-001.hasta == mnt-002.desde` y no queda hueco temporal.
- Expected: `mnt-001.hasta = 2026-09-05T10:30:00-03:00` y `mnt-002.desde = 2026-09-05T10:30:00-03:00`; igualdad exacta.
- Actual: pendiente. Status: pendiente.

### TC-04 · Cobertura-Host-Sin-Solape-Ni-Doble-Vigente

- Tipo: unit
- Cubre: I-4; RC-02; CU-09
- Setup: `§20.E-1`/`§20.E-7`, `CoberturaHost` de `host-i7infra`.
- Given coberturas de host por `hostId`, When se persisten, Then rige lo mismo que I-1/I-2: sin solape y a lo sumo una vigente (`hasta = null`).
- Expected: una cobertura vigente por host; solapamientos rechazados.
- Actual: pendiente. Status: pendiente.

### TC-05 · Entidad-Dada-De-Baja-Sigue-Consultable

- Tipo: unit
- Cubre: I-5; RC-08; CU-08, CU-12
- Setup: `§20.E-6`, `bat-2024-a` en estado `DadoDeBaja` con motivo `FinDeVidaUtil`.
- Given una batería `DadoDeBaja`, When se consulta su historial, Then la consulta la devuelve con sus 8 pruebas; ninguna consulta histórica la excluye.
- Expected: `bat-2024-a` consultable tras la baja; sus pruebas siguen accesibles.
- Actual: pendiente. Status: pendiente.

### TC-06 · Transicion-De-Estado-No-Salta-Pasos

- Tipo: unit
- Cubre: I-6; RC-08; CU-08
- Setup: máquina de estados de `UnidadFisica` (diagrama §7.8), fixture `§20.E-6`.
- Given una `UnidadFisica`, When se intenta una transición que salta pasos del diagrama, Then el dominio la rechaza; solo se aceptan las transiciones válidas (p. ej. `EnStock → EnServicio`, `EnServicio → DadoDeBaja`).
- Expected: transición `EnStock → EnServicio` de `bat-2026-a` válida; una transición ilegal falla.
- Actual: pendiente. Status: pendiente.

### TC-07 · Todo-Valor-Tiene-Origen

- Tipo: unit
- Cubre: I-7; RC-01, RN-05; N-24; CU-04
- Setup: `§20.E-2`/`§20.E-5`, `Valor<T>` con `Origen`.
- Given cualquier valor almacenado, When se construye sin `Origen`, Then el tipo `Valor<T>` no permite instanciarlo: 0 valores sin `Origen`, sin excepción.
- Expected: `input.voltage 232,9 V` lleva `o: medido`; un valor sin origen es inconstruible.
- Actual: pendiente. Status: pendiente.

### TC-08 · Valor-Derivado-Declara-De-Que-Deriva

- Tipo: unit
- Cubre: I-8; RC-01, RN-05; CU-04
- Setup: `§20.E-2`, `battery.charge` `derivado` con `de: [battery.voltage, battery.voltage.high, battery.voltage.low]`.
- Given un valor con `o: derivado`, When se construye, Then declara `de` con al menos una variable de la que deriva.
- Expected: `battery.charge` derivado con `de` no vacío; un derivado sin `de` se rechaza.
- Actual: pendiente. Status: pendiente.

### TC-09 · Tendencia-De-Salud-Rechaza-Procedencia-No-Medida

- Tipo: unit
- Cubre: I-9; RN-06; CU-07
- Setup: `§20.E-2`, `aptoParaTendenciaDeSalud()`.
- Given un valor con procedencia `derivado`, `estimadoPorDriver` o `imputado`, When se lo ofrece a la tendencia de salud, Then `aptoParaTendenciaDeSalud()` devuelve `false` y la entrada queda excluida.
- Expected: `false` para las tres procedencias; `true` solo para `medido`.
- Actual: pendiente. Status: pendiente.

### TC-10 · Techo-Duro-Del-Tiempo-Reservado-De-Apagado

- Tipo: unit
- Cubre: I-10; RN-04; N-01; CU-03, CU-05
- Setup: `§20.E-4`, `VersionPolitica.tiempoReservadoApagadoSeg`.
- Given una versión de política, When `tiempoReservadoApagadoSeg > 540`, Then el dominio la rechaza; `240` se acepta, `600` se rechaza.
- Expected: `240 s` válido; `600 s` viola I-10 (techo duro de `ups.delay.shutdown`).
- Actual: pendiente. Status: pendiente.

### TC-11 · Bloqueo-Por-Verificacion-Degrada-A-Solo-Alerta

- Tipo: unit
- Cubre: I-11; RN-01, RN-02; CU-05, CU-10
- Setup: `§20.E-4`, `acc-20260811T042000` con 3 de 4 supuestos sin verificar.
- Given una `Accion` cuya modalidad requiere supuestos y alguno está en `NuncaVerificado`/`Vencido`/`Refutado`, When se evalúa la modalidad efectiva, Then `modalidadEfectiva == "SoloAlerta"` y `resultado == "BloqueadaPorVerificacion"`.
- Expected: `modalidadSolicitada = HostLuegoUpsConRetorno`, `modalidadEfectiva = SoloAlerta`, `resultado = BloqueadaPorVerificacion`.
- Actual: pendiente. Status: pendiente.

### TC-12 · Verificacion-Vence-Sola-Por-Vigencia

- Tipo: unit
- Cubre: I-12; RN-02; N-13, N-14; CU-10
- Setup: `Verificacion` con `ultimaVerificacion` y `vigenciaDias`, reloj inyectado.
- Given una `Verificacion` con `ultimaVerificacion + vigenciaDias < ahora`, When se evalúa su estado, Then pasa a `Vencido` por sí sola.
- Expected: `ver-presupuesto-apagado` (180 días) y `ver-bios-autoencendido` (365 días) vencen al superar su vigencia; `ver-shutdown-return` (sin caducidad) nunca vence.
- Actual: pendiente. Status: pendiente.

### TC-13 · Accion-Referida-A-Version-De-Politica

- Tipo: unit
- Cubre: I-13; RC-05, RN-11; CU-05
- Setup: `§20.E-4`, `acc-20260811T042000` con `versionPoliticaId = vp-003`.
- Given una `Accion`, When se persiste, Then referencia una `VersionPolitica` (`vp-003`), nunca la `Politica` agrupadora (`pol-apagado-por-corte`).
- Expected: la acción apunta a `vp-003`; referenciar la política directa se rechaza.
- Actual: pendiente. Status: pendiente.

### TC-14 · Evento-Referido-A-Regla-Versionada

- Tipo: unit
- Cubre: I-14; RC-09; N-10, N-11; CU-04, CU-06
- Setup: `§20.E-3`/`§20.E-4`, `evt-20260811T041500` con `reglaDerivacionId = rd-transicion-ups-status`, `reglaVersion = 2`.
- Given un `Evento` derivado de muestras, When se construye, Then referencia `reglaDerivacionId` y `reglaVersion`.
- Expected: evento con `reglaVersion = 2`; un evento sin versión de regla se rechaza. Cubre la normalización de umbrales por versión (microcorte < 60 s en v2, rango [198,242] V).
- Actual: pendiente. Status: pendiente.

### TC-15 · Prueba-De-Bateria-Congela-Montaje-Vigente

- Tipo: unit
- Cubre: I-15; RC-07; CU-07
- Setup: `§20.E-5`, `prb-20260901T010000`, `bateriaIdResuelta = bat-2024-a`.
- Given una `PruebaBateria` en un instante, When se ejecuta, Then resuelve por `ResolutorTemporal` y congela `montajeBateriaId = mnt-001` (batería `bat-2024-a`).
- Expected: el `montajeBateriaId` queda fijado en el instante de la prueba, aun si luego cambia el montaje.
- Actual: pendiente. Status: pendiente.

### TC-16 · Prueba-No-Comparable-No-Entra-En-La-Tendencia

- Tipo: unit
- Cubre: I-16; RN-06; N-16; CU-07
- Setup: `§20.E-5`, `deltaCargaConcurrente` y tolerancia.
- Given una `PruebaBateria` con `deltaCargaConcurrente` fuera de tolerancia, When se registra, Then se marca `comparable: false` y no entra en la tendencia de salud.
- Expected: con carga concurrente 13 % igual a la línea base, `comparable: true`; con carga muy distinta, `comparable: false` y excluida.
- Actual: pendiente. Status: pendiente.

### TC-17 · Muestra-Perdida-No-Rompe-Los-Derivados

- Tipo: unit
- Cubre: I-17; RN-05; CU-07
- Setup: `§20.E-5`, muestras `t=9` y `t=10` con `q: perdida` y valores `null`.
- Given una serie con muestras `perdida` de valores `null`, When se calculan los derivados, Then el cálculo no rompe: los derivados se computan con las muestras válidas.
- Expected: con las 2 muestras perdidas en la conmutación, `caidaV = -0,47 V` y `segundosRecuperacion ≈ 35 s (±5 s)` calculados solo con las documentadas.
- Actual: pendiente. Status: pendiente.

### TC-18 · Dinero-Con-Moneda-Y-Fecha

- Tipo: unit
- Cubre: I-18; RN-07; CU-08, CU-11
- Setup: `§20.E-6`/`§20.E-8`, objeto `Dinero`.
- Given un `Dinero`, When se construye sin `moneda` o sin `fecha`, Then es inválido.
- Expected: `67.000 ARS @ 2026-09-05` válido; `{ monto: 12000 }` sin moneda/fecha se rechaza (`I-18`).
- Actual: pendiente. Status: pendiente.

### TC-19 · Idempotencia-Por-Clave-Sin-Duplicar

- Tipo: unit
- Cubre: I-19; RN-09; N-23; CU-11
- Setup: `§20.E-8`, `claveIdempotencia = gmao-ext-ot-88213`.
- Given una `claveIdempotencia` ya procesada, When se reenvía el mismo cuerpo, Then devuelve el registro existente (`int-20261002-001`) sin crear otro.
- Expected: misma clave + mismo cuerpo ⇒ mismo `id`, `creado = false`; no se agrega fila.
- Actual: pendiente. Status: pendiente.

### TC-20 · Agregado-Servido-Con-Cobertura-Y-Advertencia

- Tipo: unit
- Cubre: I-20; RC-04, RN-10; CU-06, CU-12
- Setup: `§20.E-7`, `Agregado` `PT1H` con `cobertura`.
- Given una respuesta que contiene un `Agregado`, When se sirve, Then incluye su `cobertura` y su advertencia de que el promedio no representa microcortes.
- Expected: agregado con `cobertura = 0,987` y advertencia presente; un agregado sin ambos campos no se sirve.
- Actual: pendiente. Status: pendiente.

### TC-21 · Vida-De-Flotacion-Exige-Temperatura-De-Referencia

- Tipo: unit
- Cubre: I-21; RN-13; CU-02
- Setup: `§20.E-1`, `ModeloBateria.vidaFlotacionEsperada`.
- Given una `vidaFlotacionEsperada` (3 a 5 años), When se declara sin `temperaturaReferenciaC`, Then es inválida.
- Expected: modelo `12V 9Ah AGM` sin temperatura de referencia se rechaza (`VIDA_FLOTACION_SIN_TEMPERATURA`); con temperatura, válido.
- Actual: pendiente. Status: pendiente.

---

## 2. Casos de la API de ingesta (integration, CU-11)

Los cuatro caminos del contrato `POST /api/v1/intervenciones` (201/200/409/422), de `§20.E-8`. Tipo: **integration**. Tooling: xUnit + WebApplicationFactory + SQLite físico.

### TC-22 · Ingesta-Camino-Creado-201

- Tipo: integration
- Cubre: CU-11 (CA-01); RN-07; N-23
- Setup: instancia con `WebApplicationFactory`, SQLite temporal, `FuenteDatos` `fd-gmao-externo` registrada.
- Given una intervención de inspección con `X-Idempotency-Key: gmao-ext-ot-88213` y `costos.total = 12.000 ARS @ 2026-10-02`, When se envía por primera vez, Then responde `201` con `creado: true`, `id: int-20261002-001` y `confianzaAsignada: media`.
- Expected: HTTP 201; `creado = true`; confianza `media` (menor que la del poller local).
- Actual: pendiente. Status: pendiente.

### TC-23 · Ingesta-Camino-Idempotente-200

- Tipo: integration
- Cubre: CU-11 (CA-02); RN-09; N-23; I-19
- Setup: continuación de TC-22 (clave ya procesada).
- Given la misma clave `gmao-ext-ot-88213` con el mismo cuerpo, When el emisor reintenta, Then responde `200` con `creado: false` y el mismo `id: int-20261002-001`, sin duplicar.
- Expected: HTTP 200; `creado = false`; mismo `id`; conteo de filas sin cambio.
- Actual: pendiente. Status: pendiente.

### TC-24 · Ingesta-Camino-Conflicto-409

- Tipo: integration
- Cubre: CU-11 (CA-03); RN-09; N-23
- Setup: continuación de TC-22.
- Given la misma clave `gmao-ext-ot-88213` con el cuerpo cambiado (`costos.total.monto: 12000 → 19500`), When se envía, Then responde `409 conflicto_idempotencia` con `huellaOriginal` y `huellaRecibida` (`sha256`) y una `accionSugerida`; nunca `200`.
- Expected: HTTP 409; error `conflicto_idempotencia`; ambas huellas `sha256` presentes.
- Actual: pendiente. Status: pendiente.

### TC-25 · Ingesta-Camino-Invariante-Roto-422

- Tipo: integration
- Cubre: CU-11 (CA-04, CA-05); RN-07, RN-08, RN-12; I-18
- Setup: instancia con `WebApplicationFactory`; `bat-2024-a` dada de baja el 2026-09-05.
- Given tres cuerpos inválidos —(a) repuestos 52.000 + mano de obra 15.000 con `total` 60.000; (b) `costos.total` sin `moneda` ni `fecha`; (c) intervención fechada 2026-11-01 sobre `bat-2024-a` dada de baja—, When se envían, Then responde `422` con el campo y el invariante violado: (a) `validacion`/`Costos.cuadra()` (total debería ser 67.000); (b) `validacion`/`I-18`; (c) `coherencia_temporal`.
- Expected: HTTP 422 en los tres; cuerpos problem+json con `campo` e `invariante`/`error` correctos.
- Actual: pendiente. Status: pendiente.

---

## 3. Camino crítico de apagado (integration/e2e, CU-05, contra adaptador simulado)

### TC-26 · Apagado-Bloqueado-Por-Verificacion-Contra-Simulado

- Tipo: integration
- Cubre: CU-05 (CA-01, CA-02, CA-03); RN-02, RN-04, RN-11; I-11, I-13; N-01, N-05
- Setup: adaptador simulado configurado con el corte de `§20.E-4` (`vp-003`, modalidad `HostLuegoUpsConRetorno`, `umbralDisparoSegundos = 300`, `tiempoReservadoApagadoSeg = 240`), 3 de 4 supuestos en `NuncaVerificado`.
- Given un corte que el simulado sostiene en `OB` durante 370 s con supuestos sin verificar, When el planificador alcanza el instante de decisión a los 300 s, Then crea `acc-20260811T042000` referida a `vp-003`, degrada a `SoloAlerta`, deja `resultado = BloqueadaPorVerificacion` y no ordena apagar el host.
- Expected: `modalidadEfectiva = SoloAlerta`; host no apagado; acción referida a versión; `tiempoReservadoApagadoSeg = 240 ≤ 540`.
- Actual: pendiente. Status: pendiente.

### TC-27 · Apagado-Con-Retorno-Confirmado-Por-Efecto-Observado

- Tipo: integration
- Cubre: CU-05 (CA-04, CA-05); RN-03; N-04
- Setup: adaptador simulado con los 4 supuestos en `Verificado` y modalidad `HostLuegoUpsConRetorno` (variante que sí ejecuta, F-3 en su parte lógica).
- Given los 4 supuestos verificados y un corte sostenido más allá del umbral, When el planificador ordena el apagado con retorno, Then confirma cada orden por efecto observado (retorno explícito del simulado), no por ausencia de error; ante `EFECTO_NO_CONFIRMADO` mantiene el estado seguro y no reporta como ejecutado lo no observado. Cubre además FA-2: un corte real señala `OB` y renueva `ver-flag-ob` por evidencia sin prueba destructiva.
- Expected: apagado ordenado con `ups.delay.start = 180 s`; efecto confirmado por retorno; `ver-flag-ob` pasa a `Verificado`.
- Actual: pendiente. Status: pendiente.

---

## 4. Salud de batería, resolución temporal y demás CU

### TC-28 · Veredicto-De-Salud-Con-Confianza-Baja

- Tipo: integration
- Cubre: CU-07 (CA-01, CA-02, CA-04); RN-06; N-08, N-16; I-15, I-16, I-17
- Setup: `§20.E-5`, `prb-20260901T010000` a 1 Hz, carga concurrente 13 % igual a la línea base `prb-20260719T010000`.
- Given una prueba densa comparable con solo 2 puntos de serie histórica, When el servicio emite el veredicto, Then `resultado = SinDegradacionDetectable`, `confianza = baja` (< 4 pruebas comparables), `calculadoPor = sai-service` con la reserva de que el equipo no lo dictamina, y `caidaV = -0,47 V`.
- Expected: veredicto con confianza `baja`; densidad 1 Hz durante la prueba, restaurada al terminar; muestras perdidas no rompen los derivados.
- Actual: pendiente. Status: pendiente.

### TC-29 · Recambio-Reatribuye-Historico-Por-Resolucion-Temporal

- Tipo: integration
- Cubre: CU-08 (CA-01, CA-02, CA-05); RC-07; I-3, I-5, I-18
- Setup: `§20.E-6`, `int-20260905-001` cierra `mnt-001` y abre `mnt-002`; SQLite temporal.
- Given un recambio registrado el 2026-09-05 10:30, When se aplican los efectos y luego se corrige la fecha del recambio, Then la métrica histórica se reatribuye a la batería correcta vía `MontajeBateria`/`ResolutorTemporal` sin migrar datos (la historia guarda dispositivo e instante, no la batería); `bat-2024-a` queda `DadoDeBaja` y consultable con sus 8 pruebas; la ficha calcula `costoPorAnioDeServicio = 37.430 ARS → 29,50 USD` sobre 654 días.
- Expected: reatribución por resolución temporal sin tocar filas de historia; ficha de vida útil cerrada correcta.
- Actual: pendiente. Status: pendiente.

### TC-30 · Autenticacion-Y-Alta-Inicial-Unica

- Tipo: integration
- Cubre: CU-01 (CA-01, CA-03, CA-04, CA-05); RN-01; ADR-16
- Setup: instancia limpia sin cuenta de administrador (`WebApplicationFactory`).
- Given el primer arranque sin cuenta, When el administrador declara `administrador` con contraseña válida, Then se crea la cuenta única, abre sesión y redirige a la página principal; un segundo intento de alta responde `ADMIN_YA_EXISTE`; una contraseña incorrecta responde `CREDENCIAL_INVALIDA`; sin sesión responde `SESION_REQUERIDA`.
- Expected: alta única exitosa; rechazos con los códigos correctos; hash PBKDF2 (ADR-16).
- Actual: pendiente. Status: pendiente.

### TC-31 · Alta-De-Equipos-Y-Siembra-De-Verificaciones

- Tipo: integration
- Cubre: CU-02 (CA-01, CA-03, CA-04, CA-05); RN-01, RN-13; I-21
- Setup: `§20.E-1`, dispositivo descubierto `0665:5161 INNO TECH` sin número de serie.
- Given un dispositivo descubierto sin número de serie y un host `i7infra`, When el administrador confirma la puesta en marcha, Then registra el dispositivo sin rechazarlo por el serie vacío, abre `mnt-001` y `cob-001` con fin abierto, crea la sesión de sondeo, y siembra las 4 verificaciones en `NuncaVerificado` con el panel en «0 de 4 supuestos verificados, modalidad SoloAlerta»; una potencia nominal desconocida se registra con procedencia `imputado`.
- Expected: alta sin serie; 4 verificaciones en `NuncaVerificado`; arranque autónomo en `SoloAlerta`.
- Actual: pendiente. Status: pendiente.

### TC-32 · Version-De-Politica-Nueva-No-Edicion

- Tipo: integration
- Cubre: CU-03 (CA-01, CA-02, CA-04); RN-04, RN-11; I-10, I-13
- Setup: política `pol-apagado-por-corte` con versión vigente 1 en `SoloAlerta`.
- Given la versión vigente 1, When el administrador crea una versión con modalidad `HostLuegoUpsConRetorno` y `tiempoReservadoApagadoSeg = 240`, Then se crea la versión 2 vigente sin alterar la 1 (que queda en historial); un `tiempoReservadoApagadoSeg = 600` se rechaza con `TIEMPO_APAGADO_EXCEDE_TECHO`; ajustar solo el umbral 300→240 crea la versión 3.
- Expected: versionado inmutable (no edición); techo de 540 s aplicado.
- Actual: pendiente. Status: pendiente.

### TC-33 · Monitoreo-En-Vivo-Calidad-Y-Procedencia

- Tipo: integration
- Cubre: CU-04 (CA-01, CA-02, CA-03, CA-04); RN-03, RN-05; N-06, N-07, N-09; I-7, I-8
- Setup: `§20.E-2`, adaptador simulado con sondeo a 5 s.
- Given un sondeo cada 5 s con el equipo en línea a `input.voltage 232,9 V`, When el planificador ejecuta una ronda completa, Then persiste una `Muestra` de calidad `completa`, la ronda de decisión se completa en < 1 s, y el panel muestra `232,9 V`; una respuesta sin `ups.load` persiste como `parcial` con su motivo; `battery.charge` se muestra `derivado`, no medido; tres sondeos consecutivos sin respuesta generan `DesconexionUsb` y la alerta de conectividad.
- Expected: muestra `completa`/`parcial`; `battery.charge` marcado `derivado`; `DesconexionUsb` al 3.er fallo; latencia de ronda < 1 s.
- Actual: pendiente. Status: pendiente.

### TC-34 · Historicos-Muestras-Y-Agregados-Con-Cobertura

- Tipo: integration
- Cubre: CU-06 (CA-01, CA-02, CA-03, CA-04); RN-10; N-18, N-19; I-14, I-20
- Setup: `§20.E-2`/`§20.E-3`/`§20.E-7`, serie de muestras `P30D` y agregados `PT1H` con `cobertura`.
- Given un período dentro de los 30 días, When el administrador grafica `input.voltage`, Then la serie de muestras aparece con las marcas de eventos; un período de más de 30 días atrás se sirve por agregados horarios con `cobertura = 0,987` y la advertencia de que el promedio no representa microcortes; el conteo de microcortes proviene de los eventos, no del promedio; un período sin datos responde `PERIODO_SIN_DATOS` sin dibujar serie vacía. Valida además el job de agregación/retención (muestras `P30D` → agregados `PT1H` durante `P10Y`).
- Expected: distinción muestra/agregado; cobertura y advertencia presentes; conteo por eventos; retención aplicada.
- Actual: pendiente. Status: pendiente.

### TC-35 · Sustitucion-Del-Sai-Dias-Sin-Proteccion

- Tipo: integration
- Cubre: CU-09 (CA-01, CA-02, CA-03, CA-04); RN-12; RC-02; I-4
- Setup: `§20.E-7` (datos R-11 reconstruidos), `cob-001` vigente por `ups-01`, `ups-02` en stock.
- Given la cobertura `cob-001` vigente, When `ups-01` sale a reparación y `ups-02` se pone a cubrir desde el instante de la salida, Then se cierra `cob-001`, `ups-01` pasa a reparación, se abre una cobertura sin solapamiento y `ups-02` pasa a `EnServicio`; un tramo de 2 días sin cobertura se informa como días sin protección (hueco entre coberturas); una sustitución por otro modelo devuelve las verificaciones de firmware a `NuncaVerificado` y dispara la caracterización.
- Expected: sucesión de coberturas sin solape; días sin protección calculados; verificaciones reiniciadas por cambio de modelo.
- Actual: pendiente. Status: pendiente.

### TC-36 · Ventana-De-Mantenimiento-Refutado-Vs-Vencido

- Tipo: integration
- Cubre: CU-10 (CA-01, CA-02, CA-03, CA-04); RN-01, RN-02, RN-03; N-13, N-14, N-15; I-11, I-12
- Setup: 4 verificaciones en `NuncaVerificado`, sistema en `SoloAlerta`, stepper de 4 pasos.
- Given la ventana de mantenimiento con presencia física, When el host arranca solo tras restaurar la energía, Then los 4 supuestos quedan `Verificado` y la modalidad `HostLuegoUpsConRetorno` queda efectiva; `ver-presupuesto-apagado` queda con vigencia 180 días y los de firmware 365; si el host no arranca solo, `ver-shutdown-return` pasa a `Refutado` y bloquea el apagado de forma permanente (`SUPUESTO_REFUTADO`); una verificación vencida pide repetir la prueba sin bloqueo permanente.
- Expected: distinción refutado (bloqueo permanente) vs vencido (repetir); vigencias correctas; desbloqueo 4 de 4. Nota: el arranque físico real no es automatizable (F-3, ver gaps de la matriz de cobertura).
- Actual: pendiente. Status: pendiente.

### TC-37 · Informe-De-Periodo-Y-Costo-Por-Anio

- Tipo: integration
- Cubre: CU-12 (CA-01, CA-02, CA-03, CA-04); RN-07, RN-10; I-5, I-18, I-20
- Setup: `§20.E-7`, período 2026 con recambio el 2026-09-05.
- Given el período 2026, When el administrador pide el informe del host `i7infra`, Then muestra `bat-2024-a` y `bat-2026-a` con intervalos recortados que suman 365 días sin solapamiento; `bat-2024-a` aparece pese a estar dada de baja; la calidad de suministro sobre agregados horarios lleva `cobertura = 0,987` y la advertencia; el `costoPorAnioDeServicio` se muestra normalizado a `29,50 USD`, marcado `derivado` con su fuente de cotización (BNA).
- Expected: intersección de intervalos correcta; baja lógica visible; cobertura/advertencia; costo por año en USD derivado.
- Actual: pendiente. Status: pendiente.

---

## 5. Casos end-to-end (e2e, panel contra adaptador simulado)

Tipo: **e2e**. Tooling: bUnit (componentes Blazor) y Playwright (recorridos de panel).

### TC-38 · E2E-Puesta-En-Marcha-Arranca-En-Solo-Alerta

- Tipo: e2e
- Cubre: F-1 / UF-1; CU-01, CU-02; RN-01; I-21
- Setup: instancia limpia, adaptador simulado con el dispositivo de `§20.E-1`, Playwright.
- Given un sistema recién desplegado sin identidad ni equipos, When el administrador se da de alta, ingresa y completa el alta de equipos, Then el sistema arranca en `SoloAlerta` por sí solo, sin que nadie lo configure así, con 0 de 4 supuestos verificados y el panel operativo.
- Expected: recorrido `Alta-Inicial → Login → Panel → Alta-De-Equipos` completo; modalidad efectiva `SoloAlerta` autónoma.
- Actual: pendiente. Status: pendiente.

### TC-39 · E2E-Corte-El-Servicio-Se-Niega-A-Apagar

- Tipo: e2e
- Cubre: F-2 / UF-3; CU-04, CU-05; RN-02, RN-03; I-11
- Setup: continuación de TC-38, adaptador simulado que reproduce el corte de `§20.E-4`, supuestos sin verificar.
- Given el sistema monitoreando en vivo con supuestos sin verificar, When el simulado inyecta un corte sostenido de 370 s, Then el panel muestra la degradación a solo aviso, el evento de corte y la acción `BloqueadaPorVerificacion`; el servicio no apaga el host (flujo de seguridad central).
- Expected: el host nunca se apaga; el panel refleja el bloqueo y el corte con su procedencia.
- Actual: pendiente. Status: pendiente.

---

## 6. Caso de contrato del adaptador (contract)

### TC-40 · Contrato-Del-Puerto-Adaptador-Cuatro-Operaciones

- Tipo: contract
- Cubre: CU-04, CU-05, CU-07; RN-03; ADR-02, ADR-22
- Setup: suite de contrato compartida ejecutada contra cada implementación del puerto (NUT y simulada; directo cuando exista). Ver `Guia-Testing-Extensibilidad-v1.0.md`.
- Given cualquier implementación de `IAdaptadorConexion`, When se ejerce cada una de las cuatro operaciones (leer estado, probar conectividad, ordenar apagado con retorno, lanzar test de batería), Then satisface la semántica del puerto: la lectura mapea calidad `completa`/`parcial`/`perdida`; la operación de apagado retorna el efecto observado (no éxito por silencio); el test de batería dispara el muestreo denso.
- Expected: la misma suite pasa contra NUT y contra el simulado; el apagado nunca reporta éxito por ausencia de error.
- Actual: pendiente. Status: pendiente.

---

## 7. Resumen de trazabilidad del catálogo

| Concepto | Cantidad | Cobertura |
| --- | --- | --- |
| Total de `TC-XX` | 40 | TC-01..TC-40 contiguos |
| Casos unitarios (invariantes) | 21 | I-1..I-21 (uno por invariante) |
| Casos de integración | 12 | TC-22..TC-27, TC-28..TC-37 |
| Casos end-to-end | 2 | TC-38 (UF-1/F-1), TC-39 (UF-3/F-2) |
| Casos de contrato | 1 | TC-40 (puerto del adaptador) |
| CU cubiertos | 12 / 12 | ver tabla siguiente |
| RN cubiertas | 13 / 13 | RN-01..RN-13 |
| Invariantes cubiertos | 21 / 21 | I-1..I-21 |

Cobertura por CU (al menos un TC por CU crítico):

| CU | TC que lo cubren |
| --- | --- |
| CU-01 | TC-30, TC-38 |
| CU-02 | TC-21, TC-31, TC-38 |
| CU-03 | TC-10, TC-13, TC-32 |
| CU-04 | TC-07, TC-08, TC-14, TC-33, TC-39, TC-40 |
| CU-05 | TC-10, TC-11, TC-13, TC-26, TC-27, TC-39, TC-40 |
| CU-06 | TC-14, TC-20, TC-34 |
| CU-07 | TC-09, TC-15, TC-16, TC-17, TC-28, TC-40 |
| CU-08 | TC-01, TC-02, TC-03, TC-05, TC-06, TC-18, TC-29 |
| CU-09 | TC-04, TC-35 |
| CU-10 | TC-11, TC-12, TC-36 |
| CU-11 | TC-18, TC-19, TC-22, TC-23, TC-24, TC-25 |
| CU-12 | TC-05, TC-20, TC-37 |

---

## 8. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Catálogo inicial de 40 casos de prueba referenciales: 21 de invariante (I-1..I-21), 4 caminos de la API de ingesta, camino crítico de apagado contra el adaptador simulado, veredicto de salud, resolución temporal del recambio, un TC por cada uno de los 12 CU y 2 flujos e2e (UF-1, UF-3) más el contrato del puerto del adaptador. Actual y status en pendiente por R-10 (dominio no implementado). |
