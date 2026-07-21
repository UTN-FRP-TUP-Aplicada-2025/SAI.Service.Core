namespace SAI.Service.Core.Domain.Inventario;

/// <summary>
/// Estado de ciclo de vida de una <see cref="UnidadFisica"/> (RC-08). Las transiciones entre
/// estados no pueden saltar pasos (I-6); la máquina de estados vive en <see cref="UnidadFisica"/>.
/// <para>
/// Los valores arrancan en 1: el <c>default</c> (0) no es un estado válido, de modo que una
/// unidad nunca queda con un estado sin asignar.
/// </para>
/// </summary>
public enum EstadoUnidad
{
    /// <summary>En stock, aún no puesta en servicio.</summary>
    EnStock = 1,

    /// <summary>En servicio (operativa).</summary>
    EnServicio = 2,

    /// <summary>Fuera de servicio temporalmente, en reparación.</summary>
    EnReparacion = 3,

    /// <summary>Dada de baja (retiro definitivo). Estado terminal; nunca se borra físicamente.</summary>
    DadoDeBaja = 4,
}
