using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Domain.Catalogo;
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

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configura el esquema estándar de Identity (AspNetUsers, AspNetRoles, etc.).
        base.OnModelCreating(builder);

        // Catálogo / inventario / vínculos / verificaciones del dominio de equipos (Etapa 2).
        ModeloEquipos.Configurar(builder);

        // Historia de monitoreo: fuentes, sesiones, muestras y agregados (Etapa 3).
        ModeloMonitoreo.Configurar(builder);
    }
}
