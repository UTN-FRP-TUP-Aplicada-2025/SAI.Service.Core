using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Domain.Politicas;

/// <summary>
/// Versión inmutable de la política de apagado (CU-03, US-06). La política se configura creando una
/// <b>versión nueva</b> en vez de editar la vigente: cada versión fija la modalidad solicitada, el umbral
/// de disparo y el tiempo reservado para el apagado. Es historia append-only (ADR-04): las versiones no se
/// editan ni se borran, solo se agregan; la <b>vigente</b> es la de mayor <see cref="Numero"/>, y toda
/// acción se ejecuta bajo la vigente (RN-11).
/// <para>
/// Defiende por construcción el techo duro de <see cref="Accion.TechoDuroApagadoSeg"/> segundos (RN-04,
/// I-10): no hay forma de construir una versión con un tiempo reservado que lo supere.
/// </para>
/// </summary>
public sealed class VersionPolitica : IEntidadHistoria
{
    /// <summary>Código de negocio de la versión (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Número de versión, incremental (la mayor es la vigente).</summary>
    public int Numero { get; private set; }

    /// <summary>Modalidad de apagado solicitada por esta versión.</summary>
    public Modalidad ModalidadSolicitada { get; private set; }

    /// <summary>Umbral de disparo del apagado, en segundos en batería.</summary>
    public int UmbralDisparoSegundos { get; private set; }

    /// <summary>Tiempo reservado para el apagado del host, en segundos (≤ techo duro, RN-04).</summary>
    public int TiempoReservadoApagadoSeg { get; private set; }

    /// <summary>Instante desde el que esta versión rige.</summary>
    public DateTimeOffset VigenteDesde { get; private set; }

    // Constructor de materialización (EF Core).
    private VersionPolitica()
    {
        Codigo = null!;
    }

    private VersionPolitica(string codigo, int numero, Modalidad modalidad, int umbralDisparoSegundos, int tiempoReservadoApagadoSeg, DateTimeOffset vigenteDesde)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        if (numero < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(numero), "El número de versión arranca en 1.");
        }

        if (umbralDisparoSegundos <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(umbralDisparoSegundos), "El umbral de disparo debe ser positivo.");
        }

        if (tiempoReservadoApagadoSeg is < 0 or > Accion.TechoDuroApagadoSeg)
        {
            throw new ArgumentOutOfRangeException(nameof(tiempoReservadoApagadoSeg),
                $"El tiempo reservado no puede superar el techo duro de {Accion.TechoDuroApagadoSeg} s (RN-04, I-10).");
        }

        Codigo = codigo;
        Numero = numero;
        ModalidadSolicitada = modalidad;
        UmbralDisparoSegundos = umbralDisparoSegundos;
        TiempoReservadoApagadoSeg = tiempoReservadoApagadoSeg;
        VigenteDesde = vigenteDesde;
    }

    /// <summary>Crea la versión inicial (número 1) de la política, sembrada en la puesta en marcha.</summary>
    public static VersionPolitica Inicial(Modalidad modalidad, int umbralDisparoSegundos, int tiempoReservadoApagadoSeg, DateTimeOffset ahora) =>
        new($"pol-v1-{Guid.NewGuid():N}", 1, modalidad, umbralDisparoSegundos, tiempoReservadoApagadoSeg, ahora);

    /// <summary>
    /// Crea la versión siguiente a esta, con el número incrementado. La actual no se modifica: la nueva
    /// queda vigente y esta pasa a ser historia consultable (append-only).
    /// </summary>
    public VersionPolitica Siguiente(Modalidad modalidad, int umbralDisparoSegundos, int tiempoReservadoApagadoSeg, DateTimeOffset ahora) =>
        new($"pol-v{Numero + 1}-{Guid.NewGuid():N}", Numero + 1, modalidad, umbralDisparoSegundos, tiempoReservadoApagadoSeg, ahora);
}
