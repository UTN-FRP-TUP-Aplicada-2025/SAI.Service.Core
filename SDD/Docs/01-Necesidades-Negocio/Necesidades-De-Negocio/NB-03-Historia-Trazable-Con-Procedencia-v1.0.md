# NB-03 — Historia trazable de métricas con procedencia y corrección retroactiva

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-03-Historia-Trazable-Con-Procedencia-v1.0.md |
| Versión | 1.1 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §3, §4 (F-07, F-18, F-19), §7 (CL-15, CL-16, CL-18), §8 (M-05), §11 (R-13); Vision-Producto-v1.0.md §1, §3, §5; Alcance-Proyecto-v1.0.md §4 (C-03, C-10) |
| Trazabilidad downstream | CU-06, CU-02, CU-04 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita una historia confiable y consultable de todo lo que le pasa a la energía del servidor: tensiones, carga, microcortes y eventos, con la posibilidad de graficar su evolución en cualquier período. Hoy el registro es texto plano, sin estructura para consultar por período ni para superponer variables con las marcas de los eventos, y sin forma de saber de dónde vino cada número.

Esa procedencia es el corazón de la necesidad. Cada valor tiene que declarar si lo midió el aparato, si lo calculó el software, si lo interpoló el propio equipo, si lo declaró una persona o si no se pudo calcular. El modo de falla más probable del sistema no es un error de código, sino una conclusión falsa construida sobre un valor que parecía medido y en realidad fue interpolado. Una historia que no distingue el origen de cada número miente sin que nada lo indique.

Además, la historia debe ser append-only y tolerar correcciones sin reescribir el pasado: si se corrige la fecha de un recambio ya cargado, todo el histórico afectado se reatribuye solo, porque la historia guarda el equipo y el instante y la batería se resuelve por consulta al vínculo temporal. Y como el volumen es grande, la agregación no puede borrar la información fina —los microcortes— que es justamente la que se quiere estudiar.

## 2. Ejemplo de uso desde la perspectiva del negocio

El administrador prepara una decisión y abre la evolución de tensión y carga superpuestas durante el último año, con las marcas de los cortes encima. Nota que un valor de carga está señalado como interpolado por el equipo y no lo usa para sacar conclusiones. Semanas después descubre que había cargado con fecha equivocada un recambio de batería; corrige la fecha y todo el histórico de ese tramo queda reatribuido a la batería correcta sin tener que migrar ni reescribir un solo dato.

## 3. Impacto

- Confiabilidad de las conclusiones: la procedencia evita decidir sobre un valor que el equipo interpoló.
- Base para la salud de batería y los informes: sin una historia trazable y agregada con cobertura, no hay tendencia ni comparación posibles.
- Auditabilidad del pasado: una historia append-only permite reconstruir qué se sabía y cuándo, sin que nadie pueda alterar los hechos.
- Robustez ante correcciones: reatribuir el histórico al corregir una fecha evita migraciones frágiles y errores silenciosos.
- Si queda sin resolver: las decisiones se apoyan en un texto plano sin origen ni estructura, y cualquier corrección corrompe la serie.

## 4. Problema específico que resuelve

- Que todo valor almacenado declare su origen, sin ninguna excepción.
- Que la historia sea append-only: los hechos no se actualizan ni se borran.
- Que corregir la fecha de una intervención reatribuya el histórico afectado sin migrar datos.
- Que la agregación conserve el mínimo y el máximo además del promedio, para no borrar los microcortes.
- Que todo agregado servido declare su cobertura y su advertencia, y nunca se presente como si fuera una muestra.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Procedencia declarada en todo valor | Valores almacenados sin origen declarado | 0 (no negociable) | Desde la introducción de la persistencia (M-05) |
| Retención a resolución completa | Período de conservación de muestras a resolución fina | 30 días (P30D) | Continuo |
| Retención de agregados | Período de conservación de los agregados horarios | 10 años (P10Y) | Continuo |
| Reatribución sin migración | Proporción del histórico afectado reatribuido al corregir una fecha, y filas migradas | 100 % reatribuido, 0 filas migradas | Por corrección |
| Preservación del detalle en la agregación | Agregados de tensión de entrada sin mínimo y máximo conservados | 0 | Continuo |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Exige que ningún número se presente sin declarar su origen y aprueba las políticas de retención |
| Administrador único (rol implementador) | Implementador | Construye la persistencia append-only, la agregación con cobertura y la reatribución por vínculo temporal |
| Administrador único (rol beneficiario) | Beneficiario | Consulta históricos y gráficas para preparar decisiones, confiando en el origen de cada valor |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-03 | CU-06 Históricos y gráficas de evolución del suministro | aprobada |
| NB-03 | CU-02 Alta del parque y puesta en marcha | aprobada |
| NB-03 | CU-04 Monitoreo en vivo del estado del SAI | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-04 (Ciclo de vida del parque): la historia guarda equipo e instante y resuelve la batería consultando los vínculos temporales del inventario.

## 9. Prioridad MoSCoW

Must Have. La procedencia y la historia append-only son la base de confianza de todo el sistema y mitigan su modo de falla más probable (SOLUTION-INTAKE §11, R-13; §4, F-07 y F-19).

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §3, §4, §7, §8 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
