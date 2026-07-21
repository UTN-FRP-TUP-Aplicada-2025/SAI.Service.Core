# Mini-Plan de Sprints — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Mini-Plan-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-07)
**Trazabilidad upstream:** 00 Roadmap-Producto (F0-F5); 06 Product-Backlog (EP-01..EP-07, US-01..US-26) y Backlog-Tecnico (BT-01..BT-30); 02 CU-01..CU-12; 05 ADR-01..ADR-22; Intake §15 (delivery por etapas) y §11 (riesgos)
**Trazabilidad downstream:** 08-Calidad-Y-Pruebas (Definition of Done canónica, pendiente de generación)

Este es un proyecto de **un solo desarrollador**. Por la regla §2.2 de 07-Rules-Plan-Sprint (modo 1 dev), la categoría 07 se reduce a este Mini-Plan, que **sustituye** a los planes de iteración por sprint, a las plantillas de review y retrospectiva y al tracking de velocidad. No existen `Plan-Iteracion-Sprint-XX-v1.0.md`, ni `Template-Sprint-Review-v1.0.md`, ni `Template-Sprint-Retrospectiva-v1.0.md`, ni `Velocidad-Equipo-v1.0.md`. Este documento condensa los objetivos de cada etapa, la lista de ítems comprometidos por sus identificadores de 06 y la bitácora de avance.

---

## 1. Información general

- **Equipo:** un único desarrollador, propietario, implementador y beneficiario del servicio, en el rol combinado de Scrum Master / Product Owner / Dev. La aprobación de cierre de cada etapa la da ese mismo administrador único.
- **Unidad de estimación:** story points en escala Fibonacci (1, 2, 3, 5, 8, 13, 21), heredada tal cual de 06. Este plan **no re-estima**: reutiliza los puntos ya fijados en el Product-Backlog y el Backlog-Tecnico.
- **Cadencia:** una etapa por incremento de valor. Las etapas se ejecutan en secuencia, respetando el grafo de dependencias de los flujos (Intake §6) y el orden topológico de §15.
- **Duración:** timebox nominal de **2 semanas (10 días hábiles)** por etapa. El arranque (Sprint 0) admite un **ciclo corto de 1 semana justificado** (§3.2) para su bloque de decisiones, porque son spikes con caja temporal explícita de 1 a 2 días cada uno; su bloque de andamiaje corre en un ciclo estándar de 2 semanas.
- **Sin fechas de calendario:** siguiendo el Roadmap §2, al ser un solo desarrollador sin fecha impuesta, el timebox vale como magnitud relativa. **Cada etapa cierra por validación humana del administrador**, no por vencimiento de calendario, y la etapa siguiente no arranca hasta que esa validación pase (criterio transversal de §15).
- **Filtro de entrada:** ningún ítem entra a una etapa sin cumplir la Definition of Ready (`06-Backlog-Tecnico/Definition-Of-Ready-v1.0.md`). El cierre lo filtra la Definition of Done canónica de 08.
- **Alcance de v1:** se comprometen las 24 historias Must y Should (153 SP). Las dos historias Could (US-25 adaptador directo, US-26 add-ons de dialecto, 8 SP) quedan **diseñadas pero no implementadas** y diferidas a v2; no se comprometen en ninguna etapa de este plan.

---

## 2. Estructura del plan por etapas

El plan se organiza en un **Sprint 0 de arranque** (sin valor de negocio) más **cinco etapas de valor**, una por fase del roadmap, cada una alineada a una épica de 06. Las etapas 2 a 5 agrupan varios flujos de usuario (UF), que se abordan en el orden topológico del grafo de §6.

