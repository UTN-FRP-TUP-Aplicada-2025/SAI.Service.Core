using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Infrastructure.Adaptadores;
using SAI.Service.Core.Infrastructure.Persistencia;

namespace SAI.Service.Core.Infrastructure;

/// <summary>
/// Cableado de servicios de la capa Infrastructure. Lo invoca el composition root
/// (proyecto Web) durante el arranque.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra la persistencia (EF Core + SQLite, ADR-18) con el interceptor
    /// append-only (ADR-04) y el adaptador de conexión (en la Etapa 1, la
    /// implementación simulada, ADR-02). La identidad del administrador (ADR-16)
    /// se cablea en el composition root (proyecto Web) sobre este mismo contexto.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación (cadena de conexión, etc.).</param>
    /// <returns>La misma colección, para encadenar.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SaiDbContext>((sp, options) =>
        {
            // Lectura diferida de la cadena: se resuelve al crear el contexto, no al
            // registrar el servicio. Así respeta cualquier override de configuración
            // aplicado después del registro (por ejemplo, la base SQLite aislada por
            // prueba de las pruebas de integración con WebApplicationFactory).
            var cadena = sp.GetService<IConfiguration>()?.GetConnectionString("Sai")
                         ?? configuration.GetConnectionString("Sai")
                         ?? "Data Source=sai.db";

            options
                .UseSqlite(cadena)
                .AddInterceptors(new InterceptorAppendOnly());
        });

        // Etapa 1: la única implementación del puerto es la simulada (stub).
        services.AddSingleton<IAdaptadorConexion, AdaptadorConexionSimulado>();

        return services;
    }
}
