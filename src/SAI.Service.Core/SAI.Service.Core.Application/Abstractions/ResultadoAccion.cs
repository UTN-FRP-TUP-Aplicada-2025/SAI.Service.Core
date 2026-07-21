namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Resultado de una accion con consecuencias sobre el equipo (apagado con retorno,
/// test de bateria). Placeholder de Sprint 0.
/// <para>
/// Toda accion se valida <b>por efecto observado</b> (ADR-11): <see cref="Aceptada"/>
/// indica que la orden fue admitida, no que el efecto ocurrio. La verificacion del
/// efecto real es responsabilidad de etapas posteriores.
/// </para>
/// </summary>
/// <param name="Aceptada">La orden fue admitida por el adaptador.</param>
/// <param name="Motivo">Motivo del resultado (por ejemplo, bloqueo por verificacion, RN-02).</param>
/// <param name="MarcaTiempoUtc">Instante en que se emitio la orden, en UTC.</param>
public sealed record ResultadoAccion(
    bool Aceptada,
    string Motivo,
    DateTimeOffset MarcaTiempoUtc);
