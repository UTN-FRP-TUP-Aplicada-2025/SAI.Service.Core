using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Fuente de datos del sondeo (de dónde provienen las lecturas: la herramienta de acceso). Es
/// historia append-only (ADR-04): se registra, no se reescribe. Su <see cref="ConfianzaBase"/>
/// modula la confianza de las conclusiones que se apoyan en sus lecturas.
/// </summary>
public sealed class FuenteDatos : IEntidadHistoria
{
    /// <summary>Código de negocio de la fuente (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Descripción legible, o nula.</summary>
    public string? Descripcion { get; private set; }

    /// <summary>Confianza base de la fuente.</summary>
    public ConfianzaFuente ConfianzaBase { get; private set; }

    // Constructor de materialización (EF Core).
    private FuenteDatos() => Codigo = null!;

    /// <summary>Registra una fuente de datos.</summary>
    public FuenteDatos(string codigo, ConfianzaFuente confianzaBase, string? descripcion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        Codigo = codigo;
        ConfianzaBase = confianzaBase;
        Descripcion = descripcion;
    }
}
