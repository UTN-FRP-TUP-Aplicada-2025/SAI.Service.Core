namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Parámetros de presentación de las verificaciones (rediseño UX). El umbral de preaviso decide cuándo
/// una verificación vigente pasa a verse como <b>por vencer</b> (H-6/4.1): es configuración, no una
/// constante en el dominio. Global (no por prueba) mientras el panel sea de un solo equipo.
/// </summary>
public sealed class OpcionesVerificacion
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Sai:Verificacion";

    /// <summary>Días antes del vencimiento en los que una verificación se marca «por vencer».</summary>
    public int DiasPreavisoVencimiento { get; set; } = 30;
}
