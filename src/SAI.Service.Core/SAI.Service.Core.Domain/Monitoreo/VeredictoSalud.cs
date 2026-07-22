namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Veredicto de salud de una batería (BT-21, ADR-13). Es una <b>tendencia relativa</b> de la caída
/// de tensión frente a la línea base a carga igualada: nunca una magnitud cuantitativa (SoH %, Ah,
/// autonomía), prohibidas por ADR-13. Siempre viaja con su confianza y su reserva de temperatura.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un veredicto válido.</para>
/// </summary>
public enum VeredictoSalud
{
    /// <summary>Sin degradación detectable frente a la línea base.</summary>
    SinDegradacionDetectable = 1,

    /// <summary>Se comporta peor que la línea base (degradación).</summary>
    Degradada = 2,

    /// <summary>Se comporta claramente peor que la línea base (degradación marcada).</summary>
    DegradacionSevera = 3,
}
