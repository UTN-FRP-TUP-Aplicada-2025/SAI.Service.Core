using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Informes;

/// <summary>Una ficha de vida útil cerrada con el modelo y la marca de su batería resueltos (comparación).</summary>
public sealed record FichaConModelo(FichaVidaUtil Ficha, string ModeloCodigo, string ModeloNombre, string? Fabricante);

/// <summary>
/// Puerto de consultas de informes (CU-12, EP-07). Reúne las lecturas históricas por período —que hoy no
/// existían en los repositorios operativos— para armar el informe (cobertura, intervenciones, sustituciones)
/// y la comparación de marcas (fichas cerradas con su modelo). Es solo lectura: no muta nada. Incluye las
/// unidades dadas de baja (RN-12): la baja lógica no las excluye de los informes históricos.
/// </summary>
public interface IRepositorioInformes
{
    /// <summary>El host del sistema (host único), o <c>null</c> si aún no hay ninguno.</summary>
    Task<Host?> HostAsync(CancellationToken ct);

    /// <summary>Todas las coberturas del host (para intersecar con el período).</summary>
    Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct);

    /// <summary>Todos los montajes de batería de los dispositivos dados (baterías intervinientes, incluye bajas).</summary>
    Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivosAsync(IReadOnlyList<string> dispositivoCodigos, CancellationToken ct);

    /// <summary>Recambios de batería ocurridos dentro del período.</summary>
    Task<IReadOnlyList<Intervencion>> IntervencionesPorPeriodoAsync(DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct);

    /// <summary>Reparaciones/sustituciones del SAI del host ocurridas dentro del período.</summary>
    Task<IReadOnlyList<SustitucionSai>> SustitucionesPorPeriodoAsync(string hostCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct);

    /// <summary>Todas las fichas de vida útil cerradas con su modelo y marca resueltos (comparación de marcas).</summary>
    Task<IReadOnlyList<FichaConModelo>> FichasCerradasConModeloAsync(CancellationToken ct);
}
