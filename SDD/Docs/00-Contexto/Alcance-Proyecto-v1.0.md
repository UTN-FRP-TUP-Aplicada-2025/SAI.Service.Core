# Alcance del Proyecto

**Proyecto:** Sai-Service-Core
**Documento:** Alcance-Proyecto-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-00)
**Trazabilidad upstream:** SOLUTION-INTAKE §1, §4, §5, §6, §9, §10, §15
**Trazabilidad downstream:** 01-Necesidades-Negocio, 02-Especificacion-Funcional, 03-UX-UI-DX, 05-Arquitectura-Tecnica, 07-Plan-Sprint, 11-Examples

## 1. Propósito

Este documento fija qué construye el proyecto y qué deja explícitamente afuera, para que las categorías siguientes trabajen sin volver a preguntar y para evitar que el alcance se expanda sin decisión. Delimita las capacidades incluidas, las exclusiones justificadas, los supuestos, las restricciones y los criterios con los que se acepta que el proyecto cumplió lo prometido.

## 2. Descripción general

El proyecto entrega un servicio propio que protege a un único servidor respaldado por un único SAI, y que resuelve lo que las herramientas existentes no cubren para ese equipo. El servicio hace tres cosas: (1) decide y ejecuta el apagado ordenado del servidor solo cuando puede probar que el servidor volverá a encenderse solo, y se niega a apagarlo mientras no pueda probarlo; (2) construye y conserva el histórico de salud de batería, ciclo de vida del parque y costos; y (3) ofrece un panel de administración accesible desde la red local y una interfaz de integración para que sistemas externos empujen datos de forma automatizada. El monitoreo básico y el apagado ordenado ante corte que las herramientas existentes ya resuelven no se reconstruyen: el proyecto se apoya en ellos y agrega lo que falta.

## 3. Objetivos del proyecto

- Garantizar el reencendido del servidor tras un corte, con una regla de seguridad que impide apagarlo mientras los supuestos de los que depende no estén verificados.
- Registrar y mantener vivo el histórico de salud de batería, con veredictos propios que declaran su confianza y sus límites.
- Modelar el ciclo de vida completo del parque, de modo que el histórico responda qué equipo protegía al servidor en cada tramo y cuántos días quedó sin protección.
- Dar visibilidad remota del estado en tiempo real y de la evolución histórica desde la red local.
- Habilitar la ingesta automatizada de intervenciones desde sistemas externos sin duplicar hechos ante reintentos.

## 4. Alcance incluido

### 4.1 Capacidades

| ID | Capacidad | Prioridad |
|---|---|---|
| C-01 | Identificación y alta del SAI y su batería desde el panel, con prueba de conexión, sin editar archivos de configuración a mano | Debe |
| C-02 | Catálogo (fabricantes, modelos) e inventario (servidor, SAI, batería) con ciclo de vida y baja lógica, más los vínculos temporales de montaje de batería y de cobertura del servidor | Debe |
| C-03 | Sondeo periódico con frecuencia configurable, y persistencia de cada muestra con su calidad (completa, parcial o perdida) y la procedencia de cada valor | Debe |
| C-04 | Derivación de eventos de energía (microcortes, cortes, retornos, tensión fuera de rango, pérdida de comunicación) con reglas versionadas, y alerta visual ante pérdida de comunicación con el equipo | Debe |
| C-05 | Políticas de apagado versionadas con modalidades de solo aviso, solo servidor, servidor y luego SAI con retorno, y ciclo forzado | Debe |
| C-06 | Verificación de supuestos de la política, con evidencia, método y vigencia, y degradación forzada a solo aviso mientras algún supuesto requerido no esté verificado | Debe |
| C-07 | Planificador interno que evalúa políticas, gestiona temporizadores con cancelación, ejecuta acciones y registra su resultado observado | Debe |
| C-08 | Panel con estado en vivo, conectividad, panel de supuestos verificados y eventos recientes, accesible desde la red local | Debe |
| C-09 | Prueba de batería programada (trimestral) y manual, con cadencia densa durante la prueba, y veredicto de salud propio con confianza explícita y comparación contra la línea base a carga igualada | Debe |
| C-10 | Históricos y gráficas de evolución (tensiones, carga, microcortes), individuales o superpuestas, con marcas de eventos, más agregación y retención con cobertura obligatoria | Debe |
| C-11 | Registro de intervenciones de servicio técnico con costos y efectos aplicados, y autenticación mínima de administrador único | Debe |
| C-12 | Interfaz de integración de ingesta idempotente para que fuentes externas empujen intervenciones sin duplicar hechos ante reintentos | Debe |
| C-13 | Ciclo de vida del SAI: reparación, sustitución y cobertura suplente, con registro de los días sin protección | Debería |
| C-14 | Informe de período y comparación de marcas y modelos por costo por año de servicio normalizado a moneda estable | Debería |
| C-15 | Modo de conexión simulado para probar políticas sin hardware ni riesgo y cubrir el camino de apagado en pruebas automatizadas | Debería |
| C-16 | Renovación automática de verificaciones por evidencia acumulada de la operación real | Debería |

