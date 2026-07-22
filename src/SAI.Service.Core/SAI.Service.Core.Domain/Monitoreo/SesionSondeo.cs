using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Valores;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Sesión de sondeo de un dispositivo (CU-04): el contexto de una serie de <see cref="Muestra"/>
/// —driver, dialecto, intervalo y el <b>mapa variable→origen</b> (US-10)—. Es historia append-only
/// (ADR-04). La procedencia de cada variable se guarda una sola vez acá, no en cada muestra.
/// </summary>
public sealed class SesionSondeo : IEntidadHistoria
{
    /// <summary>Código de negocio de la sesión (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI) sondeado.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Código de la fuente de datos.</summary>
    public string FuenteDatosCodigo { get; private set; }

    /// <summary>Driver de la herramienta de acceso (p. ej. "nutdrv_qx").</summary>
    public string Driver { get; private set; }

    /// <summary>Versión del driver, o nula.</summary>
    public string? DriverVersion { get; private set; }

    /// <summary>Dialecto/subdriver, o nulo.</summary>
    public string? Dialecto { get; private set; }

    /// <summary>Intervalo de sondeo en segundos.</summary>
    public int IntervaloSeg { get; private set; }

    /// <summary>Vigencia de la sesión (intervalo semiabierto; abierta = activa).</summary>
    public Vigencia Vigencia { get; private set; }

    /// <summary>Procedencia por variable (US-10): variable → <see cref="Origen"/>.</summary>
    public IReadOnlyDictionary<string, Origen> MapaVariableOrigen { get; private set; }

    // Constructor de materialización (EF Core).
    private SesionSondeo()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        FuenteDatosCodigo = null!;
        Driver = null!;
        MapaVariableOrigen = new Dictionary<string, Origen>();
    }

    /// <summary>Abre una sesión de sondeo con su mapa de procedencia.</summary>
    public SesionSondeo(
        string codigo,
        string dispositivoCodigo,
        string fuenteDatosCodigo,
        string driver,
        int intervaloSeg,
        Vigencia vigencia,
        IReadOnlyDictionary<string, Origen> mapaVariableOrigen,
        string? driverVersion = null,
        string? dialecto = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(fuenteDatosCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(driver);
        ArgumentNullException.ThrowIfNull(mapaVariableOrigen);
        if (intervaloSeg <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(intervaloSeg), "El intervalo de sondeo debe ser positivo.");
        }

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        FuenteDatosCodigo = fuenteDatosCodigo;
        Driver = driver;
        IntervaloSeg = intervaloSeg;
        Vigencia = vigencia;
        MapaVariableOrigen = mapaVariableOrigen;
        DriverVersion = driverVersion;
        Dialecto = dialecto;
    }

    /// <summary>Origen declarado de una variable en esta sesión, o <c>null</c> si no está en el mapa.</summary>
    public Origen? OrigenDe(string variable) =>
        MapaVariableOrigen.TryGetValue(variable, out var origen) ? origen : null;
}
