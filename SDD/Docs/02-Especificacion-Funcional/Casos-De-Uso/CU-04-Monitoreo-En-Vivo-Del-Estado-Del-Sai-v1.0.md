# CU-04 — Monitoreo en vivo del estado del SAI

**Proyecto:** Sai-Service-Core
**Documento:** CU-04-Monitoreo-En-Vivo-Del-Estado-Del-Sai-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador vea desde cualquier equipo de la LAN el estado en vivo del equipo de alimentación: tensiones, carga, estado de batería, conectividad, cuántos supuestos están verificados y los eventos recientes. Es el flujo del ochenta por ciento del tiempo. Incluye el sondeo periódico, la persistencia de cada muestra con su calidad y la derivación de eventos de energía sobre la ventana reciente.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Observa el estado en vivo y los eventos recientes desde la LAN |
| Planificador interno | Sistema | Sondea el equipo en cada intervalo, persiste la muestra y deriva eventos |
| Adaptador de conexión con el equipo | Sistema | Responde el estado del equipo cuando se lo consulta |

## 3. Precondiciones

- El parque está dado de alta y existe una sesión de sondeo activa (CU-02).
- El administrador tiene una sesión activa para ver el panel (CU-01).

## 4. Flujo principal

1. El administrador abre el panel de estado en vivo desde un equipo de la LAN.
2. El planificador, en cada intervalo de sondeo (5 segundos por defecto), solicita el estado al equipo a través del adaptador de conexión.
3. Si la respuesta llega completa, el sistema persiste una muestra con calidad completa; si llega incompleta, con calidad parcial, conservando las variables que sí llegaron; si no llega, con calidad perdida y valores sin dato, e incrementa el contador de sondeos fallidos.
4. Cada valor persistido lleva su origen: medido, derivado, estimado por el driver, o no calculable con su motivo.
5. El sistema evalúa las reglas de derivación sobre la ventana reciente y genera los eventos que correspondan (CU se apoya en la derivación de eventos como parte de este flujo).
6. El sistema empuja al panel el estado y los eventos nuevos: tensiones, carga, estado de batería con la carga de batería marcada como derivada, conectividad, cuántos de cuántos supuestos están verificados y los eventos recientes con su regla y versión.
7. Si falta algún supuesto requerido, el panel avisa en la pantalla principal que la política está degradada a solo aviso.

## 5. Flujos alternativos

- FA-1 Respuesta parcial del equipo. Disparador: el equipo responde sin alguna variable (por ejemplo sin la carga). El sistema persiste la muestra como parcial, con la variable faltante sin dato y su motivo, y conserva el resto. Retorna al paso 5.
- FA-2 Elevación de cadencia durante una prueba de batería. Disparador: se está ejecutando una prueba de batería (CU-07). El planificador eleva la cadencia de sondeo a un muestreo por segundo mientras dura la prueba y la restaura al terminar.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| PERDIDA_COMUNICACION | Tres sondeos consecutivos sin respuesta del equipo | Genera el evento de desconexión y muestra alerta visual de conectividad en el panel |
| TENSION_FUERA_DE_RANGO | Tensión de entrada fuera del rango de 198 a 242 voltios sostenida 30 segundos | Genera el evento de tensión fuera de rango y lo muestra entre los eventos recientes |

## 7. Postcondiciones

- Éxito: por cada intervalo hay una muestra persistida con su calidad y la procedencia de cada valor; el panel refleja el estado en vivo y los eventos recientes; el estado de verificación de supuestos es visible.
- Fallo: ante pérdida de comunicación, el sistema deja registro de los sondeos fallidos y alerta; no descarta silenciosamente el problema.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Un sondeo cada 5 segundos con el equipo respondiendo estado en línea a 232,9 voltios de entrada | El planificador ejecuta una ronda con respuesta completa | El sistema persiste una muestra de calidad completa y el panel muestra tensión de entrada 232,9 voltios |
| CA-02 | Una respuesta del driver sin la carga del equipo | El planificador ejecuta la ronda | El sistema persiste la muestra como parcial, con la carga sin dato y su motivo, y conserva las demás variables |
| CA-03 | La carga de batería informada por el driver | El panel muestra el estado de batería | El valor de carga de batería aparece marcado como derivado, no como medido |
| CA-04 | Tres sondeos consecutivos sin respuesta del equipo | El planificador completa el tercer sondeo fallido | El sistema genera el evento de desconexión y muestra la alerta de conectividad (PERDIDA_COMUNICACION) |
| CA-05 | Una versión de política con tres de cuatro supuestos sin verificar | El administrador observa el panel | El panel indica en la pantalla principal 1 de 4 supuestos verificados y modalidad degradada a solo aviso |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-02 (Monitoreo en vivo y alertas); toca NB-03 (procedencia) y NB-05 (panel de supuestos) |
| Reglas de negocio aplicables | RN-05, RN-06; RN-03 de forma indirecta |
| Historias de usuario a generar | US-02, US-04, US-05 en 06 |
| Componentes esperados | Planificador interno, adaptador de conexión, reglas de derivación, panel de estado en vivo (referencia tentativa a 05) |
| Tests previstos | Persistencia de muestra completa, parcial y perdida; marca de derivado en carga de batería; alerta a los 3 sondeos fallidos; visualización de supuestos (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancias de referencia: escenarios §20.E-2 (sondeo normal con procedencia por variable) y §20.E-3 (microcorte).
- La derivación de eventos de energía y la vigilancia de conectividad forman parte de este flujo; sus reglas versionadas se detallan en el modelo conceptual y en RN aplicables.
- Un microcorte más corto que el intervalo de sondeo no es detectable de forma confiable; el sistema lo declara en vez de ocultarlo.
- La disposición de gráficos y widgets en vivo es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial en cuanto a usuarios (un solo administrador). El único concurrente relevante es el planificador que escribe muestras mientras el panel lee; el modelo de un solo proceso escritor lo resuelve por diseño.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-3 (§6), §20.E-2, §20.E-3 y NB-02 |
