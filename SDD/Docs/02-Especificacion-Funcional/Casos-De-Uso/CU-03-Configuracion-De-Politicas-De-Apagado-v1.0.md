# CU-03 — Configuración de políticas de apagado versionadas

**Proyecto:** Sai-Service-Core
**Documento:** CU-03-Configuracion-De-Politicas-De-Apagado-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el administrador configure la política de apagado creando una versión nueva en lugar de editar la vigente, de modo que toda decisión pasada siga explicándose con la configuración exacta que la produjo. Cada versión fija la modalidad, el umbral de disparo, el tiempo reservado para el apagado y las verificaciones que requiere.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Define y publica una nueva versión de la política de apagado |
| Servicio de políticas | Sistema | Valida los parámetros, crea la versión inmutable y la deja vigente |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01).
- Existe la política de apagado (creada al menos con su versión inicial en la puesta en marcha).

## 4. Flujo principal

1. El administrador abre la configuración de la política de apagado y ve la versión vigente y su historial.
2. El administrador elige crear una versión nueva a partir de la vigente.
3. El administrador ajusta los parámetros: modalidad solicitada, umbral de disparo en segundos, tiempo reservado para el apagado en segundos y la cancelación al volver la red.
4. El servicio valida que el tiempo reservado para el apagado no supere el techo duro de 540 segundos y que las verificaciones que la modalidad requiere estén declaradas.
5. El servicio crea la versión nueva con su número incrementado y su fecha de vigencia, sin modificar las versiones anteriores.
6. La versión nueva queda vigente; las anteriores permanecen consultables en el historial.

## 5. Flujos alternativos

- FA-1 Modalidad que exige supuestos aún no verificados. Disparador: el administrador elige una modalidad distinta de solo aviso mientras hay verificaciones sin cumplir. El servicio acepta y publica la versión, pero advierte que la modalidad efectiva seguirá degradada a solo aviso hasta que se verifiquen los supuestos (la ejecución se rige por CU-05 y RN-02). Retorna al paso 6.
- FA-2 Ajuste del umbral de disparo. Disparador: el administrador cambia solo el umbral de disparo. El servicio crea igualmente una versión nueva; nunca se edita la vigente en el lugar.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| TIEMPO_APAGADO_EXCEDE_TECHO | El tiempo reservado para el apagado supera 540 segundos | Rechaza la creación de la versión con el motivo del techo duro (RN-04) y no la publica |
| PARAMETRO_INVALIDO | Un parámetro obligatorio queda vacío o fuera de su rango admitido | Rechaza la creación de la versión e indica el parámetro en falta |

## 7. Postcondiciones

- Éxito: existe una versión nueva de la política, inmutable, marcada como vigente; las anteriores se conservan en el historial sin cambios.
- Fallo: no se crea ninguna versión; la vigente sigue siendo la anterior.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | La política `pol-apagado-por-corte` con versión vigente 1 en modalidad solo aviso | El administrador crea una versión con modalidad host luego equipo con retorno y tiempo reservado de 240 segundos | El sistema crea la versión 2 vigente sin alterar la versión 1, que queda en el historial |
| CA-02 | Una nueva versión con tiempo reservado para el apagado de 600 segundos | El administrador intenta publicarla | El sistema la rechaza con TIEMPO_APAGADO_EXCEDE_TECHO por el techo duro de 540 segundos (RN-04) |
| CA-03 | Una versión con modalidad host luego equipo con retorno y cuatro supuestos sin verificar | El administrador la publica | El sistema publica la versión y advierte que la modalidad efectiva seguirá en solo aviso hasta verificar los supuestos |
| CA-04 | Una versión vigente 2 | El administrador ajusta solo el umbral de disparo de 300 a 240 segundos | El sistema crea la versión 3 y no modifica la versión 2 |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-07 (Configuración de políticas de apagado) |
| Reglas de negocio aplicables | RN-04, RN-11; RN-02 aplicable a la ejecución posterior |
| Historias de usuario a generar | US-03 en 06 |
| Componentes esperados | Servicio de políticas y versiones de política (referencia tentativa a 05) |
| Tests previstos | Creación de versión inmutable, rechazo por techo de 540 segundos, conservación del historial (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: la política y sus versiones de los escenarios §20.E-1 y §20.E-4.
- El versionado responde a que conviene saber con qué configuración se tomó cada decisión; por eso jamás se edita una versión en el lugar.
- La disposición del formulario de configuración es materia de 03-UX-UI-DX.

## 11. Interacción multiusuario y concurrencia

Trivial: un único administrador configura políticas; no hay ediciones concurrentes.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-2 (§6), §20.E-1, §20.E-4 y NB-07 |
