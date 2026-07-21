namespace SAI.Service.Core.Web;

/// <summary>
/// Sello de version que muestra la barra superior del panel (Identidad-De-Version).
/// Se enlaza desde la seccion <c>Sello</c> de <c>appsettings.json</c>. En el pipeline
/// real la version la calcula MinVer (§17.P.7); aca es una ranura de configuracion.
/// </summary>
public sealed class SelloVersion
{
    /// <summary>Version legible (SemVer), por ejemplo <c>0.1.0-alpha.1</c>.</summary>
    public string VersionLegible { get; init; } = "0.0.0";

    /// <summary>Modelo UX-UI de la iteracion.</summary>
    public string ModeloUx { get; init; } = string.Empty;

    /// <summary>Fecha de la iteracion de UX.</summary>
    public string FechaIteracion { get; init; } = string.Empty;

    /// <summary>Indica si es un artefacto de preestreno (sufijo <c>-alpha.N</c>).</summary>
    public bool EsPreliminar { get; init; }
}
