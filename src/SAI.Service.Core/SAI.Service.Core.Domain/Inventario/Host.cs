namespace SAI.Service.Core.Domain.Inventario;

/// <summary>
/// Host protegido: la máquina cuya continuidad el servicio resguarda (ADR-07, inventario). Su
/// cobertura por un SAI se modela como vínculo temporal (<c>CoberturaHost</c>).
/// </summary>
public sealed class Host : UnidadFisica
{
    /// <summary>Criticidad declarada del host (p. ej. "alta"), o nula si no se declaró.</summary>
    public string? Criticidad { get; }

    /// <summary>Instante desde el que el host está en servicio, o nulo si aún no.</summary>
    public DateTimeOffset? EnServicioDesde { get; }

    /// <summary>Construye un host de inventario.</summary>
    public Host(string codigo, string? criticidad = null, DateTimeOffset? enServicioDesde = null)
        : base(codigo)
    {
        Criticidad = criticidad;
        EnServicioDesde = enServicioDesde;
    }
}
