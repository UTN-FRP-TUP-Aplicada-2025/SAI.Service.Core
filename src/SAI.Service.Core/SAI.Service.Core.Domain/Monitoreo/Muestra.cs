using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Muestra puntual del estado del SAI tomada por el sondeo (US-08, CU-04). Es historia append-only
/// (ADR-04). Guarda solo los valores por variable; la procedencia se resuelve por el mapa de la
/// <see cref="SesionSondeo"/> (US-10), y la batería asociada por <c>ResolutorTemporal</c> en el
/// instante (RC-07), no por FK directa.
/// </summary>
public sealed class Muestra : IEntidadHistoria
{
    /// <summary>Código de negocio de la muestra (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI) muestreado.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Código de la sesión de sondeo a la que pertenece.</summary>
    public string SesionSondeoCodigo { get; private set; }

    /// <summary>Instante de la muestra.</summary>
    public DateTimeOffset Instante { get; private set; }

    /// <summary>Calidad de la muestra (completa/parcial/perdida).</summary>
    public CalidadMuestra Calidad { get; private set; }

    /// <summary>
    /// Lecturas por variable. Puede tener valores nulos por variable (calidad parcial) y queda vacía
    /// cuando la muestra es <see cref="CalidadMuestra.Perdida"/>.
    /// </summary>
    public IReadOnlyDictionary<string, double?> Lecturas { get; private set; }

    // Constructor de materialización (EF Core).
    private Muestra()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        SesionSondeoCodigo = null!;
        Lecturas = new Dictionary<string, double?>();
    }

    private Muestra(
        string codigo,
        string dispositivoCodigo,
        string sesionSondeoCodigo,
        DateTimeOffset instante,
        CalidadMuestra calidad,
        IReadOnlyDictionary<string, double?> lecturas)
    {
        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        SesionSondeoCodigo = sesionSondeoCodigo;
        Instante = instante;
        Calidad = calidad;
        Lecturas = lecturas;
    }

    /// <summary>
    /// Registra una muestra a partir de una lectura, determinando su calidad (US-08):
    /// <list type="bullet">
    ///   <item><b>Perdida</b> si el equipo no fue alcanzable (sin valores).</item>
    ///   <item><b>Completa</b> si llegaron todas las variables esperadas con valor.</item>
    ///   <item><b>Parcial</b> si respondió pero falta al menos una variable (las ausentes quedan sin dato).</item>
    /// </list>
    /// </summary>
    public static Muestra Registrar(
        string codigo,
        string dispositivoCodigo,
        string sesionSondeoCodigo,
        DateTimeOffset instante,
        bool alcanzable,
        IReadOnlyDictionary<string, double?> lecturas,
        IReadOnlyCollection<string> variablesEsperadas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(sesionSondeoCodigo);
        ArgumentNullException.ThrowIfNull(lecturas);
        ArgumentNullException.ThrowIfNull(variablesEsperadas);

        if (!alcanzable)
        {
            return new Muestra(codigo, dispositivoCodigo, sesionSondeoCodigo, instante,
                CalidadMuestra.Perdida, new Dictionary<string, double?>());
        }

        var completas = variablesEsperadas.All(v => lecturas.TryGetValue(v, out var valor) && valor is not null);
        var calidad = completas ? CalidadMuestra.Completa : CalidadMuestra.Parcial;

        return new Muestra(codigo, dispositivoCodigo, sesionSondeoCodigo, instante, calidad,
            new Dictionary<string, double?>(lecturas, StringComparer.Ordinal));
    }

    /// <summary>Valor de una variable, o <c>null</c> si no llegó o la muestra es perdida.</summary>
    public double? Valor(string variable) => Lecturas.TryGetValue(variable, out var valor) ? valor : null;
}
