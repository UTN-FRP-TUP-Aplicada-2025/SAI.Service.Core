using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SAI.Service.Core.Api.Endpoints;

/// <summary>
/// Placeholder de la API REST versionada por ruta <c>/api/v1/</c> (§17.P.3).
/// <para>
/// En Sprint 0 solo hay un endpoint informativo que confirma la version del
/// contrato. El contrato real hacia terceros (<c>POST /api/v1/intervenciones</c>
/// con idempotencia y sus cuatro caminos 201/200/409/422, ADR-17) llega en BT-28.
/// </para>
/// </summary>
public static class EndpointsApiV1
{
    /// <summary>Mapea el grupo <c>/api/v1</c> con su endpoint informativo.</summary>
    /// <param name="app">Constructor de rutas del host.</param>
    /// <returns>El mismo constructor, para encadenar.</returns>
    public static IEndpointRouteBuilder MapEndpointsApiV1(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/api/v1").WithTags("API v1");

        v1.MapGet("/", () => Results.Ok(new
        {
            api = "SAI.Service.Core",
            version = "v1",
            estado = "andamiaje",
            nota = "Placeholder de Sprint 0. Los recursos reales se agregan en etapas posteriores."
        }))
        .WithName("ApiV1Raiz");

        return app;
    }
}
