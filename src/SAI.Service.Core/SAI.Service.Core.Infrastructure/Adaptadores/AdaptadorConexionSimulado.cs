using SAI.Service.Core.Application.Abstractions;

namespace SAI.Service.Core.Infrastructure.Adaptadores;

/// <summary>
/// Implementacion simulada del puerto <see cref="IAdaptadorConexion"/> (ADR-02).
/// <para>
/// Stub de Sprint 0: devuelve valores fijos y razonables para que el DI compile y
/// el panel muestre datos sin hardware ni NUT. No habla con ningun equipo real.
/// El adaptador NUT real (que corre en el contenedor recibiendo el USB por ruta
/// fisica, ADR-01/ADR-03/ADR-19) llega en etapas posteriores.
/// </para>
/// </summary>
public sealed class AdaptadorConexionSimulado : IAdaptadorConexion
{
    private static DateTimeOffset Ahora => DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public Task<EstadoSai> LeerEstadoAsync(CancellationToken ct) =>
        Task.FromResult(new EstadoSai(
            Alcanzable: true,
            TensionEntradaVoltios: 220.0,
            TensionSalidaVoltios: 220.0,
            CargaSalidaPorcentaje: 35.0,
            CargaBateriaPorcentaje: 100.0,
            MarcaTiempoUtc: Ahora));

    /// <inheritdoc />
    public Task<ResultadoConectividad> ProbarConectividadAsync(CancellationToken ct) =>
        Task.FromResult(new ResultadoConectividad(
            Conectado: true,
            LatenciaMilisegundos: 5.0,
            Detalle: "Adaptador simulado: conectividad fija (stub de Sprint 0)."));

    /// <inheritdoc />
    public Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, CancellationToken ct) =>
        Task.FromResult(new ResultadoAccion(
            Aceptada: true,
            Motivo: $"Adaptador simulado: apagado con retorno aceptado (retardo {retardo}). Sin efecto real.",
            MarcaTiempoUtc: Ahora));

    /// <inheritdoc />
    public Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct) =>
        Task.FromResult(new ResultadoAccion(
            Aceptada: true,
            Motivo: "Adaptador simulado: test de bateria aceptado. Sin efecto real.",
            MarcaTiempoUtc: Ahora));
}
