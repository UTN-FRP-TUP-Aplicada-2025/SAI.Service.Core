namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Los cuatro supuestos de seguridad operativa que deben verificarse por evidencia antes de
/// habilitar cualquier modalidad distinta de solo aviso (ADR-10, US-05, US-16). Mientras alguno no
/// esté verificado, el servicio degrada a <see cref="Modalidad.SoloAlerta"/> (RN-01, RN-02).
/// </summary>
public enum Supuesto
{
    /// <summary>El tiempo reservado alcanza para apagar el host antes de agotar la batería.</summary>
    PresupuestoDeApagado = 1,

    /// <summary>El equipo reporta de forma observable que pasó a batería (estado en batería).</summary>
    SenalEnBateria = 2,

    /// <summary>La placa del host se vuelve a encender sola al restaurarse la energía.</summary>
    ReencendidoPorPlaca = 3,

    /// <summary>El apagado ordenado con retorno reenciende el host tras volver la energía.</summary>
    CorteConRetorno = 4,
}
