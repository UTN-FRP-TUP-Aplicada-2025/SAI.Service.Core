using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Registro de una intervención de <b>reparación o sustitución del SAI</b> con cobertura suplente
/// (CU-09, US-20). Es historia append-only (ADR-04): un solo acto cierra la cobertura vigente del host
/// por el equipo saliente, cambia su estado (en reparación o dado de baja) y —si hay suplente— abre una
/// cobertura nueva. Deja trazado proveedor, ejecutor, hallazgos, el costo opcional y la disposición
/// final del equipo retirado. Distingue el <see cref="InstanteOcurrido"/> (cuándo pasó) del
/// <see cref="InstanteRegistrado"/> (cuándo se cargó). <see cref="FirmwareReiniciado"/> marca si, por
/// ser el suplente de otro modelo, se reiniciaron las verificaciones de firmware (FA-2).
/// <para>
/// El costo y la disposición son <b>opcionales</b> y se guardan como columnas nullable (no como
/// value objects complejos): un <see cref="Dinero"/> siempre lleva moneda y fecha si hay monto (RN-07),
/// y la <see cref="DisposicionFinal"/> solo aplica cuando el equipo se dio de baja.
/// </para>
/// </summary>
public sealed class SustitucionSai : IEntidadHistoria
{
    /// <summary>Código de negocio de la intervención.</summary>
    public string Codigo { get; private set; }

    /// <summary>Host cuya cobertura se cerró y (si hubo suplente) se reabrió.</summary>
    public string HostCodigo { get; private set; }

    /// <summary>Equipo (SAI) que sale de servicio.</summary>
    public string DispositivoSalienteCodigo { get; private set; }

    /// <summary>Equipo suplente que pasa a cubrir, o nulo si el host queda sin protección.</summary>
    public string? DispositivoEntranteCodigo { get; private set; }

    /// <summary>Reparación (queda en reparación) o reemplazo (queda dado de baja).</summary>
    public TipoIntervencionSai Tipo { get; private set; }

    /// <summary>Instante en que ocurrió la intervención (cierra la cobertura vigente).</summary>
    public DateTimeOffset InstanteOcurrido { get; private set; }

    /// <summary>Instante en que se registró en el sistema (carga diferida).</summary>
    public DateTimeOffset InstanteRegistrado { get; private set; }

    /// <summary>Proveedor que realizó la intervención.</summary>
    public string Proveedor { get; private set; }

    /// <summary>Persona que ejecutó la intervención.</summary>
    public string Ejecutor { get; private set; }

    /// <summary>Hallazgos registrados.</summary>
    public string Hallazgos { get; private set; }

    /// <summary>Verdadero si el suplente era de otro modelo y se reiniciaron las verificaciones (FA-2).</summary>
    public bool FirmwareReiniciado { get; private set; }

    /// <summary>Monto del costo, o nulo si no se registró costo.</summary>
    public decimal? CostoMonto { get; private set; }

    /// <summary>Moneda del costo (RN-07), o nula.</summary>
    public string? CostoMoneda { get; private set; }

    /// <summary>Fecha del costo (RN-07), o nula.</summary>
    public DateOnly? CostoFecha { get; private set; }

    /// <summary>Destino de la disposición final del equipo retirado, o nulo.</summary>
    public string? DisposicionDestino { get; private set; }

    /// <summary>Receptor de la disposición final del equipo retirado, o nulo.</summary>
    public string? DisposicionReceptor { get; private set; }

    /// <summary>Costo de la intervención como <see cref="Dinero"/>, o nulo si no se registró.</summary>
    public Dinero? Costo => CostoMoneda is { } moneda && CostoFecha is { } fecha
        ? new Dinero(CostoMonto ?? 0m, moneda, fecha)
        : null;

    /// <summary>Disposición final del equipo retirado, o nula si no aplica (reparación).</summary>
    public DisposicionFinal? Disposicion => DisposicionDestino is { } destino && DisposicionReceptor is { } receptor
        ? new DisposicionFinal(destino, receptor)
        : null;

    // Constructor de materialización (EF Core).
    private SustitucionSai()
    {
        Codigo = null!;
        HostCodigo = null!;
        DispositivoSalienteCodigo = null!;
        Proveedor = null!;
        Ejecutor = null!;
        Hallazgos = null!;
    }

    /// <summary>Registra una intervención de sustitución del SAI (los efectos ya aplicados por el servicio).</summary>
    public SustitucionSai(
        string codigo,
        string hostCodigo,
        string dispositivoSalienteCodigo,
        string? dispositivoEntranteCodigo,
        TipoIntervencionSai tipo,
        DateTimeOffset instanteOcurrido,
        DateTimeOffset instanteRegistrado,
        string proveedor,
        string ejecutor,
        string hallazgos,
        bool firmwareReiniciado,
        Dinero? costo,
        DisposicionFinal? disposicion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(hostCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoSalienteCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(proveedor);
        ArgumentException.ThrowIfNullOrWhiteSpace(ejecutor);

        Codigo = codigo;
        HostCodigo = hostCodigo;
        DispositivoSalienteCodigo = dispositivoSalienteCodigo;
        DispositivoEntranteCodigo = dispositivoEntranteCodigo;
        Tipo = tipo;
        InstanteOcurrido = instanteOcurrido;
        InstanteRegistrado = instanteRegistrado;
        Proveedor = proveedor;
        Ejecutor = ejecutor;
        Hallazgos = hallazgos ?? string.Empty;
        FirmwareReiniciado = firmwareReiniciado;

        CostoMonto = costo?.Monto;
        CostoMoneda = costo?.Moneda;
        CostoFecha = costo?.Fecha;

        DisposicionDestino = disposicion?.Destino;
        DisposicionReceptor = disposicion?.Receptor;
    }
}
