# Visión de Producto

**Proyecto:** Sai-Service-Core
**Documento:** Vision-Producto-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-00)
**Trazabilidad upstream:** SOLUTION-INTAKE §1, §2, §3, §8, §10, §11, §12
**Trazabilidad downstream:** 01-Necesidades-Negocio, 02-Especificacion-Funcional, 03-UX-UI-DX, 05-Arquitectura-Tecnica, 07-Plan-Sprint, 11-Examples

## 1. Problema de negocio

Un servidor doméstico y de laboratorio, de criticidad alta y sin copias de respaldo, está respaldado por un equipo de alimentación ininterrumpida (SAI). El monitoreo básico y el apagado ordenado ante un corte ya están resueltos por herramientas existentes y verificadas: eso no se reconstruye. El dolor está en cuatro carencias que ninguna herramienta —libre ni comercial— cubre para este equipo:

- No hay histórico de salud de la batería. El equipo relevado no expone ningún indicador de salud y su autoprueba no emite veredicto: la bandera de «reemplazar batería» en la que se apoya el monitoreo convencional nunca se enciende en este equipo, así que un monitoreo estándar no alerta jamás. La salud solo puede obtenerse midiendo la caída de tensión durante la autoprueba y guardando la serie en el tiempo.
- No hay modelo de ciclo de vida de los equipos. Altas, recambios de batería, reparaciones, sustitución del SAI y la asociación de cada medición al período de la batería que estaba montada no tienen dónde registrarse; el registro actual es texto plano.
- No hay verificación viva de los supuestos. Que el servidor vuelva a encenderse solo tras un corte es un supuesto que puede volverse falso en silencio (por ejemplo, por agotamiento de la pila de la placa o un cambio de ajuste no documentado). Nada lo vigila.
- No hay panel remoto ni interfaz de administración: solo variables crudas expuestas por la herramienta existente. La alternativa del fabricante fue descartada por una vulnerabilidad de ejecución remota sin parche.

Debajo de todo eso hay un problema crítico y bien delimitado: garantizar el reencendido. El servidor solo vuelve a encenderse solo si detecta una transición de ausencia a presencia de energía; por lo tanto, el SAI debe cortar su propia salida aunque el servidor ya esté apagado. Si la energía vuelve durante la cuenta regresiva y el SAI cancela su corte, no hubo transición, no hay nada que detectar y el servidor queda apagado hasta que alguien apriete el botón. Es el peor resultado posible: el sistema se protegió de un corte breve y, a cambio, quedó fuera de servicio indefinidamente.

Qué pasa si no se construye: como el servidor no tiene respaldo, un corte prolongado corrompe datos sin red de contención; la batería se degrada sin que nada avise, porque la única señal que el monitoreo convencional observa nunca se activa; y no hay forma de saber cuánto duró de verdad una batería ni de decidir la próxima compra con datos.

Por qué ahora: el relevamiento del equipo ya está hecho y verificado (2026-07-19) y la primera medición de referencia de salud de batería ya fue tomada. La batería está en servicio desde 2024, así que cada trimestre sin registrar la tendencia es un punto de datos perdido.

## 2. Audiencia y stakeholders

Este es un proyecto interno y autopromovido: los tres roles de propietario, implementador y beneficiario recaen en una única persona, el administrador del servidor. La clasificación por categoría se declara igual para dejar explícita la cobertura, pero no describe personas distintas salvo donde se indica.

| Rol | Nombre o cargo | Categoría | Nivel de involucramiento | Responsabilidad principal |
|---|---|---|---|---|
| Administrador único | Administrador del servidor (proyecto interno) | Propietario, Implementador y Beneficiario (una sola persona) | Total, permanente | Aprueba el alcance, da de alta los equipos, configura políticas, monitorea, consulta históricos, dispara pruebas, carga intervenciones, ejecuta la ventana de mantenimiento con presencia física y emite informes |
| Servidor protegido | Host de criticidad alta, sin respaldo | Beneficiario (sistema) | Objeto de la protección | Es lo que se protege: el apagado ordenado y el reencendido automático operan sobre él |
| Proveedor / técnico externo | Servicio técnico de batería y reparación | Beneficiario indirecto / ejecutor | Puntual, por intervención | Ejecuta recambios, reparaciones e inspecciones; recibe las baterías retiradas y consta para trazabilidad ambiental |
| Sistema externo de gestión de mantenimiento | Plataforma de gestión de mantenimiento de terceros | Integrador / consumidor | Ocasional, automatizado | Empuja intervenciones sin intervención humana, con confianza declarada menor que la del dato medido localmente |

