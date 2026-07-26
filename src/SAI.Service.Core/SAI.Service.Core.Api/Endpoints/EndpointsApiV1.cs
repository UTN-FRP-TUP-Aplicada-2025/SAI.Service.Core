using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SAI.Service.Core.Application.Ingesta;

namespace SAI.Service.Core.Api.Endpoints;

/// <summary>
/// API REST versionada por ruta <c>/api/v1/</c> (§17.P.3). Además de los endpoints informativos, expone la
/// <b>ingesta idempotente de intervenciones</b> (<c>POST /api/v1/intervenciones</c>, CU-11): el único
/// contrato formal hacia terceros, con sus cuatro caminos 201/200/409/422 en problem+json (ADR-17).
/// </summary>
public static class EndpointsApiV1
{
    private static readonly JsonSerializerOptions OpcionesJson = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Mapea el grupo <c>/api/v1</c> con sus endpoints.</summary>
    /// <param name="app">Constructor de rutas del host.</param>
    /// <returns>El mismo constructor, para encadenar.</returns>
    public static IEndpointRouteBuilder MapEndpointsApiV1(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/api/v1").WithTags("API v1");

        v1.MapGet("/", () => Results.Ok(new
        {
            api = "SAI.Service.Core",
            version = "v1",
            estado = "operativo",
            nota = "Recurso de ingesta: POST /api/v1/intervenciones (CU-11)."
        }))
        .AllowAnonymous()
        .WithName("ApiV1Raiz");

        // Endpoint de demostracion protegido por Bearer (JWT). Sirve para probar el
        // flujo maquina-a-maquina de punta a punta: sin token -> 401; con token
        // valido (obtenido en POST /api/v1/token) -> 200. La policy "Api" la define
        // el composition root (Web) y exige el esquema JwtBearer, no la cookie del panel.
        v1.MapGet("/ping", (System.Security.Claims.ClaimsPrincipal usuario) => Results.Ok(new
        {
            api = "SAI.Service.Core",
            version = "v1",
            respuesta = "pong",
            usuario = usuario.Identity?.Name,
            utc = DateTimeOffset.UtcNow
        }))
        .RequireAuthorization("Api")
        .WithName("ApiV1Ping");

        // Ingesta idempotente de intervenciones (CU-11, ADR-17). Cabeceras X-Idempotency-Key y
        // X-Fuente-Datos; cuatro caminos 201/200/409/422 en problem+json.
        v1.MapPost("/intervenciones", IngerirIntervencionAsync)
            .RequireAuthorization("Api")
            .WithName("ApiV1IngestaIntervenciones");

        return app;
    }

    private static async Task<IResult> IngerirIntervencionAsync(HttpContext contexto, ServicioIngesta servicio, CancellationToken ct)
    {
        var clave = contexto.Request.Headers["X-Idempotency-Key"].ToString();
        var fuente = contexto.Request.Headers["X-Fuente-Datos"].ToString();
        if (string.IsNullOrWhiteSpace(clave) || string.IsNullOrWhiteSpace(fuente))
        {
            return Results.Problem(
                title: "Cabeceras obligatorias faltantes",
                detail: "Se requieren las cabeceras X-Idempotency-Key y X-Fuente-Datos.",
                statusCode: StatusCodes.Status400BadRequest,
                type: "cabeceras_faltantes");
        }

        // Se lee el cuerpo crudo para calcular la huella sha256 y se deserializa del mismo texto: un reintento
        // byte a byte idéntico produce la misma huella (200); un cuerpo distinto, otra (409).
        string cuerpoTexto;
        using (var lector = new StreamReader(contexto.Request.Body, Encoding.UTF8))
        {
            cuerpoTexto = await lector.ReadToEndAsync(ct);
        }

        var huella = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(cuerpoTexto)));

        EntradaIngesta? entrada;
        try
        {
            entrada = JsonSerializer.Deserialize<EntradaIngesta>(cuerpoTexto, OpcionesJson);
        }
        catch (JsonException)
        {
            return Results.Problem(title: "Cuerpo inválido", detail: "El cuerpo no es un JSON válido.",
                statusCode: StatusCodes.Status400BadRequest, type: "validacion");
        }

        if (entrada is null)
        {
            return Results.Problem(title: "Cuerpo vacío", detail: "Se esperaba un cuerpo JSON.",
                statusCode: StatusCodes.Status400BadRequest, type: "validacion");
        }

        var resultado = await servicio.IngerirAsync(clave, fuente, entrada, huella, DateTimeOffset.UtcNow, ct);

        return resultado.Codigo switch
        {
            CodigoIngesta.Creado => Results.Created($"/api/v1/intervenciones/{resultado.Id}", new
            {
                id = resultado.Id,
                creado = true,
                confianza = resultado.Confianza,
                tiempoValido = resultado.TiempoValido,
                tiempoRegistrado = resultado.TiempoRegistrado,
            }),
            CodigoIngesta.Reintento => Results.Ok(new { id = resultado.Id, creado = false }),
            CodigoIngesta.ConflictoIdempotencia => Results.Problem(
                title: "Conflicto de idempotencia",
                detail: "La misma clave de idempotencia llegó con un cuerpo distinto; no se aplica.",
                statusCode: StatusCodes.Status409Conflict,
                type: "conflicto_idempotencia",
                extensions: new Dictionary<string, object?>
                {
                    ["sha256Original"] = resultado.HuellaOriginal,
                    ["sha256Recibido"] = resultado.HuellaRecibida,
                    ["accionSugerida"] = resultado.AccionSugerida,
                }),
            CodigoIngesta.CoherenciaTemporal => Results.Problem(
                title: "Coherencia temporal",
                detail: resultado.Detalle,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                type: "coherencia_temporal",
                extensions: new Dictionary<string, object?>
                {
                    ["campo"] = resultado.Campo,
                    ["invariante"] = resultado.Invariante,
                }),
            _ => Results.Problem(
                title: "Validación",
                detail: resultado.Detalle,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                type: "validacion",
                extensions: new Dictionary<string, object?>
                {
                    ["campo"] = resultado.Campo,
                    ["invariante"] = resultado.Invariante ?? "validacion",
                }),
        };
    }
}
