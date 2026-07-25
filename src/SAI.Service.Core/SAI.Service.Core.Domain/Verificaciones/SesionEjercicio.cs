namespace SAI.Service.Core.Domain.Verificaciones;

/// <summary>Estado de una <see cref="SesionEjercicio"/>.</summary>
public enum EstadoSesionEjercicio
{
    /// <summary>El ejercicio guiado está en curso.</summary>
    EnCurso = 1,

    /// <summary>Los cuatro supuestos quedaron vigentes: el ejercicio se completó.</summary>
    Completada = 2,

    /// <summary>El operador salió del ejercicio sin completarlo.</summary>
    Abandonada = 3,
}

/// <summary>
/// Sesión de <b>ejercicio guiado</b> (P-7): acompaña al operador por los cuatro supuestos como un solo
/// ejercicio físico encadenado, en vez de cuatro verificaciones sueltas.
/// <para>
/// Deliberadamente <b>no guarda el progreso</b>: la única verdad de qué está verificado siguen siendo las
/// <see cref="Verificacion"/> (con su vigencia y su confirmación por efecto observado, ADR-11). Esta
/// entidad solo aporta <i>intención</i> ("se está haciendo el ejercicio completo") y <i>momento</i>; el
/// paso actual se deriva con <see cref="SecuenciaFisica.PrimeroPendiente"/>. Así no puede desincronizarse
/// con las verificaciones.
/// </para>
/// <para>
/// Se persiste (no vive en el circuito Blazor) para que el ejercicio se pueda retomar tras el reinicio del
/// host —que es parte normal del ejercicio— y consultar desde cualquier navegador.
/// </para>
/// </summary>
public sealed class SesionEjercicio
{
    /// <summary>Código de negocio de la sesión (identidad estable).</summary>
    public string Codigo { get; private set; }

    /// <summary>Instante en que se inició el ejercicio.</summary>
    public DateTimeOffset IniciadaEn { get; private set; }

    /// <summary>Instante en que se completó o abandonó, o nulo si sigue en curso.</summary>
    public DateTimeOffset? FinalizadaEn { get; private set; }

    /// <summary>Estado actual de la sesión.</summary>
    public EstadoSesionEjercicio Estado { get; private set; }

    // Constructor privado para EF (materialización) y la fábrica.
    private SesionEjercicio(string codigo, DateTimeOffset iniciadaEn, EstadoSesionEjercicio estado)
    {
        Codigo = codigo;
        IniciadaEn = iniciadaEn;
        Estado = estado;
    }

    /// <summary>Verdadero si la sesión sigue abierta.</summary>
    public bool EnCurso => Estado == EstadoSesionEjercicio.EnCurso;

    /// <summary>Inicia un ejercicio guiado.</summary>
    public static SesionEjercicio Iniciar(string codigo, DateTimeOffset ahora)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return new SesionEjercicio(codigo, ahora, EstadoSesionEjercicio.EnCurso);
    }

    /// <summary>Cierra la sesión como completada (los cuatro supuestos quedaron vigentes).</summary>
    /// <exception cref="InvalidOperationException">Si la sesión ya estaba cerrada.</exception>
    public void Completar(DateTimeOffset ahora) => Cerrar(EstadoSesionEjercicio.Completada, ahora);

    /// <summary>Cierra la sesión porque el operador salió del ejercicio.</summary>
    /// <exception cref="InvalidOperationException">Si la sesión ya estaba cerrada.</exception>
    public void Abandonar(DateTimeOffset ahora) => Cerrar(EstadoSesionEjercicio.Abandonada, ahora);

    private void Cerrar(EstadoSesionEjercicio estado, DateTimeOffset ahora)
    {
        if (!EnCurso)
        {
            throw new InvalidOperationException("La sesión de ejercicio ya está cerrada.");
        }

        Estado = estado;
        FinalizadaEn = ahora;
    }
}
