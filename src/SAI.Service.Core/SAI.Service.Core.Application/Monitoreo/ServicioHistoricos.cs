using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Consulta histórica para el panel de gráficas (US-11, CU-06). Elige la fuente por la regla de
/// retención (muestras crudas dentro de 30 días, agregados horarios más atrás), adjunta la cobertura
/// y advertencia del agregado (I-20, RN-10), superpone las marcas de evento y cuenta los microcortes
/// desde los eventos, nunca desde la serie agregada (CL-16). Si no hay datos, devuelve
/// <see cref="CodigoResultadoHistorico.PeriodoSinDatos"/> (no dibuja una serie vacía). Es de solo lectura.
/// </summary>
public sealed class ServicioHistoricos(IRepositorioMonitoreo repositorio)
{
    /// <summary>Días de retención de las muestras crudas.</summary>
    public const int DiasRetencionMuestras = 30;

    /// <summary>Consulta las series de <paramref name="variables"/> en el período dado.</summary>
    public async Task<ResultadoHistorico> ConsultarAsync(
        IReadOnlyList<string> variables,
        DateTimeOffset desde,
        DateTimeOffset hasta,
        ResolucionSerie? resolucionForzada,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(variables);

        var dispositivo = await repositorio.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return SinDatos();
        }

        var ahora = DateTimeOffset.UtcNow;
        var resolucion = resolucionForzada
            ?? (desde >= ahora.AddDays(-DiasRetencionMuestras) ? ResolucionSerie.Muestras : ResolucionSerie.Agregados);

        var eventos = await repositorio.EventosPorPeriodoAsync(dispositivo.Codigo, desde, hasta, ct);
        var marcas = eventos
            .Select(e => new MarcaEvento(e.Tipo, e.Instante, e.DuracionSeg, e.IncertidumbreDuracionSeg, e.ReglaDerivacionCodigo, e.ReglaVersion))
            .ToList();
        var conteoMicrocortes = eventos.Count(e => e.Tipo == TipoEvento.Microcorte);

        var series = resolucion == ResolucionSerie.Muestras
            ? await SeriesDeMuestrasAsync(dispositivo.Codigo, variables, desde, hasta, ct)
            : await SeriesDeAgregadosAsync(dispositivo.Codigo, variables, desde, hasta, ct);

        var hayDatos = series.Any(s => s.Puntos.Any(p => p.Valor is not null)) || marcas.Count > 0;
        return hayDatos
            ? new ResultadoHistorico(CodigoResultadoHistorico.Ok, resolucion, series, marcas, conteoMicrocortes)
            : SinDatos();
    }

    private async Task<List<SerieVariable>> SeriesDeMuestrasAsync(
        string dispositivoCodigo, IReadOnlyList<string> variables, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct)
    {
        var muestras = await repositorio.MuestrasPorPeriodoAsync(dispositivoCodigo, desde, hasta, ct);
        return [.. variables.Select(v => new SerieVariable(
            v, ResolucionSerie.Muestras, Procedencia(v),
            [.. muestras.Select(m => new PuntoSerie(m.Instante, m.Valor(v), null, null))],
            Cobertura: null, Advertencia: null))];
    }

    private async Task<List<SerieVariable>> SeriesDeAgregadosAsync(
        string dispositivoCodigo, IReadOnlyList<string> variables, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct)
    {
        // Para la agregación on-demand cuando no hay agregados persistidos.
        var muestras = await repositorio.MuestrasPorPeriodoAsync(dispositivoCodigo, desde, hasta, ct);

        var series = new List<SerieVariable>(variables.Count);
        foreach (var v in variables)
        {
            var agregados = await repositorio.AgregadosPorPeriodoAsync(dispositivoCodigo, v, desde, hasta, ct);
            if (agregados.Count == 0)
            {
                agregados = AgregarPorHora(dispositivoCodigo, v, muestras);
            }

            var puntos = agregados.Select(a => new PuntoSerie(a.VentanaInicio, a.Promedio, a.Minimo, a.Maximo)).ToList();
            var cobertura = agregados.Count > 0 ? agregados.Average(a => a.Cobertura) : (double?)null;
            var advertencia = agregados.FirstOrDefault(a => a.Advertencia is not null)?.Advertencia;
            series.Add(new SerieVariable(v, ResolucionSerie.Agregados, Procedencia(v), puntos, cobertura, advertencia));
        }

        return series;
    }

    private static List<Agregado> AgregarPorHora(string dispositivoCodigo, string variable, IReadOnlyList<Muestra> muestras) =>
        [.. muestras
            .GroupBy(m => new DateTimeOffset(m.Instante.Year, m.Instante.Month, m.Instante.Day, m.Instante.Hour, 0, 0, m.Instante.Offset))
            .Select(g => CalculadorAgregado.Calcular(
                $"agr-{variable}-{g.Key:O}", dispositivoCodigo, variable, g.Key,
                [.. g.Select(m => m.Valor(variable))]))
            .Where(a => a is not null)
            .Select(a => a!)];

    private static Origen Procedencia(string variable) =>
        Variables.ProcedenciaCanonica.TryGetValue(variable, out var origen) ? origen : Origen.Medido;

    private static ResultadoHistorico SinDatos() =>
        new(CodigoResultadoHistorico.PeriodoSinDatos, ResolucionSerie.Muestras, [], [], 0);
}
