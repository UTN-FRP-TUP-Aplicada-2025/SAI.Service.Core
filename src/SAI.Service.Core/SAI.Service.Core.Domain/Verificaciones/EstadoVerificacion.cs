namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Estado de una <see cref="Verificacion"/> de un supuesto (ADR-10). Arranca en
/// <see cref="NuncaVerificado"/> al sembrarse en el alta (US-05) y solo pasa a
/// <see cref="Verificado"/> por evidencia en la ventana de mantenimiento (US-16). Un supuesto
/// <see cref="Refutado"/> bloquea de forma permanente; uno <see cref="Vencido"/> vuelve a exigir
/// verificación.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un estado válido.</para>
/// </summary>
public enum EstadoVerificacion
{
    /// <summary>Nunca verificado (estado inicial sembrado en el alta). Persistido como "sinVerificar".</summary>
    NuncaVerificado = 1,

    /// <summary>Verificado por evidencia observada.</summary>
    Verificado = 2,

    /// <summary>La verificación venció y debe renovarse.</summary>
    Vencido = 3,

    /// <summary>Refutado por evidencia en contra: bloqueo permanente.</summary>
    Refutado = 4,
}
