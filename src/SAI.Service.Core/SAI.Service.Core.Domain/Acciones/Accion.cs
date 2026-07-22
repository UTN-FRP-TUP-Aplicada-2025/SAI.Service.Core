using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Domain.Acciones;

/// <summary>
/// Registro de una acción de apagado ante un disparo (CU-05, US-14, US-15). Es historia append-only
/// (ADR-04): cada disparo deja una acción que guarda la <see cref="ModalidadSolicitada"/>, la
/// <see cref="ModalidadEfectiva"/> tras el bloqueo por verificación (RN-02) y el
/// <see cref="Estado"/> por efecto observado (ADR-11, RN-03). El tiempo reservado para el apagado
/// nunca supera el techo duro de 540 s (RN-04, I-10).
/// </summary>
public sealed class Accion : IEntidadHistoria
{
    /// <summary>Techo duro del tiempo reservado para el apagado, en segundos (RN-04, I-10).</summary>
    public const int TechoDuroApagadoSeg = 540;

    /// <summary>Código de negocio de la acción.</summary>
    public string Codigo { get; private set; }

    /// <summary>Código del dispositivo (SAI) sobre el que se decidió la acción.</summary>
    public string DispositivoCodigo { get; private set; }

    /// <summary>Instante en que se decidió/ejecutó la acción, en UTC.</summary>
    public DateTimeOffset Instante { get; private set; }

    /// <summary>Modalidad solicitada por la política vigente (antes del bloqueo por verificación).</summary>
    public Modalidad ModalidadSolicitada { get; private set; }

    /// <summary>Modalidad efectiva tras el bloqueo por verificación (RN-02): la que realmente se aplica.</summary>
    public Modalidad ModalidadEfectiva { get; private set; }

    /// <summary>Resultado de la acción por efecto observado (ADR-11).</summary>
    public EstadoAccion Estado { get; private set; }

    /// <summary>Tiempo reservado para el apagado del host, en segundos (≤ 540, RN-04). 0 si no se ejecutó.</summary>
    public int TiempoReservadoSeg { get; private set; }

    /// <summary>Evidencia o motivo del resultado (efecto observado, motivo del bloqueo, etc.).</summary>
    public string Detalle { get; private set; }

    /// <summary>Código del evento de disparo que originó la acción (si lo hubo).</summary>
    public string? EventoDisparoCodigo { get; private set; }

    // Constructor de materialización (EF Core).
    private Accion()
    {
        Codigo = null!;
        DispositivoCodigo = null!;
        Detalle = null!;
    }

    private Accion(
        string codigo,
        string dispositivoCodigo,
        DateTimeOffset instante,
        Modalidad modalidadSolicitada,
        Modalidad modalidadEfectiva,
        EstadoAccion estado,
        int tiempoReservadoSeg,
        string detalle,
        string? eventoDisparoCodigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(dispositivoCodigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(detalle);
        if (tiempoReservadoSeg is < 0 or > TechoDuroApagadoSeg)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tiempoReservadoSeg),
                tiempoReservadoSeg,
                $"El tiempo reservado para el apagado no puede superar el techo duro de {TechoDuroApagadoSeg} s (RN-04, I-10).");
        }

        Codigo = codigo;
        DispositivoCodigo = dispositivoCodigo;
        Instante = instante;
        ModalidadSolicitada = modalidadSolicitada;
        ModalidadEfectiva = modalidadEfectiva;
        Estado = estado;
        TiempoReservadoSeg = tiempoReservadoSeg;
        Detalle = detalle;
        EventoDisparoCodigo = eventoDisparoCodigo;
    }

    /// <summary>
    /// La acción queda bloqueada por verificación: la modalidad solicitada era de acción pero algún
    /// supuesto no está verificado y vigente; la efectiva degrada a solo aviso (RN-02, ADR-10).
    /// </summary>
    public static Accion Bloqueada(
        string codigo, string dispositivoCodigo, Modalidad solicitada, DateTimeOffset instante, string motivo, string? eventoDisparoCodigo = null) =>
        new(codigo, dispositivoCodigo, instante, solicitada, Modalidad.SoloAlerta, EstadoAccion.BloqueadaPorVerificacion, 0, motivo, eventoDisparoCodigo);

    /// <summary>La modalidad solicitada era solo aviso: se registra el aviso, sin apagar (RN-01).</summary>
    public static Accion SoloAviso(
        string codigo, string dispositivoCodigo, DateTimeOffset instante, string motivo, string? eventoDisparoCodigo = null) =>
        new(codigo, dispositivoCodigo, instante, Modalidad.SoloAlerta, Modalidad.SoloAlerta, EstadoAccion.SoloAviso, 0, motivo, eventoDisparoCodigo);

    /// <summary>El apagado se ordenó y el equipo lo admitió (efecto observado): acción ejecutada.</summary>
    public static Accion Ejecutada(
        string codigo, string dispositivoCodigo, Modalidad solicitada, Modalidad efectiva, int tiempoReservadoSeg, DateTimeOffset instante, string evidencia, string? eventoDisparoCodigo = null) =>
        new(codigo, dispositivoCodigo, instante, solicitada, efectiva, EstadoAccion.Ejecutada, tiempoReservadoSeg, evidencia, eventoDisparoCodigo);

    /// <summary>Se ordenó el apagado pero el efecto no se confirmó (el equipo no lo admitió, RN-03).</summary>
    public static Accion EfectoNoConfirmado(
        string codigo, string dispositivoCodigo, Modalidad solicitada, Modalidad efectiva, int tiempoReservadoSeg, DateTimeOffset instante, string motivo, string? eventoDisparoCodigo = null) =>
        new(codigo, dispositivoCodigo, instante, solicitada, efectiva, EstadoAccion.EfectoNoConfirmado, tiempoReservadoSeg, motivo, eventoDisparoCodigo);
}
