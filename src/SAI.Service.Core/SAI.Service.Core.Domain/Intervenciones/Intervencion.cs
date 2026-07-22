using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Registro de una intervención de recambio de batería (CU-08, US-18). Es historia append-only
/// (ADR-04): un solo acto cierra la vigencia vieja, abre la nueva y deja trazado el costo, los
/// hallazgos, el proveedor, el ejecutor y la disposición final de la batería retirada. Distingue el
/// <see cref="InstanteOcurrido"/> (cuándo pasó) del <see cref="InstanteRegistrado"/> (cuándo se
/// cargó), para la carga diferida (CU-08 FA-1). Los costos cuadran por construcción (RN-08): el
/// servicio valida <see cref="Costos.Cuadra"/> antes de registrar.
/// </summary>
public sealed class Intervencion : IEntidadHistoria
{
    /// <summary>Código de negocio de la intervención.</summary>
    public string Codigo { get; private set; }

    /// <summary>Dispositivo (SAI) intervenido.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Posición física del montaje afectado.</summary>
    public string Posicion { get; private set; }

    /// <summary>Batería retirada (dada de baja).</summary>
    public string BateriaSalienteCodigo { get; private set; }

    /// <summary>Batería montada (puesta en servicio).</summary>
    public string BateriaEntranteCodigo { get; private set; }

    /// <summary>Instante en que ocurrió el recambio (cierra/abre las vigencias).</summary>
    public DateTimeOffset InstanteOcurrido { get; private set; }

    /// <summary>Instante en que se registró en el sistema (carga diferida, FA-1).</summary>
    public DateTimeOffset InstanteRegistrado { get; private set; }

    /// <summary>Proveedor que realizó la intervención.</summary>
    public string Proveedor { get; private set; }

    /// <summary>Persona que ejecutó la intervención.</summary>
    public string Ejecutor { get; private set; }

    /// <summary>Costo de repuestos.</summary>
    public Dinero Repuestos { get; private set; }

    /// <summary>Costo de mano de obra.</summary>
    public Dinero ManoDeObra { get; private set; }

    /// <summary>Total de la intervención (cuadra con repuestos + mano de obra, RN-08).</summary>
    public Dinero Total { get; private set; }

    /// <summary>Costos de la intervención como value object (compuesto de los tres importes). No se persiste.</summary>
    public Costos Costos => new(Repuestos, ManoDeObra, Total);

    /// <summary>Hallazgos y mediciones registrados.</summary>
    public string Hallazgos { get; private set; }

    /// <summary>Disposición final de la batería retirada.</summary>
    public DisposicionFinal Disposicion { get; private set; }

    // Constructor de materialización (EF Core).
    private Intervencion()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        Posicion = null!;
        BateriaSalienteCodigo = null!;
        BateriaEntranteCodigo = null!;
        Proveedor = null!;
        Ejecutor = null!;
        Hallazgos = null!;
    }

    /// <summary>Registra una intervención de recambio (los costos ya validados por el servicio).</summary>
    public Intervencion(
        string codigo,
        string dispositivoCodigo,
        string posicion,
        string bateriaSalienteCodigo,
        string bateriaEntranteCodigo,
        DateTimeOffset instanteOcurrido,
        DateTimeOffset instanteRegistrado,
        string proveedor,
        string ejecutor,
        Costos costos,
        string hallazgos,
        DisposicionFinal disposicion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(posicion);
        ArgumentException.ThrowIfNullOrWhiteSpace(bateriaSalienteCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(bateriaEntranteCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(proveedor);
        ArgumentException.ThrowIfNullOrWhiteSpace(ejecutor);

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        Posicion = posicion;
        BateriaSalienteCodigo = bateriaSalienteCodigo;
        BateriaEntranteCodigo = bateriaEntranteCodigo;
        InstanteOcurrido = instanteOcurrido;
        InstanteRegistrado = instanteRegistrado;
        Proveedor = proveedor;
        Ejecutor = ejecutor;
        Repuestos = costos.Repuestos;
        ManoDeObra = costos.ManoDeObra;
        Total = costos.Total;
        Hallazgos = hallazgos ?? string.Empty;
        Disposicion = disposicion;
    }
}
