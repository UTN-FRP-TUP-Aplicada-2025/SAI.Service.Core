# ADR-22 — Forma del contrato del adaptador de conexión

**Proyecto:** Sai-Service-Core
**Documento:** ADR-22-Contrato-Del-Adaptador-De-Conexion-v1.0.md
**Versión:** 1.0
**Estado:** Superado por ADR-27
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Extensibilidad

## 1. Contexto

El adaptador de conexión (ADR-02) aísla el dominio del transporte y tiene tres implementaciones previstas (NUT, directo con add-on, simulada). La fuente declara sus cuatro operaciones —leer estado, probar conectividad, ordenar apagado con retorno, lanzar test de batería— pero deja la firma exacta abierta (pendiente P-06, a cerrar en la categoría 05). La firma condiciona la testabilidad del camino de apagado y la futura capa de add-ons de dialecto (E-07). Es una decisión abierta: este ADR documenta el problema y las opciones, no cierra una elección. Motivan la decisión el caso de uso CU-05 (apagado) y las capacidades F-24, F-26, F-27.

## 2. Decisión

PENDIENTE (P-06, Sprint 0 / categoría 05). Se documentan las opciones para la firma del puerto del adaptador y qué falta para decidir. No se adopta ninguna todavía. La firma debe permitir validar cada operación por efecto observado (ADR-11) y ser implementable tanto por NUT como por el adaptador simulado.

## 3. Estado

Propuesto el 2026-07-20. Decisión abierta (§17 P.11; pendiente P-06). Se convertirá en una ADR aceptada nueva al fijarse la firma, y esta pasará a `Superado por ADR-YY`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Puerto con cuatro operaciones asíncronas que devuelven estado observado | Cada operación confirma su efecto (ADR-11); implementable por NUT y por el simulado | Hay que definir los tipos de retorno (estado, resultado de test) y el manejo de timeouts |
| Operaciones que devuelven void y un flujo de eventos aparte para el estado | Desacopla acción de observación | Complica la confirmación por efecto de cada acción; más difícil de probar linealmente |
| Un adaptador por operación (puertos separados) | Máxima granularidad | Fragmenta la abstracción; la capa de add-ons y el simulado deberían implementar varios puertos |

## 5. Consecuencias positivas

1. (Esperadas al cerrar) Una firma que permite validar cada acción por efecto observado (ADR-11).
2. El adaptador simulado implementa la misma firma que el NUT, sosteniendo la estrategia de testing (P.6, F-24).
3. La firma deja lugar a la tercera implementación (directo con add-on de dialecto) sin rediseño (E-07, F-26, F-27).

## 6. Consecuencias negativas y trade-offs

1. Mientras siga abierta, la implementación de `Infrastructure` no puede fijar el puerto y el simulado no puede escribirse definitivo.
2. Definir los tipos de retorno (estado, resultado de test de batería) y el manejo de timeouts requiere alinear con el modelo de dominio.
3. La firma condiciona cuán fácil es sostener el adaptador simulado fiel al real (riesgo de divergencia, ADR-02).

## 7. Implementación

Qué falta para decidir: (a) fijar la firma de las cuatro operaciones (leer estado, probar conectividad, ordenar apagado con retorno, lanzar test de batería) y sus tipos de retorno; (b) definir el manejo de timeouts y cancelación acorde al planificador (ADR-15); (c) verificar que NUT y el adaptador simulado la implementan sin fricción. Se cierra en esta categoría y se documenta en `Extensibilidad` / `contratos` del proyecto.

## 8. Métricas de validación

- Al cerrar: NUT y el adaptador simulado implementan la misma firma; el camino de apagado corre en e2e contra el simulado (F-24).
- Cada operación permite confirmar su efecto observado (ADR-11).

## 9. Referencias

- Intake §17 P.2, P.11 (Sprint 0); pendiente P-06; §5.2 de la fuente.
- CU-05 Ejecución del apagado ordenado ante corte; F-24, F-26, F-27; exclusión E-07.
- ADR relacionadas: ADR-02, ADR-11, ADR-15.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Decisión abierta (P-06). |
| 1.1 | 2026-07-21 | Decisión cerrada en el Sprint 0; este ADR queda superado por ADR-27. Ver ese ADR para la decisión adoptada. |
