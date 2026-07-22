namespace SAI.Service.Core.Domain.Acciones;

/// <summary>
/// Resultado de una acción de apagado ante un disparo (CU-05, US-14, US-15). Cada acción se decide
/// y se registra por efecto observado (ADR-11, RN-03): nunca se da por ejecutada por ausencia de
/// error.
/// <para>Los valores arrancan en 1: el <c>default</c> (0) no es un estado válido.</para>
/// </summary>
public enum EstadoAccion
{
    /// <summary>
    /// La modalidad solicitada era de acción pero algún supuesto no está verificado y vigente: la
    /// acción queda bloqueada y la modalidad efectiva degrada a solo aviso (ADR-10, RN-02, I-11).
    /// </summary>
    BloqueadaPorVerificacion = 1,

    /// <summary>La modalidad solicitada era solo aviso: se alerta, no se apaga nada (RN-01).</summary>
    SoloAviso = 2,

    /// <summary>El apagado se ordenó y el equipo lo admitió (efecto observado: la orden fue aceptada).</summary>
    Ejecutada = 3,

    /// <summary>Se ordenó el apagado pero el efecto no se confirmó (el equipo no lo admitió, RN-03).</summary>
    EfectoNoConfirmado = 4,
}
