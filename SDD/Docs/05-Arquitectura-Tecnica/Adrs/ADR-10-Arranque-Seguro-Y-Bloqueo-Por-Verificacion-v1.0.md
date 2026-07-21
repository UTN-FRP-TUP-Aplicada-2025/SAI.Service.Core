# ADR-10 — Arranque seguro en SoloAlerta y bloqueo por verificación

**Proyecto:** Sai-Service-Core
**Documento:** ADR-10-Arranque-Seguro-Y-Bloqueo-Por-Verificacion-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Seguridad

## 1. Contexto

El riesgo principal del proyecto es de expectativa: *"el servicio va a tomar la decisión de apagar un servidor sin backups. Si falla, falla de noche y sin testigos"* (R-12, impacto crítico). El sistema no debe apagar el host mientras no pueda probar que va a volver a encenderse. Los supuestos de los que depende el apagado (presupuesto de apagado, flag `OB`, autoencendido de BIOS, apagado con retorno) pueden estar sin verificar. Motivan la decisión las reglas de negocio RN-01 (arranque seguro en SoloAlerta) y RN-02 (bloqueo por verificación), la necesidad NB-05 (seguridad operativa) y el caso de uso CU-10 (ventana de mantenimiento).

## 2. Decisión

El sistema arranca forzado en `SoloAlerta` y no habilita ninguna otra modalidad mientras algún supuesto del que depende esté en `NuncaVerificado`, `Vencido` o `Refutado`. En ese caso la `Accion` queda `BloqueadaPorVerificacion` con su motivo y la `modalidadEfectiva` degrada a `SoloAlerta`. No es una recomendación de puesta en marcha: es un estado impuesto por el sistema.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-10 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Arranque en SoloAlerta + bloqueo por verificación | El sistema nunca apaga sin evidencia; mitiga R-12; degradación segura por defecto | El apagado automático no está disponible hasta completar la ventana de mantenimiento (UF-8) |
| Confiar en los supuestos sin verificarlos | Apagado automático disponible de inmediato | Descartado explícitamente: puede dejar el host apagado sin testigos si un supuesto es falso |
| Verificación opcional configurable | Flexibilidad para el administrador | Convierte la garantía en una opción que se puede desactivar por error; anula NB-05 |

## 5. Consecuencias positivas

1. Fail-safe por diseño: ante cualquier supuesto no probado, el sistema alerta pero no apaga (RN-01, RN-02).
2. Mitiga el riesgo principal del proyecto (R-12): no se ejecuta una acción irreversible sin evidencia.
3. El panel muestra en pantalla principal cuántos de los supuestos están verificados (n de m) y el aviso de degradación (CU-04, US-02), no enterrado en configuración.

## 6. Consecuencias negativas y trade-offs

1. El apagado automático no está disponible hasta completar al menos una ventana de mantenimiento (CU-10, M-01: pasar de 0/4 a 4/4).
2. `Refutado` bloquea permanentemente hasta que alguien lo resuelva (distinto de `Vencido`, que solo pide repetir): una prueba fallida deja el sistema en alerta.
3. Requiere modelar la entidad `Verificacion` con evidencia, método, vigencia y estados, y la lógica de degradación de modalidad (F-11).

## 7. Implementación

Entidad `Verificacion` con estados `NuncaVerificado`/`Verificado`/`Vencido`/`Refutado`, evidencia, método y vigencia. El planificador (ADR-15) evalúa los supuestos requeridos por la `VersionPolitica` vigente antes de cada acción; si alguno no está `Verificado`, la `Accion` es `BloqueadaPorVerificacion` y la modalidad efectiva es `SoloAlerta`. La siembra inicial deja las cuatro verificaciones en `NuncaVerificado` (UF-1). La renovación por evidencia (F-25) y la ventana de mantenimiento (CU-10) las hacen pasar a `Verificado`.

## 8. Métricas de validación

- M-01: de 0/4 supuestos verificados al alta a 4/4 con modalidad efectiva `HostLuegoUpsConRetorno` tras UF-8.
- Prueba: con un supuesto en `Vencido` o `Refutado`, toda acción de apagado queda `BloqueadaPorVerificacion`.
- El panel expone n de m supuestos verificados y el aviso de degradación (CU-04).

## 9. Referencias

- Intake §17 P.5, P.11 (PA-10), P.10; riesgo R-12; métrica M-01; F-11, F-25.
- RN-01 Arranque seguro en SoloAlerta; RN-02 Bloqueo por verificación; NB-05 Seguridad operativa (bloqueo por verificación).
- CU-04 Monitoreo en vivo; CU-10 Ventana de mantenimiento y verificación. US-02, US-10.
- ADR relacionadas: ADR-09, ADR-11, ADR-14, ADR-15.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-10. |
