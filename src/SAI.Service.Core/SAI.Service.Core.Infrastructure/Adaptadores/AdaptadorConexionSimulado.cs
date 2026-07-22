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
public sealed class AdaptadorConexionSimulado : IAdaptadorConexion, IDescubridorSai
{
    private static DateTimeOffset Ahora => DateTimeOffset.UtcNow;

    // Descarga simulada de la prueba de batería: al lanzarla, la tensión de batería cae y se recupera
    // durante una ventana, para que la serie densa muestre una caída real (US-12/US-13, demo).
    private static readonly TimeSpan DuracionDescarga = TimeSpan.FromSeconds(12);
    private const double TensionReposoBateria = 13.2;
    private const double CaidaMaximaDescarga = 0.7;
    private DateTimeOffset? _descargaInicio;

    /// <inheritdoc />
    public Task<IReadOnlyList<DispositivoDescubierto>> DescubrirAsync(CancellationToken ct)
    {
        IReadOnlyList<DispositivoDescubierto> candidatos =
        [
            new DispositivoDescubierto(
                NombreNut: "sai",
                Descriptor: "0000:0000 · SAI simulado (sin hardware) · serie: vacío",
                VendorId: "0000",
                ProductId: "0000",
                Driver: "simulado",
                NumeroSerie: null),
        ];
        return Task.FromResult(candidatos);
    }

    /// <inheritdoc />
    public Task<EstadoSai> LeerEstadoAsync(CancellationToken ct)
    {
        var ahora = Ahora;
        var tensionBateria = TensionReposoBateria;
        if (_descargaInicio is { } inicio && ahora - inicio <= DuracionDescarga)
        {
            // Caída con forma de valle (mínimo a mitad de la ventana) y recuperación.
            var progreso = (ahora - inicio).TotalSeconds / DuracionDescarga.TotalSeconds;
            tensionBateria = TensionReposoBateria - (CaidaMaximaDescarga * Math.Sin(progreso * Math.PI));
        }

        return Task.FromResult(new EstadoSai(
            Alcanzable: true,
            TensionEntradaVoltios: 220.0,
            TensionSalidaVoltios: 220.0,
            CargaSalidaPorcentaje: 35.0,
            CargaBateriaPorcentaje: 100.0,
            EstadoUps: EstadoUps.EnLinea,
            TensionBateriaVoltios: Math.Round(tensionBateria, 2),
            MarcaTiempoUtc: ahora));
    }

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
    public Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct)
    {
        _descargaInicio = Ahora;
        return Task.FromResult(new ResultadoAccion(
            Aceptada: true,
            Motivo: "Adaptador simulado: test de bateria iniciado (descarga simulada de la tension de bateria).",
            MarcaTiempoUtc: Ahora));
    }
}