| Etapa | Épica(s) | Fase | Flujos UF (orden topológico) | Sprints relativos (roadmap) | Release | SP |
|---|---|---|---|---|---|---|
| Sprint 0 — Arranque | EP-01 + EP-02 | F0 + F1 (etapas 1-2) | — (habilitante) | S0-S2 | v0.2 | 24 |
| Etapa 1 — Persistencia, alta de administrador y sesión | EP-03 | F1 (etapas 3-4) | — (habilitante) | S3-S4 | v0.4 | 24 |
| Etapa 2 — Alta de equipos y políticas | EP-04 | F2 | UF-1 → UF-2 | S5-S6 | v0.6 | 58 |
| Etapa 3 — Monitoreo, salud e históricos | EP-05 | F3 | UF-3 → UF-5 → UF-4 | S7-S9 | v0.9 | 79 |
| Etapa 4 — Verificación y ciclo de vida | EP-06 | F4 | UF-8 → UF-6 → UF-7 | S10-S12 | v0.12 | 91 |
| Etapa 5 — Integración e informes | EP-07 | F5 | UF-10 → UF-9 | S13-S14 | v1.0 | 47 |

Las siete épicas EP-01..EP-07 de 06 quedan cubiertas: EP-01 y EP-02 se resuelven juntas en el Sprint 0 de arranque (ambas son habilitantes y no entregan valor de negocio directo), y EP-03 a EP-07 corresponden una a una con las etapas 1 a 5.

---

## 3. Sprint 0 — Arranque (EP-01 + EP-02)

**Estado:** Propuesto · **Release:** v0.2 · **Fase:** F0 + F1 (etapas 1-2)

### 3.1 Objetivo del sprint

Cerrar las decisiones de arranque como ADR y levantar el esqueleto que compila, corre por script y se navega en el navegador, dejando la base técnica lista para construir valor.

### 3.2 Naturaleza

Sprint de arranque dedicado a setup técnico y a cerrar las decisiones abiertas que condicionan la infraestructura antes de codificarla. **No entrega valor de negocio** ni cierra ninguna necesidad de negocio; habilita todas las etapas posteriores.

### 3.3 Bloque A — Decisiones de arranque (ciclo corto de 1 semana justificado)

Cuatro spikes con caja temporal explícita que producen una ADR de cierre cada uno. Cierran las cuatro decisiones Propuestas de Sprint 0.

| ID | Tipo | Descripción | Prioridad | Estim. | Caja | ADR que cierra | Estado |
|---|---|---|---|---|---|---|---|
| BT-01 | Spike | Ubicación de la herramienta de acceso al SAI (contenedor vs host) | Must | 3 | 2 d | ADR-19 | Pendiente |
| BT-02 | Spike | Estrategia de cifrado del panel y la API en la red local | Must | 2 | 1 d | ADR-20 | Pendiente |
| BT-04 | Spike | Firma del puerto del adaptador de conexión | Must | 3 | 2 d | ADR-22 | Pendiente |
| BT-03 | Spike | Contrato del endpoint de rectificación del conflicto de ingesta | Should | 3 | 2 d | ADR-21 | Pendiente |

Total bloque A: 11 SP. BT-03 depende de BT-28 (endpoint de ingesta, Etapa 5); su cierre puede quedar como contrato documentado y confirmarse cuando se implemente BT-28.

### 3.4 Bloque B — Andamiaje y panel base (ciclo estándar de 2 semanas)

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-05 | Devops | Andamiaje en cinco assemblies con scripts y contenedor de desarrollo | Must | 8 | BT-01, BT-02 | Pendiente |
| BT-06 | Feature | Panel base: menú lateral y barra superior según maqueta | Must | 5 | BT-05 | Pendiente |

Total bloque B: 13 SP. Tareas de bootstrap que acompañan al andamiaje: repositorio y su estructura (Intake §16), contenedor de desarrollo, integración continua inicial (build, test, lint) y acuerdo de la Definition of Done con 08.

### 3.5 Criterio de cierre (validación humana)