## 3. Propuesta de valor

Lo que se usa hoy monitorea y apaga ordenadamente, y nada más: no tiene inventario, expone variables en lugar de una interfaz de administración y produce registros en texto plano. Las alternativas evaluadas y descartadas con fundamento son el software del fabricante (vulnerabilidad sin parche) y el armado con herramientas genéricas de tableros y automatización del hogar, sobre las que se concluyó que ninguna calcula salud de batería a partir de los datos disponibles: todas se limitan a retransmitir la bandera del firmware, que en este equipo nunca llega.

La promesa central tiene dos partes: (1) garantizar que el servidor vuelva a encenderse solo tras un corte, negándose a apagarlo mientras no pueda probarlo; y (2) construir el histórico de salud, ciclo de vida y costos que ninguna herramienta existente construye para este equipo.

Diferenciadores, en orden de defendibilidad:

1. Cálculo propio de salud de batería, con procedencia y límites declarados: el veredicto lo emite el servicio porque el equipo no emite ninguno, y viaja con su nivel de confianza y su advertencia.
2. Regla de bloqueo de seguridad: el sistema no habilita el apagado si algún supuesto del que depende no está verificado; la acción queda bloqueada y la modalidad efectiva degrada a solo aviso.
3. Verificación continua por evidencia: el servicio cruza sus propios registros de corte con la evidencia de arranque del servidor para probar que el reencendido automático sigue funcionando, sin repetir la prueba destructiva.
4. Trazabilidad total del origen de cada valor: cada número declara si fue medido, derivado, estimado, declarado, imputado o no calculable.
5. Comparación de marcas por desempeño real observado, con el costo por año de servicio normalizado a una moneda estable, necesario en un contexto de alta inflación donde comparar importes de años distintos sin normalizar no significa nada.

## 4. Visión a 3 años

En tres años, el servicio es la fuente única de verdad sobre la energía del servidor: decide y ejecuta el apagado ordenado solo cuando puede probar que el servidor volverá a encenderse, y mantiene esa garantía viva sin repetir pruebas destructivas. Acumula varios años de histórico de salud de batería con tendencia legible y confianza creciente, y una biblioteca de fichas de vida útil cerradas que permite decidir cada compra de batería con datos de desempeño y costo real, y no por precio de lista. El conjunto de equipos queda modelado por completo —altas, recambios, reparaciones, sustituciones y cobertura temporal—, de modo que el histórico responde en cualquier momento qué equipo protegía al servidor en cada tramo y cuántos días quedó sin protección. La solución permanece deliberadamente acotada a un único servidor, un único SAI activo y un único administrador; el crecimiento previsto es en profundidad de histórico y confianza de veredictos, no en cantidad de equipos ni de usuarios.

## 5. Objetivos SMART

Los objetivos toman su métrica y su línea base de datos verificados en la fuente; los targets son propuestos sobre esos datos y requieren ratificación del administrador (pendiente P-01 del intake).

