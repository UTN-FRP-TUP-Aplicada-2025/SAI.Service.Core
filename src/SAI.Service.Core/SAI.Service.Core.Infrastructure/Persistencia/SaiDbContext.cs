using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configura el esquema estándar de Identity (AspNetUsers, AspNetRoles, etc.).
        base.OnModelCreating(builder);

        // Etapa 1: sin entidades de negocio. El modelo de datos lógico (catálogo /
        // inventario / historia append-only) y sus migraciones llegan en etapas
        // posteriores. Los objetos de valor Valor<T>/Dinero (Domain) se mapearán
        // entonces como owned types (Modelo-Datos-Lógico §2).
    }
}
