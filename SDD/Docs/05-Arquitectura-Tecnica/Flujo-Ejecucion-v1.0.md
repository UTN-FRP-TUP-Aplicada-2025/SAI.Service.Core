# Flujo de ejecución — Planificador y apagado ordenado

**Proyecto:** Sai-Service-Core
**Documento:** Flujo-Ejecucion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)

---

## 1. Propósito y justificación de inclusión

Este documento especifica, paso a paso, el pipeline del **planificador interno** (hosted service) y el del **apagado ordenado**: el camino crítico del sistema. Documenta las transformaciones de datos y los estados por los que pasa cada ronda de sondeo hasta la eventual ejecución o el bloqueo de una acción sobre el equipo y el host.

**Por qué se incluye** (las reglas §2.1 solo exigen este artefacto para `web-monolith` cuando hay orquestación compleja, no para CRUD trivial): el planificador **no es CRUD**. Es la orquestación no trivial del **único camino con consecuencias irreversibles** del sistema — apagar un servidor sin backups y garantizar su reencendido. Reúne concurrencia interna (rondas a intervalo fijo), temporizadores con cancelación, una máquina de decisión de modalidad con degradación forzada, y una disciplina de confirmación por efecto observado que no puede quedar enterrada como bullets del documento maestro. La fuente lo califica: *"el servicio va a tomar la decisión de apagar un servidor sin backups. Si falla, falla de noche y sin testigos"* (R-12).

Trazabilidad: CU-05 (ejecución del apagado ordenado), CU-10 (ventana de mantenimiento), ADR-09 (`CicloForzado`), ADR-10 (bloqueo por verificación), ADR-11 (efecto observado), ADR-12 (disparo sin flag `LB`).

## 2. Pipeline A — Ronda de sondeo (el 80 % del tiempo, CU-04)

El planificador ejecuta una ronda cada `intervaloSegundos` (5 s por defecto; 1 Hz durante una prueba de batería). La ronda completa debe terminar en **< 1 s** para no desplazar la siguiente (N-06).

| # | Paso | Entrada | Salida | Error / cancelación |
| --- | --- | --- | --- | --- |
| A1 | Pedir estado al equipo por el adaptador de conexión | Handle del adaptador (NUT/simulado) | Lectura cruda de variables NUT clave-valor, o timeout | Sin respuesta ⇒ incrementa contador de fallidos (ver A6) |
| A2 | Construir la `Muestra` con procedencia por valor | Lectura cruda + mapa variable→origen de la `SesionSondeo` | `Muestra` con `calidad` (`completa`/`parcial`/`perdida`) y cada valor envuelto en `Valor<T>` con `Origen` | Respuesta incompleta ⇒ `parcial` (se conserva); sin respuesta ⇒ `perdida` con valores `null` (CL-12) |
| A3 | Persistir la `Muestra` (append-only) | `Muestra` | Fila en SQLite; procedencia declarada una vez por `SesionSondeo` | Un solo hilo escritor; sin actualización ni borrado (ADR-04) |
| A4 | Evaluar reglas de derivación sobre la ventana reciente | Ventana de muestras + `ReglaDerivacion` versionada | `Evento` (`Microcorte`, `CorteSuministro`, `RetornoRed`, `DesconexionUsb`, `TensionFueraDeRango`) con `reglaDerivacionId` y `reglaVersion` | Una sola muestra puede atrapar el corte (CL-11); duración con `incertidumbreDuracionSeg` = intervalo (CL-10) |
| A5 | Empujar estado y eventos nuevos al panel | Estado + eventos | Actualización del circuito Blazor; `battery.charge` marcado **derivado** | Circuito caído ⇒ se reconstruye; la verdad está en SQLite |
| A6 | Evaluar conectividad | Contador de sondeos fallidos | Si llega a 3 consecutivos: `DesconexionUsb` + alerta visual (N-09) | La respuesta que vuelve resetea el contador |
| A7 | Alimentar el pipeline de disparo | Estado en batería (`OB`) sostenido | Entra al Pipeline B (temporizador de disparo) | Si el estado vuelve a `OL` antes del umbral ⇒ no dispara (FA-1, Pipeline B) |

Transformación central de la ronda: **variable cruda del equipo → `Valor<T>` con `Origen` → `Muestra` con `calidad` → `Evento` con regla versionada**. Ninguna magnitud pierde su procedencia (I-7); ningún `Agregado` se sirve como si fuera una `Muestra` (I-20, ADR-08).

## 3. Pipeline B — Evaluación de políticas, temporizador y decisión (camino crítico, CU-05)

Se activa cuando A7 detecta el equipo en batería. **No** depende del flag `LB` ni de `battery.runtime`: usa **tiempo en `OB` + `battery.voltage`** (ADR-12, R-04).

### Estados de la condición de disparo

```
    OL (en línea) ──corte──> OB (en batería) ──[sostenido < umbral]──> vuelve OL ⇒ FIN sin acción (FA-1)
                                     │
                                     └──[sostenido ≥ umbralDisparoSegundos]──> DECISIÓN (B3)
```

