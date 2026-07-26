using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Intervención de servicio técnico <b>ingresada por la API de ingesta</b> (CU-11, US-21): un hecho que un
/// sistema externo (GMAO) empuja sin intervención humana. Es historia append-only (<see cref="IEntidadHistoria"/>):
/// la propia historia <b>es</b> el almacén de idempotencia (ADR-17), identificada por
/// <see cref="ClaveIdempotencia"/> —única— y con la <see cref="HuellaCuerpo"/> (sha256 del cuerpo) para
/// distinguir un reintento idéntico de un conflicto. Al provenir de una fuente externa sin verificación
/// cruzada, queda con <see cref="Confianza"/> media (menor que la del sondeo local, ADR-06). Registra los
/// <b>dos tiempos</b> (bitemporalidad): cuándo ocurrió (<see cref="TiempoValido"/>) y cuándo se registró
/// (<see cref="TiempoRegistrado"/>).
/// </summary>
public sealed class IntervencionIngerida : IEntidadHistoria
{
    /// <summary>Identificador del registro creado.</summary>
    public string Codigo { get; private set; }

    /// <summary>Clave de idempotencia provista por el emisor (única): resuelve reintento vs. conflicto.</summary>
    public string ClaveIdempotencia { get; private set; }

    /// <summary>Huella sha256 (hex) del cuerpo recibido: define si un reenvío es idéntico o distinto.</summary>
    public string HuellaCuerpo { get; private set; }

    /// <summary>Código de la fuente de datos externa que empujó el hecho.</summary>
    public string FuenteDatosCodigo { get; private set; }

    /// <summary>Confianza asignada al hecho (media por origen externo, ADR-06).</summary>
    public ConfianzaFuente Confianza { get; private set; }

    /// <summary>Clase de servicio técnico declarada por el emisor (inspección, recambio, etc.).</summary>
    public string TipoIntervencion { get; private set; }

    /// <summary>Dispositivo afectado por la intervención.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Baterías afectadas (según el tipo de intervención); puede estar vacío.</summary>
    public IReadOnlyList<string> Baterias { get; private set; }

    /// <summary>Ejecutor externo, si aplica.</summary>
    public string? Proveedor { get; private set; }

    /// <summary>Costo de repuestos (suma de los importes declarados).</summary>
    public Dinero Repuestos { get; private set; }

    /// <summary>Costo de mano de obra.</summary>
    public Dinero ManoDeObra { get; private set; }

    /// <summary>Total declarado; cuadra con repuestos + mano de obra (RN-08).</summary>
    public Dinero Total { get; private set; }

    /// <summary>Costos como value object (calculado; no se persiste aparte).</summary>
    public Costos Costos => new(Repuestos, ManoDeObra, Total);

    /// <summary>Observaciones de la intervención.</summary>
    public string? Hallazgos { get; private set; }

    /// <summary>Destino de la batería retirada (columna nullable; ver <see cref="Disposicion"/>).</summary>
    public string? DisposicionDestino { get; private set; }

    /// <summary>Receptor de la batería retirada (columna nullable; ver <see cref="Disposicion"/>).</summary>
    public string? DisposicionReceptor { get; private set; }

    /// <summary>Disposición final de una batería retirada, si la hubo (trazabilidad ambiental, calculada).</summary>
    public DisposicionFinal? Disposicion =>
        DisposicionDestino is { } destino && DisposicionReceptor is { } receptor
            ? new DisposicionFinal(destino, receptor)
            : null;

    /// <summary>Cuándo ocurrió la intervención (tiempo válido, bitemporalidad).</summary>
    public DateTimeOffset TiempoValido { get; private set; }

    /// <summary>Cuándo se registró en el servicio (tiempo de sistema).</summary>
    public DateTimeOffset TiempoRegistrado { get; private set; }

    // Constructor de materialización (EF Core).
    private IntervencionIngerida()
    {
        Codigo = null!;
        ClaveIdempotencia = null!;
        HuellaCuerpo = null!;
        FuenteDatosCodigo = null!;
        TipoIntervencion = null!;
        DispositivoCodigo = null!;
        Baterias = [];
    }

    /// <summary>Construye el hecho ingresado. Exige que los costos cuadren (RN-08): la ingesta valida antes.</summary>
    public IntervencionIngerida(
        string codigo, string claveIdempotencia, string huellaCuerpo, string fuenteDatosCodigo, ConfianzaFuente confianza,
        string tipoIntervencion, string dispositivoCodigo, IReadOnlyList<string> baterias, string? proveedor,
        Costos costos, string? hallazgos, DisposicionFinal? disposicion, DateTimeOffset tiempoValido, DateTimeOffset tiempoRegistrado)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(claveIdempotencia);
        ArgumentException.ThrowIfNullOrWhiteSpace(huellaCuerpo);
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteDatosCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(tipoIntervencion);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentNullException.ThrowIfNull(baterias);
        if (!costos.Cuadra())
        {
            throw new ArgumentException("Los costos de la intervención deben cuadrar (RN-08).", nameof(costos));
        }

        Codigo = codigo;
        ClaveIdempotencia = claveIdempotencia;
        HuellaCuerpo = huellaCuerpo;
        FuenteDatosCodigo = fuenteDatosCodigo;
        Confianza = confianza;
        TipoIntervencion = tipoIntervencion;
        DispositivoCodigo = dispositivoCodigo;
        Baterias = baterias;
        Proveedor = proveedor;
        Repuestos = costos.Repuestos;
        ManoDeObra = costos.ManoDeObra;
        Total = costos.Total;
        Hallazgos = hallazgos;
        DisposicionDestino = disposicion?.Destino;
        DisposicionReceptor = disposicion?.Receptor;
        TiempoValido = tiempoValido;
        TiempoRegistrado = tiempoRegistrado;
    }
}
