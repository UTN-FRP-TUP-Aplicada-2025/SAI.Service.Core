using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SAI.Service.Core.Application.Equipos;

namespace SAI.Service.Core.Infrastructure.Monitoreo;

/// <summary>
/// Etapa 4·E — al arrancar el servicio, rearma las pruebas de verificación que quedaron <b>esperando un
/// reinicio</b> del host (limpia el marcador de prueba en curso). Bajo ADR-25 (NUT en contenedor), un
/// reinicio real del host reinicia el contenedor: que el servicio vuelva a la vida es, por sí mismo, la
/// señal honesta de que el host cicló. Corre una sola vez, en su propio alcance de DI, después de que las
/// migraciones ya aplicaron (Program.cs). Un fallo del rearme no impide el arranque del servicio.
/// </summary>
public sealed partial class ServicioRearmePruebas(
    IServiceScopeFactory fabricaAlcances,
    ILogger<ServicioRearmePruebas> registro) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var alcance = fabricaAlcances.CreateScope();
            var servicio = alcance.ServiceProvider.GetRequiredService<ServicioVerificacion>();
            await servicio.RearmarPruebasPendientesAsync(cancellationToken);
            RearmeCompletado();
        }
        catch (OperationCanceledException)
        {
            // Apagado en curso durante el arranque: no es un error.
        }
        catch (Exception ex)
        {
            // El rearme es best-effort: no debe tumbar el arranque del servicio.
            RearmeConError(ex);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Rearme de pruebas de verificación completado (reinicio detectado por arranque del servicio).")]
    private partial void RearmeCompletado();

    [LoggerMessage(Level = LogLevel.Error, Message = "Error al rearmar las pruebas de verificación en el arranque.")]
    private partial void RearmeConError(Exception ex);
}
