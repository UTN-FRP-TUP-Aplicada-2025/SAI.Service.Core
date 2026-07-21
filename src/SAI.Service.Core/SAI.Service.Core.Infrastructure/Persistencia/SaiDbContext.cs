using Microsoft.EntityFrameworkCore;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Contexto de EF Core sobre SQLite (ADR-18). En Sprint 0 esta <b>vacio</b>: no
/// declara <c>DbSet</c> ni migraciones reales todavia.
/// <para>
/// El modelo de datos logico (catalogo / inventario / historia append-only) y sus
/// migraciones versionadas aplicadas al arranque llegan en BT-07. Aca solo se deja
/// el contexto y su cableado con la cadena de conexion SQLite para que el DI compile
/// y el host arranque.
/// </para>
/// </summary>
/// <param name="options">Opciones del contexto (proveedor SQLite y cadena de conexion).</param>
public class SaiDbContext(DbContextOptions<SaiDbContext> options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Sprint 0: sin entidades. El modelo se define en BT-07 y siguientes.
    }
}
