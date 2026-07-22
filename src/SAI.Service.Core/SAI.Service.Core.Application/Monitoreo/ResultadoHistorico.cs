using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>Resolución de una serie histórica: muestras crudas o agregados horarios.</summary>
public enum ResolucionSerie
{
    /// <summary>Muestras crudas (dentro de la retención de 30 días).</summary>
    Muestras = 1,

    /// <summary>Agregados horarios (fuera de la retención de muestras).</summary>
    Agregados = 2,
}

/// <summary>Código de resultado de una consulta histórica (US-11, CU-06).</summary>
public enum CodigoResultadoHistorico
{
    /// <summary>Hay datos para graficar.</summary>
    Ok = 1,

    /// <summary>No hay muestras, agregados ni eventos en el período: no se dibuja una serie vacía.</summary>
    PeriodoSinDatos = 2,

    /// <summary>Un agregado no declara cobertura: se rechaza al graficar (RN-10, I-20).</summary>
    AgregadoSinCobertura = 3,
}

/// <summary>Punto de una serie. <paramref name="Minimo"/>/<paramref name="Maximo"/> solo en agregados.</summary>
public sealed record PuntoSerie(DateTimeOffset Instante, double? Valor, double? Minimo, double? Maximo);

/// <summary>Marca de un evento sobre la gráfica (US-11).</summary>
public sealed record MarcaEvento(
    TipoEvento Tipo,
    DateTimeOffset Instante,
    double? DuracionSeg,
    double? IncertidumbreDuracionSeg,
    string ReglaCodigo,
    int ReglaVersion);

/// <summary>Serie de una variable con su procedencia y, si es agregada, su cobertura y advertencia.</summary>
public sealed record SerieVariable(
    string Variable,
    ResolucionSerie Resolucion,
    Origen Procedencia,
    IReadOnlyList<PuntoSerie> Puntos,
    double? Cobertura,
    string? Advertencia);

/// <summary>
/// Resultado de una consulta histórica (US-11): las series, las marcas de evento y el conteo de
/// microcortes (que sale de los eventos, nunca de la serie agregada, CL-16).
/// </summary>
public sealed record ResultadoHistorico(
    CodigoResultadoHistorico Codigo,
    ResolucionSerie Resolucion,
    IReadOnlyList<SerieVariable> Series,
    IReadOnlyList<MarcaEvento> Marcas,
    int ConteoMicrocortes);
