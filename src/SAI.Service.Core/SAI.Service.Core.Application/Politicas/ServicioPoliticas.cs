using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Politicas;

/// <summary>Propuesta de configuración de política (lo que el administrador arma antes de confirmar).</summary>
public sealed record PropuestaPolitica(Modalidad ModalidadSolicitada, int UmbralDisparoSegundos, int TiempoReservadoApagadoSeg, int TiempoRetornoSeg);

/// <summary>Código de resultado de crear una versión de política.</summary>
public enum CodigoPolitica
{
    /// <summary>La versión se creó y quedó vigente.</summary>
    Creada = 1,

    /// <summary>El tiempo reservado supera el techo duro de 540 s (RN-04, I-10).</summary>
    TiempoApagadoExcedeTecho = 2,

    /// <summary>Un parámetro es inválido (obligatorio vacío o fuera de rango).</summary>
    ParametroInvalido = 3,
}

/// <summary>Resultado de crear una versión; con la versión creada cuando corresponde.</summary>
public sealed record ResultadoPolitica(CodigoPolitica Codigo, string Mensaje, VersionPolitica? Version = null);

/// <summary>
/// Previsualización de una propuesta (CU-03, sin aplicar nada): la modalidad efectiva que regiría dado el
/// bloqueo por verificación (RN-02), si quedaría degradada a solo aviso, y la explicación "en palabras".
/// </summary>
public sealed record Previsualizacion(
    Modalidad ModalidadSolicitada,
    Modalidad ModalidadEfectiva,
    bool Degradada,
    int Verificados,
    int Requeridos,
    string EnPalabras);

/// <summary>
/// Configura la política de apagado versionada (CU-03, US-06): "la UI propone, el humano confirma, el
/// sistema valida". Crear una versión valida el techo duro (≤540, RN-04) y los parámetros <b>antes</b> de
/// persistir (postcondición de fallo: no se crea versión), incrementa el número y deja la nueva vigente
/// sin tocar las anteriores (append-only, ADR-04). La previsualización deriva la modalidad efectiva con el
/// bloqueo por verificación (RN-02) sin ejecutar nada.
/// </summary>
public sealed class ServicioPoliticas(IRepositorioPoliticas repositorio)
{
    /// <summary>Versión vigente, o <c>null</c> si aún no hay ninguna.</summary>
    public Task<VersionPolitica?> VigenteAsync(CancellationToken ct) => repositorio.VigenteAsync(ct);

    /// <summary>Historial de versiones, más reciente primero.</summary>
    public Task<IReadOnlyList<VersionPolitica>> HistorialAsync(CancellationToken ct) => repositorio.HistorialAsync(ct);

    /// <summary>Crea una versión nueva a partir de una propuesta (valida antes de persistir).</summary>
    public async Task<ResultadoPolitica> CrearVersionAsync(PropuestaPolitica propuesta, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(propuesta);

        if (propuesta.UmbralDisparoSegundos <= 0)
        {
            return new ResultadoPolitica(CodigoPolitica.ParametroInvalido,
                "PARAMETRO_INVALIDO: el umbral de disparo debe ser positivo.");
        }

        if (propuesta.TiempoRetornoSeg <= 0)
        {
            return new ResultadoPolitica(CodigoPolitica.ParametroInvalido,
                "PARAMETRO_INVALIDO: el tiempo de retorno del SAI debe ser positivo.");
        }

        if (propuesta.TiempoReservadoApagadoSeg < 0 || propuesta.TiempoReservadoApagadoSeg > Accion.TechoDuroApagadoSeg)
        {
            return new ResultadoPolitica(CodigoPolitica.TiempoApagadoExcedeTecho,
                $"TIEMPO_APAGADO_EXCEDE_TECHO: el tiempo reservado no puede superar los {Accion.TechoDuroApagadoSeg} s (RN-04).");
        }

        var ahora = DateTimeOffset.UtcNow;
        var vigente = await repositorio.VigenteAsync(ct);
        var nueva = vigente is null
            ? VersionPolitica.Inicial(propuesta.ModalidadSolicitada, propuesta.UmbralDisparoSegundos, propuesta.TiempoReservadoApagadoSeg, propuesta.TiempoRetornoSeg, ahora)
            : vigente.Siguiente(propuesta.ModalidadSolicitada, propuesta.UmbralDisparoSegundos, propuesta.TiempoReservadoApagadoSeg, propuesta.TiempoRetornoSeg, ahora);

        await repositorio.AgregarVersionAsync(nueva, ct);
        return new ResultadoPolitica(CodigoPolitica.Creada, $"Versión {nueva.Numero} creada y vigente.", nueva);
    }

    /// <summary>Previsualiza la propuesta sin crearla: modalidad efectiva, degradación y "en palabras".</summary>
    public async Task<Previsualizacion> PrevisualizarAsync(PropuestaPolitica propuesta, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(propuesta);

        var ahora = DateTimeOffset.UtcNow;
        var verificaciones = await repositorio.ListarVerificacionesAsync(ct);
        var efectiva = EvaluadorModalidad.Efectiva(propuesta.ModalidadSolicitada, verificaciones, ahora);
        var verificados = EvaluadorModalidad.Verificados(verificaciones, ahora);
        var degradada = propuesta.ModalidadSolicitada != Modalidad.SoloAlerta && efectiva == Modalidad.SoloAlerta;

        return new Previsualizacion(
            propuesta.ModalidadSolicitada, efectiva, degradada, verificados,
            EvaluadorModalidad.SupuestosRequeridos, ExplicacionPolitica.Redactar(propuesta));
    }
}
