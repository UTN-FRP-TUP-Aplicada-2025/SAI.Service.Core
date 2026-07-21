# Roadmap de Producto

**Proyecto:** Sai-Service-Core
**Documento:** Roadmap-Producto-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-00)
**Trazabilidad upstream:** SOLUTION-INTAKE §4, §6, §15
**Trazabilidad downstream:** 06-Backlog-Tecnico, 07-Plan-Sprint, 01-Necesidades-Negocio, 02-Especificacion-Funcional, 05-Arquitectura-Tecnica, 11-Examples

## 1. Propósito

Este documento organiza la construcción en fases con objetivo, entregable y criterio de cierre verificable. Deriva la secuencia del esquema de descomposición y delivery del intake §15 (esqueleto caminante seguido de rebanadas verticales por flujo de usuario) y de la priorización del intake §4 (los flujos y capacidades Must Have primero, luego los Should Have). Como es un proyecto de un único desarrollador sin presupuesto ni fecha impuesta, el roadmap no usa fechas calendario: cada fase se cierra con un punto de validación del administrador, y la fase siguiente no arranca hasta que esa validación pasa. Los objetivos de release se expresan como versiones internas incrementales, siguiendo que cada etapa cerrada equivale a un incremento menor.

## 2. Fases del producto

| Fase | Objetivo | Épicas asociadas | Sprints estimados | Entregable | Release target |
|---|---|---|---|---|---|
| F0 — Fundaciones y decisiones abiertas | Resolver las decisiones de arranque que condicionan la infraestructura antes de codificarla | Decisiones abiertas P-03 a P-06 (ubicación de la herramienta de acceso al equipo, cifrado en la red local, contratos aún no cerrados) | 1 | Registro de decisiones de arquitectura (ADR) del arranque | v0.0 |
| F1 — Esqueleto caminante | Una solución que compila, corre con un script, se abre en el navegador, persiste, autentica y da sesión al administrador único | Andamiaje, panel base, persistencia y alta de administrador, sesión (etapas 1 a 4 de §15) | 4 | Servicio ejecutable con panel navegable, alta de administrador, login, cierre de sesión y cambio de contraseña | v0.4 |
| F2 — Alta del parque y políticas | Registrar el parque y configurar políticas de apagado versionadas | UF-1 (alta del parque), UF-2 (configuración de políticas); capacidades C-01, C-02, C-05, C-06 | 2 | Alta del SAI y su batería desde el panel; política de apagado versionada; siembra de supuestos en no verificado, con el servicio forzado en solo aviso | v0.6 |
| F3 — Monitoreo, salud e históricos | Ver el estado en vivo, probar la batería y consultar la evolución | UF-3 (monitoreo en vivo), UF-5 (prueba de batería y salud), UF-4 (históricos y gráficas); capacidades C-03, C-04, C-07, C-08, C-09, C-10 | 3 | Panel en vivo con supuestos y eventos; prueba de batería con veredicto y confianza; gráficas históricas con marcas de eventos | v0.9 |
| F4 — Verificación y ciclo de vida del parque | Desbloquear el apagado real por evidencia y modelar recambios y sustituciones | UF-8 (ventana de mantenimiento), UF-6 (recambio de batería), UF-7 (reparación o sustitución del SAI); capacidades C-11, C-13, C-16 | 3 | Ventana de mantenimiento guiada con registro de evidencia; recambio de batería que cierra y abre vigencias; cobertura suplente con días sin protección | v0.12 |
| F5 — Integración e informes | Ingerir intervenciones externas y cerrar la comparación de marcas | UF-10 (ingesta automatizada), UF-9 (informe de período y comparación de marcas); capacidades C-12, C-14 | 2 | Interfaz de integración idempotente con cliente de referencia; informe de período y comparación por costo por año de servicio | v1.0 |

Los sprints estimados son orientativos y no calendarizados: al ser un solo desarrollador sin fecha impuesta, valen como magnitud relativa, no como compromiso de fecha.

## 3. Matriz fase → épica → sprint → release

| Fase | Épica | Sprint (relativo) | Release target |
|---|---|---|---|
| F0 | Decisiones abiertas de arranque (P-03 a P-06) | S0 | v0.0 |
| F1 | Andamiaje de la solución (etapa 1) | S1 | v0.1 |
| F1 | Panel base (etapa 2) | S2 | v0.2 |
| F1 | Persistencia y alta de administrador (etapa 3) | S3 | v0.3 |
| F1 | Sesión: login, cierre de sesión, cambio de contraseña (etapa 4) | S4 | v0.4 |
| F2 | UF-1 alta del parque | S5 | v0.5 |
| F2 | UF-2 configuración de políticas | S6 | v0.6 |
| F3 | UF-3 monitoreo en vivo | S7 | v0.7 |
| F3 | UF-5 prueba de batería y salud | S8 | v0.8 |
| F3 | UF-4 históricos y gráficas | S9 | v0.9 |
| F4 | UF-8 ventana de mantenimiento | S10 | v0.10 |
| F4 | UF-6 recambio de batería | S11 | v0.11 |
| F4 | UF-7 reparación o sustitución del SAI | S12 | v0.12 |
| F5 | UF-10 ingesta automatizada | S13 | v0.13 |
| F5 | UF-9 informe de período y comparación de marcas | S14 | v1.0 |

