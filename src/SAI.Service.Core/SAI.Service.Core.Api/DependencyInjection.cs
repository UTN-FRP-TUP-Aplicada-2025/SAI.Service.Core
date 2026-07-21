using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Api.Endpoints;

namespace SAI.Service.Core.Api;

/// <summary>
/// Cableado y mapeo de la capa Api. Lo invoca el composition root (proyecto Web).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra los servicios de la API (en Sprint 0, solo lo minimo del explorador
    /// de endpoints). No incluye persistencia ni adaptadores: eso es Infrastructure.
    /// </summary>
    /// <param name="services">Coleccion de servicios.</param>
    /// <returns>La misma coleccion, para encadenar.</returns>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        return services;
    }

    /// <summary>
    /// Mapea los endpoints de la API: salud (anonimo) y el grupo <c>/api/v1</c>.
    /// </summary>
    /// <param name="app">Constructor de rutas del host.</param>
    /// <returns>El mismo constructor, para encadenar.</returns>
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.MapEndpointsSalud();
        app.MapEndpointsApiV1();
        return app;
    }
}
