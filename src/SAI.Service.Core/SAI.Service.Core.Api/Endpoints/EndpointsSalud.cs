using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SAI.Service.Core.Api.Endpoints;

/// <summary>
/// Endpoint de salud del servicio. Es anonimo (sin autenticacion), a diferencia
/// del resto de la API y del panel (ADR-16). Lo consume el smoke test del
/// STAGE-08 del pipeline y el operador del host (<c>curl /health</c>).
/// </summary>
public static class EndpointsSalud
{
    /// <summary>Mapea <c>GET /health</c> como endpoint anonimo.</summary>
    /// <param name="app">Constructor de rutas del host.</param>
    /// <returns>El mismo constructor, para encadenar.</returns>
    public static IEndpointRouteBuilder MapEndpointsSalud(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            estado = "ok",
            servicio = "SAI.Service.Core",
            utc = DateTimeOffset.UtcNow
        }))
        .AllowAnonymous()
        .WithName("Salud")
        .WithTags("Salud");

        return app;
    }
}
