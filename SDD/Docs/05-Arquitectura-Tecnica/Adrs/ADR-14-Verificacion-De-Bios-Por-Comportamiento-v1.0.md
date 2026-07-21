# ADR-14 — Verificación del ajuste de BIOS por comportamiento, no por lectura

**Proyecto:** Sai-Service-Core
**Documento:** ADR-14-Verificacion-De-Bios-Por-Comportamiento-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

El autoencendido depende de que la BIOS tenga «Restore on AC Power Loss» = Power On. Ese ajuste puede volverse falso en silencio (pila CMOS agotada, clear CMOS, cambio no documentado) (CL-03, riesgo R-03). Leerlo por software es *"posible pero inútil"*: frágil por versión de firmware, peligroso al escribir (riesgo de NVRAM corrupta, placa muerta), y verifica lo que no importa. El sistema necesita saber que el host reencenderá, no leer un registro. Motivan la decisión la regla RN-02 (bloqueo por verificación), el caso de uso CU-10 (ventana de mantenimiento) y la necesidad NB-01.

## 2. Decisión

No se lee el ajuste de BIOS por software; se verifica por comportamiento. El supuesto `ver-bios-autoencendido` se prueba en la ventana de mantenimiento (CU-10) cortando la energía y observando si el host arranca solo, y se renueva por evidencia con cada corte real seguido de arranque automático, cruzando eventos propios contra `wtmp`.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-14 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Verificar por comportamiento (cortar energía y observar arranque) | Prueba lo que importa (que el host vuelve); se renueva sola por evidencia | Requiere una ventana de mantenimiento destructiva con presencia física (UF-8) |
| Leer `efivars`/IFR por software | Sin ventana destructiva | Frágil por versión; verifica el registro, no el comportamiento; descartada (§4.6) |
| Escribir el ajuste por software | Automatizaría la corrección | Peligroso: riesgo de NVRAM corrupta, recuperación por programador SPI, placa muerta |

## 5. Consecuencias positivas

1. Se verifica el hecho que importa —que el host reenciende— y no un registro que puede no reflejarlo.
2. La verificación se mantiene viva sola: cada corte real seguido de arranque automático renueva `ver-bios-autoencendido` (F-25), cruzando eventos contra `wtmp`.
3. Se evita el riesgo de escribir la NVRAM y dejar la placa muerta (CL-03, §4.6).

## 6. Consecuencias negativas y trade-offs

1. La verificación inicial exige una ventana de mantenimiento destructiva con presencia física (CU-10, UF-8), no automatizable.
2. Si el host no arranca en la prueba, `ver-bios-autoencendido` pasa a `Refutado` y bloquea permanentemente hasta resolución (ADR-10).
3. La vigencia es de 365 días: pasado ese plazo sin evidencia renovadora, el supuesto vence y la modalidad degrada a `SoloAlerta`.

## 7. Implementación

El supuesto `ver-bios-autoencendido` se modela como `Verificacion` (ADR-10) con vigencia 365 días. Se prueba en CU-10 (paso: cortar energía, observar arranque). La renovación por evidencia (F-25) la maneja el planificador cruzando eventos de corte propios contra `wtmp` del host. La lectura por software de `efivars`/IFR queda explícitamente fuera de alcance (E-06, F-32). Sin sensor ni escritura de BIOS.

## 8. Métricas de validación

- CU-10: `ver-bios-autoencendido` pasa a `Verificado` si el host arranca solo; a `Refutado` si no.
- F-25: un corte real seguido de arranque automático renueva la verificación sin repetir la prueba destructiva.
- El sistema no ejecuta ninguna lectura ni escritura de la NVRAM/BIOS.

## 9. Referencias

- Intake §17 P.11 (PA-14); CL-03; riesgo R-03; F-25, F-32; exclusión E-06.
- RN-02 Bloqueo por verificación; NB-01 Apagado ordenado y reencendido garantizado.
- CU-10 Ventana de mantenimiento y verificación. US-10.
- ADR relacionadas: ADR-10, ADR-11.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-14. |
