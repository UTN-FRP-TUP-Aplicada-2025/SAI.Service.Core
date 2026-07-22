using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Regla versionada de derivación de eventos (BT-19, RC-09). La <b>versión es parte de la
/// identidad</b>: cambiar un umbral crea una fila nueva con la versión siguiente (append-only,
/// ADR-04); las anteriores quedan consultables. Cada <see cref="Evento"/> graba el par
/// (código, versión) exacto con el que se derivó, para no mezclar eventos de umbrales distintos.
/// </summary>
public sealed class ReglaDerivacion : IEntidadHistoria
{
    /// <summary>Código de negocio de la regla.</summary>
    public string Codigo { get; private set; }

    /// <summary>Versión de la regla (parte de la identidad).</summary>
    public int Version { get; private set; }

    /// <summary>Descripción legible, o nula.</summary>
    public string? Descripcion { get; private set; }

    /// <summary>Parámetros/umbrales de la regla (p. ej. min/max de tensión, segundos sostenidos).</summary>
    public IReadOnlyDictionary<string, double> Parametros { get; private set; }

    /// <summary>Instante desde el que la regla está vigente.</summary>
    public DateTimeOffset VigenteDesde { get; private set; }

    // Constructor de materialización (EF Core).
    private ReglaDerivacion()
    {
        Codigo = null!;
        Parametros = new Dictionary<string, double>();
    }

    /// <summary>Crea una versión de una regla de derivación.</summary>
    public ReglaDerivacion(
        string codigo,
        int version,
        DateTimeOffset vigenteDesde,
        IReadOnlyDictionary<string, double> parametros,
        string? descripcion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentNullException.ThrowIfNull(parametros);
        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "La versión de la regla debe ser positiva.");
        }

        Codigo = codigo;
        Version = version;
        VigenteDesde = vigenteDesde;
        Parametros = parametros;
        Descripcion = descripcion;
    }

    /// <summary>Valor de un parámetro, o <c>null</c> si no está definido.</summary>
    public double? Parametro(string clave) => Parametros.TryGetValue(clave, out var valor) ? valor : null;
}