### 4.2 Entregables

- El servicio en ejecución, entregado por etapas, cada una con una pantalla usable validada por el administrador.
- El panel de administración accesible desde la red local.
- La interfaz de integración de ingesta y un ejemplo de cliente de referencia que ejercita sus caminos de respuesta.
- El histórico persistente de muestras, eventos, pruebas, intervenciones y verificaciones.

### 4.3 Ambientes

- Ambiente de producción: el propio servidor protegido; el servicio corre allí, acotado a la red local.
- Ambiente de desarrollo: entorno reproducible en la máquina del desarrollador; el detalle de plataformas y versiones se documenta en `Compatibilidad-Plataformas-v1.0.md`.
- No hay ambiente intermedio de ensayo: no habría a qué equipo conectarlo.

## 5. Alcance excluido

| Funcionalidad excluida | Justificación | Versión futura tentativa |
|---|---|---|
| Apagado de otros equipos de la red | Excede el alcance; implica un protocolo de coordinación cuyo modo de falla es la corrupción simultánea en varias máquinas | No previsto |
| Múltiples SAI simultáneos | El modelo de datos ya los contempla, pero la implementación se acota a un solo SAI activo para no cargar la primera entrega | Versión futura: el modelo ya lo soporta |
| Notificaciones externas (correo, SMS) como mecanismo primario de alerta | En un corte de energía la red también cae, así que la notificación remota no es confiable; el histórico local es la fuente primaria | Como extra, no primario, una vez consolidado el histórico local |
| Gestión de usuarios y roles | Hay un único administrador; no hay jerarquía de usuarios que modelar | Solo si la solución se usara en un contexto multiusuario |
| Lectura del ajuste de arranque de la placa base por software | Frágil por versión de firmware y peligrosa al escribir; se sustituye por verificación por comportamiento | Nunca; reemplazada por verificación por comportamiento |
| Afirmaciones cuantitativas de salud de batería (porcentaje de estado, capacidad remanente, autonomía) conformes a norma | El equipo no expone los datos necesarios y la norma técnica de referencia es de pago y no fue adquirida; el método adoptado es tendencia relativa | Solo comprando la norma y con instrumental adicional |
| Reescritura del traductor de protocolo del equipo actual | Ya resuelto y verificado por la herramienta existente; reescribirlo reintroduciría riesgos ya conocidos | Nunca para este equipo |
| Implementación de la capa de extensiones de dialecto de protocolo | Queda diseñada pero no implementada; su interfaz no tiene sentido cerrarla antes de tener el servicio y un equipo que la necesite | Cuando aparezca un equipo no cubierto por la herramienta existente |

## 6. Supuestos

