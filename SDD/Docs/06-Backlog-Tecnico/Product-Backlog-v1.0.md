# Product Backlog — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Product-Backlog-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Técnica de estimación:** Fibonacci (1, 2, 3, 5, 8, 13, 21) — adoptada como técnica única de todo el backlog
**Trazabilidad upstream:** 00 Roadmap-Producto; 01 NB-01..NB-08; 02 CU-01..CU-12; Intake §4 (F-01..F-33) y §15
**Documento hermano:** `Backlog-Tecnico-v1.0.md` (vista técnica y matriz BT↔US↔CU)

---

## 1. Objetivos del producto

Sai-Service-Core es un servicio web que garantiza que un host Linux sin respaldo se apague de forma ordenada ante un corte y **vuelva a encenderse solo**, negándose a apagarlo mientras no pueda probar que el reencendido funciona; y que construye el histórico de salud de batería, ciclo de vida de los equipos y costos que ninguna herramienta existente arma para este equipo. El **MVP** lo definen las historias Must: un servicio ejecutable con panel navegable, alta de equipos y políticas versionadas, monitoreo en vivo con eventos y procedencia, prueba de batería con veredicto de salud, ejecución del apagado ordenado con bloqueo por verificación, ventana de mantenimiento guiada e ingesta idempotente de intervenciones. Las historias Should completan el ciclo de vida (recambio, sustitución, informes y comparación de marcas); las Could quedan documentadas para v2 sin comprometerse en la primera entrega.

## 2. Épicas

Las épicas se derivan de las fases del Roadmap-Producto (00) y del esquema de delivery del Intake §15 (esqueleto caminante seguido de rebanadas verticales por flujo de usuario, en orden topológico de dependencias). Sprints estimados no calendarizados: valen como magnitud relativa para un único desarrollador.

| Épica | Nombre | Descripción | Fase | Sprints | Release |
|---|---|---|---|---|---|
| EP-01 | Fundaciones y decisiones de arranque | Cierre de las decisiones abiertas de Sprint 0 que condicionan la infraestructura antes de codificarla (ubicación de la herramienta de acceso, cifrado en la LAN, contratos aún abiertos, firma del adaptador) | F0 | S0 | v0.0 |
| EP-02 | Esqueleto caminante y panel base | Solución en cinco assemblies que compila y corre por script, con el layout del panel (menú lateral y barra superior) validado contra la maqueta | F1 (etapas 1-2) | S1-S2 | v0.2 |
| EP-03 | Persistencia, alta de administrador y sesión | Integración de persistencia con migraciones, alta inicial del administrador único, login, cierre de sesión y cambio de contraseña | F1 (etapas 3-4) | S3-S4 | v0.4 |
| EP-04 | Alta de equipos y políticas de apagado | Descubrimiento del dispositivo, alta de catálogo e inventario con vínculos temporales y baja lógica, siembra de supuestos, y políticas de apagado versionadas | F2 (UF-1, UF-2) | S5-S6 | v0.6 |
| EP-05 | Monitoreo, salud e históricos | Panel en vivo desde la LAN, sondeo y persistencia con calidad, derivación de eventos, procedencia visible, prueba de batería con veredicto de salud, e históricos con marcas de eventos | F3 (UF-3, UF-5, UF-4) | S7-S9 | v0.9 |
| EP-06 | Verificación y ciclo de vida de los equipos | Ejecución del apagado ordenado con bloqueo por verificación, ventana de mantenimiento guiada, recambio de batería con ficha de vida útil y sustitución del SAI con cobertura suplente | F4 (UF-8, UF-6, UF-7) | S10-S12 | v0.12 |
| EP-07 | Integración e informes | Ingesta idempotente de intervenciones por API, informe de período y comparación de marcas por costo por año de servicio normalizado | F5 (UF-10, UF-9) | S13-S14 | v1.0 |

**Nota sobre EP-01 y EP-02.** Son épicas habilitantes (esqueleto caminante): su trabajo se materializa como tareas técnicas en el `Backlog-Tecnico-v1.0.md` (BT-01 a BT-06) y no producen historias de usuario con valor funcional directo, salvo las de sesión que ya viven en EP-03. Se declaran como épicas para preservar la secuencia del roadmap y la trazabilidad de las decisiones de arranque.

## 3. Historias por épica

El proyecto tiene **26 historias de usuario (> 20)**, por lo que cada historia vive en su archivo individual bajo `historias-usuario/US-XX-<Nombre>-v1.0.md` con las siete secciones obligatorias (§4.4 de las reglas). Esta tabla es el índice maestro priorizado.

