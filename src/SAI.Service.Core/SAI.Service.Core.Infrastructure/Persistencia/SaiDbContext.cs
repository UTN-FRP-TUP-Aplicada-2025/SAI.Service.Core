using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;
using SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Contexto de EF Core sobre SQLite (ADR-18). Hereda de
/// <see cref="IdentityDbContext{TUser}"/> para alojar el esquema de identidad del
/// administrador único (ADR-16): las tablas <c>AspNet*</c> se materializan por
/// migraciones aplicadas al arranque.
/// <para>
/// Aplica la disciplina append-only (ADR-04, BT-09) registrando
/// <see cref="InterceptorAppendOnly"/>. En la Etapa 1 no hay entidades de historia
/// todavía (solo Identity); el interceptor queda cableado para cuando lleguen las
/// tablas de hechos en etapas posteriores.
/// </para>
/// </summary>
/// <param name="options">Opciones del contexto (proveedor SQLite y cadena de conexión).</param>
public class SaiDbContext(DbContextOptions<SaiDbContext> options)
    : IdentityDbContext<AdministradorUser>(options)
{
    /// <summary>Catálogo: fabricantes (ADR-07).</summary>
    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();

    /// <summary>Catálogo: modelos de dispositivo.</summary>
    public DbSet<ModeloDispositivo> ModelosDispositivo => Set<ModeloDispositivo>();

    /// <summary>Catálogo: modelos de batería.</summary>
    public DbSet<ModeloBateria> ModelosBateria => Set<ModeloBateria>();

    /// <summary>Inventario: unidades físicas (Host/Dispositivo/Bateria, TPH).</summary>
    public DbSet<UnidadFisica> Unidades => Set<UnidadFisica>();

    /// <summary>Vínculos temporales: montajes de batería.</summary>
    public DbSet<MontajeBateria> Montajes => Set<MontajeBateria>();

    /// <summary>Vínculos temporales: coberturas de host.</summary>
    public DbSet<CoberturaHost> Coberturas => Set<CoberturaHost>();

    /// <summary>Verificaciones de los cuatro supuestos de seguridad operativa (ADR-10).</summary>
    public DbSet<Verificacion> Verificaciones => Set<Verificacion>();

    /// <summary>Historia de monitoreo: fuentes de datos (Etapa 3).</summary>
    public DbSet<FuenteDatos> FuentesDatos => Set<FuenteDatos>();

    /// <summary>Historia de monitoreo: sesiones de sondeo.</summary>
    public DbSet<SesionSondeo> SesionesSondeo => Set<SesionSondeo>();

    /// <summary>Historia de monitoreo: muestras del sondeo (append-only).</summary>
    public DbSet<Muestra> Muestras => Set<Muestra>();

    /// <summary>Historia de monitoreo: agregados por ventana (append-only).</summary>
    public DbSet<Agregado> Agregados => Set<Agregado>();

    /// <summary>Reglas de derivación de eventos versionadas (append-only, RC-09).</summary>
    public DbSet<ReglaDerivacion> Reglas => Set<ReglaDerivacion>();

    /// <summary>Eventos derivados del sondeo (append-only).</summary>
    public DbSet<Evento> Eventos => Set<Evento>();

    /// <summary>Pruebas de batería con su veredicto de salud (append-only, CU-07).</summary>
    public DbSet<PruebaBateria> PruebasBateria => Set<PruebaBateria>();

    /// <summary>Historia de acciones de apagado (Etapa 4·B, CU-05, ADR-04).</summary>
    public DbSet<Accion> Acciones => Set<Accion>();

    /// <summary>Historia de intervenciones de recambio de batería (Etapa 4·C, CU-08, ADR-04).</summary>
    public DbSet<Intervencion> Intervenciones => Set<Intervencion>();

    /// <summary>Fichas de vida útil proyectadas al recambiar (Etapa 4·C, US-19).</summary>
    public DbSet<FichaVidaUtil> FichasVidaUtil => Set<FichaVidaUtil>();

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // SQLite no ordena ni compara DateTimeOffset almacenado como TEXT (falla el ORDER BY de las
        // muestras/eventos por instante). Se guardan como long (DateTimeOffsetToBinaryConverter
        // preserva el offset y el orden cronológico), habilitando ordenar por instante y comparar la
        // vigencia de las reglas. Aplica a DateTimeOffset y DateTimeOffset?.
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configura el esquema estándar de Identity (AspNetUsers, AspNetRoles, etc.).
        base.OnModelCreating(builder);

        // Catálogo / inventario / vínculos / verificaciones del dominio de equipos (Etapa 2).
        ModeloEquipos.Configurar(builder);

        // Historia de monitoreo: fuentes, sesiones, muestras y agregados (Etapa 3).
        ModeloMonitoreo.Configurar(builder);

        // Historia de intervenciones: recambio de batería y ficha de vida útil (Etapa 4·C).
        ModeloIntervenciones.Configurar(builder);
    }
}
