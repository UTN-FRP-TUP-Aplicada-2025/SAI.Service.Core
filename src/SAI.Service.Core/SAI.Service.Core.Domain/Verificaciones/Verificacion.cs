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
}