| US | Título | MoSCoW | SP | Estado | CU relacionados | Épica |
|---|---|---|---|---|---|---|
| US-01 | Alta inicial del administrador único | Must | 3 | Borrador | CU-01 | EP-03 |
| US-02 | Login, cierre de sesión y cambio de contraseña | Must | 3 | Borrador | CU-01 | EP-03 |
| US-03 | Descubrimiento del dispositivo y prueba de conexión | Must | 5 | Borrador | CU-02 | EP-04 |
| US-04 | Alta de catálogo e inventario con vínculos temporales y baja lógica | Must | 8 | Borrador | CU-02 | EP-04 |
| US-05 | Siembra de verificaciones y arranque forzado en solo aviso | Must | 3 | Borrador | CU-02, CU-10 | EP-04 |
| US-06 | Configuración de política de apagado versionada | Must | 5 | Borrador | CU-03 | EP-04 |
| US-07 | Panel de estado en vivo desde la LAN | Must | 5 | Borrador | CU-04 | EP-05 |
| US-08 | Sondeo periódico y persistencia de muestras con calidad | Must | 8 | Borrador | CU-04 | EP-05 |
| US-09 | Derivación de eventos y alerta de pérdida de comunicación | Must | 5 | Borrador | CU-04 | EP-05 |
| US-10 | Procedencia visible de cada valor | Must | 3 | Borrador | CU-04, CU-06 | EP-05 |
| US-11 | Históricos y gráficas de evolución con marcas de eventos | Must | 8 | Borrador | CU-06 | EP-05 |
| US-12 | Prueba de batería programada y manual con cadencia densa | Must | 8 | Borrador | CU-07 | EP-05 |
| US-13 | Veredicto de salud con confianza y comparación a carga igualada | Must | 8 | Borrador | CU-07 | EP-05 |
| US-14 | Ejecución del apagado ordenado ante corte sostenido | Must | 13 | Borrador | CU-05 | EP-06 |
| US-15 | Bloqueo por verificación y validación por efecto observado | Must | 8 | Borrador | CU-05, CU-10 | EP-06 |
| US-16 | Ventana de mantenimiento guiada de los cuatro supuestos | Must | 8 | Borrador | CU-10 | EP-06 |
| US-17 | Renovación de verificaciones por evidencia | Should | 5 | Borrador | CU-10, CU-05 | EP-06 |
| US-18 | Registro de recambio de batería con cierre y apertura de vigencia | Should | 8 | Borrador | CU-08 | EP-06 |
| US-19 | Ficha de vida útil con costo por año normalizado | Should | 5 | Borrador | CU-08, CU-12 | EP-06 |
| US-20 | Reparación o sustitución del SAI con cobertura suplente | Should | 8 | Borrador | CU-09 | EP-06 |
| US-21 | Ingesta idempotente de intervenciones por API | Must | 8 | Borrador | CU-11 | EP-07 |
| US-22 | Rechazo de conflictos de idempotencia e invariantes rotos | Must | 5 | Borrador | CU-11 | EP-07 |
| US-23 | Informe de período | Should | 8 | Borrador | CU-12 | EP-07 |
| US-24 | Comparación de marcas por costo por año de servicio en USD | Should | 5 | Borrador | CU-12, CU-08 | EP-07 |
| US-25 | Adaptador de conexión directo para equipos no cubiertos por la herramienta de acceso al SAI | Could | 5 | Borrador | CU-02, CU-04 | EP-07 |
| US-26 | Capa de add-ons de dialecto de protocolo (diseñada) | Could | 3 | Borrador | CU-04 | EP-07 |

## 4. Métricas de avance

Resumen por prioridad al inicio del backlog (todas las historias en estado Borrador; nada cerrado aún).

| Prioridad | Cantidad de US | Story points | % del backlog (SP) | % cerrado |
|---|---|---|---|---|
| Must | 18 | 114 | 71 % | 0 % |
| Should | 6 | 39 | 24 % | 0 % |
| Could | 2 | 8 | 5 % | 0 % |
| Won't (v1.0) | — | — | — | — |
| **Total** | **26** | **161** | **100 %** | **0 %** |

- **Distribución MoSCoW:** 69 % Must (por cantidad), 23 % Should, 8 % Could. El núcleo Must es amplio porque las ocho NB y las capacidades F-01..F-20 del intake son todas Must; el reparto Should/Could recae en el ciclo de vida extendido (F-21..F-25) y en las capacidades de extensibilidad diseñadas pero no implementadas en v1 (F-26, F-27).
- **Won't (v1.0):** documentadas fuera del backlog en el Intake §4 (F-28 apagado de otros equipos, F-29 múltiples SAI, F-30 notificaciones externas primarias, F-31 gestión de usuarios y roles, F-32 lectura del ajuste de BIOS por software, F-33 traductor de protocolo propio). No generan historias.
- **Deuda en backlog:** 161 SP sin comprometer a sprint. La cadencia de cierre la marca la validación humana por etapa (Roadmap §5), no una fecha.

## 5. Refinamiento

- **Cadencia:** una sesión de refinement por sprint, como fija la tabla §2.2 de las reglas para web-monolith. Con un único desarrollador que es propietario, implementador y beneficiario, el refinement es **ligero y autogestionado**: una revisión al cierre de cada etapa de §15 que deja la etapa siguiente con sus US en estado Ready antes de arrancarla.
- **Responsable:** el administrador único, en el rol de Scrum Master / Product Owner combinado. La titularidad del backlog es del AG-06.
- **Formato de estimación:** Planning Poker no aplica con un solo estimador; se usa **estimación por analogía** contra historias ya cerradas, manteniendo la escala Fibonacci declarada en la cabecera. La primera pasada usa valores relativos (US de alta ≈ 8, US de sesión ≈ 3, US del motor de apagado ≈ 13) que se recalibran cuando haya velocity real de los primeros sprints.
- **Entradas del refinement:** roadmap (00), CU y RN (02), ADR y contratos (05). Cada sesión revisa que ninguna US entre a un sprint sin cumplir la Definition of Ready (`Definition-Of-Ready-v1.0.md`).
- **Salida:** US promovidas a Ready, BT desbloqueadas y matriz BT↔US↔CU actualizada en el `Backlog-Tecnico-v1.0.md`.

---

## Control de cambios

| Versión | Fecha | Motivo |
| --- | --- | --- |
| 1.0 | 2026-07-21 | Versión inicial del product backlog (7 épicas, 26 US, métricas y refinamiento). |
| 1.1 | 2026-07-21 | Corrección de conformidad: abstracción de nombres de stack a capacidad + ADR tras audit de Fase D (título de US-25). |
