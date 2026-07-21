namespace SAI.Service.Core.Domain.Catalogo;

/// <summary>
/// Fabricante del <b>catálogo</b> ("qué es", ADR-07). El catálogo describe tipos, no ejemplares:
/// no es una <c>UnidadFisica</c>.
/// </summary>
public sealed class Fabricante
{
    /// <summary>Código de negocio del fabricante (identidad estable).</summary>
    public string Codigo { get; }

    /// <summary>Nombre del fabricante.</summary>
    public string Nombre { get; }

    /// <summary>
    /// Falso para el fabricante genérico "sin identificar", que agrupa equipos cuyo fabricante real
    /// no se conoce. Verdadero por defecto.
    /// </summary>
    public bool Identificado { get; }

    /// <summary>Construye un fabricante de catálogo.</summary>
    public Fabricante(string codigo, string nombre, bool identificado = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Codigo = codigo;
        Nombre = nombre;
        Identificado = identificado;
    }
}
