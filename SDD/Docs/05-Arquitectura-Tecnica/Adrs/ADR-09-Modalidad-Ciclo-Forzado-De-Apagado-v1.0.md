# ADR-09 — Modalidad CicloForzado: no cancelar el corte del SAI aunque vuelva la red

**Proyecto:** Sai-Service-Core
**Documento:** ADR-09-Modalidad-Ciclo-Forzado-De-Apagado-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

El problema crítico del proyecto es garantizar el reencendido. La BIOS solo dispara el autoencendido cuando detecta una transición de ausencia a presencia de energía; por lo tanto el SAI debe cortar su salida aunque el host ya esté apagado. Si vuelve la energía durante la cuenta regresiva y el SAI cancela su apagado, no hubo transición, la BIOS no tiene nada que detectar y el host queda apagado hasta que alguien apriete el botón (CL-01): *"el peor resultado posible"*. Motivan la decisión el caso de uso CU-05 (ejecución del apagado ante corte) y la necesidad NB-01 (apagado ordenado y reencendido garantizado).

## 2. Decisión

Se adopta la modalidad `CicloForzado`: una vez iniciada la secuencia de apagado, el corte del SAI no se cancela aunque vuelva la red. No se usa `shutdown.stop`. Es preferible un apagón controlado de tres minutos a un servidor apagado hasta la mañana siguiente.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-09 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| `CicloForzado`: no cancelar el corte | Garantiza la transición de energía y el autoencendido de la BIOS | Un apagón controlado de ~3 min cuando el corte resulta breve (trade-off aceptado T-06) |
| Cancelar al retornar la energía (comportamiento por defecto) | Evita el apagón si el corte fue breve | Produce el bloqueo: sin transición, el host queda apagado indefinidamente (CL-01) |
| Decidir por duración estimada del corte | Optimiza casos breves | El futuro del corte no es predecible; una mala estimación reproduce el bloqueo |

## 5. Consecuencias positivas

1. Se garantiza la transición de energía que la BIOS necesita para el autoencendido (NB-01, objetivo 1).
2. El host nunca queda apagado indefinidamente esperando intervención manual (T-06).
3. La modalidad es explícita y versionada en la política (ADR-15, RN-11), de modo que cada acción se explica con la configuración que la produjo.

## 6. Consecuencias negativas y trade-offs

1. Trade-off central y aceptado (T-06): un apagón controlado de ~3 minutos cuando el corte resulta ser breve.
2. Depende de que el firmware soporte el apagado con retorno (`shutdown.return`), lo que se verifica antes de habilitar la modalidad (CL-02, ADR-14, CU-10).
3. La modalidad efectiva solo llega a `HostLuegoUpsConRetorno`/`CicloForzado` si las verificaciones están cumplidas; si no, degrada a `SoloAlerta` (ADR-10).

## 7. Implementación

Modalidades de política `SoloAlerta`, `SoloHost`, `HostLuegoUpsConRetorno`, `CicloForzado` en `Domain`. El planificador (ADR-15) ejecuta la secuencia y no invoca cancelación del corte una vez iniciada. El disparo usa tiempo en `OB` + `battery.voltage`, nunca `LB` (ADR-12). La acción se valida por efecto observado (ADR-11) y referencia siempre una `VersionPolitica` (RN-11).

## 8. Métricas de validación

- Prueba sobre el adaptador simulado (ADR-02): iniciada la secuencia, un retorno de red no cancela el corte.
- CU-10/UF-8: `ver-shutdown-return` verificado antes de habilitar cualquier modalidad distinta de `SoloAlerta`.
- M-03: cero arranques `crash` en `wtmp` atribuibles a un corte no gestionado.

## 9. Referencias

- Intake §17 P.11 (PA-09), P.10; CL-01, CL-02; trade-off T-06; riesgo R-01.
- CU-05 Ejecución del apagado ordenado ante corte; NB-01 Apagado ordenado y reencendido garantizado.
- ADR relacionadas: ADR-10, ADR-11, ADR-12, ADR-14, ADR-15.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-09. |
