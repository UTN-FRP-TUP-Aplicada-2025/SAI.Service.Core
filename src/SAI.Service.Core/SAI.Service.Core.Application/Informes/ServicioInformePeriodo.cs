using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Informes;

/// <summary>
/// Arma el informe de período y la comparación de marcas (CU-12, US-11) intersecando historia ya
/// registrada: no muta nada. La sección de cobertura recorta los intervalos al período (días con y sin
/// protección, baterías intervinientes incluidas las dadas de baja, RN-12); la de costos agrega por tipo
/// con moneda y fecha (RN-07); la de eventos y calidad reutiliza <see cref="ServicioHistoricos"/> —el conteo
/// de microcortes sale de los eventos, y la calidad sobre agregados viaja con su cobertura y advertencia
/// (RN-10)—. Un período sin actividad no arma un informe engañoso (<see cref="CodigoInforme.PeriodoSinDatos"/>).
/// </summary>
public sealed class ServicioInformePeriodo(IRepositorioInformes repositorio, ServicioHistoricos historicos)
{
    /// <summary>Arma el informe del período <c>[desde, hasta)</c> para el host del sistema.</summary>
    public async Task<InformePeriodo> ArmarInformeAsync(DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct)
    {
        var host = await repositorio.HostAsync(ct);
        if (host is null)
        {
            return new InformePeriodo(CodigoInforme.PeriodoSinDatos, desde, hasta, null, null, null);
        }

        var coberturas = await repositorio.CoberturasDeHostAsync(host.Codigo, ct);
        var intervenciones = await repositorio.IntervencionesPorPeriodoAsync(desde, hasta, ct);
        var sustituciones = await repositorio.SustitucionesPorPeriodoAsync(host.Codigo, desde, hasta, ct);

        // --- Cobertura: recorte de intervalos al período ---
        var coberturasEnPeriodo = coberturas.Where(c => c.Vigencia.Intersecar(desde, hasta) is not null).ToList();
        var dispositivos = coberturasEnPeriodo.Select(c => c.DispositivoCodigo).Distinct().ToList();

        var totalPeriodo = (hasta - desde).TotalDays;
        var diasConRaw = coberturas.Sum(c => c.Vigencia.DiasEnPeriodo(desde, hasta));
        var diasCon = (int)Math.Round(Math.Min(diasConRaw, totalPeriodo));
        var diasSin = (int)Math.Round(Math.Max(0, totalPeriodo - diasConRaw));

        var montajes = await repositorio.MontajesDeDispositivosAsync(dispositivos, ct);
        var baterias = montajes
            .Select(m => (m, recorte: m.Vigencia.Intersecar(desde, hasta)))
            .Where(x => x.recorte is not null)
            .Select(x => new BateriaInterviniente(
                x.m.BateriaCodigo, x.recorte!.Value.Desde, x.recorte.Value.Hasta!.Value,
                (int)Math.Round(x.m.Vigencia.DiasEnPeriodo(desde, hasta))))
            .OrderBy(b => b.Desde)
            .ToList();

        var cobertura = new SeccionCobertura(dispositivos, diasCon, diasSin, baterias);

        // --- Intervenciones y costos por tipo (RN-07: moneda y fecha) ---
        var porTipo = new List<CostoPorTipo>();
        if (intervenciones.Count > 0)
        {
            porTipo.Add(new CostoPorTipo("Recambio de batería", intervenciones.Count,
                Sumar(intervenciones.Select(i => (Dinero?)i.Total))));
        }

        foreach (var grupo in sustituciones.GroupBy(s => s.Tipo))
        {
            porTipo.Add(new CostoPorTipo(EtiquetaSustitucion(grupo.Key), grupo.Count(),
                Sumar(grupo.Select(s => s.Costo))));
        }

        var costos = new SeccionCostos(porTipo);

        // --- Eventos y calidad de suministro (reusa ServicioHistoricos: microcortes desde eventos, RN-10) ---
        var historico = await historicos.ConsultarAsync([Variables.TensionEntrada], desde, hasta, resolucionForzada: null, ct);
        if (historico.Codigo == CodigoResultadoHistorico.AgregadoSinCobertura)
        {
            return new InformePeriodo(CodigoInforme.AgregadoSinCobertura, desde, hasta, cobertura, costos, null);
        }

        var eventosPorTipo = historico.Marcas
            .GroupBy(m => m.Tipo)
            .Select(g => new EventoPorTipo(g.Key, g.Count()))
            .OrderBy(e => e.Tipo)
            .ToList();
        var serieAgregada = historico.Series.FirstOrDefault(s => s.Resolucion == ResolucionSerie.Agregados);
        var eventosCalidad = new SeccionEventosCalidad(
            historico.ConteoMicrocortes, eventosPorTipo,
            serieAgregada?.Cobertura, serieAgregada?.Advertencia, serieAgregada is not null);

        // Sin ninguna actividad en el período no se arma un informe engañoso (CU-12).
        var hayActividad = coberturasEnPeriodo.Count > 0 || intervenciones.Count > 0
            || sustituciones.Count > 0 || historico.Codigo != CodigoResultadoHistorico.PeriodoSinDatos;
        if (!hayActividad)
        {
            return new InformePeriodo(CodigoInforme.PeriodoSinDatos, desde, hasta, null, null, null);
        }

        return new InformePeriodo(CodigoInforme.Ok, desde, hasta, cobertura, costos, eventosCalidad);
    }