- La solución **compila y corre** mediante los scripts; el administrador valida visualmente la estructura de la solución.
- El panel base cumple la maqueta aprobada (Fase B2 de UX 03), validado **en el navegador**.
- Están decididas y registradas como ADR la ubicación de la herramienta de acceso al SAI (ADR-19), la estrategia de cifrado en la red local (ADR-20) y la firma del adaptador (ADR-22); el contrato de rectificación (ADR-21) queda identificado con su categoría responsable de cierre.

### 3.6 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | Ninguno se cierra; BT-05 y BT-06 son precondición de CU-01 (autenticación) y CU-04 (panel en vivo) |
| NB que avanzan | Ninguna NB de negocio se cierra; etapa habilitante de todas |
| ADR que gobiernan | ADR-15 (cinco assemblies), ADR-19, ADR-20, ADR-21, ADR-22 |

### 3.7 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-08/R-05 — Decisión abierta sobre dónde vive la herramienta de acceso al SAI y competencia por el nodo USB entre procesos | Alta | Medio | Cerrar BT-01 como ADR-19 antes de codificar el adaptador; anclar el nodo por ruta física del puerto (previsto en BT-13) para un único consumidor |
| R-10 — El modelo de datos no está implementado: los invariantes son hipótesis de diseño hasta que corran como pruebas | Cierta | Medio | Preparar en el andamiaje el proyecto de pruebas de invariantes (I-1 a I-21) para escribirlos antes de codificar el dominio en la Etapa 1 |

---

## 4. Etapa 1 — Persistencia, alta de administrador y sesión (EP-03)

**Estado:** Propuesto · **Release:** v0.4 · **Fase:** F1 (etapas 3-4) · **SP:** 24

### 4.1 Objetivo del sprint

Dejar el servicio ejecutable y navegable en la red local con el administrador único capaz de darse de alta, iniciar sesión, cerrar sesión y cambiar su contraseña sobre una persistencia versionada y con procedencia obligatoria.

### 4.2 Ítems comprometidos

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-07 | BT | Persistencia con migraciones versionadas aplicadas al arranque | Must | 5 | BT-05 | Pendiente |
| BT-08 | BT | Objeto de valor `Valor<T>` con `Origen` obligatorio (invariante I-7) | Must | 5 | BT-07 | Pendiente |
| BT-09 | BT | Historia append-only como disciplina de escritura | Must | 3 | BT-07 | Pendiente |
| BT-10 | BT | Autenticación de administrador único y endpoint de salud público | Must | 5 | BT-07 | Pendiente |
| US-01 | US | Alta inicial del administrador único | Must | 3 | BT-10 | Pendiente |
| US-02 | US | Login, cierre de sesión y cambio de contraseña | Must | 3 | US-01 | Pendiente |

Total comprometido: 24 SP (6 de US + 18 de BT).

### 4.3 Criterio de cierre (validación humana)

El alta del administrador único, el login, el cierre de sesión y el cambio de contraseña funcionan **en el navegador**; el esquema se aplica por migraciones al arranque; ningún valor de dominio se persiste sin `Origen`; las escrituras destructivas sobre las tablas de hechos se rechazan.

### 4.4 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | CU-01 (autenticación del administrador) |
| NB que avanzan | Habilita el acceso seguro sobre el que se apoyan NB-02 y NB-05; sienta las fundaciones de procedencia (NB-03) con `Valor<T>` y de la historia append-only |
| ADR que gobiernan | ADR-18 (persistencia con migraciones), ADR-06 (procedencia), ADR-04 (append-only), ADR-16 (administrador único) |

### 4.5 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-10 — Los invariantes son hipótesis de diseño hasta correr como pruebas | Cierta | Medio | Escribir los invariantes I-1 a I-21 como pruebas antes de codificar el dominio; el invariante I-7 (procedencia) se valida en el pipeline con cero excepciones |
| R-13 — Guardar un valor derivado sin marcar su procedencia produce una conclusión falsa sobre datos que parecían medidos | Alta | Alto | `Valor<T>` con `Origen` obligatorio (BT-08); ningún valor se persiste sin procedencia declarada, base para que el panel marque después todo valor derivado o estimado |

