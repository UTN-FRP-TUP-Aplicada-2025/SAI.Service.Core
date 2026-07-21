# NB-04 — Gestión del ciclo de vida del parque de dispositivos y baterías

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-04-Ciclo-De-Vida-Del-Parque-v1.0.md |
| Versión | 1.1 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §4 (F-04, F-05, F-14, F-21, F-22, F-23), §7 (CL-17, CL-19, CL-20), §8 (M-04); Vision-Producto-v1.0.md §1, §4; Alcance-Proyecto-v1.0.md §4 (C-01, C-02, C-11, C-13, C-14) |
| Trazabilidad downstream | CU-02, CU-08, CU-09, CU-12 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita modelar el ciclo de vida completo del parque: dar de alta el equipo y su batería, registrar recambios, reparaciones y sustituciones, y saber en cualquier momento qué equipo protegía al servidor en cada tramo y cuántos días quedó sin protección. Hoy no hay modelo de inventario: las herramientas existentes no distinguen el modelo de producto de la unidad física, no registran altas ni bajas, y el recambio de una batería no queda asociado al período en que estuvo montada.

La necesidad tiene una consecuencia decisiva: cada medición debe poder atribuirse a la batería que estaba puesta cuando se tomó, y una batería puede retirarse, probarse en banco y volver a instalarse, o moverse a otro equipo. Eso solo se representa si la relación entre unidades es un vínculo con intervalo, no un atributo de la batería. Con la vigencia como dato de la batería, el caso en que un equipo se va a reparación y otro lo cubre ni siquiera es representable.

Además, dar de alta el parque debe hacerse desde el panel, sin editar archivos de configuración a mano, y nada se borra: toda baja es lógica y la unidad retirada sigue consultable con su historial pero no operable. Al cerrar la vida de una batería, el negocio necesita su ficha de vida útil —cuánto duró, qué soportó, si cumplió la expectativa y cuánto costó por año— para decidir la próxima compra con datos y no por precio de lista.

## 2. Ejemplo de uso desde la perspectiva del negocio

El administrador abre el panel por primera vez, identifica el equipo, declara marca, modelo y potencia nominal, y da de alta el servidor, el equipo de alimentación y su batería, abriendo los vínculos de montaje y cobertura. Meses después, un equipo se va a reparación y otro que tenía en stock lo cubre; lo registra, y el histórico pasa a decir con exactitud qué equipo protegía al servidor en cada tramo y cuántos días quedó sin cobertura. Al año, cuando reemplaza la batería, cierra su ficha de vida útil y compara su costo por año de servicio con el de otro modelo para decidir cuál comprar.

## 3. Impacto

- Trazabilidad del parque: responde qué equipo y qué batería protegían al servidor en cada tramo, algo hoy imposible.
- Atribución correcta de métricas: cada medición queda ligada al período de la batería que estaba montada.
- Decisión de compra con datos: la ficha de vida útil y el costo por año habilitan comparar marcas por desempeño real.
- Integridad del histórico: la baja lógica conserva el pasado de toda unidad retirada sin permitir operarla.
- Si queda sin resolver: el ciclo de vida sigue en texto plano, sin poder reconstruir cobertura, días sin protección ni costo real de una batería.

## 4. Problema específico que resuelve

- Que el alta del equipo y su batería se haga desde el panel, sin editar archivos de configuración a mano.
- Que la relación entre batería, equipo y servidor sea un vínculo temporal con intervalo, para representar retiros, pruebas en banco y reinstalaciones.
- Que ninguna unidad se borre físicamente: baja lógica siempre, con la unidad consultable pero no operable.
- Que un recambio, una reparación o una sustitución quede registrado y actualice la cobertura y los días sin protección.
- Que al cerrar la vida de una batería se proyecte su ficha de vida útil con su costo por año, comparable entre modelos.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Alta sin edición manual de archivos | Pasos de edición de archivos de configuración para dar de alta el parque | 0 | Desde el onboarding |
| Ausencia de borrado físico | Unidades borradas físicamente en lugar de baja lógica | 0 | Continuo |
| Reconstrucción de cobertura | Tramos del período con equipo protector identificable | 100 % | Continuo |
| Días sin protección cuantificados | Huecos entre coberturas medidos y reportados | 100 % de los huecos | Por período |
| Historia apta para decidir compra | Modelos de batería con ficha de vida útil cerrada y costo por año en moneda estable | ≥ 2 modelos comparables (target propuesto, requiere ratificación — P-01) | Al primer recambio posterior a la puesta en marcha (M-04) |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Aprueba el modelo de inventario y la política de baja lógica, y decide las compras con la ficha de vida útil |
| Administrador único (rol implementador) | Implementador | Construye el catálogo, el inventario, los vínculos temporales y la proyección de la ficha de vida útil |
| Proveedor / técnico externo (prov-taller-electronica-sur) | Beneficiario indirecto / ejecutor | Ejecuta recambios y reparaciones, retira las baterías agotadas y consta como receptor para trazabilidad ambiental |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-04 | CU-02 Alta del parque y puesta en marcha | aprobada |
| NB-04 | CU-08 Registro de recambio de batería y ficha de vida útil | aprobada |
| NB-04 | CU-09 Reparación y sustitución del SAI con cobertura suplente | aprobada |
| NB-04 | CU-12 Informe de período y comparación de marcas | aprobada |

## 8. Dependencias con otras NB

Sin dependencias. Es la necesidad fundacional: modela las unidades y los vínculos temporales sobre los que se apoyan la historia, el monitoreo y la salud de batería.

## 9. Prioridad MoSCoW

Must Have. El modelo de inventario y los vínculos temporales son la base sobre la que se atribuyen las métricas y se reconstruye la cobertura; sin ellos ninguna otra necesidad se sostiene (SOLUTION-INTAKE §4, F-04 y F-05).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §4, §7 y §8, y de Vision-Producto-v1.0.md. Incluye la sustitución del SAI (F-21) y el informe de comparación de marcas (F-22, F-23), de prioridad Should en el intake, como refinamientos sobre el mismo modelo fundacional |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
