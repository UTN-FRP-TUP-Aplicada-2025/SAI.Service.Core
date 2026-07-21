using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Pruebas de integración del acceso (BT-10, US-01, US-02): guarda global, alta por
/// formulario y el flujo de token JWT por ROPC para la API.
/// </summary>
public class AccesoIntegracionTests
{
    private const string Usuario = "administrador";
    private const string Contrasena = "Contrasena-Segura-2026";

    private static WebApplicationFactoryClientOptions SinRedireccion() =>
        new() { AllowAutoRedirect = false };

    [Fact]
    public async Task RutaProtegidaSinAdministradorRedirigeAAltaInicial()
    {
        using var fabrica = new FabricaSai();
        var cliente = fabrica.CreateClient(SinRedireccion());

        var respuesta = await cliente.GetAsync("/");

        ((int)respuesta.StatusCode).Should().BeInRange(300, 399);
        respuesta.Headers.Location!.OriginalString.Should().Contain("alta-inicial");
    }

    [Fact]
    public async Task RutaProtegidaConAdministradorSinSesionRedirigeAAcceso()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient(SinRedireccion());

        var respuesta = await cliente.GetAsync("/");

        ((int)respuesta.StatusCode).Should().BeInRange(300, 399);
        respuesta.Headers.Location!.OriginalString.Should().Contain("acceso");
    }

    [Fact]
    public async Task AltaPorFormularioCreaElAdministrador()
    {
        using var fabrica = new FabricaSai();
        var cliente = fabrica.CreateClient(SinRedireccion());

        // La superficie de alta debe estar disponible en el primer arranque.
        var pagina = await cliente.GetAsync("/alta-inicial");
        pagina.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await pagina.Content.ReadAsStringAsync();
        html.Should().Contain("Crear la cuenta de administrador");

        // Reenvía los campos ocultos del formulario (antiforgería + handler) más las credenciales.
        var campos = ExtraerCamposOcultos(html);
        if (!campos.ContainsKey("_handler"))
        {
            campos["_handler"] = "alta-inicial";
        }
        campos["Entrada.Usuario"] = Usuario;
        campos["Entrada.Contrasena"] = Contrasena;
        campos["Entrada.Repetir"] = Contrasena;

        var respuesta = await cliente.PostAsync("/alta-inicial", new FormUrlEncodedContent(campos));
        ((int)respuesta.StatusCode).Should().BeInRange(200, 399);

        // Verifica que el administrador quedó creado en el store de Identity.
        using var scope = fabrica.Services.CreateScope();
        var usuarios = scope.ServiceProvider.GetRequiredService<UserManager<AdministradorUser>>();
        usuarios.Users.Any().Should().BeTrue();
    }

    [Fact]
    public async Task LaSuperficieDeAltaEmiteUnUnicoTokenAntiforgery()
    {
        using var fabrica = new FabricaSai();
        var cliente = fabrica.CreateClient(SinRedireccion());

        var html = await (await cliente.GetAsync("/alta-inicial")).Content.ReadAsStringAsync();

        // Regresión: EditForm SSR (method="post" + FormName) ya emite el token antiforgery.
        // Un <AntiforgeryToken/> explícito adicional duplica el campo; el navegador envía
        // los dos valores y la validación antiforgery falla con HTTP 400. Debe haber uno solo.
        Regex.Count(html, "name=\"__RequestVerificationToken\"").Should().Be(1);
    }

    [Fact]
    public async Task LaSuperficieDeAccesoEmiteUnUnicoTokenAntiforgery()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient(SinRedireccion());

        var html = await (await cliente.GetAsync("/acceso")).Content.ReadAsStringAsync();

        Regex.Count(html, "name=\"__RequestVerificationToken\"").Should().Be(1);
    }

    [Fact]
    public async Task CambioDeContrasenaAutenticadoRedirigeAlAcceso()
    {
        const string ContrasenaNueva = "Contrasena-Nueva-2026";
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient(SinRedireccion());

        await IniciarSesionAsync(cliente, Usuario, Contrasena);

        // Formulario autenticado (requiere sesión): GET + POST con la nueva contraseña.
        var pagina = await cliente.GetAsync("/cuenta/cambiar-contrasena");
        pagina.StatusCode.Should().Be(HttpStatusCode.OK);
        var campos = ExtraerCamposOcultos(await pagina.Content.ReadAsStringAsync());
        campos.TryAdd("_handler", "cambiar-contrasena");
        campos["Entrada.Actual"] = Contrasena;
        campos["Entrada.Nueva"] = ContrasenaNueva;
        campos["Entrada.Repetir"] = ContrasenaNueva;

        var respuesta = await cliente.PostAsync("/cuenta/cambiar-contrasena", new FormUrlEncodedContent(campos));

        // Regresión: la validación antiforgery de un formulario SSR autenticado depende de que
        // UseAntiforgery corra DESPUÉS de UseAuthentication; si no, el token no coincide con el
        // usuario actual y el POST devuelve 400 ("different claims-based user"). Debe redirigir
        // al acceso (Acceso-Monousuario invalida la sesión al cambiar la contraseña).
        ((int)respuesta.StatusCode).Should().BeInRange(300, 399);
        respuesta.Headers.Location!.OriginalString.Should().Contain("acceso");
    }

    [Fact]
    public async Task EndpointApiSinTokenDevuelve401()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient();

        var respuesta = await cliente.GetAsync("/api/v1/ping");

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenRopcPermiteLlamarElEndpointProtegido()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient();

        var token = await ObtenerTokenAsync(cliente, Usuario, Contrasena);
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrEmpty();
        token.TokenType.Should().Be("Bearer");

        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ping");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var respuesta = await cliente.SendAsync(solicitud);

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TokenConCredencialesInvalidasDevuelve400()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient();

        var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = Usuario,
            ["password"] = "clave-incorrecta",
        });

        var respuesta = await cliente.PostAsync("/api/v1/token", formulario);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task IniciarSesionAsync(HttpClient cliente, string usuario, string contrasena)
    {
        var pagina = await cliente.GetAsync("/acceso");
        pagina.StatusCode.Should().Be(HttpStatusCode.OK);
        var campos = ExtraerCamposOcultos(await pagina.Content.ReadAsStringAsync());
        campos.TryAdd("_handler", "acceso");
        campos["Entrada.Usuario"] = usuario;
        campos["Entrada.Contrasena"] = contrasena;

        var respuesta = await cliente.PostAsync("/acceso", new FormUrlEncodedContent(campos));

        // Inicio de sesión correcto: redirección al panel con la cookie de sesión emitida.
        ((int)respuesta.StatusCode).Should().BeInRange(300, 399);
    }

    private static async Task<RespuestaToken?> ObtenerTokenAsync(HttpClient cliente, string usuario, string contrasena)
    {
        var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = usuario,
            ["password"] = contrasena,
        });

        var respuesta = await cliente.PostAsync("/api/v1/token", formulario);
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        return await respuesta.Content.ReadFromJsonAsync<RespuestaToken>();
    }

    private static Dictionary<string, string> ExtraerCamposOcultos(string html)
    {
        var campos = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match input in Regex.Matches(html, "<input[^>]*type=\"hidden\"[^>]*>"))
        {
            var nombre = Regex.Match(input.Value, "name=\"([^\"]+)\"");
            var valor = Regex.Match(input.Value, "value=\"([^\"]*)\"");
            if (nombre.Success)
            {
                campos[nombre.Groups[1].Value] =
                    WebUtility.HtmlDecode(valor.Success ? valor.Groups[1].Value : string.Empty);
            }
        }

        return campos;
    }

    private sealed record RespuestaToken(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
