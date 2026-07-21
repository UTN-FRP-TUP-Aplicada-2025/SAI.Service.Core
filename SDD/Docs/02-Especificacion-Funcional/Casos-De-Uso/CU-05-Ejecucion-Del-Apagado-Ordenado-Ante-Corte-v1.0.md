# CU-05 — Ejecución del apagado ordenado ante corte sostenido

**Proyecto:** Sai-Service-Core
**Documento:** CU-05-Ejecucion-Del-Apagado-Ordenado-Ante-Corte-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Ejecutar la decisión del camino crítico: ante un corte de energía que se sostiene más allá del umbral de disparo de la política vigente, el planificador decide y ejecuta el apagado ordenado del host y, según la modalidad, el corte de la salida del equipo con retorno, garantizando el reencendido. El sistema se niega a apagar si algún supuesto requerido no está verificado y, en ese caso, degrada a solo aviso. Es la única acción con consecuencias irreversibles.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Planificador interno | Primario | Detecta la condición de disparo, evalúa la modalidad efectiva y ejecuta o bloquea la acción |
| Adaptador de conexión con el equipo | Sistema | Ordena el apagado con retorno y confirma el efecto observado |
| Host protegido | Secundario | Recibe el apagado ordenado y debe reencender al restablecerse la energía |

## 3. Precondiciones

- Existe una versión de política vigente con una modalidad y un umbral de disparo (CU-03).
- El monitoreo en vivo está activo y detecta el estado del equipo (CU-04).
- Existe el conjunto de verificaciones de supuestos requeridas por la modalidad.

## 4. Flujo principal

1. El planificador detecta que el equipo está en batería y que la condición se sostiene el tiempo del umbral de disparo (por ejemplo 300 segundos).
2. El planificador crea la acción, referida a la versión de política vigente, con la modalidad solicitada.
3. El planificador evalúa las verificaciones requeridas por la modalidad. Si todas están verificadas, la modalidad efectiva coincide con la solicitada.
4. El planificador ordena el apagado ordenado del host, respetando el tiempo reservado para el apagado, que no supera el techo duro de 540 segundos.
5. El planificador ordena al equipo cortar su salida con retorno, de modo que produzca la transición que dispara el reencendido, y no cancela ese corte aunque vuelva la red si la modalidad es de ciclo forzado.
6. El planificador confirma cada acción por su efecto observado, no por ausencia de error, y registra el resultado.
7. Al restablecerse la energía, el equipo repone la salida y el host reenciende solo; el sistema registra el cierre del ciclo.

## 5. Flujos alternativos

- FA-1 Corte breve por debajo del umbral. Disparador: el estado en batería vuelve a línea antes de alcanzar el umbral de disparo. El planificador registra el evento (microcorte o corte breve) sin desencadenar acción, con el motivo por el que no actuó. Termina el caso sin apagar.
- FA-2 Corte real que verifica un supuesto por evidencia. Disparador: durante el corte, el equipo señala estar en batería, lo que constituye evidencia. El sistema renueva por evidencia la verificación del supuesto correspondiente, sin prueba destructiva, y lo registra.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| BLOQUEADA_POR_VERIFICACION | Alguna verificación requerida por la modalidad está sin verificar, vencida o refutada | La acción resulta bloqueada por verificación, la modalidad efectiva degrada a solo aviso y no se apaga el host (RN-02) |
| EFECTO_NO_CONFIRMADO | Una orden al equipo o al host no puede confirmarse por efecto observado | No da la acción por ejecutada, registra el efecto no confirmado y mantiene el estado seguro (RN-03) |
| NOTIFICACION_REMOTA_FALLIDA | Los canales remotos de aviso no salen porque la red también cayó | Registra el fallo de cada canal; el registro local sobrevive y es la fuente primaria; no se considera fallo del apagado |

## 7. Postcondiciones

- Éxito con supuestos verificados: el host se apagó de forma ordenada, el equipo cortó con retorno y el host reenciende al volver la energía; la acción queda registrada con su versión de política y su resultado.
- Bloqueo por verificación: no se apagó nada; la acción quedó registrada como bloqueada por verificación con su motivo y la modalidad efectiva fue solo aviso.
- Fallo de confirmación: el sistema no reporta como ejecutado lo que no pudo observar.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Una versión de política con modalidad host luego equipo con retorno y tres de cuatro supuestos sin verificar, durante un corte de 370 segundos | El planificador alcanza el instante de decisión a los 300 segundos en batería | La acción resulta bloqueada por verificación, la modalidad efectiva es solo aviso y el host no se apaga (RN-02) |
| CA-02 | La misma situación de corte | El planificador registra la acción de decisión | La acción referencia una versión de política, nunca la política directamente (RN-11) |
| CA-03 | Una versión de política con tiempo reservado para el apagado de 240 segundos | El planificador prepara el apagado | El tiempo reservado respeta el techo duro de 540 segundos (RN-04) |
| CA-04 | Un corte real en el que el equipo señala estar en batería | El planificador observa el estado en batería | El sistema renueva por evidencia la verificación del supuesto de señal en batería, sin prueba destructiva |
| CA-05 | Los cuatro supuestos requeridos verificados y un corte sostenido más allá del umbral | El planificador ejecuta el apagado con retorno | El sistema apaga el host, ordena el corte con retorno y confirma cada acción por efecto observado (RN-03) |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-01 (Apagado ordenado y reencendido garantizado); NB-05 (Seguridad operativa) |
| Reglas de negocio aplicables | RN-02, RN-03, RN-04, RN-11 |
| Historias de usuario a generar | US-02, US-04 en 06 (ejecución y visibilidad del bloqueo) |
| Componentes esperados | Planificador interno con temporizadores cancelables, adaptador de conexión, máquina de decisión de modalidad (referencia tentativa a 05) |
| Tests previstos | Degradación a solo aviso con supuestos sin verificar, acción referida a versión de política, respeto del techo de 540 segundos, validación por efecto observado, renovación por evidencia (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-4 (corte prolongado, la política dispara y el sistema se niega), incluida la diferencia entre modalidad solicitada y modalidad efectiva.
- La prueba física completa del ciclo de apagado y reencendido no es automatizable; se cubre con el adaptador simulado para la lógica y con la evidencia de la ventana de mantenimiento (CU-10) para el comportamiento real.
- El actor primario es el planificador, que actúa de forma autónoma; el administrador es beneficiario y observa el resultado en el panel.

## 11. Interacción multiusuario y concurrencia

Trivial en usuarios. La concurrencia relevante es interna: la ronda de evaluación debe completarse en menos de un segundo para no desplazar la ronda siguiente en un intervalo de cinco segundos.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §6, §20.E-4, NB-01 y NB-05 |
