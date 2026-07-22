using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Domain.Monitoreo;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Mapea el <see cref="EstadoSai"/> leído por el adaptador a las lecturas por variable de una
/// <see cref="Muestra"/> (US-08). El estado de alimentación se codifica numéricamente (0 = en línea,
/// 1 = en batería) para viajar en el diccionario de lecturas junto al resto.
/// </summary>
internal static class MapeoLecturas
{
    public static Dictionary<string, double?> DesdeEstado(EstadoSai estado) => new(StringComparer.Ordinal)
    {
        [Variables.TensionEntrada] = estado.TensionEntradaVoltios,
        [Variables.TensionSalida] = estado.TensionSalidaVoltios,
        [Variables.CargaSalida] = estado.CargaSalidaPorcentaje,
        [Variables.CargaBateria] = estado.CargaBateriaPorcentaje,
        [Variables.EstadoUps] = estado.EstadoUps switch
        {
            EstadoUps.EnBateria => Variables.CodigoEnBateria,
            EstadoUps.EnLinea => Variables.CodigoEnLinea,
            _ => null,
        },
        [Variables.TensionBateria] = estado.TensionBateriaVoltios,
    };
}
