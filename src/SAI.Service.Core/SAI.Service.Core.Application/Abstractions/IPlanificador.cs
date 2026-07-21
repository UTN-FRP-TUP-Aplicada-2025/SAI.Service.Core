namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Marcador del planificador interno de sondeo (§17.P.2, ADR-15).
/// <para>
/// En etapas posteriores el planificador corre como <i>hosted service</i>: ejecuta
/// rondas de sondeo en el intervalo configurado, evalua politicas, usa temporizadores
/// con cancelacion (una condicion debe sostenerse N segundos antes de disparar, para
/// no actuar ante microcortes), detecta perdida de comunicacion y eleva la cadencia
/// durante una prueba de bateria.
/// </para>
/// <para>
/// En Sprint 0 es solo el contrato marcador: no hay logica de sondeo ni evaluacion
/// de politicas todavia. Se define aca, en Application, para que el hosted service
/// futuro se registre por esta abstraccion desde el composition root.
/// </para>
/// </summary>
public interface IPlanificador
{
    /// <summary>Indica si el planificador esta activo. En Sprint 0 siempre es <c>false</c>.</summary>
    bool Activo { get; }
}