---

## 5. Etapa 2 — Alta de equipos y políticas (EP-04)

**Estado:** Propuesto · **Release:** v0.6 · **Fase:** F2 · **Flujos:** UF-1 → UF-2 · **SP:** 58

### 5.1 Objetivo del sprint

Permitir que el administrador registre el SAI y su batería desde el panel con vínculos temporales y publique políticas de apagado versionadas, quedando el servicio forzado en solo aviso con los cuatro supuestos en no verificado.

### 5.2 Ítems comprometidos

**Flujo UF-1 · Alta de equipos y puesta en marcha (S5)**

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-11 | BT | Modelo de catálogo, inventario e historia con baja lógica | Must | 8 | BT-08, BT-09 | Pendiente |
| BT-12 | BT | Vigencia como entidad con intervalo y `ResolutorTemporal` | Must | 8 | BT-11 | Pendiente |
| BT-13 | BT | Anclaje del nodo USB por ruta física del puerto | Must | 3 | BT-01 | Pendiente |
| BT-14 | BT | Puerto del adaptador de conexión (contrato mínimo) | Must | 5 | BT-04 | Pendiente |
| BT-15 | BT | Implementación de la herramienta de acceso al SAI | Must | 8 | BT-14, BT-13 | Pendiente |
| US-03 | US | Descubrimiento del dispositivo y prueba de conexión | Must | 5 | BT-15 | Pendiente |
| US-04 | US | Alta de catálogo e inventario con vínculos temporales y baja lógica | Must | 8 | BT-12 | Pendiente |
| US-05 | US | Siembra de verificaciones y arranque forzado en solo aviso | Must | 3 | US-04 | Pendiente |

**Flujo UF-2 · Configuración de políticas (S6)**

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-16 | BT | Políticas de apagado versionadas con techo duro de 540 s | Must | 5 | BT-11 | Pendiente |
| US-06 | US | Configuración de política de apagado versionada | Must | 5 | BT-16 | Pendiente |

Total comprometido: 58 SP (21 de US + 37 de BT).

### 5.3 Criterio de cierre (validación humana)

El alta del SAI y su batería desde el panel funciona con prueba de conexión; una política de apagado se crea como versión nueva sin editar la vigente y respeta el techo de 540 s; el servicio arranca forzado en solo aviso con los supuestos en no verificado y el panel lo muestra en la pantalla principal.

### 5.4 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | CU-02 (alta de equipos y puesta en marcha), CU-03 (configuración de políticas), y la siembra de supuestos de CU-10 |
| NB que avanzan | NB-04 (ciclo de vida de los equipos — alta), NB-07 (configuración de políticas de apagado), y el arranque seguro en solo aviso de NB-05 |
| ADR que gobiernan | ADR-07 (catálogo/inventario/historia), ADR-05 (vigencia temporal), ADR-03 (anclaje USB), ADR-02 y ADR-22 (adaptador), ADR-01 (acceso al SAI) |

### 5.5 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-05 — Competencia por el nodo USB entre procesos concurrentes | Media | Medio | Anclaje por ruta física del puerto (BT-13) para garantizar un único consumidor; la ubicación de la herramienta de acceso quedó decidida en ADR-19 |
| R-06 — Desapariciones del bus USB documentadas en este modelo | Media | Medio | Anclaje que sobrevive a reconexiones y no depende del número de serie; deja preparada la vigilancia de conectividad que se completa en la Etapa 3 (BT-24) |

---

## 6. Etapa 3 — Monitoreo, salud e históricos (EP-05)

**Estado:** Propuesto · **Release:** v0.9 · **Fase:** F3 · **Flujos:** UF-3 → UF-5 → UF-4 · **SP:** 79

### 6.1 Objetivo del sprint

