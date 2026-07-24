using MudBlazor;
using SAI.Service.Core.Application.Abstractions;

namespace SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;

/// <summary>Cómo se muestra el estado en vivo del SAI: título, detalle, color e ícono.</summary>
public sealed record VistaEnVivo(string Titulo, string? Detalle, Color Tono, string Icono, bool EnBateria);

/// <summary>
/// Traduce la lectura del SAI (<see cref="EstadoSai"/>) a la retroalimentación en vivo del panel. Es
/// pura y testeable: hace visible lo que el sistema está viendo del equipo, para que confirmar la señal
/// deje de ser a ciegas. <b>No</b> verifica nada —la verificación sigue siendo un acto deliberado del
/// operador—; solo informa. El color codifica estado (ámbar = en batería), coherente con 5.1.
/// </summary>
public static class DescriptorEnVivo
{
    /// <summary>Describe el estado para mostrar; <c>null</c> = aún sin lectura.</summary>
    public static VistaEnVivo Describir(EstadoSai? estado)
    {
        if (estado is null)
        {
            return new VistaEnVivo("Leyendo el estado del equipo…", null, Color.Default, Icons.Material.Filled.Sync, false);
        }

        if (!estado.Alcanzable)
        {
            return new VistaEnVivo(
                "Sin lectura del equipo",
                "No se pudo leer el SAI en la última consulta. Revisá la conexión.",
                Color.Error, Icons.Material.Filled.CloudOff, false);
        }

        if (estado.EstadoUps == EstadoUps.EnBateria)
        {
            return new VistaEnVivo(
                "El equipo está en batería ahora",
                Detalle(estado),
                Color.Warning, Icons.Material.Filled.BatteryAlert, EnBateria: true);
        }

        return new VistaEnVivo(
            "El equipo está en línea",
            Detalle(estado),
            Color.Success, Icons.Material.Filled.Power, false);
    }

    // Detalle secundario con lo que haya: tensión de entrada y carga de batería.
    private static string? Detalle(EstadoSai estado)
    {
        var partes = new List<string>();
        if (estado.TensionEntradaVoltios is { } entrada)
        {
            partes.Add($"entrada {entrada:0.#} V");
        }

        if (estado.CargaBateriaPorcentaje is { } bateria)
        {
            partes.Add($"batería {bateria:0}%");
        }

        return partes.Count > 0 ? string.Join(" · ", partes) : null;
    }
}