| Objetivo | Métrica | Target | Plazo | Responsable |
|---|---|---|---|---|
| Salir del modo degradado y habilitar el apagado real | Supuestos verificados sobre supuestos requeridos | 4 de 4 verificados, con modalidad efectiva de apagado con retorno | 1 mes desde la puesta en marcha | Administrador |
| Volver la tendencia de salud estadísticamente utilizable | Pruebas de batería comparables acumuladas | ≥ 4 pruebas comparables (umbral para pasar de confianza baja a tendencia legible) | 12 meses, a cadencia trimestral | Administrador |
| Mantener el servidor protegido de forma continua | Días con cobertura vigente sobre días del período, y arranques no gestionados | ≥ 0,98 de días con cobertura y 0 arranques por corte no gestionado | Primer año calendario completo de operación | Administrador |
| Volver el histórico apto para decidir una compra | Modelos de batería con ficha de vida útil cerrada y costo por año de servicio normalizado | ≥ 2 modelos comparables | Al cierre del primer recambio posterior a la puesta en marcha | Administrador |
| No mentir sobre el origen de los datos | Valores almacenados sin origen declarado | 0 excepciones | Desde la introducción de la persistencia | Administrador |

## 6. Métricas de éxito

Se reusan las cinco métricas de resultado de negocio del intake §8. Sus targets son propuestos sobre las líneas base verificadas y requieren ratificación (pendiente P-01).

| Criterio | Métrica | Target | Plazo | Fuente del dato |
|---|---|---|---|---|
| El servicio sale del modo degradado y puede apagar el servidor | Supuestos verificados sobre requeridos | 4 de 4, modalidad efectiva de apagado con retorno (target propuesto) | 1 mes desde la puesta en marcha | Registro de verificaciones (línea base 0 de 4 al alta) |
| La tendencia de salud de batería es utilizable | Pruebas de batería comparables acumuladas | ≥ 4 pruebas comparables (target propuesto) | 12 meses | Registro de pruebas (línea base 1 prueba, 2026-07-19) |
| El servidor queda protegido de forma continua | Días con cobertura sobre días del período; arranques no gestionados | ≥ 0,98 y 0 arranques por corte no gestionado (target propuesto) | Primer año completo | Registro de cobertura y evidencia de arranque (línea base 1,0 en período de ejemplo) |
| El histórico es apto para decidir una compra | Modelos con ficha de vida útil cerrada y costo por año normalizado | ≥ 2 modelos comparables (target propuesto) | Al primer recambio posterior | Fichas de vida útil (línea base 1 ficha proyectable, batería desde 2024) |
| El servicio no miente sobre el origen de sus datos | Valores almacenados sin origen declarado | 0 (target no negociable) | Desde la persistencia | Prueba de invariante en el proceso de construcción |

## 7. Restricciones

- Proyecto interno de un único desarrollador que es a la vez propietario, implementador y beneficiario. No hay presupuesto asignado ni fecha impuesta: la ausencia es deliberada. El ritmo lo marcan etapas de validación humana, cada una cerrada con una verificación del administrador.
- Alcance físico acotado y no negociable: un solo servidor protegido, un solo SAI activo (el conectado al servidor) y un solo administrador. Sin gestión de usuarios ni de roles.
- Acceso restringido a la red local, sin exposición a internet.
- El reencendido automático depende de un ajuste de la placa base del servidor que no es legible por software; solo puede verificarse por comportamiento.
- El corte del SAI dispone de un techo de tiempo duro impuesto por el equipo para completar el apagado; ese presupuesto es fijo y parte de él ya está comprometido por la contención del entorno del servidor.
- Restricciones legales acotadas: las normas técnicas de referencia sobre baterías son de pago y no fueron adquiridas, por lo que el sistema no puede presentar sus veredictos como conformes a ninguna norma; y la disposición final de cada batería retirada se registra para trazabilidad ambiental (queda pendiente confirmar si existe normativa local aplicable, P-02). El sistema no maneja datos personales más allá del contacto de un proveedor.
- Todo importe se registra con moneda y fecha, y su equivalente en moneda estable viaja marcado como derivado; el contexto de alta inflación lo exige.

## 8. Riesgos

