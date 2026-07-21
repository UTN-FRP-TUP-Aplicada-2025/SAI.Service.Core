using Microsoft.AspNetCore.Identity;
using SAI.Service.Core.Infrastructure.Persistencia;
using SAI.Service.Core.Web.Autenticacion;

namespace SAI.Service.Core.Web.Endpoints;

/// <summary>
/// Endpoints de acceso que requieren <c>HttpContext</c> y el store de Identity y que,
/// por eso, viven en el composition root (proyecto Web):
/// <list type="bullet">
///   <item><c>POST /api/v1/token</c>: emisión de token JWT por ROPC (máquina-a-máquina).</item>
///   <item><c>POST /cuenta/cerrar-sesion</c>: cierre de sesión del panel (cookie).</item>
/// </list>
/// El alta, el login y el cambio de contraseña del panel son componentes SSR
/// (formularios con antiforgería); ver <c>Components/Pages/Acceso</c>.
/// </summary>
public static class EndpointsAcceso
{
    /// <summary>Mapea los endpoints de acceso del host.</summary>
    /// <param name="app">Constructor de rutas del host.</param>
    /// <returns>El mismo constructor, para encadenar.</returns>
    public static IEndpointRouteBuilder MapEndpointsAcceso(this IEndpointRouteBuilder app)
    {
        MapToken(app);
        MapCerrarSesion(app);
        return app;
    }

    // POST /api/v1/token — Resource Owner Password Credentials (password grant).
    //
    // ROPC está desaconsejado por OAuth 2.1 para terceros; acá es aceptable por ser un
    // cliente de PRIMERA PARTE y confiable dentro de la LAN (el GMAO de ingesta, CU-11),
    // no un tercero público. Se documenta la excepción sin bloquear (ADR-16 / ADR-20).
    // Anónimo y sin antiforgería (contrato de máquina, no formulario de navegador).
    private static void MapToken(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/token", async (
                HttpRequest solicitud,
                UserManager<AdministradorUser> usuarios,
                GeneradorTokens generador) =>
            {
                if (!solicitud.HasFormContentType)
                {
                    return Results.BadRequest(new { error = "invalid_request" });
                }

                var formulario = await solicitud.ReadFormAsync();
                var grantType = formulario["grant_type"].ToString();
                var usuario = formulario["username"].ToString();
                var contrasena = formulario["password"].ToString();

                if (!string.Equals(grantType, "password", StringComparison.Ordinal))
                {
                    return Results.BadRequest(new { error = "unsupported_grant_type" });
                }

                // Rechazo indiferenciado: no se revela si falló el usuario o la contraseña.
                var cuenta = await usuarios.FindByNameAsync(usuario);
                if (cuenta is null || !await usuarios.CheckPasswordAsync(cuenta, contrasena))
                {
                    return Results.BadRequest(new { error = "invalid_grant" });
                }

                var (accessToken, expiraEn) = generador.Generar(cuenta);
                return Results.Ok(new
                {
                    access_token = accessToken,
                    token_type = "Bearer",
                    expires_in = expiraEn,
                });
            })
            .AllowAnonymous()
            .DisableAntiforgery()
            .WithName("TokenRopc")
            .WithTags("Acceso");
    }

    // POST /cuenta/cerrar-sesion — cierra la sesión de cookie y vuelve al acceso.
    // Antiforgería deshabilitada: se invoca desde un formulario de la barra superior
    // (circuito interactivo) por navegación completa; el logout no expone estado sensible.
    private static void MapCerrarSesion(IEndpointRouteBuilder app)
    {
        app.MapPost("/cuenta/cerrar-sesion", async (SignInManager<AdministradorUser> ingresos) =>
            {
                await ingresos.SignOutAsync();
                return Results.Redirect("/acceso");
            })
            .DisableAntiforgery()
            .WithName("CerrarSesion")
            .WithTags("Acceso");
    }
}
