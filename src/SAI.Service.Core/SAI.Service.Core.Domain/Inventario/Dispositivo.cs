namespace SAI.Service.Core.Domain.Inventario;

/// <summary>
/// Dispositivo (el SAI/UPS físico) del inventario (ADR-07). Es una unidad de un
/// <c>ModeloDispositivo</c> del catálogo; aloja baterías (vínculo temporal <c>MontajeBateria</c>)
/// y cubre hosts (<c>CoberturaHost</c>).
/// </summary>
public sealed class Dispositivo : UnidadFisica
{
    /// <summary>Código del modelo de dispositivo (catálogo) del que esta unidad es un ejemplar.</summary>
    public string ModeloDispositivoCodigo { get; }

    /// <summary>Número de serie, o nulo: es anulable a propósito (no todo equipo lo expone).</summary>
    public string? NumeroSerie { get; }

    /// <summary>Construye un dispositivo de inventario ligado a su modelo de catálogo.</summary>
    public Dispositivo(string codigo, string modeloDispositivoCodigo, string? numeroSerie = null)
        : base(codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeloDispositivoCodigo);
        ModeloDispositivoCodigo = modeloDispositivoCodigo;
        NumeroSerie = numeroSerie;
    }
}
