namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Orden en que los cuatro supuestos ocurren <b>físicamente</b> durante el ejercicio de mantenimiento:
/// cortás la red → el SAI avisa que pasó a batería → el host se apaga → el SAI corta y repone su salida
/// → el host arranca solo. Es conocimiento del dominio (no de la vista) porque define tanto el orden en
/// que se presentan las pruebas como el paso en el que va un ejercicio guiado.
/// <para>No coincide con el orden de declaración de <see cref="Supuesto"/>, que es histórico.</para>
/// </summary>
public static class SecuenciaFisica
{
    private static readonly Supuesto[] OrdenFisico =
    [
        Supuesto.SenalEnBateria,
        Supuesto.PresupuestoDeApagado,
        Supuesto.CorteConRetorno,
        Supuesto.ReencendidoPorPlaca,
    ];

    /// <summary>Los cuatro supuestos en orden físico.</summary>
    public static IReadOnlyList<Supuesto> Orden => OrdenFisico;

    /// <summary>Número de paso (1–4) del supuesto en la secuencia física.</summary>
    public static int Numero(Supuesto supuesto) => Array.IndexOf(OrdenFisico, supuesto) + 1;

    /// <summary>
    /// Primer supuesto de la secuencia que todavía <b>no cuenta como verificado</b> en
    /// <paramref name="ahora"/>, o <c>null</c> si los cuatro están vigentes. Es la posición derivada de
    /// un ejercicio guiado: no se persiste, se calcula a partir de las verificaciones (única verdad).
    /// </summary>
    public static Supuesto? PrimeroPendiente(IReadOnlyList<Verificacion> verificaciones, DateTimeOffset ahora)
    {
        foreach (var supuesto in Orden)
        {
            var verificacion = verificaciones.FirstOrDefault(v => v.Supuesto == supuesto);
            if (verificacion is null || !verificacion.CuentaComoVerificada(ahora))
            {
                return supuesto;
            }
        }

        return null;
    }
}
