using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Catalogo;

/// <summary>
/// Modelo de dispositivo (SAI/UPS) del <b>catálogo</b> ("qué es", ADR-07). Describe los atributos
/// nominales de un tipo de equipo; las unidades concretas son <c>Dispositivo</c> del inventario.
/// </summary>
public sealed class ModeloDispositivo
{
    /// <summary>Código de negocio del modelo (identidad estable).</summary>
    public string Codigo { get; }

    /// <summary>Código del <see cref="Fabricante"/> del catálogo.</summary>
    public string FabricanteCodigo { get; }

    /// <summary>Nombre comercial del modelo.</summary>
    public string Nombre { get; }

    /// <summary>Línea/topología (p. ej. "line-interactive"), o nula si no se declaró.</summary>
    public string? LineaTopologia { get; }

    /// <summary>Tensión nominal en voltios, o nula si el equipo no la expone.</summary>
    public double? TensionNominalV { get; }

    /// <summary>
    /// Potencia nominal en VA con su procedencia (I-7): es un dato que suele venir declarado por la
    /// ficha del fabricante, no medido. Nulo si no se conoce.
    /// </summary>
    public Valor<double>? PotenciaVaNominal { get; }

    /// <summary>Construye un modelo de dispositivo de catálogo.</summary>
    public ModeloDispositivo(
        string codigo,
        string fabricanteCodigo,
        string nombre,
        string? lineaTopologia = null,
        double? tensionNominalV = null,
        Valor<double>? potenciaVaNominal = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(fabricanteCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        Codigo = codigo;
        FabricanteCodigo = fabricanteCodigo;
        Nombre = nombre;
        LineaTopologia = lineaTopologia;
        TensionNominalV = tensionNominalV;
        PotenciaVaNominal = potenciaVaNominal;
    }
}
