using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Evento derivado del sondeo (US-09, CU-04). Es historia append-only (ADR-04) y referencia
/// obligatoriamente la <see cref="ReglaDerivacion"/> versionada con la que se derivó (RC-09, I-14):
/// guarda el par (código, versión) exacto. La duración puede llevar una incertidumbre estructural
/// (CL-10): un microcorte más breve que el intervalo de sondeo se declara con su margen.
/// </summary>
public sealed class Evento : IEntidadHistoria
{
    /// <summary>Código de negocio del evento.</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI) al que refiere el evento.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Tipo de evento.</summary>
    public TipoEvento Tipo { get; private set; }

    /// <summary>Instante del evento.</summary>
    public DateTimeOffset Instante { get; private set; }

    /// <summary>Duración del evento en segundos, o nula (evento puntual).</summary>
    public double? DuracionSeg { get; private set; }

    /// <summary>Incertidumbre de la duración en segundos, o nula (CL-10).</summary>
    public double? IncertidumbreDuracionSeg { get; private set; }

    /// <summary>Código de la regla de derivación con la que se derivó (RC-09).</summary>
    public string ReglaDerivacionCodigo { get; private set; }

    /// <summary>Versión de la regla de derivación con la que se derivó (RC-09).</summary>
    public int ReglaVersion { get; private set; }

    // Constructor de materialización (EF Core).
    private Evento()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        ReglaDerivacionCodigo = null!;
    }

    /// <summary>Registra un evento derivado por una regla versionada.</summary>
    public Evento(
        string codigo,
        string dispositivoCodigo,
        TipoEvento tipo,
        DateTimeOffset instante,
        ReglaDerivacion regla,
        double? duracionSeg = null,
        double? incertidumbreDuracionSeg = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentNullException.ThrowIfNull(regla);

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        Tipo = tipo;
        Instante = instante;
        ReglaDerivacionCodigo = regla.Codigo;
        ReglaVersion = regla.Version;
        DuracionSeg = duracionSeg;
        IncertidumbreDuracionSeg = incertidumbreDuracionSeg;
    }
}
