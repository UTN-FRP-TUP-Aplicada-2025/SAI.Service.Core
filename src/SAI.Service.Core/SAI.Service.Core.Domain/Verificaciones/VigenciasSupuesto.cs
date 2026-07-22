namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Vigencia de la verificación de cada supuesto (US-16, US-17, ADR-14). El presupuesto de apagado
/// caduca a los 180 días (la carga del host cambia); la señal en batería y el reencendido por placa,
/// a los 365; el corte con retorno <b>no caduca</b> (una vez probado que el firmware soporta el
/// retorno, no se degrada por tiempo — R-01).
/// </summary>
public static class VigenciasSupuesto
{
    /// <summary>Días de vigencia por supuesto; <c>null</c> = sin caducidad.</summary>
    public static IReadOnlyDictionary<Supuesto, int?> DiasVigencia { get; } =
        new Dictionary<Supuesto, int?>
        {
            [Supuesto.PresupuestoDeApagado] = 180,
            [Supuesto.SenalEnBateria] = 365,
            [Supuesto.ReencendidoPorPlaca] = 365,
            [Supuesto.CorteConRetorno] = null,
        };

    /// <summary>Instante hasta el que valdría una verificación del supuesto hecha en <paramref name="ahora"/>.</summary>
    public static DateTimeOffset? VigenciaHasta(Supuesto supuesto, DateTimeOffset ahora) =>
        DiasVigencia.TryGetValue(supuesto, out var dias) && dias is { } d ? ahora.AddDays(d) : null;
}
