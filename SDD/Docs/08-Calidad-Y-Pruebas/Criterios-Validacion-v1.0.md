# Criterios de validación — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Criterios-Validacion-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-08)

Criterios numéricos que permiten declarar a Sai-Service-Core validado para release. Referencian los NFR N-01..N-25 de `05-Arquitectura-Tecnica/Arquitectura-Solucion-v1.0.md §8`, los CU de 02 y los invariantes I-1..I-21. No redefinen la Definition of Done; la complementan del lado del release.

---

## 1. Propósito

Un release de Sai-Service-Core está **validado** cuando: todos los CU críticos están verdes, todos los NFR con objetivo numérico cumplen su SLA medido en un ambiente equivalente al productivo, la suite de regresión está verde, la cobertura por capa cumple sus pisos sin warnings, y toda excepción a lo anterior está aceptada con ADR y plan de remediación. "Validado" no significa "sin defectos": significa que las dos garantías del producto —no apagar sin poder probar el reencendido, y no mentir sobre el origen de un dato— están verificadas mecánicamente.

## 2. Criterios funcionales (cada CU crítico verde)

Cada CU tiene al menos un test por criterio Given-When-Then, verde, trazado en la `Matriz-Cobertura-Pruebas`. Los marcados como críticos gobiernan la aptitud de release; el resto debe estar verde salvo excepción con ADR.

| CU | Criticidad | Condición de validación |
|---|---|---|
| CU-01 Autenticación del administrador | Alta | Alta inicial única, login, logout y cambio de contraseña verdes; endpoint de salud público |
| CU-02 Alta de equipos y puesta en marcha | Crítica | Descubrimiento USB, alta con vínculos temporales, siembra de verificaciones en `NuncaVerificado` y arranque forzado en `SoloAlerta` verdes |
| CU-03 Configuración de políticas | Alta | Versión nueva de política sin editar la vigente; techo de 540 s rechazado por formulario (I-10) |
| CU-04 Monitoreo en vivo | Crítica | Calidad de muestra (`completa`/`parcial`/`perdida`), `battery.charge` marcado derivado, alerta a los 3 sondeos fallidos verdes |
| CU-05 Ejecución del apagado ordenado | Crítica | Degradación a `SoloAlerta` cuando falta un supuesto (I-11), ciclo forzado, validación por efecto observado y acción referida a versión de política (I-13) verdes contra el adaptador simulado |
| CU-06 Históricos y gráficas | Media | Superposición de series con marcas de eventos; agregado con cobertura y advertencia (I-20) |
| CU-07 Prueba de batería y veredicto de salud | Alta | Caída de tensión a carga igualada, comparabilidad (I-16), confianza `baja` con < 4 pruebas (N-16) verdes |
| CU-08 Recambio de batería y ficha | Alta | Cierre/apertura de `MontajeBateria` sin hueco (I-3), `Costos.cuadra()` (I-18), costo por año en USD |
| CU-09 Reparación y sustitución del SAI | Media | Cobertura suplente y días sin protección (escenario E-9); binding por ruta física |
| CU-10 Ventana de mantenimiento y verificación | Crítica | Transición de supuestos, `Refutado` vs `Vencido` (I-12), presupuesto de apagado; parte de software verde, ejecución física registrada como evidencia |
| CU-11 Ingesta automatizada | Crítica | Cuatro caminos 201/200/409/422 (I-19), idempotencia y coherencia temporal verdes |
| CU-12 Informe de período y comparación de marcas | Media | Cobertura del período, `costoPorAnioDeServicio`, agregación con cobertura |

CU críticos para release: CU-02, CU-04, CU-05, CU-10, CU-11. Ninguno puede ir a release en rojo.

## 3. Criterios no funcionales (cada NFR cumple su SLA numérico)

Cada NFR con objetivo numérico se mide en el ambiente de pruebas equivalente al productivo. Los PENDIENTE se declaran como tales y no bloquean el release de v1 salvo que se resuelvan antes.

