using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Prueba de batería con su veredicto de salud (CU-07, US-12, US-13). Es historia append-only
/// (ADR-04). El montaje evaluado se <b>congela</b> (I-15, RC-07): se resuelve una sola vez en el
/// instante de la prueba y no se recalcula aunque luego cambie. Persiste solo el derivado de la
/// caída (con su procedencia), la carga concurrente, la comparabilidad y el veredicto/confianza; la
/// serie densa vive como <see cref="Muestra"/> aparte (ADR-08).
/// </summary>
public sealed class PruebaBateria : IEntidadHistoria
{
    /// <summary>Código de negocio de la prueba.</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI) probado.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Código del montaje de batería congelado (resuelto en el instante, I-15).</summary>
    public string MontajeBateriaCodigo { get; private set; }

    /// <summary>Instante de la prueba.</summary>
    public DateTimeOffset Instante { get; private set; }

    /// <summary>Caída de tensión (reposo → mínima), derivada; valor nulo si no calculable.</summary>
    public Valor<double?> CaidaTension { get; private set; }

    /// <summary>Carga concurrente durante la prueba (para la comparabilidad), si se conoce.</summary>
    public int? CargaPorcentaje { get; private set; }

    /// <summary>Verdadero si la prueba es comparable a carga igualada (RN-06, I-16).</summary>
    public bool Comparable { get; private set; }

    /// <summary>Veredicto de tendencia, o nulo si no comparable / no calculable.</summary>
    public VeredictoSalud? Veredicto { get; private set; }

    /// <summary>Confianza del veredicto (arranca baja), o nula si no hay veredicto.</summary>
    public ConfianzaVeredicto? Confianza { get; private set; }

    // Constructor de materialización (EF Core).
    private PruebaBateria()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        MontajeBateriaCodigo = null!;
    }

    /// <summary>Registra una prueba a partir del veredicto calculado por <see cref="CalculadorSaludBateria"/>.</summary>
    public static PruebaBateria Registrar(
        string codigo,
        string dispositivoCodigo,
        string montajeBateriaCodigo,
        DateTimeOffset instante,
        ResultadoSalud resultado)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(montajeBateriaCodigo);
        ArgumentNullException.ThrowIfNull(resultado);

        return new PruebaBateria
        {
            Codigo = codigo,
            DispositivoCodigo = dispositivoCodigo,
            MontajeBateriaCodigo = montajeBateriaCodigo,
            Instante = instante,
            CaidaTension = resultado.Caida,
            CargaPorcentaje = resultado.CargaPorcentaje,
            Comparable = resultado.Comparable,
            Veredicto = resultado.Veredicto,
            Confianza = resultado.Veredicto is null ? null : resultado.Confianza,
        };
    }
}
