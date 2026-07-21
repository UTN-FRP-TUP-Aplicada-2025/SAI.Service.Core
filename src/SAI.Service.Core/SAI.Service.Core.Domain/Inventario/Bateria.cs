namespace SAI.Service.Core.Domain.Inventario;

/// <summary>
/// Batería del inventario (ADR-07). Es una unidad de un <c>ModeloBateria</c> del catálogo; se
/// monta en un dispositivo durante una vigencia (<c>MontajeBateria</c>).
/// <para>
/// La edad se cuenta desde la <see cref="FechaFabricacion"/>, que puede ser anterior a la
/// <see cref="FechaCompra"/> (una batería puede llevar tiempo en depósito antes de comprarse).
/// </para>
/// </summary>
public sealed class Bateria : UnidadFisica
{
    /// <summary>Código del modelo de batería (catálogo) del que esta unidad es un ejemplar.</summary>
    public string ModeloBateriaCodigo { get; }

    /// <summary>Fecha de fabricación (base para la edad), o nula si no se conoce.</summary>
    public DateOnly? FechaFabricacion { get; }

    /// <summary>Fecha de compra, o nula si no se conoce.</summary>
    public DateOnly? FechaCompra { get; }

    /// <summary>Construye una batería de inventario ligada a su modelo de catálogo.</summary>
    public Bateria(
        string codigo,
        string modeloBateriaCodigo,
        DateOnly? fechaFabricacion = null,
        DateOnly? fechaCompra = null)
        : base(codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modeloBateriaCodigo);
        ModeloBateriaCodigo = modeloBateriaCodigo;
        FechaFabricacion = fechaFabricacion;
        FechaCompra = fechaCompra;
    }
}
