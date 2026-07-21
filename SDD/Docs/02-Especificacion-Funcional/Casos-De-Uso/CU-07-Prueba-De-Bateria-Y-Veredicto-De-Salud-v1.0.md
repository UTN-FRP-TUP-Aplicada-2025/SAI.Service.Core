# CU-07 — Prueba de batería y veredicto de salud

**Proyecto:** Sai-Service-Core
**Documento:** CU-07-Prueba-De-Bateria-Y-Veredicto-De-Salud-v1.0.md
**Versión:** 1.0
**Estado:** Borrador
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-02)

## 1. Propósito

Permitir que el servicio pruebe la batería, de forma programada trimestral o a pedido del administrador, midiendo la caída de tensión durante el autotest con muestreo denso, y emita un veredicto de salud propio, con su confianza explícita y su reserva, comparado contra la línea base a carga igualada. El veredicto lo calcula el servicio porque el equipo no lo da.

## 2. Actores

| Actor | Tipo | Rol |
| --- | --- | --- |
| Administrador | Primario | Dispara la prueba manual o consulta el resultado de la programada |
| Planificador interno | Sistema | Dispara la prueba programada, eleva la cadencia de sondeo y recoge la serie |
| Adaptador de conexión con el equipo | Sistema | Ejecuta el comando de autotest y responde el estado durante la prueba |

## 3. Precondiciones

- El administrador tiene una sesión activa (CU-01) para el disparo manual.
- Existe un montaje de batería vigente en el dispositivo (CU-02).
- Se cumple el tiempo mínimo en flotación tras el último corte, para que la prueba mida degradación y no la recuperación de un corte reciente.

## 4. Flujo principal

1. El disparo ocurre por cadencia programada trimestral o por acción manual del administrador.
2. El planificador eleva la cadencia de sondeo a un muestreo por segundo mientras dura la prueba.
3. El sistema resuelve el montaje de batería vigente en el instante de la prueba y lo congela en el registro de la prueba.
4. El adaptador ordena el autotest; el sistema recoge la serie de tensión de batería, conservando las muestras perdidas como estado de primera clase.
5. El sistema calcula los derivados: tensión de reposo, tensión mínima, caída, caída relativa y segundos de recuperación; lo que no puede calcular queda no calculable con su motivo.
6. El sistema compara contra la línea base y determina si la prueba es comparable según la diferencia de carga concurrente.
7. El sistema emite el veredicto de salud, con su confianza y su reserva, indicando que lo calculó el servicio y no el equipo, y restaura la cadencia de sondeo normal.

## 5. Flujos alternativos

- FA-1 Prueba no comparable por carga distinta. Disparador: la diferencia de carga concurrente respecto de la línea base excede la tolerancia. El sistema registra la prueba pero la marca no comparable y la excluye de la tendencia de salud. Retorna al paso 7 con la marca correspondiente.
- FA-2 Muestras perdidas durante la conmutación. Disparador: el equipo deja de atender consultas mientras conmuta y se pierden muestras. El sistema registra esas muestras con calidad perdida y valores sin dato, y sigue calculando los derivados con las muestras válidas. Retorna al paso 5.

## 6. Excepciones y errores

| Código | Causa | Respuesta del sistema |
| --- | --- | --- |
| FLOTACION_INSUFICIENTE | No se cumple el tiempo mínimo en flotación tras un corte reciente | No dispara la prueba y explica que mediría la recuperación del corte, no la salud |
| PRUEBA_NO_COMPARABLE | La carga concurrente difiere de la línea base más allá de la tolerancia | Registra la prueba, la marca no comparable y no la incorpora a la tendencia (RN-06) |
| DERIVADO_NO_CALCULABLE | Falta un insumo para un derivado, por ejemplo la corriente de descarga para la resistencia interna | Registra el derivado como no calculable con su motivo, sin inventar un número |

## 7. Postcondiciones

- Éxito: existe una prueba de batería con su montaje congelado, su serie, sus derivados y su veredicto con confianza y reserva; la cadencia de sondeo vuelve a la normal.
- No comparable: la prueba queda registrada pero fuera de la tendencia de salud.
- Fallo por flotación insuficiente: no se ejecuta la prueba y nada se registra como resultado.

## 8. Criterios de aceptación

| ID | Given | When | Then |
| --- | --- | --- | --- |
| CA-01 | Una prueba trimestral programada con carga concurrente del 13 por ciento, igual a la línea base | El planificador ejecuta la prueba | El sistema la marca comparable, registra caída de tensión de menos 0,47 voltios y recuperación de unos 35 segundos |
| CA-02 | La misma prueba, con el equipo sin señal de veredicto de test | El sistema emite el resultado | El veredicto queda calculado por el servicio, con confianza baja por tener solo dos puntos de serie, e indica que no lo dictaminó el equipo |
| CA-03 | Dos muestras perdidas justo en el instante de conmutación | El sistema calcula los derivados | Las muestras perdidas tienen valores sin dato y no rompen el cálculo de los derivados (RN-05, aptitud de datos) |
| CA-04 | Una prueba con carga concurrente muy distinta de la línea base | El sistema compara contra la línea base | La prueba se marca no comparable y no entra en la tendencia de salud (RN-06) |
| CA-05 | Una prueba disparada poco después de un corte | El administrador intenta la prueba | El sistema responde FLOTACION_INSUFICIENTE y no la ejecuta |

## 9. Trazabilidad

| Dimensión | Referencia |
| --- | --- |
| Necesidad de negocio | NB-06 (Evaluación de salud de baterías) |
| Reglas de negocio aplicables | RN-05, RN-06; congelado del montaje según el modelo conceptual |
| Historias de usuario a generar | US-07 en 06 |
| Componentes esperados | Planificador con cadencia elevada, cálculo de derivados de prueba, resolutor temporal (referencia tentativa a 05) |
| Tests previstos | Prueba comparable vs no comparable, muestra perdida que no rompe derivados, veredicto con confianza, congelado del montaje (referencia tentativa a 08) |

## 10. Notas y supuestos

- Instancia de referencia: escenario §20.E-5 (prueba de batería periódica con muestras perdidas y veredicto calculado por el servicio).
- El método de salud es una tendencia relativa en unidades arbitrarias; el sistema no afirma capacidad remanente, estado de salud en porcentaje ni autonomía. Toda conclusión lleva la reserva por falta de sensor de temperatura.
- Se necesitan al menos cuatro pruebas comparables para pasar de confianza baja a una tendencia legible.

## 11. Interacción multiusuario y concurrencia

Trivial en usuarios. La prueba y el sondeo normal comparten el mismo canal con el equipo; el planificador serializa el acceso elevando la cadencia mientras dura la prueba.

## 12. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Redacción inicial derivada de SOLUTION-INTAKE UF-5 (§6), §20.E-5 y NB-06 |
