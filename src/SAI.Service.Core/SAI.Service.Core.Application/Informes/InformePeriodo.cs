using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Informes;

/// <summary>Código de resultado de un informe de período o comparación (CU-12).</summary>
public enum CodigoInforme
{
    /// <summary>El informe o la comparación se armaron con datos.</summary>
    Ok = 1,

    /// <summary>El período no tiene actividad: no se arma un informe vacío como si fuera real (CU-12).</summary>
    PeriodoSinDatos = 2,

    /// <summary>La calidad de suministro se serviría sin cobertura ni advertencia: no se sirve (RN-10, I-20).</summary>
    AgregadoSinCobertura = 3,
}

/// <summary>Una batería que estuvo montada durante el período, con su intervalo recortado (incluye bajas, RN-12).</summary>
public sealed record BateriaInterviniente(string BateriaCodigo, DateTimeOffset Desde, DateTimeOffset Hasta, int Dias);

/// <summary>
/// Sección de cobertura del informe: quién protegió al host y cuántos días, intersecando los intervalos
/// con el período. Los días sin protección son el complemento del período no cubierto (FA-2).
/// </summary>
public sealed record SeccionCobertura(
    IReadOnlyList<string> DispositivosActivos,
    int DiasConProteccion,
    int DiasSinProteccion,
    IReadOnlyList<BateriaInterviniente> BateriasIntervinientes);

/// <summary>Intervenciones y su costo agregado para un tipo (recambio de batería, reparación o sustitución del SAI).</summary>
/// <param name="Tipo">Etiqueta del tipo de intervención.</param>
/// <param name="Cantidad">Cuántas intervenciones de ese tipo cayeron en el período.</param>
/// <param name="Total">Costo total con moneda y fecha (RN-07), o <c>null</c> si esas intervenciones no llevaron costo.</param>
public sealed record CostoPorTipo(string Tipo, int Cantidad, Dinero? Total);

/// <summary>Sección de intervenciones y costos por tipo. Los importes viajan en su moneda y fecha (RN-07).</summary>
public sealed record SeccionCostos(IReadOnlyList<CostoPorTipo> PorTipo);

/// <summary>Conteo de eventos de un tipo en el período.</summary>
public sealed record EventoPorTipo(TipoEvento Tipo, int Cantidad);

/// <summary>
/// Sección de eventos y calidad de suministro. El conteo de microcortes sale de los eventos, nunca del
/// promedio (RN-10, CL-16). Cuando la calidad se construye sobre agregados, viaja con su cobertura y
/// advertencia (RN-10): no se sirve un promedio como verdad completa.
/// </summary>
public sealed record SeccionEventosCalidad(
    int Microcortes,
    IReadOnlyList<EventoPorTipo> EventosPorTipo,
    double? CoberturaAgregados,
    string? AdvertenciaAgregados,
    bool SobreAgregados);

/// <summary>
/// Informe de un período (CU-12): cobertura, intervenciones y costos, eventos y calidad de suministro,
/// todo intersecando intervalos. Ante un período sin actividad, <see cref="Codigo"/> es
/// <see cref="CodigoInforme.PeriodoSinDatos"/> y las secciones quedan nulas (no se arma un informe engañoso).
/// </summary>
public sealed record InformePeriodo(
    CodigoInforme Codigo,
    DateTimeOffset Desde,
    DateTimeOffset Hasta,
    SeccionCobertura? Cobertura,
    SeccionCostos? Costos,
    SeccionEventosCalidad? EventosCalidad);
