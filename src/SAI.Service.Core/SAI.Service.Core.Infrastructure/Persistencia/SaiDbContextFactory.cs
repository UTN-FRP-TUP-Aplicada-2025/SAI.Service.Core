using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Fábrica de diseño para las herramientas de EF Core (<c>dotnet ef migrations</c>). Permite
/// construir el <see cref="SaiDbContext"/> en tiempo de diseño sin levantar el host del proyecto Web
/// (evita depender de su configuración de arranque). La base indicada solo se usa para generar el
/// modelo/migraciones; en ejecución la cadena real la inyecta el composition root (ADR-18).
/// </summary>
public sealed class SaiDbContextFactory : IDesignTimeDbContextFactory<SaiDbContext>
{
    /// <inheritdoc />
    public SaiDbContext CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<SaiDbContext>()
            .UseSqlite("Data Source=sai-design.db")
            .Options;

        return new SaiDbContext(opciones);
    }
}