| # | Paso | Entrada | Salida | Error / cancelación |
| --- | --- | --- | --- | --- |
| B1 | Arrancar el temporizador de disparo | Transición OL→OB | Temporizador de `umbralDisparoSegundos` (300 s de partida) armado, con token de cancelación | — |
| B2 | Sostener la condición | Rondas sucesivas en `OB` | Al cumplirse el umbral, continúa a B3 | Si vuelve `OL` antes ⇒ **cancelar** el token, registrar el evento (microcorte/corte breve) con el motivo de no actuar, FIN (FA-1) |
| B3 | Crear la `Accion` referida a la versión de política vigente | `VersionPolitica` vigente + `modalidadSolicitada` | `Accion` con `modalidadSolicitada` (nunca referida a `Politica`, siempre a `VersionPolitica`: RN-11, I-13) | — |
| B4 | Evaluar las verificaciones requeridas por la modalidad | `Accion` + conjunto de `Verificacion` requeridas | Si **todas** `Verificado`: `modalidadEfectiva` = `modalidadSolicitada`. Si alguna en `NuncaVerificado`/`Vencido`/`Refutado`: `modalidadEfectiva` = `SoloAlerta`, resultado `BloqueadaPorVerificacion` con su motivo | **Bloqueo por verificación** (ADR-10, RN-02): FIN sin apagar; registra la acción bloqueada (postcondición de bloqueo de CU-05) |
| B5 | Renovación por evidencia | Señal de `OB` durante un corte real | Renueva por evidencia la `Verificacion` del supuesto de señal en batería, sin prueba destructiva (FA-2 de CU-05) | — |

Salida de B4: o bien **acción habilitada** (pasa al Pipeline C), o bien **acción bloqueada** con `modalidadEfectiva = SoloAlerta` (el sistema solo avisa; postcondición de bloqueo). Escenario de referencia: §20.E-4 (corte prolongado, la política dispara y el sistema se niega por supuestos sin verificar).

## 4. Pipeline C — Ejecución de la acción y registro por efecto observado (CU-05)

Solo se ejecuta si B4 habilitó la acción (todos los supuestos verificados).

| # | Paso | Entrada | Salida | Error / cancelación |
| --- | --- | --- | --- | --- |
| C1 | Ordenar el apagado ordenado del host | `tiempoReservadoApagadoSeg` (≤ 540 s, I-10) | Invocación de `shutdown` del SO; contenedores detenidos dentro del presupuesto | `EFECTO_NO_CONFIRMADO` si no se observa el efecto ⇒ no se da por ejecutado, estado seguro (ADR-11, RN-03) |
| C2 | Ordenar al equipo cortar la salida con retorno | Orden `shutdown.return` por el adaptador | El SAI corta la salida al host, produciendo la transición que dispara el reencendido | No usar `shutdown.stop`; en `CicloForzado` el corte **no se cancela** aunque vuelva la red (ADR-09) |
| C3 | Confirmar cada acción por efecto observado | Estado del equipo/host tras la orden | Registro del resultado observado, no de la ausencia de error | Un comando que no llega no produce error (CL-07): se valida el efecto, nunca se asume éxito |
| C4 | Cerrar el ciclo al restablecerse la energía | `ups.delay.start` (180 s) transcurrido; energía repuesta | El equipo repone la salida; el host reenciende solo; se registra el cierre del ciclo | Si el host no reenciende ⇒ evidencia negativa (se resuelve en CU-10, no aquí) |

Transformación central del camino crítico: **condición sostenida → `Accion` (solicitada) → `modalidadEfectiva` (posiblemente degradada) → orden por el adaptador → resultado observado registrado**. El trade-off central del proyecto (T-06) está aquí: en `CicloForzado`, un apagón controlado de ~3 minutos es preferible a un host apagado indefinidamente.

## 5. Manejo de error y cancelación (consolidado)

| Situación | Origen | Respuesta |
| --- | --- | --- |
| Corte breve por debajo del umbral | B2 | Cancelación del token; registro del evento sin acción, con motivo (FA-1) |
| Supuesto requerido no verificado | B4 | `BloqueadaPorVerificacion`, degradación a `SoloAlerta`, no se apaga (RN-02) |
| Orden no confirmable por efecto observado | C1–C3 | `EFECTO_NO_CONFIRMADO`; no se reporta como ejecutado; estado seguro (RN-03) |
| Notificación remota que no sale (la red también cayó) | Transversal | Se registra el fallo del canal; el registro local sobrevive y es la fuente primaria; no es fallo del apagado (CL-08) |
| Pérdida de comunicación con el equipo | A6 | 3 sondeos fallidos ⇒ `DesconexionUsb` + alerta (CL-14) |

Toda cancelación usa el token del temporizador de B1; toda confirmación usa la disciplina de efecto observado de ADR-11.

## 6. Enlace con la ventana de mantenimiento (CU-10)

El Pipeline C solo se habilita si los cuatro supuestos están verificados, y esa verificación se produce fuera de este flujo: en la **ventana de mantenimiento** (CU-10), que es destructiva, exige presencia física y no es automatizable. Hasta que ocurra, B4 degrada siempre a `SoloAlerta`. El **adaptador simulado** (F-24, ADR-02) es lo que permite ejercitar los Pipelines B y C en pruebas automatizadas sin hardware ni riesgo; el comportamiento real del firmware y de la BIOS se registra como evidencia de una `Verificacion`, no como un test (T-08).

## 7. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| CU cubiertos | CU-04 (Pipeline A), CU-05 (Pipelines B y C), CU-10 (habilitación) |
| RN aplicables | RN-01, RN-02, RN-03, RN-04, RN-11 |
| ADR que lo gobiernan | ADR-09, ADR-10, ADR-11, ADR-12; estructura del hosted service en ADR-15 |
| Escenario de datos | §20.E-4 (corte prolongado, disparo y negativa); §20.E-3 (microcorte con incertidumbre) |
| Tests previstos en 08 | Cancelación por corte breve, degradación a `SoloAlerta`, respeto del techo de 540 s, `CicloForzado` sin cancelación, efecto observado, latencia de ronda < 1 s |

## Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Pipeline inicial del planificador (ronda de sondeo) y del apagado ordenado (temporizador con cancelación, decisión con degradación, ejecución con efecto observado), con transformaciones de datos, estados, manejo de error y trazabilidad a CU-05/CU-10 y ADR-09/10/11/12. |
