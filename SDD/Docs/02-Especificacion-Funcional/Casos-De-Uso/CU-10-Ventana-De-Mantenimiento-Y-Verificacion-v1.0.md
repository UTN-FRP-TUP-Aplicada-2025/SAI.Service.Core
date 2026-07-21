# CU-10 — Ventana de mantenimiento y verificación de supuestos

**Proyecto:** Sai-Service-Core
**Documento:** CU-10-Ventana-De-Mantenimiento-Y-Verificacion-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador, con presencia física y en una ventana planificada, verifique uno por uno los cuatro supuestos de los que depende el apagado automático, para desbloquearlo con evidencia y no con optimismo. Es el flujo crítico que rara vez pasa y no puede fallar: implica cortar la energía y es destructivo por naturaleza. Sin él, el sistema nunca sale del modo de solo aviso.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador con presencia física | Primario | Ejecuta el checklist de verificación paso a paso y registra la evidencia |
| Servicio de verificaciones | Sistema | Presenta el checklist, registra el resultado de cada supuesto y actualiza su vigencia |
| Adaptador de conexión con el equipo | Sistema | Ejecuta el corte con retorno y responde el estado durante la prueba |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01) y está físicamente presente junto al equipo.
- Existen las cuatro verificaciones sembradas (CU-02), en estado sin verificar o vencido.
- La ventana se planificó y los contenedores del host se pueden detener de forma controlada.

## 4. Flujo principal

1. El administrador inicia la ventana de verificación desde el panel, que muestra el checklist de los cuatro supuestos.
2. Con los procesos del host detenidos, se cronometra el apagado completo bajo carga y se registra el tiempo; el supuesto del presupuesto de apagado pasa a verificado, con vigencia corta a propósito de 180 días, porque la carga del host cambia.
3. Se corta la alimentación de red al equipo y se observa que el equipo señala estar en batería; el supuesto de la señal en batería pasa a verificado, con vigencia de 365 días.
4. Se ejecuta un corte con retorno controlado; el equipo corta la salida al host.
5. Se restaura la energía; el equipo repone la salida.
6. Si el host arranca solo, sin tocar el botón, los supuestos de reencendido por la placa y de corte con retorno pasan a verificado, y la modalidad de host luego equipo con retorno queda efectiva.

## 5. Flujos alternativos

- FA-1 El host no arranca solo. Disparador: tras restaurar la energía, el host no reenciende sin intervención. El supuesto de reencendido por la placa pasa a refutado, que bloquea de forma permanente hasta que alguien lo resuelva. Un refutado no es un vencido: una prueba fallida bloquea, una vencida solo pide repetirla. Termina el caso con el apagado automático bloqueado.
- FA-2 Renovación por evidencia fuera de la ventana. Disparador: más adelante, un corte real que muestre al equipo en batería, o un corte seguido de arranque automático, aporta evidencia. El sistema renueva la verificación correspondiente sin repetir la prueba destructiva (se apoya en CU-05).

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| SUPUESTO_REFUTADO | El host no arranca solo tras restaurar la energía | Marca el supuesto de reencendido como refutado y mantiene el apagado automático bloqueado de forma permanente hasta resolución |
| EFECTO_NO_CONFIRMADO | Una orden al equipo durante la prueba no puede confirmarse por efecto observado | No da el paso por superado, lo registra como no confirmado y no habilita la modalidad (RN-03) |

## 7. Postcondiciones

- Éxito: los cuatro supuestos quedan verificados con sus vigencias; la modalidad de host luego equipo con retorno es efectiva; el panel deja de mostrar el aviso de solo aviso por supuestos.
- Refutación: al menos un supuesto queda refutado y el apagado automático permanece bloqueado.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Las cuatro verificaciones en estado sin verificar y el sistema en solo aviso | El administrador completa la ventana y el host arranca solo tras restaurar la energía | Los cuatro supuestos quedan verificados y la modalidad host luego equipo con retorno queda efectiva |
| CA-02 | La verificación del presupuesto de apagado recién verificada | El sistema registra su vigencia | La vigencia queda en 180 días, más corta que la de 365 días de los supuestos de firmware, porque la carga del host cambia |
| CA-03 | La ventana en curso hasta el paso de reencendido | El host no arranca solo tras restaurar la energía | El supuesto de reencendido pasa a refutado y bloquea el apagado automático de forma permanente hasta resolución (SUPUESTO_REFUTADO) |
| CA-04 | Una verificación vencida por el paso del tiempo | El sistema evalúa su vigencia | La verificación pasa a vencida, lo que pide repetir la prueba pero no bloquea de forma permanente como una refutada |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-01 (Apagado ordenado y reencendido garantizado); NB-05 (Seguridad operativa) |
| Reglas de negocio aplicables | RN-01, RN-02, RN-03 |
| Historias de usuario a generar | US-10 en 06 |
| Componentes esperados | Servicio de verificaciones, adaptador de conexión, adaptador simulado para pruebas, panel de checklist (referencia tentativa a 05) |
| Tests previstos | Transición de supuestos a verificado con sus vigencias, refutación que bloquea, distinción entre vencido y refutado, validación por efecto observado (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-4 y las verificaciones sembradas en §20.E-1.
- Es el único flujo que no se puede validar solo con software: exige presencia física y pruebas destructivas. La etapa entrega la interfaz guiada y el registro de evidencias; la ejecución real es una actividad operativa posterior. Hasta que ocurra, el sistema permanece en solo aviso.
- El detalle de la interfaz guiada es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador ejecuta la ventana, con presencia física.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-8 (§6), §20.E-1, §20.E-4, NB-01 y NB-05 |
