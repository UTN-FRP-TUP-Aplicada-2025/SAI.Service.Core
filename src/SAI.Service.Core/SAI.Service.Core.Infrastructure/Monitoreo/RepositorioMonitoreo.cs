using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Vinculos;
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<Muestra>> MuestrasRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.Muestras
            .Where(m => m.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(m => m.Instante)
            .Take(cantidad)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, ReglaDerivacion>> ReglasVigentesAsync(DateTimeOffset instante, CancellationToken ct)
    {
        var vigentes = await contexto.Reglas
            .Where(r => r.VigenteDesde <= instante)
            .ToListAsync(ct);

        return vigentes
            .GroupBy(r => r.Codigo)
            .ToDictionary(g => g.Key, g => g.MaxBy(r => r.Version)!, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public async Task GuardarEventosAsync(IReadOnlyList<Evento> eventos, CancellationToken ct)
    {
        contexto.Eventos.AddRange(eventos);
        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Evento>> EventosRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.Eventos
            .Where(e => e.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(e => e.Instante)
            .Take(cantidad)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task GuardarAccionAsync(Accion accion, CancellationToken ct)
    {
        contexto.Acciones.Add(accion);
        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Accion>> AccionesRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.Acciones
            .Where(a => a.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(a => a.Instante)
            .Take(cantidad)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Muestra>> MuestrasPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await contexto.Muestras
            .Where(m => m.DispositivoCodigo == dispositivoCodigo && m.Instante >= desde && m.Instante <= hasta)
            .OrderBy(m => m.Instante)
            .Take(10000)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Agregado>> AgregadosPorPeriodoAsync(string dispositivoCodigo, string variable, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await contexto.Agregados
            .Where(a => a.DispositivoCodigo == dispositivoCodigo && a.Variable == variable && a.VentanaInicio >= desde && a.VentanaInicio <= hasta)
            .OrderBy(a => a.VentanaInicio)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Evento>> EventosPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await contexto.Eventos
            .Where(e => e.DispositivoCodigo == dispositivoCodigo && e.Instante >= desde && e.Instante <= hasta)
            .OrderBy(e => e.Instante)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<MontajeBateria?> MontajeVigenteAsync(string dispositivoCodigo, CancellationToken ct) =>
        contexto.Montajes.FirstOrDefaultAsync(m => m.DispositivoCodigo == dispositivoCodigo && m.Vigencia.Hasta == null, ct);

    /// <inheritdoc />
    public Task<Evento?> UltimoCorteAsync(string dispositivoCodigo, CancellationToken ct) =>
        contexto.Eventos
            .Where(e => e.DispositivoCodigo == dispositivoCodigo && e.Tipo == TipoEvento.CorteSuministro)
            .OrderByDescending(e => e.Instante)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PruebaBateria>> PruebasDeMontajeAsync(string montajeCodigo, CancellationToken ct) =>
        await contexto.PruebasBateria
            .Where(p => p.MontajeBateriaCodigo == montajeCodigo)
            .OrderBy(p => p.Instante)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PruebaBateria>> PruebasDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.PruebasBateria
            .Where(p => p.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(p => p.Instante)
            .Take(cantidad)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task GuardarPruebaConSerieAsync(PruebaBateria prueba, IReadOnlyList<Muestra> serie, SesionSondeo sesionDensa, FuenteDatos? nuevaFuente, CancellationToken ct)
    {
        if (nuevaFuente is not null)
        {
            contexto.FuentesDatos.Add(nuevaFuente);
        }

        contexto.SesionesSondeo.Add(sesionDensa);
        contexto.Muestras.AddRange(serie);
        contexto.PruebasBateria.Add(prueba);
        await contexto.SaveChangesAsync(ct);
    }
}