El orden de las épicas de flujo respeta el grafo de dependencias del intake §6: ningún flujo se construye antes que aquellos de los que depende. UF-9 va último porque consume las salidas de UF-4, UF-6, UF-7 y UF-10.

## 4. Dependencias entre fases

- F1 depende de F0: no se codifica la infraestructura hasta cerrar las decisiones de arranque (dónde vive la herramienta de acceso al equipo y cómo se cifra el acceso en la red local).
- F2 depende de F1: sin persistencia, sesión y panel base no hay dónde dar de alta el parque ni configurar políticas.
- F3 depende de F2: el monitoreo evalúa las políticas configuradas en F2 sobre el parque dado de alta en F2.
- F4 depende de F3: la ventana de mantenimiento cronometra el apagado bajo la carga observada por el monitoreo, y el recambio de batería cierra vigencias que se abrieron en F2 y midieron en F3.
- F5 depende de F4 y de F3: el informe y la comparación de marcas consumen históricos (F3), recambios y sustituciones (F4) e ingesta externa; por eso van al final.

La ventana de mantenimiento (en F4) entrega la interfaz guiada y el registro de evidencias, pero su ejecución real exige presencia física y pruebas destructivas; hasta que esa ejecución ocurra, el servicio permanece forzado en modo solo aviso, independientemente del avance del roadmap.

## 5. Criterios de transición entre fases

| Fase origen | Fase destino | Criterios verificables |
|---|---|---|
| F0 | F1 | - [ ] Está decidida y registrada como ADR la ubicación de la herramienta de acceso al equipo<br>- [ ] Está decidida y registrada como ADR la estrategia de cifrado del acceso en la red local<br>- [ ] Están identificados los contratos aún abiertos con su categoría responsable de cierre |
| F1 | F2 | - [ ] La solución compila y corre mediante los scripts, validada visualmente por el administrador<br>- [ ] El panel base cumple la maqueta aprobada, validado en el navegador<br>- [ ] El alta del administrador único, el login, el cierre de sesión y el cambio de contraseña funcionan en el navegador |
| F2 | F3 | - [ ] El alta del SAI y su batería desde el panel funciona, con prueba de conexión<br>- [ ] Se crea una política de apagado como versión nueva sin editar la vigente<br>- [ ] El servicio arranca forzado en solo aviso con los supuestos en no verificado, y el panel lo muestra en la pantalla principal |
| F3 | F4 | - [ ] El panel muestra estado en vivo, conectividad, supuestos verificados y eventos recientes<br>- [ ] La prueba de batería produce un veredicto con confianza explícita comparado contra la línea base a carga igualada<br>- [ ] Las gráficas históricas se ven, individuales o superpuestas, con marcas de eventos |
| F4 | F5 | - [ ] La ventana de mantenimiento guiada recorre los cuatro supuestos y registra su evidencia<br>- [ ] Un recambio de batería cierra la vigencia vieja, abre la nueva y proyecta la ficha de vida útil en un solo acto<br>- [ ] Una sustitución del SAI queda registrada con su cobertura suplente y sus días sin protección |
| F5 | Cierre v1.0 | - [ ] La interfaz de integración responde de forma idempotente ante el reintento y rechaza hechos incoherentes o con costos que no cuadran<br>- [ ] El informe de período se emite con dispositivos, cobertura, intervenciones, eventos y calidad de suministro<br>- [ ] La comparación de marcas presenta el costo por año de servicio normalizado a moneda estable |

## 6. Trazabilidad downstream

Este roadmap alimenta directamente a 06-Backlog-Tecnico y 07-Plan-Sprint: cada fase se descompone en las épicas de la columna correspondiente, y cada épica de flujo se mapea a las capacidades y funcionalidades del intake §4 (F-01 a F-27) que la realizan. Las decisiones de F0 se cierran como ADR en 05-Arquitectura-Tecnica. Los criterios de transición son la base de la definición de listo y de terminado que 06 y 08 formalizan. La épica de ingesta de F5 se ilustra en 11-Examples con el cliente de referencia. Los flujos y su grafo de dependencias provienen del intake §6; ningún reordenamiento de fases puede violar ese grafo.
