using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAI.Service.Core.Application.Monitoreo;

namespace SAI.Service.Core.Infrastructure.Monitoreo;

/// <summary>
/// Planificador de sondeo (BT-17): servicio en segundo plano que ejecuta una ronda a intervalo
/// configurable (5 s por defecto). Cada ronda corre en su propio alcance de DI (un
/// <c>DbContext</c> por ronda, no uno de vida larga) y persiste una muestra con su calidad (US-08).
/// Es resiliente: una ronda que falla no detiene el planificador. No conserva estado crítico (el
/// contador de fallidos es reconstruible desde la historia).
/// </summary>
public sealed partial class ServicioSondeo(
    IServiceScopeFactory fabricaAlcances,
    OpcionesSondeo opciones,
    ILogger<ServicioSondeo> registro) : BackgroundService
{
    private int _fallidosConsecutivos;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!opciones.Habilitado)
        {
            PlanificadorDeshabilitado();
            return;
        }

        var intervalo = TimeSpan.FromSeconds(Math.Max(1, opciones.IntervaloSeg));
        PlanificadorIniciado(intervalo.TotalSeconds);
        using var temporizador = new PeriodicTimer(intervalo);

        while (!stoppingToken.IsCancellationRequested)
        {
            await EjecutarRondaAsync(stoppingToken);

            try
            {
                if (!await temporizador.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task EjecutarRondaAsync(CancellationToken ct)
    {
        try
        {
            using var alcance = fabricaAlcances.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<ServicioMonitoreo>();
            var resultado = await servicio.SondearAsync(opciones.IntervaloSeg, ct);

            if (resultado == ResultadoSondeo.Perdida)
            {
                _fallidosConsecutivos++;
                SondeoPerdido(_fallidosConsecutivos);
            }
            else
            {
                _fallidosConsecutivos = 0;
            }
        }
        catch (OperationCanceledException)
        {
            // Apagado en curso: no es un error.
        }
        catch (Exception ex)
        {
            RondaConError(ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Planificador de sondeo deshabilitado por configuración.")]
    private partial void PlanificadorDeshabilitado();

    [LoggerMessage(Level = LogLevel.Information, Message = "Planificador de sondeo iniciado (intervalo {IntervaloSeg} s).")]
    private partial void PlanificadorIniciado(double intervaloSeg);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sondeo perdido ({Consecutivos} consecutivos): el SAI no respondió.")]
    private partial void SondeoPerdido(int consecutivos);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error en una ronda de sondeo.")]
    private partial void RondaConError(Exception ex);
}
