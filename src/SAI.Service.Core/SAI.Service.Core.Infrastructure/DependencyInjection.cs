using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Infrastructure.Adaptadores;
using SAI.Service.Core.Infrastructure.Adaptadores.Nut;
using SAI.Service.Core.Infrastructure.Monitoreo;
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

        // Adaptador de conexión con el SAI (ADR-02): NUT real o simulado, según 'Sai:Adaptador'.
        // Ambas implementaciones cubren el puerto de operación (IAdaptadorConexion, ADR-27) y el
        // descubrimiento (IDescubridorSai, US-03); se resuelven a la MISMA instancia. Por defecto,
        // el simulado (sin hardware); en un despliegue con SAI real se elige "Nut".
        var usarNut = string.Equals(configuration["Sai:Adaptador"], "Nut", StringComparison.OrdinalIgnoreCase);
        if (usarNut)
        {
            var opcionesNut = LeerOpcionesNut(configuration);
            services.AddSingleton<IClienteNut>(new ClienteNut(opcionesNut));
            services.AddSingleton<AdaptadorConexionNut>();
            services.AddSingleton<IAdaptadorConexion>(sp => sp.GetRequiredService<AdaptadorConexionNut>());
            services.AddSingleton<IDescubridorSai>(sp => sp.GetRequiredService<AdaptadorConexionNut>());
        }
        else
        {
            services.AddSingleton<AdaptadorConexionSimulado>();
            services.AddSingleton<IAdaptadorConexion>(sp => sp.GetRequiredService<AdaptadorConexionSimulado>());
            services.AddSingleton<IDescubridorSai>(sp => sp.GetRequiredService<AdaptadorConexionSimulado>());
        }

        // Alta de equipos (CU-02): repositorio EF y el caso de uso. Scoped, sobre el DbContext.
        services.AddScoped<IRepositorioEquipos, Persistencia.RepositorioEquipos>();
        services.AddScoped<ServicioAltaEquipos>();

        // Monitoreo (Etapa 3): planificador de sondeo (hosted service) y persistencia de muestras
        // append-only. El repositorio y el orquestador son scoped (una ronda = un alcance de DI).
        services.AddSingleton(LeerOpcionesSondeo(configuration));
        services.AddScoped<IRepositorioMonitoreo, RepositorioMonitoreo>();
        services.AddScoped<ServicioMonitoreo>();
        services.AddHostedService<ServicioSondeo>();

        return services;
    }

    // Lee 'Sai:Nut' de forma manual (sin el binder de configuración, que Infrastructure no referencia).
    private static OpcionesNut LeerOpcionesNut(IConfiguration configuration)
    {
        var seccion = configuration.GetSection(OpcionesNut.Seccion);
        var defecto = new OpcionesNut();
        return new OpcionesNut
        {
            Host = seccion["Host"] ?? defecto.Host,
            Puerto = int.TryParse(seccion["Puerto"], out var puerto) ? puerto : defecto.Puerto,
            Ups = seccion["Ups"] ?? defecto.Ups,
            TimeoutSegundos = int.TryParse(seccion["TimeoutSegundos"], out var timeout) ? timeout : defecto.TimeoutSegundos,
        };
    }

    // Lee 'Sai:Sondeo' de forma manual (sin el binder de configuración).
    private static OpcionesSondeo LeerOpcionesSondeo(IConfiguration configuration)
    {
        var seccion = configuration.GetSection(OpcionesSondeo.Seccion);
        var defecto = new OpcionesSondeo();
        return new OpcionesSondeo
        {
            IntervaloSeg = int.TryParse(seccion["IntervaloSeg"], out var intervalo) ? intervalo : defecto.IntervaloSeg,
            Habilitado = bool.TryParse(seccion["Habilitado"], out var habilitado) ? habilitado : defecto.Habilitado,
        };
    }
}
