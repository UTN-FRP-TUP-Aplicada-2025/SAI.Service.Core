using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Variables que sensa el sondeo y su <b>procedencia canónica</b> (US-10, Matriz-Sensado §5, RN-05).
/// La procedencia se declara una sola vez por sesión de sondeo (mapa variable→origen) en lugar de
/// repetirse en cada muestra. Punto clave: <c>battery.charge</c> es <b>derivada</b> (interpolación
/// del driver), nunca medida; las tensiones y la carga de salida son medidas.
/// </summary>
public static class Variables
{
    /// <summary>Tensión de entrada (V). Medida.</summary>
    public const string TensionEntrada = "input.voltage";

    /// <summary>Tensión de salida (V). Medida.</summary>
    public const string TensionSalida = "output.voltage";

    /// <summary>Carga de salida (%). Medida.</summary>
    public const string CargaSalida = "ups.load";

    /// <summary>Carga de batería (%). Derivada por el driver, nunca medida (DM-03).</summary>
    public const string CargaBateria = "battery.charge";

    /// <summary>Estado de alimentación (ups.status), codificado 0 = en línea / 1 = en batería (DM-05). Medido.</summary>
    public const string EstadoUps = "ups.status";

    /// <summary>Tensión de batería (V, battery.voltage). Medida (DM-02); base del disparo BT-20.</summary>
    public const string TensionBateria = "battery.voltage";

    /// <summary>Código de <see cref="EstadoUps"/> para "en línea" (OL) en las lecturas.</summary>
    public const double CodigoEnLinea = 0;

    /// <summary>Código de <see cref="EstadoUps"/> para "en batería" (OB) en las lecturas.</summary>
    public const double CodigoEnBateria = 1;

    /// <summary>
    /// Procedencia canónica de cada variable sensada (US-10). Es el mapa que se guarda en la sesión
    /// de sondeo y que la API/panel expanden al servir cada valor.
    /// </summary>
    public static IReadOnlyDictionary<string, Origen> ProcedenciaCanonica { get; } =
        new Dictionary<string, Origen>(StringComparer.Ordinal)
        {
            [TensionEntrada] = Origen.Medido,
            [TensionSalida] = Origen.Medido,
            [CargaSalida] = Origen.Medido,
            [CargaBateria] = Origen.Derivado,
            [EstadoUps] = Origen.Medido,
            [TensionBateria] = Origen.Medido,
        };
}
