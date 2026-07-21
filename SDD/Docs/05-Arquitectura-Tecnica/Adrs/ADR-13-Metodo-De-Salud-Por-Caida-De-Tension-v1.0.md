# ADR-13 — Método de salud por tendencia de la caída de tensión

**Proyecto:** Sai-Service-Core
**Documento:** ADR-13-Metodo-De-Salud-Por-Caida-De-Tension-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Observabilidad

## 1. Contexto

El equipo no expone ningún indicador de salud y su autotest no devuelve veredicto (51 muestras sin `TEST`, sin `RB`, sin `ups.alarm`). El estándar de facto —confiar en la bandera `RB`— nunca se enciende en este equipo, así que un monitoreo convencional no alerta nunca. Ningún proyecto de software libre calcula salud desde NUT: todos retransmiten la bandera del firmware (riesgo R-14). La salud solo puede obtenerse midiendo la caída de tensión durante el autotest y guardando la serie temporal. Motivan la decisión el caso de uso CU-07 (prueba de batería y veredicto de salud), la necesidad NB-06 y la regla RN-06 (aptitud de datos para tendencia).

## 2. Decisión

El método de salud es la tendencia relativa de la caída de tensión durante el autotest, a carga igualada, comparada contra la línea base. El veredicto lo emite el servicio (porque el equipo no emite ninguno), viaja con su confianza explícita —arrancando en `baja`— y su advertencia. No se afirman magnitudes cuantitativas de salud (SoH %, capacidad en Ah, autonomía, resistencia interna absoluta).

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-13 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Tendencia de caída de tensión a carga igualada | Lo único que los datos disponibles permiten afirmar honestamente; con procedencia y confianza | Solo dice «se comporta peor que antes»; sensible a temperatura sin sensor (T-04) |
| Medición óhmica absoluta / coup de fouet / constante de recuperación / tensión de flotación | Métodos reconocidos de salud | Las cuatro descartadas con fundamento: el equipo no expone los datos necesarios |
| Retransmitir la bandera del firmware (como el resto) | Trivial | La bandera nunca se enciende en este equipo; no alerta nunca |

## 5. Consecuencias positivas

1. El sistema alerta sobre degradación donde un monitoreo convencional callaría (diferenciador central, R-14).
2. El veredicto es honesto y defendible: tendencia relativa con confianza y reserva declaradas (T-03).
3. La comparación a carga igualada evita conclusiones sobre pruebas no comparables (CL-26): si `deltaCargaConcurrente` excede la tolerancia, la prueba se marca `comparable: false` y no entra en la tendencia (I-16).

## 6. Consecuencias negativas y trade-offs

1. El método no certifica que una batería sirva: una batería al 20 % puede flotar como una nueva (CL-23). Solo afirma «se comporta peor que antes».
2. Sin sensor de temperatura, la oscilación estacional puede rivalizar con la señal de degradación (CL-24, T-04, R-09): toda conclusión lleva esa reserva.
3. Requiere ≥ 4 pruebas comparables para una tendencia legible; con menos, la confianza es `baja` (M-02).

## 7. Implementación

La `PruebaBateria` (programada trimestral o manual) eleva la cadencia de sondeo a 1 Hz, congela el `montajeBateriaId` y registra la serie de tensión. El cálculo del veredicto en `Domain` compara la caída contra la línea base a carga igualada, usa solo valores aptos (I-9, ADR-06), y marca `comparable: false` si la carga difirió (CL-26). El veredicto lleva confianza (`baja` con < 4 pruebas) y advertencia (reserva de temperatura). Precondición: tiempo mínimo en flotación cumplido (CL-25).

## 8. Métricas de validación

- M-02: ≥ 4 pruebas comparables acumuladas para pasar de confianza `baja` a tendencia legible.
- I-9/RN-06: el cálculo excluye valores `derivado`, `estimadoPorDriver` e `imputado`.
- CL-26/I-16: una prueba con carga fuera de tolerancia se marca `comparable: false` y no entra en la tendencia.

## 9. Referencias

- Intake §17 P.10, P.11 (PA-13); CL-23, CL-24, CL-25, CL-26; riesgos R-09, R-14; trade-offs T-03, T-04; métrica M-02.
- RN-06 Aptitud de datos para tendencia de salud; RN-13 Vida de flotación con temperatura de referencia (I-21).
- CU-07 Prueba de batería y veredicto de salud; NB-06 Evaluación de salud de baterías; exclusión E-08.
- ADR relacionadas: ADR-06, ADR-08.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-13. |