| NFR | SLA | Criterio de validación |
|---|---|---|
| N-01 | `ups.delay.shutdown` ≤ 540 s | Test de invariante I-10; el formulario rechaza `tiempoReservadoApagadoSeg > 540` |
| N-02 | 150 s de presupuesto para el grace de Docker | Medición cronometrada del grace del contenedor |
| N-03 | Resto del apagado del SO | **PENDIENTE de medición** — se cronometra en la ventana de mantenimiento (CU-10); evidencia de `ver-presupuesto-apagado` |
| N-04 | `ups.delay.start` 180 s | Lectura de la variable del equipo |
| N-05 | Umbral de disparo 300 s de partida | Config de `VersionPolitica`; test del temporizador |
| N-06 | Ronda del planificador < 1 s | Métrica de duración de ronda logueada por el hosted service |
| N-07 | Intervalo de sondeo 5 s | Medición de cadencia real |
| N-08 | Cadencia 1 Hz en prueba de batería | Densidad de muestreo registrada en la `PruebaBateria` |
| N-09 | 3 sondeos sin respuesta ⇒ `DesconexionUsb` | Test de integración del contador de sondeos fallidos |
| N-10 | `input.voltage` en [198, 242] V; fuera 30 s ⇒ `TensionFueraDeRango` | Test de derivación versionada |
| N-11 | Microcorte < 60 s (regla v2) | Test de derivación con normalización por versión (CL-15) |
| N-12 | `battery.voltage` < 13,3 V / > 14,5 V ⇒ alarma dura | Test de umbrales sobre la muestra |
| N-13 | Vigencia `ver-presupuesto-apagado` 180 días | Test de vencimiento de la `Verificacion` |
| N-14 | Vigencia `ver-bios-autoencendido` y `ver-flag-ob` 365 días | Test de caducidad |
| N-15 | `ver-shutdown-return` sin caducidad | Test de que no vence |
| N-16 | ≥ 4 pruebas comparables para tendencia; con menos, confianza `baja` | Conteo de `PruebaBateria` con `comparable = true` |
| N-17 | Prueba de batería trimestral | Programación del planificador |
| N-18 | ~6,3 M filas/año | Conteo de filas; validación de agregación |
| N-19 | Retención Muestra `P30D`, Agregado `PT1H`/`P10Y`, Evento indefinido | Test de retención y descarte |
| N-20 | Tamaño máximo del archivo SQLite | **PENDIENTE** — no dimensionado (R-07); se valida antes de producción bajo carga simulada |
| N-21 | Cobertura global ≥ 80 % líneas / 70 % ramas | Quality gate 4 |
| N-22 | Cobertura `Domain` ≥ 90 % líneas / 85 % ramas | Quality gate 3 |
| N-23 | Idempotencia 100 % | Test de los cuatro caminos; I-19 |
| N-24 | 0 valores sin `Origen` | Test de invariante I-7 en el pipeline |
| N-25 | SLO de disponibilidad del servicio | **PENDIENTE** — sin respaldo en la fuente; propuesta «rondas completadas / esperadas ≥ 0,99 mensual» [derivado]; se cierra por ADR |

Los tres PENDIENTE (N-03, N-20, N-25) se listan como huecos conocidos, no como criterios cumplidos. Presentar el release como validado exige o bien resolverlos con su medición, o bien aceptarlos con ADR y plan de remediación (§6).

## 4. Criterios de regresión

- La suite de regresión completa se ejecuta y queda verde antes del release.
- Ningún test que estaba verde en la versión anterior pasó a rojo sin justificación registrada en ADR.
- Todo defecto cerrado desde el release anterior generó al menos un test de regresión que lo previene (no vuelve sin que la suite lo detecte).
- Los tests de invariantes I-1..I-21 —la corrección del dominio— están todos verdes; ninguno deshabilitado sin motivo documentado.

## 5. Criterios de calidad de código

- Cobertura por capa cumplida y **reportada por capa**, nunca como número global único, según la fuente única de umbrales: `Estrategia-Testing-v1.0.md §2` (cobertura por capa). Los pisos son Domain 90/85, Application 80/70, Infrastructure 70/60, Api 80/70, Web (presentación) 60/50 y el conjunto de la solución 80/70; no se repiten acá para no divergir de esa fuente.
- Build en Release con cero errores y **cero warnings** (`TreatWarningsAsErrors`, gate 1).
- Análisis estático sin diagnósticos de severidad error y sin diferencias de formato (gate 2); ningún warning nuevo respecto de la línea base.
- SCA sin vulnerabilidades de severidad alta o crítica (gate 6); SBOM válido adjunto (gate 7).
- Mutation testing no adoptado en v1 (no exigido para `web-monolith`); no es criterio de release.

## 6. Excepciones documentadas

Cualquier criterio de las secciones 2 a 5 que no se cumpla se acepta para release **solo** con ADR explícita que registre: el criterio incumplido, la razón, el impacto y el plan de remediación con su ítem de backlog (BT) asociado. Sin ADR, un criterio incumplido bloquea el release. Los PENDIENTE N-03, N-20 y N-25 son los candidatos naturales a esta vía en v1: se aceptan con ADR y BT de remediación, o se resuelven con su medición antes del release.

---

## 7. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Versión inicial: criterios funcionales (12 CU), no funcionales (N-01..N-25 con SLA, PENDIENTE marcados), regresión, calidad de código y excepciones con ADR. |
| 1.1 | 2026-07-21 | Corrección de conformidad tras audit de Fase E: la cobertura por capa de §5 se alinea a la tabla canónica y remite a `Estrategia-Testing-v1.0.md §2` como fuente única (Domain 90/85, Application 80/70, Infrastructure 70/60, Api 80/70, Web 60/50, Global 80/70), sin repetir números propios. |
