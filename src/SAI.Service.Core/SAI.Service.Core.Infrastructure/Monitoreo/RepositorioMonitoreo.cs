using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Infrastructure.Persistencia;

namespace SAI.Service.Core.Infrastructure.Monitoreo;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioMonitoreo"/>. Las muestras y sesiones son
/// append-only (el interceptor las protege); cada guardado es una inserción.
/// </summary>
public sealed class RepositorioMonitoreo(SaiDbContext contexto) : IRepositorioMonitoreo
{
    /// <inheritdoc />
    public Task<Dispositivo?> DispositivoEnServicioAsync(CancellationToken ct) =>
        contexto.Unidades.OfType<Dispositivo>()
            .FirstOrDefaultAsync(d => d.Estado == EstadoUnidad.EnServicio, ct);

    /// <inheritdoc />
    public Task<SesionSondeo?> SesionActivaDeAsync(string dispositivoCodigo, CancellationToken ct) =>
        contexto.SesionesSondeo
            .FirstOrDefaultAsync(s => s.DispositivoCodigo == dispositivoCodigo && s.Vigencia.Hasta == null, ct);

    /// <inheritdoc />
    public Task<bool> ExisteFuenteAsync(string codigo, CancellationToken ct) =>
        contexto.FuentesDatos.AnyAsync(f => f.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task GuardarSesionAsync(FuenteDatos? nuevaFuente, SesionSondeo sesion, CancellationToken ct)
    {
        if (nuevaFuente is not null)
        {
            contexto.FuentesDatos.Add(nuevaFuente);
        }

        contexto.SesionesSondeo.Add(sesion);
        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task GuardarMuestraAsync(Muestra muestra, CancellationToken ct)
    {
        contexto.Muestras.Add(muestra);
        await contexto.SaveChangesAsync(ct);
    }
}
