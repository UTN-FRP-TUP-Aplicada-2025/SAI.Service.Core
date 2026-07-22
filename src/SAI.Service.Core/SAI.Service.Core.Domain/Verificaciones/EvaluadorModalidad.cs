namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Deriva la <b>modalidad efectiva</b> del servicio a partir de la modalidad solicitada y el estado
/// de los cuatro supuestos (ADR-10, RN-01, RN-02). Es el bloqueo por verificación: mientras los
/// cuatro no estén verificados <b>y vigentes</b>, la modalidad efectiva degrada a
/// <see cref="Modalidad.SoloAlerta"/> (no se apaga nada). Una verificación vencida deja de contar (US-17).
/// </summary>
public static class EvaluadorModalidad
{
    /// <summary>Cantidad de supuestos que deben estar verificados para habilitar una acción (los cuatro).</summary>
    public const int SupuestosRequeridos = 4;

    /// <summary>Modalidad efectiva evaluada en el instante actual.</summary>
    public static Modalidad Efectiva(Modalidad solicitada, IEnumerable<Verificacion> verificaciones) =>
        Efectiva(solicitada, verificaciones, DateTimeOffset.UtcNow);

    /// <summary>
    /// Modalidad efectiva: la <paramref name="solicitada"/> solo si <b>todos</b> los supuestos cuentan
    /// como verificados y vigentes en <paramref name="ahora"/>; en caso contrario, <see cref="Modalidad.SoloAlerta"/>.
    /// </summary>
    public static Modalidad Efectiva(Modalidad solicitada, IEnumerable<Verificacion> verificaciones, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(verificaciones);
        return TodosVerificados(verificaciones, ahora) ? solicitada : Modalidad.SoloAlerta;
    }

    /// <summary>Cantidad de supuestos verificados y vigentes en el instante actual.</summary>
    public static int Verificados(IEnumerable<Verificacion> verificaciones) =>
        Verificados(verificaciones, DateTimeOffset.UtcNow);

    /// <summary>Cantidad de supuestos que cuentan como verificados y vigentes en <paramref name="ahora"/>.</summary>
    public static int Verificados(IEnumerable<Verificacion> verificaciones, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(verificaciones);
        return verificaciones.Count(v => v.CuentaComoVerificada(ahora));
    }

    private static bool TodosVerificados(IEnumerable<Verificacion> verificaciones, DateTimeOffset ahora)
    {
        var total = 0;
        var verificados = 0;
        foreach (var verificacion in verificaciones)
        {
            total++;
            if (verificacion.CuentaComoVerificada(ahora))
            {
                verificados++;
            }
        }

        return total >= SupuestosRequeridos && verificados == total;
    }
}
