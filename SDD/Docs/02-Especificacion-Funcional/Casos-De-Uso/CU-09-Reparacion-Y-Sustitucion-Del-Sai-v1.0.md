# CU-09 — Reparación y sustitución del SAI con cobertura suplente

**Proyecto:** Sai-Service-Core
**Documento:** CU-09-Reparacion-Y-Sustitucion-Del-Sai-v1.0.md
**Versión:** 1.1
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador registre que el equipo de alimentación se fue a reparación o se sustituyó, y que otro equipo cubrió al host durante ese tramo, de modo que el histórico diga qué equipo protegía al host en cada momento y cuántos días quedó sin protección. Cierra y abre coberturas del host como vínculos temporales, sin solaparlas.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Registra la salida a reparación o la sustitución y la cobertura suplente |
| Servicio de intervenciones | Sistema | Aplica los efectos: cambia estados de equipos y cierra o abre coberturas del host |
| Proveedor o técnico externo | Secundario | Ejecuta la reparación o el reemplazo del equipo |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- Existe una cobertura del host vigente por el equipo que sale de servicio.
- Existe un equipo suplente en stock, o se registra la sustitución por uno nuevo.

## 4. Flujo principal

1. El administrador registra la intervención sobre el equipo: reparación o reemplazo, con su instante.
2. El sistema cambia el estado del equipo que sale a en reparación o dado de baja, según corresponda.
3. El sistema cierra la cobertura del host vigente por ese equipo, en el instante de la intervención.
4. Si un equipo suplente cubre al host, el sistema lo pasa a en servicio y abre una nueva cobertura del host desde el instante en que empieza a cubrir.
5. Si hay un tramo en que el host quedó sin ningún equipo cubriéndolo, el sistema deja el hueco entre coberturas, del que se calculan los días sin protección.
6. El sistema deja registrada la sucesión de coberturas, consultable por período.

## 5. Flujos alternativos

- FA-1 Reparación con retorno del mismo equipo. Disparador: el equipo vuelve reparado. El administrador registra el retorno; el sistema lo pasa a en servicio y abre una cobertura nueva desde ese instante. La cobertura anterior no se reabre: son dos tramos.
- FA-2 Sustitución por otro modelo. Disparador: el equipo se reemplaza por otro modelo. El sistema registra que las verificaciones de firmware vuelven a estado sin verificar, porque fueron probadas contra otro equipo, y el panel dispara el procedimiento de caracterización.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| COBERTURA_SOLAPADA | Se intentaría abrir una cobertura del host que se solapa con otra vigente para el mismo host | Rechaza la apertura; a lo sumo puede haber una cobertura vigente por host (RN-12 y reglas del modelo) |
| COHERENCIA_TEMPORAL | La intervención se fecha después de la baja del equipo afectado | Rechaza la intervención por coherencia temporal (RN-12) |

## 7. Postcondiciones

- Éxito: la cobertura del equipo saliente está cerrada; si hubo suplente, existe una cobertura nueva sin solapamiento; los días sin protección se pueden calcular de los huecos.
- Fallo: si habría solapamiento o incoherencia temporal, no se aplica el efecto y las coberturas quedan como estaban.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | La cobertura `cob-001` del host `i7infra` vigente por `ups-01` | El administrador registra que `ups-01` sale a reparación en un instante dado | El sistema cierra `cob-001` en ese instante y pone `ups-01` en reparación |
| CA-02 | El equipo de repuesto `ups-02` en stock | El administrador lo pone a cubrir al host desde el instante de la salida del anterior | El sistema abre una nueva cobertura sin solapamiento y pasa `ups-02` a en servicio |
| CA-03 | Un tramo de dos días en que ningún equipo cubrió al host | El administrador consulta el período | El sistema informa esos dos días como días sin protección, calculados del hueco entre coberturas |
| CA-04 | Una sustitución por un equipo de otro modelo | El sistema aplica el reemplazo | Las verificaciones de firmware vuelven a estado sin verificar y el panel dispara la caracterización |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-04 (Ciclo de vida de los equipos) |
| Reglas de negocio aplicables | RN-12; RN-01 y RN-02 aplicables a la reanudación de la modalidad tras la sustitución |
| Historias de usuario a generar | US-09 en 06 |
| Componentes esperados | Servicio de intervenciones, vínculos temporales de cobertura, máquina de estados de la unidad física (referencia tentativa a 05) |
| Tests previstos | Cierre y apertura de cobertura sin solapamiento, cálculo de días sin protección, reinicio de verificaciones al sustituir por otro modelo (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-1 parcial, con el equipo `ups-02` en stock previsto para que la sucesión de coberturas sea representable. El intake declara que este flujo aún no tiene un escenario de datos completo (riesgo R-11): se propone un escenario E-9 cuando se implemente.
- Vigencia como entidad con intervalo: con la vigencia guardada en la batería o el equipo como atributo, el caso de un equipo que se va a reparación y otro que cubre no sería representable.
- El detalle del formulario es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador registra estas intervenciones.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-7 (§6), §20.E-1 parcial y NB-04 |
| 1.1 | 2026-07-20 | Retroalimentación de la Fase B2: unificación de terminología "parque" → "equipos" |
