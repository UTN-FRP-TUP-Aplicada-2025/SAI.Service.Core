using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Infrastructure.Monitoreo;
using SAI.Service.Core.Infrastructure.Persistencia;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Fábrica de pruebas que levanta el host real con una base SQLite temporal y aislada por
/// instancia (para no colisionar entre pruebas) y una clave de firma JWT de prueba. El
/// esquema se aplica por migraciones al arrancar (como en producción).
/// </summary>
public sealed class FabricaSai : WebApplicationFactory<Program>
{
    private readonly string _rutaBase =
        Path.Combine(Path.GetTempPath(), $"sai-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Sai"] = $"Data Source={_rutaBase}",
                // Clave de firma solo para pruebas (>= 32 bytes).
                ["Jwt:ClaveFirma"] = "clave-de-pruebas-integracion-sai-service-core-0123456789abcdef",
                ["Jwt:Emisor"] = "sai-service-core",
                ["Jwt:Audiencia"] = "sai-service-core-api",
            });
        });

        // El planificador de sondeo no debe correr en las pruebas (escribiría muestras en segundo
        // plano y las haría no deterministas): se quita el hosted service. Las pruebas ejercitan el
        // orquestador de sondeo directamente cuando lo necesitan.
        builder.ConfigureTestServices(services =>
        {
            var poller = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(ServicioSondeo));
            if (poller is not null)
            {
                services.Remove(poller);
            }

            // La prueba de batería, sin demoras ni precondición de flotación, para ser determinista.
            services.RemoveAll<OpcionesPrueba>();
            services.AddSingleton(new OpcionesPrueba
            {
                NumeroMuestras = 5,
                IntervaloMuestraMs = 0,
                FlotacionMinimaSeg = 0,
                ToleranciaCargaPct = 5,
            });
        });
    }

    /// <summary>Crea un administrador directamente por el store de Identity (semilla de prueba).</summary>
    public async Task CrearAdministradorAsync(string usuario, string contrasena)
    {
        using var scope = Services.CreateScope();
        var usuarios = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AdministradorUser>>();

        var admin = new AdministradorUser { UserName = usuario };
        var resultado = await usuarios.CreateAsync(admin, contrasena);
        if (!resultado.Succeeded)
        {
            var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"No se pudo crear el administrador de prueba: {errores}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_rutaBase))
        {
            try
            {
                File.Delete(_rutaBase);
            }
            catch (IOException)
            {
                // Best-effort: el archivo temporal se limpia cuando el SO lo libere.
            }
        }
    }
}
