using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Informes;

/// <summary>
/// Una fila de la comparación de marcas: un modelo de batería con su costo por año de servicio normalizado
/// a USD (marcado como derivado con su fuente, RN-07), si cumplió la expectativa de vida y su desvío.
/// </summary>
public sealed record FilaModelo(
    string ModeloCodigo,
    string ModeloNombre,
    string? Fabricante,
    int FichasCerradas,
    Dinero CostoPorAnioUsd,
    string FuenteCotizacion,
    bool CumplioExpectativa,
    int DesvioDiasPromedio);

/// <summary>
/// Comparación de modelos de batería por costo por año de servicio normalizado (CU-12, US-11): agrupa las
/// fichas de vida útil cerradas por modelo. Con menos de dos modelos con ficha cerrada la comparación no es
/// concluyente (FA-1): se muestra igual, pero con el aviso de confianza baja.
/// </summary>
public sealed record ComparacionMarcas(
    CodigoInforme Codigo,
    IReadOnlyList<FilaModelo> Modelos,
    bool Concluyente,
    string? Aviso);
