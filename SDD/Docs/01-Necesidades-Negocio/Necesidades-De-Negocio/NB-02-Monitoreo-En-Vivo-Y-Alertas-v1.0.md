# NB-02 — Monitoreo en vivo del estado del SAI y alertas de conectividad

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-02-Monitoreo-En-Vivo-Y-Alertas-v1.0.md |
| Versión | 1.2 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §4 (F-06, F-08, F-09, F-13), §7 (CL-11, CL-12, CL-14), §11 (R-06); Vision-Producto-v1.0.md §1, §3; Alcance-Proyecto-v1.0.md §4 (C-03, C-04, C-08) |
| Trazabilidad downstream | CU-04, CU-02, CU-06 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita ver el estado del equipo de alimentación en tiempo real desde cualquier puesto de la red local, y enterarse de un problema sin estar sentado frente al servidor. Hoy las herramientas existentes exponen variables crudas, no una interfaz de administración: para saber qué pasa hay que consultar el equipo a mano, y no hay una vista única que muestre tensiones, estado de la red eléctrica, conectividad y eventos recientes.

Además, la comunicación con el equipo puede caerse en silencio: el nodo físico de conexión desaparece del bus sin producir ningún error, y un monitoreo ingenuo seguiría mostrando el último valor conocido como si todo estuviera bien. La necesidad no es solo mostrar datos, sino vigilar la propia capacidad de observar: avisar cuando el sistema dejó de ver el equipo, en vez de mentir con un estado viejo.

Importa porque el monitoreo en vivo es el 80 por ciento del uso cotidiano y es la base sobre la que se toman las decisiones de apagado y se detectan los cortes. Un dato fresco y honesto sobre conectividad es la primera línea de confianza del servicio.

## 2. Ejemplo de uso desde la perspectiva del negocio

El administrador abre el panel desde su portátil en la red local un día cualquiera. Ve la tensión de entrada, el estado de la red, la carga del equipo y cuántos de los supuestos de la política están verificados. A media tarde, el cable de conexión del equipo se afloja. En pocos segundos el panel deja de mostrar valores frescos y aparece una alerta de pérdida de comunicación, en la pantalla principal y no enterrada en una pantalla de configuración. El administrador revisa el conector antes de que el problema pase inadvertido durante días.

## 3. Impacto

- Visibilidad operativa: da una vista única del estado que hoy solo se obtiene consultando variables crudas del equipo.
- Detección temprana de fallas: una pérdida de comunicación no anunciada deja al sistema ciego justo cuando más importa.
- Insumo para la decisión de apagado: el estado en vivo y los eventos derivados alimentan el camino crítico (NB-01).
- Honestidad del dato: distingue un valor fresco de uno viejo, y una muestra completa de una parcial, en vez de presentar todo por igual.
- Si queda sin resolver: el administrador sigue dependiendo de la consulta manual y de la suerte de estar mirando en el momento justo.

## 4. Problema específico que resuelve

- Que el estado del equipo se actualice a intervalo corto y se vea desde la red local sin exposición a internet.
- Que una pérdida de comunicación con el equipo se detecte y se avise, en vez de mostrar el último estado como vigente.
- Que las muestras incompletas se conserven en lugar de descartarse, porque perder la muestra entera perdería las variables que sí llegaron.
- Que a partir de la serie reciente se deriven los eventos de energía (microcortes, cortes, retornos, tensión fuera de rango) que el negocio necesita para su historia.
- Que la tensión de entrada fuera del rango aceptable se marque como evento cuando se sostiene el tiempo suficiente.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Frescura del estado en el panel | Intervalo de sondeo | 5 s (configurable) | Desde el monitoreo en producción |
| Detección de pérdida de comunicación | Sondeos consecutivos sin respuesta hasta emitir alerta | 3 | Continuo |
| Detección de tensión fuera de rango | Tiempo sostenido fuera de [198, 242] V hasta registrar el evento | 30 s | Continuo |
| Conservación de muestras parciales | Muestras parciales descartadas | 0 (se conservan) | Continuo |
| Disponibilidad del sondeo | Rondas de sondeo completadas sobre esperadas | ≥ 0,99 mensual [derivado, SLO propuesto] | Mensual |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Define qué debe verse en la pantalla principal y aprueba el umbral de alerta de conectividad |
| Administrador único (rol implementador) | Implementador | Construye el sondeo periódico, la vigilancia de conectividad y la derivación de eventos |
| Administrador único (rol beneficiario) | Beneficiario | Consume el panel en vivo desde la red local para enterarse de un problema sin estar frente al servidor |
| Poller local fd-poller-local | Componente del sistema (fuente de dato de máxima confianza) | Aporta las muestras medidas con su calidad y su procedencia |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-02 | CU-04 Monitoreo en vivo del estado del SAI | aprobada |
| NB-02 | CU-02 Alta de equipos y puesta en marcha | aprobada |
| NB-02 | CU-06 Históricos y gráficas de evolución del suministro | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-03 (Historia trazable con procedencia): cada muestra medida se persiste con su calidad y su origen.
- Depende de NB-04 (Ciclo de vida de los equipos): para sondear hace falta un equipo dado de alta y su vínculo de cobertura vigente.

## 9. Prioridad MoSCoW

Must Have. Es el uso cotidiano del servicio y la base de observación sobre la que se apoyan el apagado y la historia (SOLUTION-INTAKE §4, F-06, F-09 y F-13).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §4, §7 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
| 1.2 | 2026-07-20 | Retroalimentación de la Fase B2: unificación de terminología 'parque' → 'equipos' |
