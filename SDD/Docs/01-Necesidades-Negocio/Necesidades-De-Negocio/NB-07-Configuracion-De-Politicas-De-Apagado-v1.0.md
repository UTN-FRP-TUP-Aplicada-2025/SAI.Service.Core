# NB-07 — Configuración de políticas de apagado versionadas

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-07-Configuracion-De-Politicas-De-Apagado-v1.0.md |
| Versión | 1.1 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §4 (F-10, F-12), §7 (CL-04, CL-06, CL-15), §10, §11 (R-04); Vision-Producto-v1.0.md §1; Alcance-Proyecto-v1.0.md §4 (C-05, C-07), §7 |
| Trazabilidad downstream | CU-03, CU-05 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita definir cómo se comporta el servicio ante un corte —solo avisar, apagar el servidor, apagar el servidor y luego el equipo con retorno, o forzar el ciclo— y ajustar esa decisión a medida que aprende de su propio histórico. Y necesita hacerlo de forma que las decisiones pasadas sigan explicándose con la configuración exacta que las produjo. Hoy esa configuración vive dispersa en archivos de texto del equipo, sin versión ni historia: si se cambia un umbral, se pierde el rastro de con qué configuración se tomó una decisión anterior.

La necesidad, entonces, no es solo tener parámetros configurables, sino que cada cambio cree una versión nueva en vez de editar la vigente, de modo que toda acción quede ligada a la versión de política con la que se ejecutó. Además, la política tiene que respetar restricciones duras del equipamiento: el tiempo reservado para el apagado no puede superar el techo de corte del equipo, y la decisión de disparo no puede depender de señales que este equipo no produce de forma confiable, sino del tiempo sostenido sin energía de red y de la tensión de la batería.

Importa porque la política es el contrato entre el administrador y el camino crítico: define cuándo y cómo el sistema toma la decisión irreversible de apagar, y una configuración sin historia deja las decisiones pasadas sin explicación.

## 2. Ejemplo de uso desde la perspectiva del negocio

Tras semanas de histórico, el administrador decide que el sistema debe esperar un poco más antes de disparar el apagado, porque observó varios cortes breves. En vez de editar la política vigente, crea una versión nueva con el umbral ajustado. La versión anterior queda archivada y las acciones que se ejecutaron bajo ella siguen apuntando a esa versión, de modo que cualquier decisión pasada se puede explicar con la configuración que la produjo. Cuando intenta fijar un tiempo de reserva por encima del techo del equipo, el formulario lo rechaza.

## 3. Impacto

- Control del camino crítico: define cuándo y cómo se toma la decisión irreversible de apagar.
- Explicabilidad de las decisiones: cada acción queda ligada a la versión de política vigente en su momento.
- Adaptación con el aprendizaje: permite ajustar umbrales a medida que el histórico revela el patrón de cortes.
- Respeto de las restricciones del equipo: impide configurar un tiempo de reserva que el equipo no puede cumplir.
- Si queda sin resolver: la configuración sigue sin historia y las decisiones pasadas quedan sin explicación reproducible.

## 4. Problema específico que resuelve

- Que cada cambio de política cree una versión nueva en vez de editar la vigente.
- Que toda acción quede ligada a la versión de política con la que se ejecutó, y nunca a la política en abstracto.
- Que el formulario rechace un tiempo de reserva de apagado por encima del techo de corte del equipo.
- Que el disparo se base en el tiempo sostenido sin energía de red y en la tensión de batería, y no en señales que este equipo no produce de forma confiable.
- Que las cuatro modalidades de comportamiento estén disponibles y elegibles por versión.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Versionado inmutable | Ediciones destructivas de una versión de política ya vigente | 0 (siempre versión nueva) | Continuo |
| Trazabilidad de acción a versión | Acciones que referencian una política sin su versión | 0 | Continuo |
| Techo de corte validado | Políticas aceptadas con tiempo de reserva de apagado mayor a 540 s | 0 | Continuo |
| Umbral de disparo configurable | Umbral de disparo como punto de partida, ajustable por versión | 300 s (propuesto en la fuente) | Desde la primera política vigente |
| Independencia de señales no confiables | Políticas cuyo disparo depende de la señal de batería baja o del tiempo de autonomía | 0 | Continuo |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Define las modalidades y los umbrales, y exige que cada decisión pasada se explique con su versión de política |
| Administrador único (rol implementador) | Implementador | Construye el versionado de políticas, la validación del techo de corte y la evaluación por el planificador |
| Host protegido i7infra | Beneficiario (sistema) | Es el objeto de la política: su apagado y reencendido se rigen por la versión vigente |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-07 | CU-03 Configuración de políticas de apagado versionadas | aprobada |
| NB-07 | CU-05 Ejecución del apagado ordenado ante corte sostenido | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-02 (Monitoreo en vivo y alertas): la evaluación de una política se hace sobre el estado en vivo y los eventos de energía derivados.

## 9. Prioridad MoSCoW

Must Have. La política es el contrato que gobierna el camino crítico del apagado y sin ella no hay decisión configurable ni explicable (SOLUTION-INTAKE §4, F-10).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §4, §7, §10 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