| ID | Riesgo | Probabilidad | Impacto | Mitigación | Responsable |
|---|---|---|---|---|---|
| R-12 | El servicio toma la decisión de apagar un servidor sin respaldo; si falla, falla de noche y sin testigos | Media | Crítico | Arranque forzado en modo solo aviso y bloqueo por verificación: no apaga nada hasta poder probar que el servidor vuelve a encenderse | Administrador |
| R-01 | El ciclo de apagado y reencendido no está verificado en este equipo; una trampa de firmware podría dejar el SAI apagado indefinidamente | Media | Crítico | Prueba física en ventana de mantenimiento antes de habilitar cualquier modalidad distinta de solo aviso | Administrador |
| R-09 | Sin sensor de temperatura, la oscilación estacional puede rivalizar con la señal de degradación de la batería | Cierta | Alto para la salud de batería | Toda conclusión de salud lleva esa reserva declarada de forma explícita | Administrador |
| R-13 | Guardar un valor interpolado como si fuera medido produce una conclusión falsa sobre datos que parecían medidos | Alta | Alto | Procedencia obligatoria en todo valor; los valores derivados o estimados no entran en la tendencia de salud y se marcan en pantalla | Administrador |
| R-02 | El presupuesto de tiempo de apagado no está medido contra el apagado real del servidor | Alta | Alto | Cronometrar en la ventana de mantenimiento antes de habilitar el apagado; verificación con vigencia corta | Administrador |
| R-14 | Ninguna herramienta libre calcula salud de batería desde estos datos: obliga a ser conservador con las conclusiones | Cierta | Medio | Confianza explícita en cada veredicto, arrancando en baja; el veredicto solo afirma que la batería se comporta peor que antes | Administrador |

## 9. Glosario del dominio

| Término | Definición | Sinónimos o notas |
|---|---|---|
| SAI | Sistema de alimentación ininterrumpida: equipo que sostiene la alimentación del servidor cuando falla la red eléctrica | UPS |
| Apagado ordenado | Detener el sistema y sincronizar los discos antes de perder la alimentación, en vez de sufrir un corte abrupto | — |
| Reencendido automático | Que el servidor vuelva a arrancar solo al restablecerse la energía; depende de que detecte una transición de ausencia a presencia de energía, y por eso exige que el SAI corte su salida aunque el servidor ya esté apagado | Autoencendido |
| Ciclo forzado | Modalidad en la que, iniciada la secuencia de apagado, el corte del SAI no se cancela aunque vuelva la red, para evitar que el servidor quede apagado indefinidamente | — |
| Salud de batería | Grado de degradación de la batería respecto de su estado nuevo; acá no es un porcentaje sino una tendencia relativa derivada de la caída de tensión durante la autoprueba a carga igualada | Término «estado de salud» evitado a propósito |
| Línea base | Primera medición de referencia contra la que se comparan las pruebas posteriores | — |
| Supuesto y verificación | Afirmación de la que depende la política de apagado, con su evidencia, su método, su fecha y su vigencia; estados: nunca verificado, verificado, vencido, refutado (refutado bloquea; vencido solo pide repetir) | — |
| Procedencia | Origen declarado de cada valor almacenado: medido, derivado, estimado, declarado, imputado o no calculable | Responde si un número lo midió el aparato o lo calculó el software |
| Vínculo temporal | La relación «qué estuvo con qué y cuándo», modelada como período con intervalo, que permite representar que una batería se retiró, se probó y se reinstaló | — |
| Días sin protección | Días en que el servidor no tuvo ningún SAI cubriéndolo, medidos como el hueco entre dos períodos de cobertura | — |

## 10. Trazabilidad

Upstream: este documento deriva del SOLUTION-INTAKE §1 (idea y problema), §2 (audiencia y stakeholders), §3 (propuesta de valor y diferenciación), §8 (métricas de éxito de negocio), §10 (restricciones del cliente), §11 (riesgos) y §12 (glosario del dominio).

Downstream: alimenta 01-Necesidades-Negocio (necesidades derivadas del problema y la propuesta de valor), 02-Especificacion-Funcional (casos de uso y reglas de negocio), 03-UX-UI-DX (experiencia del panel y del administrador), 05-Arquitectura-Tecnica (restricciones y riesgos que condicionan las decisiones y los ADR), 07-Plan-Sprint (objetivos y métricas que fijan prioridad) y 11-Examples (ejemplos ejecutables que ilustran la propuesta de valor). Los targets de las métricas quedan como decisión abierta P-01 y su ratificación se registra donde corresponda.
