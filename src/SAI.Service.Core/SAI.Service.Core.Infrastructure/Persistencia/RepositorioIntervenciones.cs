using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioIntervenciones"/> sobre
/// <see cref="SaiDbContext"/>. El recambio se aplica con un único <c>SaveChanges</c> (transacción):
/// el cierre del montaje viejo, la baja de la batería retirada, el alta de la nueva con su montaje y
/// la intervención/ficha se persisten juntas o nada (CU-08 postcondición de fallo).
/// </summary>
public sealed class RepositorioIntervenciones(SaiDbContext contexto) : IRepositorioIntervenciones
{
    /// <inheritdoc />
    public Task<Dispositivo?> DispositivoEnServicioAsync(CancellationToken ct) =>
        contexto.Unidades.OfType<Dispositivo>()
            .FirstOrDefaultAsync(d => d.Estado == EstadoUnidad.EnServicio, ct);

    /// <inheritdoc />
    public Task<MontajeBateria?> MontajeVigenteAsync(string dispositivoCodigo, string posicion, CancellationToken ct) =>
        contexto.Montajes
            .FirstOrDefaultAsync(m => m.DispositivoCodigo == dispositivoCodigo && m.Posicion == posicion && m.Vigencia.Hasta == null, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivoAsync(string dispositivoCodigo, string posicion, CancellationToken ct) =>
        await contexto.Montajes
            .Where(m => m.DispositivoCodigo == dispositivoCodigo && m.Posicion == posicion)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<Bateria?> BateriaAsync(string codigo, CancellationToken ct) =>
        contexto.Unidades.OfType<Bateria>().FirstOrDefaultAsync(b => b.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<ModeloBateria?> ModeloBateriaAsync(string codigo, CancellationToken ct) =>
        contexto.ModelosBateria.FirstOrDefaultAsync(m => m.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<bool> ExisteUnidadAsync(string codigo, CancellationToken ct) =>
        contexto.Unidades.AnyAsync(u => u.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task GuardarRecambioAsync(
        MontajeBateria montajeCerrado,
        Bateria bateriaRetirada,
        Bateria bateriaNueva,
        MontajeBateria montajeNuevo,
        Intervencion intervencion,
        FichaVidaUtil ficha,
        CancellationToken ct)
    {
        // El montaje viejo y la batería retirada ya vienen cargados y mutados en este contexto
        // (tracked); el Update es idempotente. La nueva batería, su montaje y la historia se agregan.
        contexto.Montajes.Update(montajeCerrado);
        contexto.Unidades.Update(bateriaRetirada);
        contexto.Unidades.Add(bateriaNueva);
        contexto.Montajes.Add(montajeNuevo);
        contexto.Intervenciones.Add(intervencion);
        contexto.FichasVidaUtil.Add(ficha);

        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Intervencion>> IntervencionesDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.Intervenciones
            .Where(i => i.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(i => i.InstanteOcurrido)
            .Take(cantidad)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FichaVidaUtil>> FichasDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) =>
        await contexto.FichasVidaUtil
            .Where(f => f.DispositivoCodigo == dispositivoCodigo)
            .OrderByDescending(f => f.IntervencionCodigo)
            .Take(cantidad)
            .ToListAsync(ct);
}
