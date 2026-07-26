using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Ingesta;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioIngesta"/> (CU-11) sobre <see cref="SaiDbContext"/>.
/// La búsqueda por clave de idempotencia resuelve los caminos 200/409; las unidades se consultan sin filtrar
/// por baja (RN-12: la baja no las excluye de las consultas). El alta es un único <c>SaveChanges</c>.
/// </summary>
public sealed class RepositorioIngesta(SaiDbContext contexto) : IRepositorioIngesta
{
    /// <inheritdoc />
    public Task<IntervencionIngerida?> BuscarPorClaveAsync(string clave, CancellationToken ct) =>
        contexto.IntervencionesIngeridas.FirstOrDefaultAsync(i => i.ClaveIdempotencia == clave, ct);

    /// <inheritdoc />
    public Task<FuenteDatos?> FuenteAsync(string codigo, CancellationToken ct) =>
        contexto.FuentesDatos.FirstOrDefaultAsync(f => f.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<UnidadFisica?> UnidadAsync(string codigo, CancellationToken ct) =>
        contexto.Unidades.FirstOrDefaultAsync(u => u.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task AgregarAsync(IntervencionIngerida intervencion, CancellationToken ct)
    {
        contexto.IntervencionesIngeridas.Add(intervencion);
        await contexto.SaveChangesAsync(ct);
    }
}
