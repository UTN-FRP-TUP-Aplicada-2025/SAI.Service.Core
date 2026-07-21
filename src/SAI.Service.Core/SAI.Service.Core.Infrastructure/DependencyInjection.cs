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
    /// Registra la persistencia (EF Core + SQLite, ADR-18) y el adaptador de
    /// conexion (en Sprint 0, la implementacion simulada, ADR-02).
    /// </summary>
    /// <param name="services">Coleccion de servicios.</param>
    /// <param name="configuration">Configuracion de la aplicacion (cadena de conexion, etc.).</param>
    /// <returns>La misma coleccion, para encadenar.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cadena = configuration.GetConnectionString("Sai")
                     ?? "Data Source=sai.db";

        services.AddDbContext<SaiDbContext>(options => options.UseSqlite(cadena));

        // Sprint 0: la unica implementacion del puerto es la simulada (stub).
        services.AddSingleton<IAdaptadorConexion, AdaptadorConexionSimulado>();

        return services;
    }
}
