using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioEquipos"/> sobre <see cref="SaiDbContext"/>.
/// El alta se guarda con un único <c>SaveChanges</c>, que EF ejecuta como una transacción: si algo
/// falla (p. ej. el índice único parcial de "una vigente"), no queda nada persistido (CU-02).
/// </summary>
public sealed class RepositorioEquipos(SaiDbContext contexto) : IRepositorioEquipos
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivoAsync(
        string dispositivoCodigo, string posicion, CancellationToken ct) =>
        await contexto.Montajes
            .Where(m => m.DispositivoCodigo == dispositivoCodigo && m.Posicion == posicion)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct) =>
        await contexto.Coberturas
            .Where(c => c.HostCodigo == hostCodigo)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task GuardarAltaAsync(ConjuntoAlta alta, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(alta);

        contexto.Fabricantes.Add(alta.Fabricante);
        contexto.ModelosDispositivo.Add(alta.ModeloDispositivo);
        contexto.ModelosBateria.Add(alta.ModeloBateria);
        contexto.Unidades.AddRange(alta.Host, alta.Dispositivo, alta.Bateria);
        contexto.Montajes.Add(alta.Montaje);
        contexto.Coberturas.Add(alta.Cobertura);
        contexto.Verificaciones.AddRange(alta.Verificaciones);

        await contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct) =>
        await contexto.Verificaciones.ToListAsync(ct);

    /// <inheritdoc />
    public Task<Verificacion?> VerificacionDeSupuestoAsync(Supuesto supuesto, CancellationToken ct) =>
        contexto.Verificaciones.FirstOrDefaultAsync(v => v.Supuesto == supuesto, ct);

    /// <inheritdoc />
    public Task ActualizarVerificacionAsync(Verificacion verificacion, CancellationToken ct)
    {
        // La verificación se cargó y mutó en este mismo contexto (tracked): basta con guardar.
        contexto.Verificaciones.Update(verificacion);
        return contexto.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public Task<bool> HayEquiposAsync(CancellationToken ct) => contexto.Unidades.AnyAsync(ct);
}
