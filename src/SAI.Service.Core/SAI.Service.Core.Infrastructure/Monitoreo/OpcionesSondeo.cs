namespace SAI.Service.Core.Infrastructure.Monitoreo;

/// <summary>
/// Configuración del planificador de sondeo (BT-17). Se enlaza desde la sección <c>Sai:Sondeo</c>.
/// El intervalo por defecto es 5 s (N-07); las pruebas deshabilitan el poller para ser deterministas.
/// </summary>
public sealed class OpcionesSondeo
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Sai:Sondeo";

    /// <summary>Intervalo entre rondas de sondeo, en segundos (5 por defecto).</summary>
    public int IntervaloSeg { get; set; } = 5;

    /// <summary>Si el planificador corre. Se puede apagar por configuración (p. ej. en pruebas).</summary>
    public bool Habilitado { get; set; } = true;
}
