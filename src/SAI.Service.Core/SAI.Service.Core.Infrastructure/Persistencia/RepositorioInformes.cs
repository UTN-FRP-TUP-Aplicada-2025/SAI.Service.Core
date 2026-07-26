using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Application.Informes;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Implementación EF Core del puerto <see cref="IRepositorioInformes"/> (CU-12): consultas de solo lectura
/// por período sobre <see cref="SaiDbContext"/>. No filtra por estado de baja: las unidades dadas de baja
/// siguen apareciendo en los informes históricos (RN-12). La comparación de marcas resuelve el modelo y la
/// marca de cada ficha uniendo con catálogo e inventario.
/// </summary>
public sealed class RepositorioInformes(SaiDbContext contexto) : IRepositorioInformes
{
    /// <inheritdoc />
    public Task<Host?> HostAsync(CancellationToken ct) =>
        contexto.Unidades.OfType<Host>().OrderBy(h => h.Codigo).FirstOrDefaultAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct) =>
        await contexto.Coberturas.Where(c => c.HostCodigo == hostCodigo).ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivosAsync(IReadOnlyList<string> dispositivoCodigos, CancellationToken ct) =>
        await contexto.Montajes.Where(m => dispositivoCodigos.Contains(m.DispositivoCodigo)).ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Intervencion>> IntervencionesPorPeriodoAsync(DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await contexto.Intervenciones
            .Where(i => i.InstanteOcurrido >= desde && i.InstanteOcurrido < hasta)
            .OrderBy(i => i.InstanteOcurrido)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<SustitucionSai>> SustitucionesPorPeriodoAsync(string hostCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) =>
        await contexto.Sustituciones
            .Where(s => s.HostCodigo == hostCodigo && s.InstanteOcurrido >= desde && s.InstanteOcurrido < hasta)
            .OrderBy(s => s.InstanteOcurrido)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FichaConModelo>> FichasCerradasConModeloAsync(CancellationToken ct)
    {
        var fichas = await contexto.FichasVidaUtil.ToListAsync(ct);
        if (fichas.Count == 0)
        {
            return [];
        }

        // Resolución del modelo y la marca por unión en memoria (las fichas son pocas): ficha → batería →
        // modelo → fabricante. Incluye baterías dadas de baja (RN-12): no se filtra por estado.
        var baterias = await contexto.Unidades.OfType<Bateria>()
            .ToDictionaryAsync(b => b.Codigo, b => b.ModeloBateriaCodigo, ct);
        var modelos = await contexto.ModelosBateria
            .ToDictionaryAsync(m => m.Codigo, m => new { m.Nombre, m.FabricanteCodigo }, ct);
        var fabricantes = await contexto.Fabricantes
            .ToDictionaryAsync(f => f.Codigo, f => f.Nombre, ct);

        var resultado = new List<FichaConModelo>(fichas.Count);
        foreach (var ficha in fichas)
        {
            var modeloCodigo = baterias.GetValueOrDefault(ficha.BateriaCodigo);
            if (modeloCodigo is null || !modelos.TryGetValue(modeloCodigo, out var modelo))
            {
                // Sin modelo resoluble no se puede comparar por marca; se omite.
                continue;
            }

            var fabricante = fabricantes.GetValueOrDefault(modelo.FabricanteCodigo);
            resultado.Add(new FichaConModelo(ficha, modeloCodigo, modelo.Nombre, fabricante));
        }

        return resultado;
    }
}
