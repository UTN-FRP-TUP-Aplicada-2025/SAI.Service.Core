using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Intervenciones;

/// <summary>Un tramo de la línea de tiempo de un host: cubierto por un SAI, o sin protección (hueco).</summary>
/// <param name="Desde">Inicio del tramo.</param>
/// <param name="Hasta">Fin del tramo, o <c>null</c> si sigue abierto.</param>
/// <param name="DispositivoCodigo">SAI que cubre, o <c>null</c> si es un hueco sin protección.</param>
public sealed record TramoProteccion(DateTimeOffset Desde, DateTimeOffset? Hasta, string? DispositivoCodigo)
{
    /// <summary>Verdadero si el host quedó sin protección en este tramo.</summary>
    public bool SinProteccion => DispositivoCodigo is null;
}

/// <summary>
/// Deriva la <b>sucesión de coberturas</b> de un host y los <b>días sin protección</b> (CU-09 §4.6): a
/// partir de las coberturas (vínculos temporales), reconstruye la línea de tiempo intercalando los huecos
/// —tramos legítimos en que ningún SAI protegió al host (RC-03)—. Es una función pura y testeable; no
/// consulta nada.
/// </summary>
public static class CalculadorProteccion
{
    /// <summary>
    /// Línea de tiempo del host: las coberturas ordenadas por inicio, con los huecos entre ellas como
    /// tramos «sin protección». Los solapamientos no se esperan (RC-02); si los hubiera, no se inventan
    /// huecos negativos.
    /// </summary>
    public static IReadOnlyList<TramoProteccion> Sucesion(IReadOnlyList<CoberturaHost> coberturas)
    {
        var ordenadas = coberturas.OrderBy(c => c.Vigencia.Desde).ToList();
        var tramos = new List<TramoProteccion>();
        DateTimeOffset? finAnterior = null;

        foreach (var cobertura in ordenadas)
        {
            if (finAnterior is { } fin && cobertura.Vigencia.Desde > fin)
            {
                tramos.Add(new TramoProteccion(fin, cobertura.Vigencia.Desde, null));
            }

            tramos.Add(new TramoProteccion(cobertura.Vigencia.Desde, cobertura.Vigencia.Hasta, cobertura.DispositivoCodigo));
            // El fin abierto (null) domina: una vez que hay una cobertura vigente, no hay hueco posterior.
            finAnterior = cobertura.Vigencia.Hasta is null || finAnterior is null
                ? cobertura.Vigencia.Hasta
                : (cobertura.Vigencia.Hasta > finAnterior ? cobertura.Vigencia.Hasta : finAnterior);
        }

        return tramos;
    }

    /// <summary>
    /// Días totales sin protección hasta <paramref name="hasta"/>: la suma de los huecos entre coberturas.
    /// Un hueco final abierto (host descubierto ahora) se cuenta hasta <paramref name="hasta"/>.
    /// </summary>
    public static int DiasSinProteccion(IReadOnlyList<CoberturaHost> coberturas, DateTimeOffset hasta)
    {
        var ordenadas = coberturas.OrderBy(c => c.Vigencia.Desde).ToList();
        if (ordenadas.Count == 0)
        {
            return 0;
        }

        double dias = 0;
        DateTimeOffset finAnterior = ordenadas[0].Vigencia.Hasta ?? hasta;
        var hayVigente = ordenadas[0].Vigencia.Hasta is null;

        for (var i = 1; i < ordenadas.Count; i++)
        {
            var c = ordenadas[i];
            if (c.Vigencia.Desde > finAnterior)
            {
                dias += (c.Vigencia.Desde - finAnterior).TotalDays;
            }

            if (c.Vigencia.Hasta is null)
            {
                hayVigente = true;
            }
            else if (c.Vigencia.Hasta.Value > finAnterior)
            {
                finAnterior = c.Vigencia.Hasta.Value;
            }
        }

        // Si el último tramo no dejó una cobertura vigente, el host está descubierto hasta 'hasta'.
        if (!hayVigente && finAnterior < hasta)
        {
            dias += (hasta - finAnterior).TotalDays;
        }

        return (int)Math.Round(dias);
    }
}
