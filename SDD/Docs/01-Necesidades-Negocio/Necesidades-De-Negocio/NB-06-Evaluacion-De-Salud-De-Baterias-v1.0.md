# NB-06 — Evaluación de la salud de las baterías por tendencia, con confianza y reservas

| Campo | Valor |
| --- | --- |
| Proyecto | Sai-Service-Core |
| Documento | NB-06-Evaluacion-De-Salud-De-Baterias-v1.0.md |
| Versión | 1.2 |
| Estado | Borrador |
| Fecha | 2026-07-20 |
| Autor | Orquestador SDD (AG-01) |
| Trazabilidad upstream | SOLUTION-INTAKE §1, §3, §4 (F-16, F-17), §7 (CL-13, CL-23, CL-24, CL-25, CL-26), §8 (M-02), §11 (R-09, R-14); Vision-Producto-v1.0.md §1, §3; Alcance-Proyecto-v1.0.md §4 (C-09) |
| Trazabilidad downstream | CU-07, CU-08, CU-12 (02-Especificacion-Funcional) |

## 1. Descripción de la necesidad

El negocio necesita saber si la batería se está degradando, para planificar el recambio antes de que falle. El problema es que este equipo no expone ningún indicador de salud y su autoprueba no emite veredicto: la señal de reemplazo en la que se apoya el monitoreo convencional nunca se enciende en este equipo, así que un monitoreo estándar no alerta jamás. Ninguna herramienta libre ni comercial calcula salud a partir de los datos disponibles: todas se limitan a retransmitir la bandera del firmware que acá no llega.

La única opción es que el propio servicio calcule la salud, midiendo la caída de tensión durante la autoprueba a carga igualada y guardando la serie en el tiempo. Pero esa salud no es un porcentaje ni una afirmación cuantitativa: es una tendencia relativa que solo puede afirmar que una batería se comporta peor que antes. La necesidad incluye ser honesto sobre sus límites: cada veredicto viaja con su nivel de confianza, arranca en confianza baja y no es utilizable hasta acumular varias pruebas comparables. Y como no hay sensor de temperatura, toda conclusión lleva la reserva de que la oscilación estacional puede rivalizar con la señal de degradación.

Importa porque es el diferenciador de mayor valor a largo plazo y la razón de la urgencia: la batería está en servicio desde 2024 y cada trimestre sin registrar la tendencia es un punto de datos perdido.

## 2. Ejemplo de uso desde la perspectiva del negocio

Cada trimestre el servicio prueba la batería solo, con una cadencia de muestreo densa durante la prueba, y compara la caída de tensión contra la línea base tomada al inicio, a carga igualada. Tras cuatro pruebas comparables, la tendencia deja de estar en confianza baja y el administrador puede leer que la batería se comporta peor que antes y conviene revisarla, sin que el sistema pretenda decir un porcentaje de salud. La conclusión llega con su reserva por falta de sensor de temperatura, y el administrador la usa para planificar el recambio con anticipación.

## 3. Impacto

- Planificación del recambio: da una señal anticipada de degradación donde el monitoreo convencional no da ninguna.
- Diferenciador de valor: es la capacidad que ninguna herramienta existente ofrece para este equipo.
- Honestidad del veredicto: la confianza y las reservas explícitas evitan sobre-interpretar una señal indirecta.
- Insumo para la decisión de compra: la tendencia y la ficha de vida útil alimentan la comparación de modelos.
- Si queda sin resolver: la batería se degrada sin que nada avise y el recambio se decide a ciegas o tras una falla.

## 4. Problema específico que resuelve

- Que el servicio calcule su propia salud de batería, porque el equipo no emite ningún veredicto.
- Que la prueba se ejecute con cadencia densa y a carga igualada para que las pruebas sean comparables entre sí.
- Que cada veredicto declare su nivel de confianza y arranque en confianza baja hasta acumular suficientes pruebas.
- Que solo entren en la tendencia los valores medidos, y nunca los derivados, estimados o imputados.
- Que toda conclusión lleve la reserva por ausencia de sensor de temperatura, y que una prueba con carga no comparable no entre en la tendencia.

## 5. Criterios de éxito

| Criterio | Métrica | Target | Plazo |
| --- | --- | --- | --- |
| Tendencia estadísticamente utilizable | Pruebas comparables acumuladas | ≥ 4 (target propuesto, requiere ratificación — P-01) | 12 meses a cadencia trimestral (M-02) |
| Cadencia de prueba programada | Período entre pruebas automáticas | Trimestral (cada 3 meses) | Continuo |
| Confianza declarada en todo veredicto | Veredictos sin nivel de confianza explícito | 0 | Desde el primer veredicto |
| Exclusión de valores no medidos | Valores derivados, estimados o imputados que entran en la tendencia | 0 | Continuo |
| Reserva por temperatura | Veredictos sin la reserva de temperatura declarada | 0 | Continuo |
| Descarte de pruebas no comparables | Pruebas con carga concurrente fuera de tolerancia que entran en la tendencia | 0 (se marcan no comparables) | Continuo |

## 6. Stakeholders involucrados

| Rol | Nivel | Qué pide o aporta |
| --- | --- | --- |
| Administrador único (rol propietario) | Propietario | Exige un veredicto honesto con confianza y reservas, y usa la tendencia para planificar el recambio |
| Administrador único (rol implementador) | Implementador | Construye la prueba a carga igualada, el cálculo de tendencia y la gestión de confianza y reservas |
| Administrador único (rol beneficiario) | Beneficiario | Consume la tendencia de salud para decidir el recambio con anticipación |

## 7. Trazabilidad a CU

| NB | CU prevista | Estado |
| --- | --- | --- |
| NB-06 | CU-07 Prueba de batería y veredicto de salud | aprobada |
| NB-06 | CU-08 Registro de recambio de batería y ficha de vida útil | aprobada |
| NB-06 | CU-12 Informe de período y comparación de marcas | aprobada |

## 8. Dependencias con otras NB

- Depende de NB-03 (Historia trazable con procedencia): la tendencia se construye sobre la serie de pruebas guardada con el origen de cada valor.
- Depende de NB-04 (Ciclo de vida del parque): el veredicto se atribuye a la batería montada en el período de la prueba, resuelta por el vínculo temporal.

## 9. Prioridad MoSCoW

Must Have. El SOLUTION-INTAKE §4 marca F-16 y F-17 (prueba de batería y veredicto de salud) como Must Have de la primera entrega, y es el segundo propósito declarado del servicio; el veredicto de salud es el núcleo Must, mientras que los informes y la comparación de marcas derivados (F-22, F-23) son alcance posterior de prioridad Should sin que ello degrade esta NB.

## 10. Control de cambios

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE §1, §3, §4, §7, §8 y §11, y de Vision-Producto-v1.0.md |
| 1.1 | 2026-07-20 | Corrección de prioridad MoSCoW a Must Have por alineación con SOLUTION-INTAKE §4 tras audit de Fase A |
| 1.2 | 2026-07-20 | Reconciliación de trazabilidad §7 con los CU vigentes de 02 tras audit de Fase B |
