using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioSustituciones"/> sobre
/// <see cref="SaiDbContext"/>. La sustitución se aplica con un único <c>SaveChanges</c> (transacción):
/// el cierre de la cobertura vigente, el cambio de estado del equipo saliente, el alta/servicio del
/// suplente con su cobertura nueva, la intervención y las verificaciones reiniciadas se persisten juntas
/// o nada (CU-09 postcondición de fallo).
/// </summary>
public sealed class RepositorioSustituciones(SaiDbContext contexto) : IRepositorioSustituciones
{
    /// <inheritdoc />
    public Task<CoberturaHost?> CoberturaVigenteAsync(CancellationToken ct) =>
        contexto.Coberturas.FirstOrDefaultAsync(c => c.Vigencia.Hasta == null, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct) =>
        await contexto.Coberturas.Where(c => c.HostCodigo == hostCodigo).ToListAsync(ct);

    /// <inheritdoc />
    public Task<Dispositivo?> DispositivoAsync(string codigo, CancellationToken ct) =>
        contexto.Unidades.OfType<Dispositivo>().FirstOrDefaultAsync(d => d.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<bool> ExisteModeloDispositivoAsync(string codigo, CancellationToken ct) =>
        contexto.ModelosDispositivo.AnyAsync(m => m.Codigo == codigo, ct);

    /// <inheritdoc />
    public Task<bool> ExisteUnidadAsync(string codigo, CancellationToken ct) =>
        contexto.Unidades.AnyAsync(u => u.Codigo == codigo, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct) =>
        await contexto.Verificaciones.ToListAsync(ct);

    /// <inheritdoc />
    public async Task GuardarSustitucionAsync(
        CoberturaHost coberturaCerrada,
        CoberturaHost? coberturaNueva,
        Dispositivo dispositivoSaliente,
        Dispositivo? dispositivoEntrante,
        bool entranteEsNuevo,
        SustitucionSai sustitucion,
        IReadOnlyList<Verificacion> verificacionesReiniciadas,
        CancellationToken ct)
    {
        // La cobertura vieja y el dispositivo saliente ya vienen cargados y mutados en este contexto
        // (tracked); el Update es idempotente. El resto se agrega o actualiza según corresponda.
        contexto.Coberturas.Update(coberturaCerrada);
        contexto.Unidades.Update(dispositivoSaliente);

        if (coberturaNueva is not null)
        {
            contexto.Coberturas.Add(coberturaNueva);
        }

        if (dispositivoEntrante is not null)
        {
            if (entranteEsNuevo)
            {
                contexto.Unidades.Add(dispositivoEntrante);
            }
            else
            {
                contexto.Unidades.Update(dispositivoEntrante);
            }
        }

        foreach (var verificacion in verificacionesReiniciadas)
        {
            contexto.Verificaciones.Update(verificacion);
        }

        contexto.Sustituciones.Add(sustitucion);
        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SustitucionSai>> SustitucionesDeHostAsync(string hostCodigo, int cantidad, CancellationToken ct) =>
        await contexto.Sustituciones
            .Where(s => s.HostCodigo == hostCodigo)
            .OrderByDescending(s => s.InstanteOcurrido)
            .Take(cantidad)
            .ToListAsync(ct);
}
