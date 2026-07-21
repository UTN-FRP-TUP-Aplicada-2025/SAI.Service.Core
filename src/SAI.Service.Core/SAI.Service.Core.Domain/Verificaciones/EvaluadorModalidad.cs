namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Deriva la <b>modalidad efectiva</b> del servicio a partir de la modalidad solicitada y el estado
/// de los cuatro supuestos (ADR-10, RN-01, RN-02). Es la materialización del bloqueo por
/// verificación: mientras los cuatro supuestos no estén verificados, la modalidad efectiva degrada a
/// <see cref="Modalidad.SoloAlerta"/> (no se apaga nada). Es una función pura.
/// </summary>
public static class EvaluadorModalidad
{
    /// <summary>Cantidad de supuestos que deben estar verificados para habilitar una acción (los cuatro).</summary>
    public const int SupuestosRequeridos = 4;

    /// <summary>
    /// Modalidad efectiva: la <paramref name="solicitada"/> solo si <b>todos</b> los supuestos están
    /// verificados; en caso contrario, <see cref="Modalidad.SoloAlerta"/> (RN-02).
    /// </summary>
    public static Modalidad Efectiva(Modalidad solicitada, IEnumerable<Verificacion> verificaciones)
    {
        ArgumentNullException.ThrowIfNull(verificaciones);
        return TodosVerificados(verificaciones) ? solicitada : Modalidad.SoloAlerta;
    }

    /// <summary>Cantidad de supuestos actualmente verificados.</summary>
    public static int Verificados(IEnumerable<Verificacion> verificaciones)
    {
        ArgumentNullException.ThrowIfNull(verificaciones);
        return verificaciones.Count(v => v.Estado == EstadoVerificacion.Verificado);
    }

    private static bool TodosVerificados(IEnumerable<Verificacion> verificaciones)
    {
        var total = 0;
        var verificados = 0;
        foreach (var verificacion in verificaciones)
        {
            total++;
            if (verificacion.Estado == EstadoVerificacion.Verificado)
            {
                verificados++;
            }
        }

        return total >= SupuestosRequeridos && verificados == total;
    }
}
