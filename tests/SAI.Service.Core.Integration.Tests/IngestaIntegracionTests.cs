using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la API de ingesta (CU-11, ADR-17 §8): los cuatro caminos 201/200/409/422 de
/// <c>POST /api/v1/intervenciones</c> con token JWT y las cabeceras de idempotencia y fuente. Es la métrica
/// de validación exigida por el contrato.
/// </summary>
public class IngestaIntegracionTests
{
    private const string Usuario = "administrador";
    private const string Contrasena = "Contrasena-Segura-2026";
    private const string Fuente = "fd-gmao-externo";

    private const string CuerpoValido =
        """{"tipoIntervencionId":"ti-inspeccion","dispositivoId":"ups","bateriaIds":[],"tiempoValido":"2026-06-01T10:00:00+00:00","costos":{"repuestos":[],"manoDeObra":{"monto":12000,"moneda":"ARS","fecha":"2026-06-01"},"total":{"monto":12000,"moneda":"ARS","fecha":"2026-06-01"}},"hallazgos":"inspeccion"}""";

    // CA-01: clave nueva y cuerpo válido → 201 creado, confianza media.
    [Fact]
    public async Task ClaveNuevaDevuelve201Creado()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        var respuesta = await PostAsync(cliente, token, "ot-88213", CuerpoValido);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Created);
        var cuerpo = await LeerJsonAsync(respuesta);
        cuerpo.GetProperty("creado").GetBoolean().Should().BeTrue();
        cuerpo.GetProperty("confianza").GetString().Should().Be("media");
        cuerpo.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    // CA-02: misma clave con el mismo cuerpo → 200 creado:false, mismo id.
    [Fact]
    public async Task MismaClaveMismoCuerpoDevuelve200SinDuplicar()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        var primera = await PostAsync(cliente, token, "ot-88213", CuerpoValido);
        var idOriginal = (await LeerJsonAsync(primera)).GetProperty("id").GetString();

        var reintento = await PostAsync(cliente, token, "ot-88213", CuerpoValido);

        reintento.StatusCode.Should().Be(HttpStatusCode.OK);
        var cuerpo = await LeerJsonAsync(reintento);
        cuerpo.GetProperty("creado").GetBoolean().Should().BeFalse();
        cuerpo.GetProperty("id").GetString().Should().Be(idOriginal);
    }

    // CA-03: misma clave con cuerpo distinto → 409 conflicto con ambas huellas, nunca 200.
    [Fact]
    public async Task MismaClaveCuerpoDistintoDevuelve409()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        await PostAsync(cliente, token, "ot-88213", CuerpoValido);
        var cuerpoDistinto = CuerpoValido.Replace("12000", "19500", StringComparison.Ordinal);

        var respuesta = await PostAsync(cliente, token, "ot-88213", cuerpoDistinto);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problema = await LeerJsonAsync(respuesta);
        problema.GetProperty("type").GetString().Should().Be("conflicto_idempotencia");
        problema.GetProperty("sha256Original").GetString().Should()
            .NotBe(problema.GetProperty("sha256Recibido").GetString());
    }

    // CA-04: costos que no cuadran → 422 validacion.
    [Fact]
    public async Task CostosQueNoCuadranDevuelve422()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        var noCuadra =
            """{"tipoIntervencionId":"ti-recambio","dispositivoId":"ups","bateriaIds":[],"tiempoValido":"2026-06-01T10:00:00+00:00","costos":{"repuestos":[{"monto":52000,"moneda":"ARS","fecha":"2026-06-01"}],"manoDeObra":{"monto":15000,"moneda":"ARS","fecha":"2026-06-01"},"total":{"monto":60000,"moneda":"ARS","fecha":"2026-06-01"}},"hallazgos":"x"}""";

        var respuesta = await PostAsync(cliente, token, "ot-99001", noCuadra);

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await LeerJsonAsync(respuesta)).GetProperty("type").GetString().Should().Be("validacion");
    }

    // CA-05: intervención fechada después de la baja de una batería → 422 coherencia_temporal.
    [Fact]
    public async Task IntervencionPosteriorALaBajaDevuelve422CoherenciaTemporal()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        // La batería 'bat' (del alta) se da de baja el 2026-09-05.
        using (var scope = fabrica.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();
            var bateria = await db.Unidades.OfType<Bateria>().FirstAsync(b => b.Codigo == "bat");
            bateria.DarDeBaja(new DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero), "agotada");
            await db.SaveChangesAsync();
        }

        var posterior =
            """{"tipoIntervencionId":"ti-inspeccion","dispositivoId":"ups","bateriaIds":["bat"],"tiempoValido":"2026-11-01T10:00:00+00:00","costos":{"repuestos":[],"manoDeObra":{"monto":1000,"moneda":"ARS","fecha":"2026-11-01"},"total":{"monto":1000,"moneda":"ARS","fecha":"2026-11-01"}},"hallazgos":"x"}""";

        var respuesta = await PostAsync(cliente, token, "ot-99002", posterior);

        respuesta.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await LeerJsonAsync(respuesta)).GetProperty("type").GetString().Should().Be("coherencia_temporal");
    }

    [Fact]
    public async Task SinTokenDevuelve401()
    {
        using var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient();

        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/intervenciones")
        {
            Content = new StringContent(CuerpoValido, Encoding.UTF8, "application/json"),
        };
        solicitud.Headers.Add("X-Idempotency-Key", "ot-1");
        solicitud.Headers.Add("X-Fuente-Datos", Fuente);

        var respuesta = await cliente.SendAsync(solicitud);

        respuesta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SinCabecerasDevuelve400()
    {
        var (fabrica, cliente, token) = await PrepararAsync();
        using var _ = fabrica;

        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/intervenciones")
        {
            Content = new StringContent(CuerpoValido, Encoding.UTF8, "application/json"),
        };
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var respuesta = await cliente.SendAsync(solicitud);

        respuesta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- Infraestructura de prueba ---

    private static async Task<(FabricaSai, HttpClient, string)> PrepararAsync()
    {
        var fabrica = new FabricaSai();
        await fabrica.CrearAdministradorAsync(Usuario, Contrasena);
        var cliente = fabrica.CreateClient();

        // El alta crea el host/dispositivo/batería y su modelo (para CA-05).
        using (var scope = fabrica.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<ServicioAltaEquipos>()
                .RegistrarAsync(SolicitudValida(), CancellationToken.None);
        }

        var token = await ObtenerTokenAsync(cliente);
        return (fabrica, cliente, token);
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient cliente, string token, string clave, string cuerpo)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/api/v1/intervenciones")
        {
            Content = new StringContent(cuerpo, Encoding.UTF8, "application/json"),
        };
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Headers.Add("X-Idempotency-Key", clave);
        solicitud.Headers.Add("X-Fuente-Datos", Fuente);
        return cliente.SendAsync(solicitud);
    }

    private static async Task<JsonElement> LeerJsonAsync(HttpResponseMessage respuesta) =>
        JsonSerializer.Deserialize<JsonElement>(await respuesta.Content.ReadAsStringAsync());

    private static async Task<string> ObtenerTokenAsync(HttpClient cliente)
    {
        var formulario = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = Usuario,
            ["password"] = Contrasena,
        });
        var respuesta = await cliente.PostAsync("/api/v1/token", formulario);
        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await respuesta.Content.ReadFromJsonAsync<RespuestaToken>();
        return token!.AccessToken;
    }

    private static SolicitudAltaEquipos SolicitudValida() => new(
        new DateTimeOffset(2026, 1, 5, 10, 30, 0, TimeSpan.FromHours(-3)),
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");

    private sealed record RespuestaToken(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("token_type")] string TokenType,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
