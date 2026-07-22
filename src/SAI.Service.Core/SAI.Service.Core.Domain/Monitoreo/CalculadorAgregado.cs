namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Calcula el <see cref="Agregado"/> de una variable en una ventana a partir de los valores de las
/// muestras de esa ventana (BT-18). Conserva mínimo y máximo además del promedio, y computa la
/// cobertura (fracción de muestras con dato) con su advertencia (I-20). Es una función pura: tolera
/// nulos por variable (las muestras perdidas o parciales aportan un valor nulo que no cuenta).
/// </summary>
public static class CalculadorAgregado
{
    /// <summary>Ventana horaria estándar (ISO-8601).</summary>
    public const string VentanaHoraria = "PT1H";

    /// <summary>Umbral de cobertura por debajo del cual el agregado lleva advertencia.</summary>
    public const double CoberturaMinimaSinAdvertencia = 0.95;

    /// <summary>
    /// Calcula el agregado de <paramref name="valores"/> (uno por muestra de la ventana; nulo cuando
    /// esa muestra no tenía la variable). Devuelve <c>null</c> si no había muestras en la ventana.
    /// </summary>
    public static Agregado? Calcular(
        string codigo,
        string dispositivoCodigo,
        string variable,
        DateTimeOffset ventanaInicio,
        IReadOnlyCollection<double?> valores,
        string ventanaDuracion = VentanaHoraria)
    {
        ArgumentNullException.ThrowIfNull(valores);
        if (valores.Count == 0)
        {
            return null;
        }

        var conValor = valores.Where(v => v.HasValue).Select(v => v!.Value).ToList();
        var cobertura = (double)conValor.Count / valores.Count;

        double? promedio = conValor.Count > 0 ? conValor.Average() : null;
        double? minimo = conValor.Count > 0 ? conValor.Min() : null;
        double? maximo = conValor.Count > 0 ? conValor.Max() : null;

        var advertencia = cobertura < CoberturaMinimaSinAdvertencia
            ? $"cobertura baja: {conValor.Count} de {valores.Count} muestras con dato ({cobertura:P0})"
            : null;

        return new Agregado(
            codigo, dispositivoCodigo, variable, ventanaInicio, ventanaDuracion,
            nMuestras: conValor.Count,
            cobertura: cobertura,
            promedio: promedio,
            minimo: minimo,
            maximo: maximo,
            advertencia: advertencia);
    }
}
