namespace SAI.Service.Core.Application.Abstractions;

/// <summary>
/// Puerto del adaptador de conexion con el SAI (ADR-02, ADR-22 pendiente).
/// <para>
/// Aisla el dominio del <i>como</i> se habla con el equipo. Las implementaciones
/// viven en Infrastructure: NUT (primera entrega), directo (disenado, no
/// implementado) y <b>simulado</b> (permite probar politicas sin hardware).
/// En Sprint 0 la unica implementacion es
/// <c>AdaptadorConexionSimulado</c>, un stub con valores fijos.
/// </para>
/// <para>
/// Cada operacion se valida por efecto observado (ADR-11): el resultado describe
/// lo que se observo, nunca se asume exito por ausencia de excepcion. La firma
/// definitiva se cerrara en la ADR-22.
/// </para>
/// </summary>
public interface IAdaptadorConexion
{
    /// <summary>Lee el estado actual del SAI.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    Task<EstadoSai> LeerEstadoAsync(CancellationToken ct);

    /// <summary>Prueba la conectividad con el SAI.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    Task<ResultadoConectividad> ProbarConectividadAsync(CancellationToken ct);

    /// <summary>
    /// Ordena el apagado del host con retorno (encendido automatico al restaurarse
    /// la energia), tras el <paramref name="retardo"/> indicado.
    /// </summary>
    /// <param name="retardo">Retardo antes de ejecutar el apagado.</param>
    /// <param name="ct">Token de cancelacion.</param>
    Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, CancellationToken ct);

    /// <summary>Lanza una prueba de bateria en el equipo.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct);
}
