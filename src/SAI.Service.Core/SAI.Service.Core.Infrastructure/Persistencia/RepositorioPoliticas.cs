using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Politicas;
using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioPoliticas"/> sobre <see cref="SaiDbContext"/>.
/// Las versiones son historia append-only (el interceptor impide editarlas): la vigente es la de mayor
/// <see cref="VersionPolitica.Numero"/>. Agregar una versión es un único <c>SaveChanges</c>.
/// </summary>
public sealed class RepositorioPoliticas(SaiDbContext contexto) : IRepositorioPoliticas
{
    /// <inheritdoc />
    public Task<VersionPolitica?> VigenteAsync(CancellationToken ct) =>
        contexto.Politicas.OrderByDescending(p => p.Numero).FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<VersionPolitica>> HistorialAsync(CancellationToken ct) =>
        await contexto.Politicas.OrderByDescending(p => p.Numero).ToListAsync(ct);

    /// <inheritdoc />
    public async Task AgregarVersionAsync(VersionPolitica version, CancellationToken ct)
    {
        contexto.Politicas.Add(version);
        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<bool> ExisteAlgunaAsync(CancellationToken ct) =>
        contexto.Politicas.AnyAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct) =>
        await contexto.Verificaciones.ToListAsync(ct);
}
