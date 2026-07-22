using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Intervenciones;

/// <summary>
/// Costos de una intervención (RN-08, BT-26): repuestos, mano de obra y total, cada uno un
/// <see cref="Dinero"/> con su moneda y su fecha (RN-07). El total debe <b>cuadrar</b>: igualar la
/// suma de repuestos y mano de obra, en la misma moneda. La validación es explícita
/// (<see cref="Cuadra"/>) para que el servicio devuelva <c>COSTOS_NO_CUADRAN</c> sin aplicar efectos.
/// </summary>
public readonly record struct Costos
{
    /// <summary>Costo de repuestos.</summary>
    public Dinero Repuestos { get; }

    /// <summary>Costo de mano de obra.</summary>
    public Dinero ManoDeObra { get; }

    /// <summary>Total declarado de la intervención.</summary>
    public Dinero Total { get; }

    /// <summary>Construye los costos (sin exigir cuadre: el cuadre se valida con <see cref="Cuadra"/>).</summary>
    public Costos(Dinero repuestos, Dinero manoDeObra, Dinero total)
    {
        Repuestos = repuestos;
        ManoDeObra = manoDeObra;
        Total = total;
    }

    /// <summary>
    /// Verdadero si el total cuadra: misma moneda en los tres importes y
    /// <c>Total == Repuestos + ManoDeObra</c> (RN-08).
    /// </summary>
    public bool Cuadra() =>
        string.Equals(Repuestos.Moneda, ManoDeObra.Moneda, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Repuestos.Moneda, Total.Moneda, StringComparison.OrdinalIgnoreCase)
        && Total.Monto == Repuestos.Monto + ManoDeObra.Monto;
}