    /// <summary>Compara los modelos de batería por costo por año de servicio normalizado a USD (US-11).</summary>
    public async Task<ComparacionMarcas> CompararMarcasAsync(CancellationToken ct)
    {
        var fichas = await repositorio.FichasCerradasConModeloAsync(ct);
        if (fichas.Count == 0)
        {
            return new ComparacionMarcas(CodigoInforme.PeriodoSinDatos, [], false,
                "No hay fichas de vida útil cerradas para comparar.");
        }

        var modelos = fichas
            .GroupBy(f => f.ModeloCodigo)
            .Select(g =>
            {
                var reciente = g.OrderByDescending(f => f.Ficha.CostoPorAnioServicioUsd.Fecha).First();
                var montoPromedio = g.Average(f => f.Ficha.CostoPorAnioServicioUsd.Monto);
                var cumplio = g.Count(f => f.Ficha.CumplioExpectativa) * 2 >= g.Count(); // mayoría
                var desvio = (int)Math.Round(g.Average(f => f.Ficha.DesvioDias));
                return new FilaModelo(
                    g.Key, reciente.ModeloNombre, reciente.Fabricante, g.Count(),
                    new Dinero(Math.Round(montoPromedio, 2), reciente.Ficha.CostoPorAnioServicioUsd.Moneda, reciente.Ficha.CostoPorAnioServicioUsd.Fecha),
                    reciente.Ficha.FuenteCotizacion, cumplio, desvio);
            })
            .OrderBy(m => m.CostoPorAnioUsd.Monto)
            .ToList();

        var concluyente = modelos.Count >= 2;
        var aviso = concluyente
            ? null
            : "La comparación necesita al menos dos modelos con ficha de vida útil cerrada para ser concluyente.";
        return new ComparacionMarcas(CodigoInforme.Ok, modelos, concluyente, aviso);
    }

    // Suma una lista de importes (misma moneda) conservando la moneda y la fecha más reciente (RN-07).
    // Devuelve null si no hay ninguno con costo (p. ej. sustituciones sin costo declarado).
    private static Dinero? Sumar(IEnumerable<Dinero?> importes)
    {
        var lista = importes.Where(d => d is not null).Select(d => d!.Value).ToList();
        if (lista.Count == 0)
        {
            return null;
        }

        var total = lista.Sum(d => d.Monto);
        var fecha = lista.Max(d => d.Fecha);
        return new Dinero(total, lista[0].Moneda, fecha);
    }

    private static string EtiquetaSustitucion(TipoIntervencionSai tipo) => tipo switch
    {
        TipoIntervencionSai.Reparacion => "Reparación del SAI",
        TipoIntervencionSai.Reemplazo => "Sustitución del SAI",
        _ => tipo.ToString(),
    };
}
