namespace SAI.Service.Core.Domain.Inventario;

/// <summary>
/// Unidad física del <b>inventario</b> ("cuál es"), supertipo del ciclo de vida (ADR-07). Es la
/// base de <see cref="Host"/>, <see cref="Dispositivo"/> y <see cref="Bateria"/> y concentra la
/// baja lógica y la máquina de estados.
/// <para>
/// La unidad <b>nunca se borra físicamente</b> (RC-08): el retiro se expresa con estado
/// <see cref="EstadoUnidad.DadoDeBaja"/> más fecha y motivo, y sigue siendo consultable (I-5).
/// Las transiciones de estado no pueden saltar pasos (I-6) y no admite operaciones fechadas
/// después de su baja (RC-08).
/// </para>
/// </summary>
public abstract class UnidadFisica
{
    // Máquina de estados (I-6): destinos válidos desde cada estado. DadoDeBaja es terminal.
    private static readonly Dictionary<EstadoUnidad, EstadoUnidad[]> TransicionesValidas =
        new()
        {
            [EstadoUnidad.EnStock] = [EstadoUnidad.EnServicio, EstadoUnidad.DadoDeBaja],
            [EstadoUnidad.EnServicio] = [EstadoUnidad.EnReparacion, EstadoUnidad.DadoDeBaja],
            [EstadoUnidad.EnReparacion] = [EstadoUnidad.EnServicio, EstadoUnidad.DadoDeBaja],
            [EstadoUnidad.DadoDeBaja] = [],
        };

    /// <summary>Código de negocio de la unidad (identidad estable).</summary>
    public string Codigo { get; }

    /// <summary>Estado de ciclo de vida. Arranca en <see cref="EstadoUnidad.EnStock"/>.</summary>
    public EstadoUnidad Estado { get; private set; }

    /// <summary>Instante de baja, o nulo si no está dada de baja. Coherente con <see cref="Estado"/> (RC-08).</summary>
    public DateTimeOffset? FechaBaja { get; private set; }

    /// <summary>Motivo de la baja, o nulo si no está dada de baja.</summary>
    public string? MotivoBaja { get; private set; }

    /// <summary>Inicializa la unidad en <see cref="EstadoUnidad.EnStock"/>.</summary>
    protected UnidadFisica(string codigo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        Codigo = codigo;
        Estado = EstadoUnidad.EnStock;
    }

    /// <summary>Pone la unidad en servicio (desde stock o tras una reparación).</summary>
    public void PonerEnServicio() => Transicionar(EstadoUnidad.EnServicio);

    /// <summary>Envía la unidad a reparación (desde servicio).</summary>
    public void EnviarAReparacion() => Transicionar(EstadoUnidad.EnReparacion);

    /// <summary>
    /// Da de baja la unidad (retiro definitivo) con su fecha y motivo. La baja es coherente por
    /// construcción: quedar en <see cref="EstadoUnidad.DadoDeBaja"/> implica tener fecha y motivo (RC-08).
    /// </summary>
    /// <exception cref="InvalidOperationException">Si la transición al estado de baja no es válida (I-6).</exception>
    public void DarDeBaja(DateTimeOffset fecha, string motivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(motivo);

        Transicionar(EstadoUnidad.DadoDeBaja);
        FechaBaja = fecha;
        MotivoBaja = motivo;
    }

    /// <summary>
    /// Verdadero si la unidad admite una operación fechada en <paramref name="instante"/>. Una
    /// unidad dada de baja no admite operaciones posteriores a su baja (coherencia temporal, RC-08).
    /// </summary>
    public bool AdmiteOperacionEn(DateTimeOffset instante) =>
        !(Estado == EstadoUnidad.DadoDeBaja && FechaBaja is not null && instante > FechaBaja.Value);

    private void Transicionar(EstadoUnidad destino)
    {
        if (!TransicionesValidas[Estado].Contains(destino))
        {
            throw new InvalidOperationException(
                $"Transición de estado inválida de {Estado} a {destino}: el ciclo de vida no puede saltar pasos (I-6).");
        }

        Estado = destino;
    }
}
