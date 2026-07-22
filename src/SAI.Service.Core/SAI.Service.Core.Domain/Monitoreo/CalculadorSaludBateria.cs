using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Resultado de evaluar la salud de una prueba de batería.
/// </summary>
/// <param name="Caida">Caída de tensión (reposo → mínima), derivada; valor nulo si no calculable.</param>
/// <param name="CargaPorcentaje">Carga concurrente durante la prueba, si se conoce.</param>
/// <param name="Comparable">Verdadero si la prueba es comparable a carga igualada (RN-06, I-16).</param>
/// <param name="Veredicto">Veredicto de tendencia, o nulo si no comparable / no calculable.</param>
/// <param name="Confianza">Confianza del veredicto (arranca baja).</param>
/// <param name="Reserva">Reserva textual que acompaña todo veredicto (temperatura, R-09).</param>
/// <param name="MotivoNoCalculable">Motivo si la caída no se pudo calcular; nulo si se calculó.</param>
public sealed record ResultadoSalud(
    Valor<double?> Caida,
    int? CargaPorcentaje,
    bool Comparable,
    VeredictoSalud? Veredicto,
    ConfianzaVeredicto Confianza,
    string Reserva,
    string? MotivoNoCalculable);

/// <summary>
/// Deriva el veredicto de salud de una batería a partir de la serie densa de la prueba (BT-21,
/// ADR-13). Es una función pura: solo usa <c>battery.voltage</c> medido (nunca <c>battery.charge</c>
/// derivado, RN-06), ignora las muestras perdidas (no interpola, CL-13), y produce una tendencia
/// relativa contra la línea base a carga igualada —nunca una magnitud cuantitativa—. La confianza
/// arranca baja y solo sube con cuatro o más pruebas comparables; el veredicto siempre lleva la
/// reserva por falta de sensor de temperatura (R-09).
/// </summary>
public static class CalculadorSaludBateria
{
    /// <summary>Reserva textual fija que acompaña todo veredicto (R-09).</summary>
    public const string ReservaTemperatura = "sin corrección por temperatura (sin sensor)";

    /// <summary>Pruebas comparables necesarias para elevar la confianza por encima de baja.</summary>
    public const int ComparablesParaSubirConfianza = 4;

    /// <summary>Mínimo de muestras válidas para poder calcular la caída.</summary>
    public const int MuestrasMinimas = 3;

    /// <summary>Empeoramiento relativo de la caída que se considera degradación (20 %).</summary>
    public const double UmbralDegradacion = 1.2;

    /// <summary>
    /// Evalúa la salud de la serie. <paramref name="caidaLineaBase"/> y <paramref name="cargaLineaBase"/>
    /// describen la prueba de referencia (nulos si esta prueba es la línea base);
    /// <paramref name="comparablesPrevias"/> es el conteo de pruebas comparables ya acumuladas.
    /// </summary>
    public static ResultadoSalud Evaluar(
        IReadOnlyList<Muestra> serie,
        int? cargaConcurrente,
        double? caidaLineaBase,
        int? cargaLineaBase,
        int toleranciaCargaPct,
        int comparablesPrevias)
    {
        ArgumentNullException.ThrowIfNull(serie);

        // Solo muestras válidas con tensión de batería medida (RN-06): las perdidas/parciales se saltan.
        var voltajes = serie
            .Where(m => m.Calidad != CalidadMuestra.Perdida)
            .Select(m => m.Valor(Variables.TensionBateria))
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (voltajes.Count < MuestrasMinimas)
        {
            return new ResultadoSalud(
                new Valor<double?>(null, Origen.NoCalculable),
                cargaConcurrente, Comparable: false, Veredicto: null,
                ConfianzaVeredicto.Baja, ReservaTemperatura,
                MotivoNoCalculable: "serie insuficiente (muestras válidas por debajo del mínimo)");
        }

        var reposo = voltajes[0];
        var minima = voltajes.Min();
        var caida = minima - reposo; // negativa: cae respecto del reposo

        // Comparabilidad a carga igualada. Si no hay línea base, esta prueba la establece (comparable).
        var comparable = cargaLineaBase is null
            || (cargaConcurrente is { } carga && Math.Abs(carga - cargaLineaBase.Value) <= toleranciaCargaPct);

        VeredictoSalud? veredicto = null;
        if (comparable)
        {
            veredicto = caidaLineaBase is { } baseCaida && Math.Abs(caida) > Math.Abs(baseCaida) * UmbralDegradacion
                ? VeredictoSalud.Degradada
                : VeredictoSalud.SinDegradacionDetectable;
        }

        var comparablesTotal = comparablesPrevias + (comparable ? 1 : 0);
        var confianza = comparable && comparablesTotal >= ComparablesParaSubirConfianza
            ? ConfianzaVeredicto.Media
            : ConfianzaVeredicto.Baja;

        return new ResultadoSalud(
            new Valor<double?>(caida, Origen.Derivado),
            cargaConcurrente, comparable, veredicto, confianza, ReservaTemperatura, MotivoNoCalculable: null);
    }
}
