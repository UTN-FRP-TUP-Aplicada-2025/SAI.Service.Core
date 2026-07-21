using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Resultado del alta de un equipo (CU-02). En caso de éxito describe la modalidad efectiva
/// (siempre <see cref="Modalidad.SoloAlerta"/> al recién sembrar, US-05) y el conteo de supuestos
/// verificados; en caso de fallo trae un <see cref="CodigoError"/> de dominio (p. ej.
/// <c>VIDA_FLOTACION_SIN_TEMPERATURA</c>, <c>MONTAJE_SOLAPADO</c>).
/// </summary>
/// <param name="Exito">Verdadero si el alta se registró.</param>
/// <param name="CodigoError">Código de error de dominio si el alta falló; nulo si tuvo éxito.</param>
/// <param name="ModalidadEfectiva">Modalidad efectiva tras el alta.</param>
/// <param name="SupuestosVerificados">Cantidad de supuestos verificados (0 al recién sembrar).</param>
/// <param name="SupuestosTotales">Cantidad total de supuestos (4).</param>
public sealed record ResultadoAltaEquipos(
    bool Exito,
    string? CodigoError,
    Modalidad ModalidadEfectiva,
    int SupuestosVerificados,
    int SupuestosTotales)
{
    /// <summary>Crea un resultado de éxito con la modalidad y el conteo de supuestos.</summary>
    public static ResultadoAltaEquipos Ok(Modalidad modalidad, int verificados, int totales) =>
        new(true, null, modalidad, verificados, totales);

    /// <summary>Crea un resultado de fallo con su código de error de dominio.</summary>
    public static ResultadoAltaEquipos Error(string codigo) =>
        new(false, codigo, Modalidad.SoloAlerta, 0, EvaluadorModalidad.SupuestosRequeridos);
}
