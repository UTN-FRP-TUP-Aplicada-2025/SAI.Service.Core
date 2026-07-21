# ADR-11 — Validación por efecto observado, no por ausencia de error

**Proyecto:** Sai-Service-Core
**Documento:** ADR-11-Validacion-Por-Efecto-Observado-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Observabilidad

## 1. Contexto

Durante el relevamiento, un comando que nunca llegó al equipo no produjo ningún mensaje de error; se detectó solo comparando datos (CL-07). Un servicio que asuma que «no hubo excepción» equivale a «se ejecutó» va a mentir, y en el camino de apagado esa mentira tiene consecuencias irreversibles. También hay que vigilar la propia conectividad con el equipo (CL-14, O-U11). Motivan la decisión la regla de negocio RN-03 (validación por efecto observado) y los casos de uso CU-04 (monitoreo) y CU-05 (apagado).

## 2. Decisión

Ninguna acción sobre el equipo se da por ejecutada porque no haya habido excepción: toda acción se valida por su efecto observado. La pérdida de comunicación se detecta activamente: 3 sondeos consecutivos sin respuesta generan el evento `DesconexionUsb` y una alerta visual en el panel.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-11 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Validar por efecto observado | Detecta comandos que se pierden sin error; base de la confianza operativa | Requiere leer el estado tras cada acción; lógica de confirmación adicional |
| Asumir éxito ante ausencia de excepción | Código más simple | Descartado: *"va a mentir"*; un comando perdido pasa por exitoso (CL-07) |
| Reintentos ciegos sin confirmar efecto | Robustez aparente | Puede repetir acciones peligrosas sobre el equipo sin saber si la primera surtió efecto |

## 5. Consecuencias positivas

1. Un comando que se pierde no se contabiliza como exitoso: se detecta por el estado que no cambió (CL-07).
2. La vigilancia de conectividad convierte una desaparición del bus USB en un evento y una alerta, en vez de en un silencio (CL-14, F-09, R-06).
3. La verificación por comportamiento de otros supuestos (autoencendido de BIOS, ADR-14) se apoya en esta misma disciplina: se cruzan eventos propios contra `wtmp`.

## 6. Consecuencias negativas y trade-offs

1. Cada acción cuesta una lectura de confirmación adicional: más latencia y más código que un disparo ciego.
2. La confirmación del efecto no siempre es inmediata (un apagado tarda), lo que obliga a modelar temporizadores y ventanas de observación en el planificador.
3. Un microcorte más corto que el intervalo de sondeo no es observable de forma confiable; el sistema lo declara con incertidumbre en vez de ocultarlo (CL-10, T-02).

## 7. Implementación

El planificador (ADR-15) confirma el efecto de cada acción leyendo el estado posterior a través del adaptador de conexión (ADR-02). El contador de sondeos fallidos dispara `DesconexionUsb` a los 3 consecutivos. Las acciones registran su resultado observado en la historia (`Accion`, append-only, ADR-04). La renovación de verificaciones por evidencia (F-25) usa el cruce de eventos propios contra `wtmp`.

## 8. Métricas de validación

- Prueba sobre el adaptador simulado: un comando sin efecto no se registra como exitoso.
- F-09: 3 sondeos fallidos consecutivos generan `DesconexionUsb` y alerta (CL-14).
- Cada `Accion` en la historia declara su resultado observado.

## 9. Referencias

- Intake §17 P.3, P.5, P.10, P.11 (PA-11); CL-07, CL-10, CL-14; riesgo R-06; F-09.
- RN-03 Validación por efecto observado.
- CU-04 Monitoreo en vivo; CU-05 Ejecución del apagado ordenado ante corte.
- ADR relacionadas: ADR-02, ADR-09, ADR-14, ADR-15.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-11. |