Dar al administrador un panel en vivo desde la red local con eventos y procedencia visible, la prueba de batería con veredicto de salud y confianza explícita, y las gráficas históricas con marcas de eventos.

### 6.2 Ítems comprometidos

**Flujo UF-3 · Monitoreo en vivo (S7)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| BT-17 | BT | Planificador con cadencia configurable | Must | 8 | Pendiente |
| BT-18 | BT | Persistencia de `Muestra` con calidad y agregación con min/max | Must | 8 | Pendiente |
| BT-19 | BT | Reglas de derivación versionadas de eventos | Must | 5 | Pendiente |
| BT-20 | BT | Disparo del apagado por tiempo en batería y tensión, sin flag LB | Must | 5 | Pendiente |
| US-07 | US | Panel de estado en vivo desde la red local | Must | 5 | Pendiente |
| US-08 | US | Sondeo periódico y persistencia de muestras con calidad | Must | 8 | Pendiente |
| US-09 | US | Derivación de eventos y alerta de pérdida de comunicación | Must | 5 | Pendiente |
| US-10 | US | Procedencia visible de cada valor | Must | 3 | Pendiente |

**Flujo UF-5 · Prueba de batería y salud (S8)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| BT-21 | BT | Método de salud por tendencia de la caída de tensión | Must | 8 | Pendiente |
| US-12 | US | Prueba de batería programada y manual con cadencia densa | Must | 8 | Pendiente |
| US-13 | US | Veredicto de salud con confianza y comparación a carga igualada | Must | 8 | Pendiente |

**Flujo UF-4 · Históricos y gráficas (S9)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| US-11 | US | Históricos y gráficas de evolución con marcas de eventos | Must | 8 | Pendiente |

Total comprometido: 79 SP (45 de US + 34 de BT).

### 6.3 Criterio de cierre (validación humana)

El panel muestra estado en vivo, conectividad, supuestos verificados y eventos recientes; la prueba de batería produce un veredicto con confianza explícita comparado contra la línea base a carga igualada; las gráficas históricas se ven, individuales o superpuestas, con marcas de eventos.

### 6.4 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | CU-04 (monitoreo en vivo), CU-07 (prueba de batería y veredicto), CU-06 (históricos y gráficas) |
| NB que avanzan | NB-02 (monitoreo en vivo y alertas), NB-06 (evaluación de salud de baterías), NB-03 (historia trazable con procedencia) |
| ADR que gobiernan | ADR-08 (agregado no hereda de muestra), ADR-12 (disparo sin flag LB), ADR-13 (salud por caída de tensión) |

### 6.5 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-09 — Sin sensor de temperatura, el confusor de la tendencia de salud no tiene solución con este equipo | Cierta | Alto (para salud de batería) | Toda conclusión de salud lleva la reserva explícita en su advertencia; el veredicto arranca en confianza baja y sube solo con pruebas comparables |
| R-13 — Marcar `battery.charge` como medido cuando es una interpolación produce una conclusión falsa | Alta | Alto | Procedencia obligatoria (I-7); `aptoParaTendenciaDeSalud()` devuelve falso para derivado, estimado por driver e imputado; el panel marca en pantalla todo valor derivado o estimado (US-10) |
| R-07 — Retención de métricas no probada contra el volumen real (~6,3 millones de filas/año) | Media | Bajo | Validar la agregación con mínimo y máximo y la política de retención (30 d muestra / 10 a agregado) antes de producción; el conteo de microcortes sale de los eventos, nunca de la serie agregada |
| R-04 — El flag de batería baja no fue observado y la política no debería depender de él | Alta | Medio | El disparo decide por tiempo en batería más tensión, nunca por el flag; prueba con serie sintética que nunca enciende ese flag y aun así dispara (BT-20) |

---

## 7. Etapa 4 — Verificación y ciclo de vida de los equipos (EP-06)

