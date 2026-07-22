namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Calidad de una <see cref="Muestra"/> del sondeo (US-08, CU-04). Distingue una lectura completa
/// de una parcial (faltaron variables) y de una perdida (el equipo no respondió), para que los
/// cálculos posteriores toleren los huecos por variable, no solo por muestra (CL-12).
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es una calidad válida.</para>
/// </summary>
public enum CalidadMuestra
{
    /// <summary>Llegaron todas las variables esperadas.</summary>
    Completa = 1,

    /// <summary>El equipo respondió pero faltó al menos una variable (las ausentes quedan sin dato).</summary>
    Parcial = 2,

    /// <summary>El equipo no respondió (timeout / no alcanzable): la muestra no tiene valores.</summary>
    Perdida = 3,
}
