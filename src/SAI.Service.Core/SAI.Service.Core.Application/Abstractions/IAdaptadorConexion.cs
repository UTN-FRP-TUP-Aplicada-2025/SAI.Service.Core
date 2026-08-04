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
    /// la energia). El SAI corta su salida tras el <paramref name="retardo"/> —la ventana que tiene el host
    /// para bajar limpio— y la restaura tras el <paramref name="retardoRetorno"/> una vez que vuelve la red,
    /// para forzar la transicion ausencia→presencia que necesita el autoencendido de la BIOS.
    /// </summary>
    /// <param name="retardo">Retardo antes de cortar la salida (ventana de apagado del host).</param>
    /// <param name="retardoRetorno">Retardo del SAI, tras el retorno de la red, antes de restaurar la salida.</param>
    /// <param name="ct">Token de cancelacion.</param>
    Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, TimeSpan retardoRetorno, CancellationToken ct);

    /// <summary>Lanza una prueba de bateria en el equipo.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct);
}
