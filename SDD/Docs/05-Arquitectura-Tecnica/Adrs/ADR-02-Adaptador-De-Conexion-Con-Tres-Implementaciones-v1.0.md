# ADR-02 — Adaptador de conexión con tres implementaciones

**Proyecto:** Sai-Service-Core
**Documento:** ADR-02-Adaptador-De-Conexion-Con-Tres-Implementaciones-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-20
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Extensibilidad

## 1. Contexto

El sistema debe hablar con el equipo para leer estado, probar conectividad, ordenar apagado con retorno y lanzar la prueba de batería. Si el dominio dependiera directamente de NUT, el camino de apagado —lo único con consecuencias irreversibles— sería imposible de probar sin hardware y sin riesgo real. Además, la fuente prevé equipos futuros que NUT no cubra (capa de add-ons, diseñada pero no implementada, E-07). El diseño necesita un punto de aislamiento entre el dominio y el transporte, y un mecanismo para ejercitar el flujo de apagado en pruebas automatizadas. Motivan la decisión CU-05 (apagado) y la capacidad F-24 (adaptador simulado para pruebas).

## 2. Decisión

Se define un adaptador de conexión como puerto en `Application`, con implementaciones en `Infrastructure`. Se prevén tres implementaciones: NUT (única en la primera entrega), directo con add-on de dialecto (diseñada, no implementada) y simulada (para probar políticas sin hardware ni riesgo). El contrato mínimo del puerto expone cuatro operaciones: leer estado, probar conectividad, ordenar apagado con retorno y lanzar test de batería. La firma exacta se cierra en ADR-22.

## 3. Estado

Aceptado el 2026-07-20. Decisión pre-tomada PA-02 del intake §17 P.11.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
| --- | --- | --- |
| Puerto en Application con tres implementaciones | Aísla el dominio del transporte; hace testeable el apagado; abre la puerta a equipos futuros | Una indirección más; obliga a mantener el adaptador simulado sincronizado con el real |
| Acceso directo a NUT desde la capa de aplicación | Menos código | Haría el apagado imposible de probar sin hardware; acopla el dominio a NUT |
| Una sola implementación (solo NUT), sin puerto | Simplicidad inmediata | No permite el adaptador simulado ni la capa de add-ons; rompe la estrategia de testing de P.6 |

## 5. Consecuencias positivas

1. El flujo de apagado y las políticas se prueban contra el adaptador simulado, sin riesgo ni hardware (F-24, P.6 nivel integración y e2e).
2. El dominio no conoce NUT: cambiar de mecanismo de transporte no toca la lógica (T-07).
3. La capa de add-ons de dialecto (E-07) encaja como una tercera implementación sin rediseño.

## 6. Consecuencias negativas y trade-offs

1. El adaptador simulado debe mantenerse fiel al comportamiento real; una divergencia daría falsa confianza (mitigado por las pruebas de integración del adaptador NUT).
2. El flujo físico completo de apagado y reencendido no es automatizable ni con el simulado (T-08): la verdad del firmware se registra como evidencia en la ventana de mantenimiento (CU-10), no como test.
3. La firma del contrato queda pendiente (P-06), lo que se resuelve en ADR-22.

## 7. Implementación

Puerto `IConexionSai` (nombre tentativo) en `Application`; implementaciones `ConexionNut`, `ConexionSimulada` y, futuro, `ConexionDirecta` en `Infrastructure`, registradas por inyección de dependencias. El planificador interno (ADR-15) consume el puerto, nunca una implementación concreta. Las pruebas de integración y e2e sustituyen la implementación real por la simulada.

## 8. Métricas de validación

- Los flujos UF-1, UF-3 y el camino de apagado de UF-8 corren en e2e contra el adaptador simulado.
- Cobertura del camino de decisión de apagado sin invocar hardware.
- Cero referencias a tipos de NUT desde `Domain` o `Application`.

## 9. Referencias

- Intake §17 P.2, P.11 (PA-02); §5.2 de la fuente; P.6 estrategia de testing.
- CU-05 Ejecución del apagado ordenado ante corte; F-24, F-26, F-27; exclusión E-07.
- ADR relacionadas: ADR-01, ADR-15, ADR-22. Pendiente P-06.

## 10. Control de cambios

| Versión | Fecha | Descripción |
| --- | --- | --- |
| 1.0 | 2026-07-20 | Versión inicial. Deriva de PA-02. |
