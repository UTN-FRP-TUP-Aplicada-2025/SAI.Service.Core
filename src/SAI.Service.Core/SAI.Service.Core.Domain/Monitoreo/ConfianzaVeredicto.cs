namespace SAI.Service.Core.Domain.Monitoreo;

/// <summary>
/// Confianza de un <see cref="VeredictoSalud"/> (ADR-13, US-13). Arranca <see cref="Baja"/> y solo
/// sube cuando se acumulan pruebas comparables a carga igualada (con menos de cuatro comparables la
/// confianza es baja). Refleja que, sin sensor de temperatura, la oscilación estacional puede
/// rivalizar con la señal de degradación (R-09).
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es una confianza válida.</para>
/// </summary>
public enum ConfianzaVeredicto
{
    /// <summary>Confianza baja (pocas pruebas comparables).</summary>
    Baja = 1,

    /// <summary>Confianza media.</summary>
    Media = 2,

    /// <summary>Confianza alta.</summary>
    Alta = 3,
}
