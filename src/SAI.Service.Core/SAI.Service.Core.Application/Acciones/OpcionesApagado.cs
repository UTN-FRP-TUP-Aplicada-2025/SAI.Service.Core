using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Acciones;

/// <summary>
/// Parámetros de la política de apagado (CU-05, US-14). La modalidad solicitada es la que se pide;
/// la efectiva la deriva el bloqueo por verificación (RN-02). El tiempo reservado nunca supera el
/// techo duro de 540 s (RN-04, I-10). Arranca en solo aviso (estado base seguro, ADR-10/RN-01)
/// salvo que se configure otra cosa en <c>Sai:Apagado</c>.
/// </summary>
public sealed class OpcionesApagado
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Sai:Apagado";

    /// <summary>Modalidad solicitada por la política vigente (antes del bloqueo por verificación).</summary>
    public Modalidad ModalidadSolicitada { get; set; } = Modalidad.SoloAlerta;

    /// <summary>Tiempo reservado para el apagado del host, en segundos (≤ 540, RN-04).</summary>
    public int TiempoReservadoSeg { get; set; } = 120;
}
