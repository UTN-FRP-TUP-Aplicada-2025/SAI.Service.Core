namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>
/// Verificación del estado de un <see cref="Supuesto"/> de seguridad operativa (ADR-10). Es una
/// entidad de estado actual (no append-only): su estado se renueva por evidencia a lo largo del
/// tiempo (US-16, US-17). Se siembra en el alta (US-05) en <see cref="EstadoVerificacion.NuncaVerificado"/>.
/// </summary>
public sealed class Verificacion
{
    /// <summary>Código de negocio de la verificación (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Supuesto que esta verificación cubre.</summary>
    public Supuesto Supuesto { get; private set; }

    /// <summary>Estado actual de la verificación.</summary>
    public EstadoVerificacion Estado { get; private set; }

    /// <summary>Método con el que se verificó, o nulo si aún no se verificó.</summary>
    public string? Metodo { get; private set; }

    /// <summary>Evidencia observada que respalda el estado, o nula.</summary>
    public string? Evidencia { get; private set; }

    /// <summary>Instante hasta el que la verificación es válida, o nulo (no vence / aún sin verificar).</summary>
    public DateTimeOffset? VigenciaHasta { get; private set; }

    /// <summary>Instante de la última actualización del estado.</summary>
    public DateTimeOffset ActualizadoEn { get; private set; }

    /// <summary>
    /// Instante en que se disparó una prueba física que exige un reinicio del host antes de repetirse
    /// (Etapa 4·E), o nulo si no hay ninguna en curso. Mientras no sea nulo, la acción queda bloqueada en
    /// el panel (<see cref="EsperandoReinicio"/>): es el freno para que no se re-dispare el apagado "a lo
    /// loco". Es ortogonal al <see cref="Estado"/>: una verificación puede quedar verificada y aun así
    /// esperar el reinicio para admitir una nueva prueba.
    /// </summary>
    public DateTimeOffset? PruebaEnCursoDesde { get; private set; }

    /// <summary>Verdadero si hay una prueba disparada esperando el reinicio del host (Etapa 4·E).</summary>
    public bool EsperandoReinicio => PruebaEnCursoDesde is not null;

    // Constructor privado para EF (materialización) y las fábricas.
    private Verificacion(string codigo, Supuesto supuesto, EstadoVerificacion estado, DateTimeOffset actualizadoEn)
    {
        Codigo = codigo;
        Supuesto = supuesto;
        Estado = estado;
        ActualizadoEn = actualizadoEn;
    }

    /// <summary>
    /// Siembra una verificación en <see cref="EstadoVerificacion.NuncaVerificado"/> (US-05): es el
    /// estado inicial de los cuatro supuestos al completarse el alta.
    /// </summary>
    public static Verificacion Sembrar(string codigo, Supuesto supuesto, DateTimeOffset ahora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return new Verificacion(codigo, supuesto, EstadoVerificacion.NuncaVerificado, ahora);
    }

    /// <summary>
    /// Marca el supuesto como verificado por evidencia observada (US-16, US-17). Un supuesto
    /// <see cref="EstadoVerificacion.Refutado"/> es un bloqueo permanente y no puede reverificarse.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la verificación está refutada.</exception>
    public void Verificar(string metodo, string? evidencia, DateTimeOffset? vigenciaHasta, DateTimeOffset ahora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metodo);
        if (Estado == EstadoVerificacion.Refutado)
        {
            throw new InvalidOperationException("Un supuesto refutado es un bloqueo permanente: no puede reverificarse.");
        }

        Estado = EstadoVerificacion.Verificado;
        Metodo = metodo;
        Evidencia = evidencia;
        VigenciaHasta = vigenciaHasta;
        ActualizadoEn = ahora;
    }

    /// <summary>
    /// Refuta el supuesto por evidencia en contra (US-16 FA-1: el host no arranca solo). Es un
    /// <b>bloqueo permanente</b>: un supuesto refutado no vuelve a verificarse desde la ventana.
    /// </summary>
    public void Refutar(string metodo, string? evidencia, DateTimeOffset ahora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(metodo);
        Estado = EstadoVerificacion.Refutado;
        Metodo = metodo;
        Evidencia = evidencia;
        VigenciaHasta = null;
        PruebaEnCursoDesde = null; // refutar es un bloqueo permanente: no queda ninguna prueba pendiente.
        ActualizadoEn = ahora;
    }

    /// <summary>
    /// Marca que se disparó una prueba física que exige un reinicio del host antes de repetirse (Etapa
    /// 4·E): la acción queda bloqueada (<see cref="EsperandoReinicio"/>) hasta que el servicio vuelva a
    /// arrancar tras el reinicio. Un supuesto <see cref="EstadoVerificacion.Refutado"/> es un bloqueo
    /// permanente y no admite disparar una prueba.
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la verificación está refutada.</exception>
    public void IniciarPrueba(DateTimeOffset ahora)
    {
        if (Estado == EstadoVerificacion.Refutado)
        {
            throw new InvalidOperationException("Un supuesto refutado es un bloqueo permanente: no admite disparar una prueba.");
        }

        PruebaEnCursoDesde = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>
    /// Rearma la verificación tras detectarse el reinicio del host (Etapa 4·E): limpia el marcador de
    /// prueba en curso, de modo que la acción vuelve a habilitarse en el panel. Es idempotente: si no
    /// había ninguna prueba en curso, no hace nada.
    /// </summary>
    public void RearmarPorReinicio(DateTimeOffset ahora)
    {
        if (PruebaEnCursoDesde is null)
        {
            return;
        }

        PruebaEnCursoDesde = null;
        ActualizadoEn = ahora;
    }

    /// <summary>
    /// Verdadero si la verificación cuenta como vigente en <paramref name="ahora"/>: está verificada y
    /// su vigencia no venció (o no caduca). Es lo que habilita el desbloqueo (RN-02).
    /// </summary>
    public bool CuentaComoVerificada(DateTimeOffset ahora) =>
        Estado == EstadoVerificacion.Verificado && (VigenciaHasta is not { } hasta || hasta >= ahora);

    /// <summary>
    /// Estado efectivo en <paramref name="ahora"/>: una verificación verificada cuya vigencia ya venció
    /// se ve como <see cref="EstadoVerificacion.Vencido"/> (vencimiento perezoso, US-17).
    /// </summary>
    public EstadoVerificacion EstadoEfectivo(DateTimeOffset ahora) =>
        Estado == EstadoVerificacion.Verificado && VigenciaHasta is { } hasta && hasta < ahora
            ? EstadoVerificacion.Vencido
            : Estado;
}