**Estado:** Propuesto · **Release:** v0.12 · **Fase:** F4 · **Flujos:** UF-8 → UF-6 → UF-7 · **SP:** 91

**Etapa crítica del proyecto:** materializa el objetivo primario (apagado ordenado y reencendido garantizado) y concentra los riesgos de severidad crítica.

### 7.1 Objetivo del sprint

Desbloquear el apagado ordenado real por evidencia verificada en la ventana de mantenimiento y registrar el ciclo de vida de los equipos con sus recambios de batería y sustituciones del SAI.

### 7.2 Ítems comprometidos

**Flujo UF-8 · Ventana de mantenimiento y ejecución del apagado (S10)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| BT-22 | BT | Modalidad de ciclo forzado (no cancelar el corte aunque vuelva la red) | Must | 5 | Pendiente |
| BT-23 | BT | Arranque seguro y bloqueo por verificación | Must | 8 | Pendiente |
| BT-24 | BT | Validación por efecto observado y detección de pérdida de comunicación | Must | 5 | Pendiente |
| BT-25 | BT | Verificación del ajuste de arranque por comportamiento | Must | 5 | Pendiente |
| US-14 | US | Ejecución del apagado ordenado ante corte sostenido | Must | 13 | Pendiente |
| US-15 | US | Bloqueo por verificación y validación por efecto observado | Must | 8 | Pendiente |
| US-16 | US | Ventana de mantenimiento guiada de los cuatro supuestos | Must | 8 | Pendiente |
| US-17 | US | Renovación de verificaciones por evidencia | Should | 5 | Pendiente |

**Flujo UF-6 · Recambio de batería (S11)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| BT-26 | BT | Registro de intervenciones con cuadre de costos y `Dinero` con moneda y fecha | Must | 8 | Pendiente |
| BT-27 | BT | Ficha de vida útil con costo por año normalizado | Should | 5 | Pendiente |
| US-18 | US | Registro de recambio de batería con cierre y apertura de vigencia | Should | 8 | Pendiente |
| US-19 | US | Ficha de vida útil con costo por año normalizado | Should | 5 | Pendiente |

**Flujo UF-7 · Reparación o sustitución del SAI (S12)**

| ID | Tipo | Descripción | Prioridad | Estim. | Estado |
|---|---|---|---|---|---|
| US-20 | US | Reparación o sustitución del SAI con cobertura suplente | Should | 8 | Pendiente |

Total comprometido: 91 SP (55 de US + 36 de BT).

### 7.3 Criterio de cierre (validación humana)

La ventana de mantenimiento guiada recorre los cuatro supuestos y registra su evidencia; un recambio de batería cierra la vigencia vieja, abre la nueva y proyecta la ficha de vida útil en un solo acto; una sustitución del SAI queda registrada con su cobertura suplente y sus días sin protección. La etapa entrega la interfaz guiada y el registro de evidencias; hasta que la ejecución física real ocurra, el servicio permanece forzado en solo aviso.

### 7.4 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | CU-05 (ejecución del apagado ordenado), CU-10 (ventana de mantenimiento y verificación), CU-08 (recambio de batería y ficha), CU-09 (reparación y sustitución del SAI) |
| NB que avanzan | NB-01 (apagado ordenado y reencendido garantizado), NB-05 (seguridad operativa — bloqueo por verificación), NB-04 (ciclo de vida — recambio y sustitución) |
| ADR que gobiernan | ADR-09 (ciclo forzado), ADR-10 (arranque seguro y bloqueo), ADR-11 (efecto observado), ADR-14 (verificación por comportamiento), RN-04 (techo de apagado), RN-07 y RN-08 (importes y cuadre de costos) |

