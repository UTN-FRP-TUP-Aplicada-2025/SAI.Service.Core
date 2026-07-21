namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Resultado de una prueba de conectividad contra el SAI.
/// <para>
/// Placeholder de Sprint 0. La conectividad se valida por efecto observado
/// (ADR-11): que no haya excepcion no alcanza para darla por buena.
/// </para>
/// </summary>
/// <param name="Conectado">Verdadero si el equipo respondio de forma observable.</param>
/// <param name="LatenciaMilisegundos">Latencia de ida y vuelta del sondeo, si se pudo medir.</param>
/// <param name="Detalle">Descripcion legible del resultado (para diagnostico y panel).</param>
public sealed record ResultadoConectividad(
    bool Conectado,
    double? LatenciaMilisegundos,
    string Detalle);