- El monitoreo básico y el apagado ordenado ante corte ya funcionan y no se reconstruyen; el proyecto los da por resueltos y verificados.
- El equipo relevado es representativo de lo que el servicio va a administrar; el relevamiento y la primera medición de referencia (2026-07-19) son válidos como punto de partida.
- El administrador tiene acceso físico al equipo para ejecutar la ventana de mantenimiento, que es destructiva por naturaleza y no se puede validar solo con software.
- La operación es de un único servidor, un único SAI activo y un único administrador durante todo el horizonte del proyecto.
- Las decisiones abiertas del arranque técnico (ubicación de la herramienta de acceso al equipo, cifrado del acceso en la red local, contratos aún no cerrados) se resuelven al inicio y no invalidan el alcance aquí definido; se listan como pendientes P-03 a P-06.

## 7. Restricciones

- Sin presupuesto asignado y sin fecha impuesta; el ritmo lo marcan las etapas de validación humana, cada una cerrada con una verificación del administrador antes de arrancar la siguiente.
- Alcance físico fijo: un solo servidor, un solo SAI activo, un solo administrador; acceso restringido a la red local, sin exposición a internet.
- El corte del SAI tiene un techo de tiempo duro para completar el apagado, con parte de ese presupuesto ya comprometido; el formulario de políticas debe rechazar valores por encima del techo.
- El reencendido automático depende de un ajuste de la placa base no legible por software; solo se verifica por comportamiento.
- Todo importe se registra con moneda y fecha; el equivalente en moneda estable viaja marcado como derivado.
- El sistema no puede presentar sus veredictos de salud como conformes a ninguna norma técnica, porque la norma de referencia no fue adquirida.

## 8. Criterios de aceptación del proyecto

- El servicio arranca siempre en modo solo aviso y no ejecuta ningún apagado mientras algún supuesto requerido no esté verificado.
- Toda capacidad marcada como debe (C-01 a C-12) está entregada y validada en pantalla por el administrador.
- Todo valor almacenado declara su procedencia, sin excepción, verificado como prueba de invariante.
- El histórico permite reconstruir, para cualquier tramo, qué equipo protegía al servidor y cuántos días quedó sin protección.
- Cada acción sobre el equipo se registra por su efecto observado, nunca por ausencia de error.
- La interfaz de integración responde de forma idempotente ante el reintento del mismo hecho y rechaza los hechos incoherentes o con costos que no cuadran.
- El veredicto de salud de batería se emite con su confianza explícita y su reserva declarada, y nunca como afirmación cuantitativa conforme a norma.

## 9. Gestión de cambios de alcance

Al ser un proyecto de un único responsable que concentra propietario, implementador y beneficiario, no hay comité ni cliente externo que apruebe cambios. Aun así, todo cambio de alcance se registra de forma explícita: se incorpora como nueva capacidad o exclusión en una versión posterior de este documento, con su justificación, en vez de ampliarse de hecho durante la construcción. Las capacidades marcadas como debería (C-13 a C-16) y las exclusiones con versión futura tentativa son los puntos de expansión previstos; promoverlas a incluidas es un cambio de alcance que exige nueva versión. Las decisiones abiertas del arranque técnico no son cambios de alcance: se cierran como decisiones de arquitectura sin alterar lo aquí definido.

## 10. Trazabilidad

Upstream: deriva del SOLUTION-INTAKE §1 (problema), §4 (alcance funcional pretendido y su priorización), §5 (historias de usuario), §6 (flujos típicos), §9 (exclusiones declaradas), §10 (restricciones del cliente) y §15 (esquema de descomposición y delivery por etapas).

Downstream: alimenta 01-Necesidades-Negocio (necesidades por capacidad incluida), 02-Especificacion-Funcional (casos de uso; las exclusiones quedan registradas para que no se generen por error), 03-UX-UI-DX (pantallas del panel por capacidad), 05-Arquitectura-Tecnica (restricciones y decisiones abiertas que se cierran como ADR), 07-Plan-Sprint (capacidades priorizadas que ordenan las etapas) y 11-Examples (ejemplo de la interfaz de integración). Las decisiones abiertas se registran como pendientes P-03 a P-06 del intake.
