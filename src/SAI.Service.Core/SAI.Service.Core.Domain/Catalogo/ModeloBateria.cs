namespace SAI.Service.Core.Domain.Catalogo;

/// <summary>
/// Modelo de batería del <b>catálogo</b> ("qué es", ADR-07). Describe los atributos nominales de un
/// tipo de batería; las unidades concretas son <c>Bateria</c> del inventario.
/// <para>
/// Invariante <b>I-21</b> (RN-13): la vida de flotación declarada solo tiene sentido a una
/// temperatura de referencia (la vida útil de una batería depende fuertemente de la temperatura).
/// Declarar vida de flotación sin temperatura de referencia se rechaza.
/// </para>
/// </summary>
public sealed class ModeloBateria
{
    /// <summary>Código de negocio del modelo (identidad estable).</summary>
    public string Codigo { get; }

    /// <summary>Código del <see cref="Fabricante"/> del catálogo.</summary>
    public string FabricanteCodigo { get; }

    /// <summary>Nombre comercial del modelo.</summary>
    public string Nombre { get; }

    /// <summary>Tecnología (p. ej. "VRLA-AGM"), o nula si no se declaró.</summary>
    public string? Tecnologia { get; }

    /// <summary>Capacidad nominal en amperios-hora, o nula si no se conoce.</summary>
    public double? CapacidadAh { get; }

    /// <summary>Tensión nominal en voltios, o nula si no se conoce.</summary>
    public double? TensionNominalV { get; }

    /// <summary>Vida de flotación mínima en años a la temperatura de referencia, o nula.</summary>
    public double? VidaFlotacionAniosMin { get; }

    /// <summary>Vida de flotación máxima en años a la temperatura de referencia, o nula.</summary>
    public double? VidaFlotacionAniosMax { get; }

    /// <summary>Temperatura de referencia en °C para la vida de flotación, o nula.</summary>
    public double? TemperaturaReferenciaC { get; }

    /// <summary>Construye un modelo de batería de catálogo, validando el invariante I-21 (RN-13).</summary>
    /// <exception cref="ArgumentException">
    /// Si se declara vida de flotación (mín o máx) sin temperatura de referencia (I-21).
    /// </exception>
    public ModeloBateria(
        string codigo,
        string fabricanteCodigo,
        string nombre,
        string? tecnologia = null,
        double? capacidadAh = null,
        double? tensionNominalV = null,
        double? vidaFlotacionAniosMin = null,
        double? vidaFlotacionAniosMax = null,
        double? temperaturaReferenciaC = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(fabricanteCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        var declaraVidaFlotacion = vidaFlotacionAniosMin is not null || vidaFlotacionAniosMax is not null;
        if (declaraVidaFlotacion && temperaturaReferenciaC is null)
        {
            throw new ArgumentException(
                "La vida de flotación solo tiene sentido a una temperatura de referencia: no se puede "
                + "declarar sin temperatura de referencia (invariante I-21, RN-13).",
                nameof(temperaturaReferenciaC));
        }

        Codigo = codigo;
        FabricanteCodigo = fabricanteCodigo;
        Nombre = nombre;
        Tecnologia = tecnologia;
        CapacidadAh = capacidadAh;
        TensionNominalV = tensionNominalV;
        VidaFlotacionAniosMin = vidaFlotacionAniosMin;
        VidaFlotacionAniosMax = vidaFlotacionAniosMax;
        TemperaturaReferenciaC = temperaturaReferenciaC;
    }
}