### 7.5 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-12 — El servicio decide apagar un servidor sin respaldo; si falla, falla de noche y sin testigos | Media | Crítico | Bloqueo por verificación (BT-23): el sistema arranca forzado en solo aviso y no apaga nada hasta poder probar que el host vuelve a encenderse |
| R-01 — El ciclo de apagado y reencendido no está verificado; la trampa de firmware podría dejar el SAI apagado para siempre | Media | Crítico | Prueba física en la ventana de mantenimiento (US-16) antes de habilitar cualquier modalidad distinta de solo aviso; el supuesto de retorno con corte no caduca |
| R-02 — El presupuesto de 540 s no está medido contra el apagado real del host | Alta | Alto | Cronometrar en la ventana de mantenimiento; el supuesto de presupuesto de apagado vence a los 180 días y el panel muestra cuánto consume el apagado de contenedores |
| R-03 — El ajuste de arranque tras corte no es legible por software y puede volverse falso en silencio | Media | Alto | Verificación por comportamiento (BT-25) cruzando eventos propios contra los registros de arranque del sistema; una prueba fallida deja el supuesto refutado y vuelve a bloquear |
| R-11 — La secuencia completa de sustitución del SAI (UF-7) no tiene escenario de datos que la ejercite | Cierta | Bajo | Agregar un escenario de datos de sustitución cuando se implemente el flujo, para poder ejercitar la cobertura suplente en pruebas |

---

## 8. Etapa 5 — Integración e informes (EP-07)

**Estado:** Propuesto · **Release:** v1.0 · **Fase:** F5 · **Flujos:** UF-10 → UF-9 · **SP:** 47

### 8.1 Objetivo del sprint

Abrir la ingesta idempotente de intervenciones desde sistemas externos y cerrar el informe de período con la comparación de marcas por costo por año de servicio normalizado a moneda estable.

### 8.2 Ítems comprometidos

**Flujo UF-10 · Ingesta automatizada (S13)**

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-28 | BT | Endpoint de ingesta idempotente con cuatro caminos de respuesta | Must | 8 | BT-26 | Pendiente |
| BT-29 | BT | Adaptador de conexión simulado para pruebas | Should | 5 | BT-14 | Pendiente |
| US-21 | US | Ingesta idempotente de intervenciones por API | Must | 8 | BT-28 | Pendiente |
| US-22 | US | Rechazo de conflictos de idempotencia e invariantes rotos | Must | 5 | BT-28, BT-03 | Pendiente |

**Flujo UF-9 · Informe de período y comparación de marcas (S14)**

| ID | Tipo | Descripción | Prioridad | Estim. | Dependencias | Estado |
|---|---|---|---|---|---|---|
| BT-30 | BT | Informe de período y comparación de marcas | Should | 8 | BT-12, BT-27 | Pendiente |
| US-23 | US | Informe de período | Should | 8 | BT-30 | Pendiente |
| US-24 | US | Comparación de marcas por costo por año de servicio | Should | 5 | BT-30 | Pendiente |

Total comprometido: 47 SP (26 de US + 21 de BT).

### 8.3 Criterio de cierre (validación humana)

La interfaz de integración responde de forma idempotente ante el reintento y rechaza hechos incoherentes o con costos que no cuadran; el informe de período se emite con dispositivos, cobertura, intervenciones, eventos y calidad de suministro; la comparación de marcas presenta el costo por año de servicio normalizado a moneda estable.

### 8.4 Trazabilidad

| Dimensión | Referencia |
|---|---|
| CU que avanzan | CU-11 (ingesta automatizada de intervenciones), CU-12 (informe de período y comparación de marcas) |
| NB que avanzan | NB-08 (ingesta automatizada de intervenciones); el informe y la comparación cierran el valor de NB-03 (historia trazable) y NB-06 (salud y costo de baterías) |
| ADR que gobiernan | ADR-17 (manejo de errores de la API de ingesta), ADR-21 (contrato de rectificación), RN-09 (idempotencia), RN-07 y RN-08 (importes y cuadre) |

### 8.5 Riesgos y mitigaciones

| Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|
| R-10 — Los invariantes de idempotencia y de cuadre de costos son hipótesis hasta correr como pruebas | Cierta | Medio | Escribir los invariantes de ingesta como pruebas: misma clave y cuerpo devuelve el mismo id; clave repetida con cuerpo distinto devuelve conflicto, nunca éxito silencioso (US-22) |
| R-07 — El informe interseca intervalos y agrega grandes volúmenes sin haber probado el rendimiento contra el volumen real | Media | Bajo | Validar la consulta de informe sobre datos de retención de agregados; los agregados adjuntan cobertura y advertencia para no reportar sobre huecos de datos |

---

## 9. Definition of Done aplicada

Este plan **no define ni duplica** la Definition of Done. El cierre de cada ítem se rige por la DoD canónica del proyecto, que vive en **08-Calidad-Y-Pruebas / Definition-of-Done (pendiente de generación)**. La Definition of Ready de 06 (`Definition-Of-Ready-v1.0.md`) filtra el arranque de cada ítem; la DoD de 08 filtra su cierre. Criterio específico transversal de este plan, adicional a la DoD canónica: cada etapa cierra con una **validación visual o funcional en el navegador** por parte del administrador, según fija §15 del intake.

---

## 10. Bitácora de avance

Plantilla semanal lista para completar a medida que se ejecuta cada etapa. Una fila por semana; se anota la etapa vigente, los ítems en curso, los cerrados en la semana y los bloqueos con su acción de destrabe.

| Semana | Etapa vigente | Ítems en curso | Ítems cerrados esta semana | Bloqueos / acción de destrabe |
|---|---|---|---|---|
| — | Sprint 0 — Arranque | — | — | — |
| — | — | — | — | — |
| — | — | — | — | — |
| — | — | — | — | — |

Notas de uso de la bitácora:

- Al cerrar cada etapa, registrar en la fila de la semana el cumplimiento del criterio de cierre (§ correspondiente) y la validación del administrador.
- Si un ítem se arrastra de una etapa a la siguiente, anotarlo como bloqueo con su motivo, para no acumular arrastre silencioso.
- Los cambios de alcance durante una etapa se registran en el Control de cambios (§12), no reescribiendo el compromiso original de la etapa.

---

## 11. Trazabilidad global CU y NB por etapa

Resumen de qué casos de uso y qué necesidades de negocio avanzan al cierre de cada etapa. Cubre los 12 CU (CU-01..CU-12) y las 8 NB (NB-01..NB-08).

| Etapa | Épica(s) | CU que avanzan | NB que avanzan |
|---|---|---|---|
| Sprint 0 — Arranque | EP-01 + EP-02 | Habilitante (precondición de CU-01, CU-04) | Habilitante (ninguna NB se cierra) |
| Etapa 1 | EP-03 | CU-01 | Fundaciones de NB-03; habilita NB-02 y NB-05 |
| Etapa 2 | EP-04 | CU-02, CU-03, CU-10 (siembra) | NB-04, NB-07, arranque seguro de NB-05 |
| Etapa 3 | EP-05 | CU-04, CU-06, CU-07 | NB-02, NB-06, NB-03 |
| Etapa 4 | EP-06 | CU-05, CU-08, CU-09, CU-10 | NB-01, NB-05, NB-04 |
| Etapa 5 | EP-07 | CU-11, CU-12 | NB-08, cierre de NB-03 y NB-06 |

---

## 12. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Versión inicial del Mini-Plan para proyecto de un solo desarrollador. Estructura en Sprint 0 de arranque (EP-01 + EP-02) más cinco etapas de valor (EP-03 a EP-07), alineadas a las fases F0-F5 del roadmap y a los flujos UF en orden topológico de §15. Reutiliza los identificadores US-XX/BT-XX y las estimaciones de 06 sin re-estimar. Sustituye a los planes de sprint, plantillas de review y retrospectiva y tracking de velocidad (regla §2.2, modo 1 dev). |
