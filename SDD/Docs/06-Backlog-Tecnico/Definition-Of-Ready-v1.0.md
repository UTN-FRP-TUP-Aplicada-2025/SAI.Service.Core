# Definition of Ready — Sai-Service-Core

**Proyecto:** Sai-Service-Core
**Documento:** Definition-Of-Ready-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-06)
**Trazabilidad:** filtra la entrada del `Product-Backlog-v1.0.md` y del `Backlog-Tecnico-v1.0.md`; no se solapa con la Definition of Done de 08

La Definition of Ready (DoR) define **cuándo un ítem puede empezar**: qué condiciones debe cumplir una US o una BT antes de entrar a Sprint Planning. Es distinta de la Definition of Done de la categoría 08, que define cuándo un ítem está terminado. La DoR habla de arranque; la DoD, de cierre. Cada criterio se responde con sí o no de manera objetiva.

---

## 1. Criterios DoR para US

Una historia de usuario está Ready cuando cumple los siete criterios:

1. **Valor explícito.** La cláusula `para [...]` enuncia un beneficio para el rol, no un sinónimo de la acción.
2. **Trazabilidad a CU.** La historia referencia al menos un CU de 02 en su columna `CU relacionados`; no hay historias huérfanas.
3. **NB de origen identificada.** La historia declara al menos una NB de 01 que la motiva.
4. **Criterios de aceptación en Given/When/Then.** Al menos dos escenarios, con un happy path y un edge case (obligatorio para Must y Should).
5. **Estimada en la escala declarada.** Tiene story points en Fibonacci; si no es estimable, se divide o se abre un spike previo.
6. **Sin dependencias bloqueantes abiertas.** Las US o BT de las que depende están terminadas, o la dependencia está planificada antes en la misma etapa.
7. **Alcance acotado a la etapa.** Cabe en una etapa de §15 con holgura; si abarca más de un flujo, se descompone.

## 2. Criterios DoR para BT

Una tarea técnica está Ready cuando cumple los cinco criterios:

1. **Fuente upstream declarada.** La BT referencia una NB, un CU, una ADR o un contrato de 05; sin justificación upstream no entra.
2. **US consumidora o justificación de infraestructura.** Declara al menos una US que la consume, o se justifica como infraestructura compartida con la ADR que la exige.
3. **Criterios de aceptación técnicos verificables.** La BT enuncia qué significa que compile, que sus pruebas pasen o que el contrato o invariante se respete.
4. **Tipo declarado.** Feature, spike, refactor, devops o docs; los spikes llevan caja temporal explícita.
5. **Dependencias técnicas resueltas o identificadas.** Las BT previas están terminadas o su bloqueo está registrado.

## 3. Excepciones admitidas

- **Spike exploratorio.** Una BT de tipo spike puede entrar sin criterios de aceptación cerrados en términos de resultado, siempre que tenga caja temporal explícita y una pregunta a responder. Es el caso de BT-01 a BT-04 (decisiones de Sprint 0). El resultado del spike alimenta una ADR de cierre, no decide por sí solo.
- **US Could de capacidad diseñada no implementada.** Las historias Could que documentan capacidades diseñadas pero no implementadas en v1 (US-25, US-26) pueden entrar al backlog sin datos de prueba disponibles, porque su implementación se difiere a v2; se mantienen en estado Borrador y no se promueven a Ready hasta que exista el equipo que las justifique.
- **Dependencia de validación humana operativa.** La ventana de mantenimiento (US-16) depende de una actividad física destructiva que no se puede automatizar; su parte de software (interfaz guiada y registro de evidencia) puede estar Ready aunque la ejecución real quede pendiente. Se cubre en pruebas con el adaptador simulado (BT-29).

En todos los casos, quien aprueba la excepción es el aprobador de §4.

## 4. Aprobador

El **administrador único**, en su rol combinado de Scrum Master y Product Owner, valida que un ítem cumple la DoR antes de entrar a Sprint Planning. La titularidad del artefacto y de su curaduría es del AG-06 (Scrum Master / Agile Coach). Las revisiones acotadas de trazabilidad (AG-02), de justificación técnica (AG-05) y de verificabilidad de criterios (AG-08) pueden observar el cumplimiento de la DoR, pero la aprobación final es del aprobador declarado.
