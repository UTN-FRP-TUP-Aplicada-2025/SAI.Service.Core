# ADR-27 — Contrato del puerto del adaptador de conexión

**Proyecto:** Sai-Service-Core
**Documento:** ADR-27-Contrato-Del-Puerto-Del-Adaptador-De-Conexion-v1.0.md
**Versión:** 1.0
**Estado:** Aceptado
**Fecha:** 2026-07-21
**Autor:** Orquestador SDD (AG-05)
**Categoría:** Extensibilidad

## 1. Contexto

ADR-02 fijó el adaptador de conexión con tres implementaciones (NUT, directo, simulado) y su puerto en `Application`, pero ADR-22 dejó abierta la firma exacta del contrato (decisión de Sprint 0, pendiente P-06). El intake declara las cuatro operaciones del puerto (leer estado, probar conectividad, ordenar apagado con retorno, lanzar prueba de batería) pero no la firma. Este ADR cierra el contrato para poder codificar `Application` e `Infrastructure`.

## 2. Decisión

El puerto es la interfaz `IAdaptadorConexion` en `SAI.Service.Core.Application.Abstractions`, con cuatro operaciones asíncronas y cancelables, cuyos resultados se validan **por efecto observado** (ADR-11), nunca por ausencia de excepción:

```csharp
public interface IAdaptadorConexion
{
    Task<EstadoSai> LeerEstadoAsync(CancellationToken ct);
    Task<ResultadoConectividad> ProbarConectividadAsync(CancellationToken ct);
    Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, CancellationToken ct);
    Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct);
}
```

- `EstadoSai`: instantánea de las variables leídas del equipo, cada una con su procedencia (`Origen`, ADR-06); incluye el driver y el dialecto de la sesión de sondeo.
- `ResultadoConectividad`: si el equipo responde, con la latencia observada y el motivo si no responde.
- `ResultadoAccion`: el efecto **observado** de la acción (no un booleano de "sin error"): estado del equipo después de ordenar la acción, con la evidencia que permite afirmar que se ejecutó. Toda acción irreversible se valida contrastando el estado observado, coherente con ADR-11 y RN-03.

Las operaciones no lanzan excepción como forma de reportar falla de dominio: devuelven un resultado que el planificador interpreta. Una excepción representa una falla de transporte, no un veredicto sobre el equipo.

## 3. Estado

Aceptado — 2026-07-21. Supera a ADR-22, que pasa a estado `Superado por ADR-27`.

## 4. Alternativas consideradas

| Alternativa | Pros | Contras |
|---|---|---|
| Cuatro operaciones async con resultados por efecto observado (elegida) | Fuerza la validación por efecto (ADR-11/RN-03); cancelable; testeable con el adaptador simulado | Tipos de resultado más ricos que un booleano; más código |
| Métodos que devuelven `bool`/`void` y lanzan excepción ante falla | Firma simple | Descartada: "sin excepción" no prueba que la acción llegó al equipo (RN-03, PA-11); oculta el efecto observado |
| Un único método genérico `EjecutarComando(string)` | Flexible | Descartada: reintroduce el riesgo de construir comandos a mano (ADR-01) y pierde el tipado del efecto |

## 5. Consecuencias positivas

1. El contrato materializa la validación por efecto observado (ADR-11) en el tipo de retorno, no en una convención.
2. Las tres implementaciones (NUT, directo, simulado) comparten la misma firma; el adaptador simulado hace testeable el camino de apagado sin hardware.
3. Operaciones cancelables: el planificador puede abortar una ronda o un temporizador.

## 6. Consecuencias negativas y trade-offs

1. Los tipos de resultado (`EstadoSai`, `ResultadoConectividad`, `ResultadoAccion`) requieren diseño de detalle en las etapas de dominio; en el Sprint 0 se definen como placeholders razonables.
2. El contrato del apagado con retorno asume el modelo de NUT (`ups.delay.shutdown`/`ups.delay.start`); una implementación directa deberá mapear a ese modelo.

## 7. Implementación

`IAdaptadorConexion` y los records de resultado viven en `SAI.Service.Core.Application/Abstractions/`. `SAI.Service.Core.Infrastructure` provee `AdaptadorConexionNut` (Sprint posterior) y `AdaptadorConexionSimulado` (desde el Sprint 0, con valores fijos para que el DI y las pruebas compilen). La selección de implementación se hace por configuración en el composition root (`Web`), con el simulado como opción de DEV/pruebas. El detalle de los records se completa junto con el dominio en las etapas siguientes.

## 8. Métricas de validación

- Las tres implementaciones satisfacen la misma interfaz sin castear.
- Las pruebas del camino de apagado (08, TC del CU-05) corren contra `AdaptadorConexionSimulado` sin hardware.
- Ningún método reporta éxito de una acción irreversible por ausencia de excepción; hay un aserto sobre el efecto observado.

## 9. Referencias

- ADR-22 (decisión abierta que este ADR cierra), ADR-02 (adaptador con tres implementaciones), ADR-11 (validación por efecto observado), ADR-01 (NUT), RN-03.
- Intake §17 P.2, P.11 (P-06); Extensibilidad-v1.0.md; guía de testing de extensibilidad de 08.

## 10. Control de cambios

| Versión | Fecha | Descripción |
|---|---|---|
| 1.0 | 2026-07-21 | Cierre de la decisión de Sprint 0 (P-06): firma del puerto `IAdaptadorConexion` con cuatro operaciones y resultados por efecto observado. Supera a ADR-22. |
