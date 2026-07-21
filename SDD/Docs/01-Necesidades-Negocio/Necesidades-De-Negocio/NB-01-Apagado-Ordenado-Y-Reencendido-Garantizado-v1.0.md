# NB-01 — Apagado ordenado y reencendido automático garantizado del host

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-01-Apagado-Ordenado-Y-Reencendido-Garantizado-v1.0.md |
| Versión | 1.1 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §4 (F-10, F-12), §7 (CL-01), §11 (R-01, R-12); Vision-Producto-v1.0.md §1, §4; Alcance-Proyecto-v1.0.md §3 |
| Trazabilidad downstream | CU-05, CU-10 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita que un servidor de criticidad alta y sin copias de respaldo se apague de forma ordenada ante un corte de energía y, sobre todo, que vuelva a encenderse solo cuando la energía se restablece. El apagado ordenado ante corte ya está cubierto por las herramientas existentes; lo que ninguna herramienta garantiza para este equipo es el reencendido. El servidor solo arranca solo si detecta una transición de ausencia a presencia de energía, lo que obliga a que el equipo de alimentación corte su propia salida aunque el servidor ya esté apagado.

Hoy ese comportamiento es un supuesto no garantizado. Si la energía vuelve durante la cuenta regresiva del apagado y el equipo cancela su corte, no hubo transición, no hay nada que el servidor pueda detectar y queda apagado hasta que una persona aprieta el botón. Es el peor resultado posible: el sistema se protegió de un corte que resultó breve y, a cambio, quedó fuera de servicio por tiempo indefinido, de noche y sin testigos.

Importa porque el servidor no tiene respaldo: equivocarse en esta decisión es grave. La necesidad, entonces, no es solo apagar bien, sino no quedar apagado, y tomar esa decisión con consecuencias irreversibles solamente cuando el sistema puede probar que el servidor va a volver.

## 2. Ejemplo de uso desde la perspectiva del negocio

Un sábado a la madrugada se corta la luz del edificio. El servidor sostiene la carga con la batería mientras la cuenta regresiva avanza. A los pocos minutos vuelve la energía, pero apenas un instante, y se corta de nuevo. Con el comportamiento por defecto, el equipo habría cancelado su corte en ese instante de energía y el servidor habría quedado apagado toda la noche. Con la necesidad resuelta, una vez iniciada la secuencia el corte no se cancela: el servidor sufre un apagón controlado de unos minutos y, al volver la energía de forma estable, arranca solo. El administrador se entera a la mañana leyendo el registro, no encontrando el servidor muerto.

## 3. Impacto

- Continuidad del servicio: es la diferencia entre un apagón controlado de minutos y un servidor fuera de servicio por horas o días.
- Integridad de datos del servidor sin respaldo: un apagado ordenado evita la corrupción que produce un corte abrupto.
- Confianza en la automatización: si el reencendido no se puede probar, toda la propuesta de valor del servicio se degrada a un mero aviso.
- Camino crítico irreversible: es la única acción del sistema con consecuencias que no se pueden deshacer; concentra el riesgo principal del proyecto (R-12).
- Si queda sin resolver: el servidor sigue expuesto a la trampa de firmware que lo deja apagado indefinidamente, y el administrador no puede delegar la decisión de apagado con tranquilidad.

## 4. Problema específico que resuelve

- Que el corte del equipo, una vez iniciada la secuencia, no se cancele aunque vuelva la red (modalidad de ciclo forzado).
- Que el equipo corte su salida aunque el servidor ya esté apagado, para producir la transición que dispara el reencendido.
- Que el apagado del sistema operativo quepa dentro del techo de tiempo duro del corte, sin que el equipo corte con el servidor a medio bajar.
- Que la decisión de apagar solo se ejecute cuando el reencendido está probado, y no por optimismo.
- Que el reencendido comprobado se mantenga vivo por evidencia, sin repetir la prueba destructiva cada vez.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Habilitación de la modalidad de apagado con retorno | Supuestos verificados sobre supuestos requeridos | 4 de 4 (target propuesto, requiere ratificación — P-01) | 1 mes desde la puesta en marcha (M-01) |
| Reencendido efectivo tras corte | Arranques no gestionados atribuibles a un corte, en la evidencia de arranque del servidor | 0 | Primer año calendario completo (M-03) |
| Respeto del techo de corte | Tiempo reservado para el apagado configurado en la política vigente | ≤ 540 s | Desde la primera política vigente |
| No cancelación en ciclo forzado | Secuencias de apagado ya iniciadas que se cancelan al volver la red | 0 | Continuo |
| Latencia de decisión del planificador | Segundos por ronda de evaluación | < 1 s [derivado del intervalo de sondeo de 5 s] | Desde el planificador en producción |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Aprueba la prioridad crítica de esta necesidad y ratifica el target de supuestos requeridos (P-01) |
| Administrador único (rol implementador) | Implementador | Construye y mantiene la lógica de decisión y la ejecuta con presencia física en la ventana de mantenimiento |
| Host protegido i7infra | Beneficiario (sistema) | Es el objeto de la protección: el apagado ordenado y el reencendido operan sobre él |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-01 | CU-05 Ejecución del apagado ordenado ante corte sostenido | aprobada |
| NB-01 | CU-10 Ventana de mantenimiento y verificación de supuestos | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-02 (Monitoreo en vivo y alertas): la decisión de apagar se toma sobre el estado en vivo y los eventos de energía.
- Depende de NB-05 (Seguridad operativa): el apagado solo se ejecuta si no está bloqueado por verificación y el arranque seguro lo permite.
- Depende de NB-07 (Configuración de políticas de apagado): toda ejecución se rige por una versión de política vigente.

## 9. Prioridad MoSCoW

Must Have. Es el valor central del producto y el camino crítico irreversible; sin él el servicio no cumple su primer propósito y no hay MVP defendible (SOLUTION-INTAKE §4, F-10 y F-12).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §4, §7 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
