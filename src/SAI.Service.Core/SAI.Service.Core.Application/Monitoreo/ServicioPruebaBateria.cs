using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>Resultado de ejecutar una prueba de batería.</summary>
/// <param name="Exito">Verdadero si la prueba se ejecutó y persistió.</param>
/// <param name="CodigoError">Código de error de dominio si falló; nulo si tuvo éxito.</param>
/// <param name="Prueba">La prueba registrada, si tuvo éxito.</param>
/// <param name="MuestrasTomadas">Cantidad de muestras densas recogidas.</param>
/// <param name="MuestrasPerdidas">Cantidad de muestras perdidas en la conmutación (no interpoladas, CL-13).</param>
public sealed record ResultadoPrueba(
    bool Exito,
    string? CodigoError,
    PruebaBateria? Prueba,
    int MuestrasTomadas,
    int MuestrasPerdidas)
{
    /// <summary>Crea un resultado de fallo con su código de error.</summary>
    public static ResultadoPrueba Error(string codigo) => new(false, codigo, null, 0, 0);
}

/// <summary>
/// Orquesta una prueba de batería (CU-07, US-12): valida la precondición de flotación, congela el
/// montaje vigente (I-15), lanza el autotest por el adaptador, recoge la serie densa (1 Hz) de
/// tensión, calcula el veredicto de salud con <see cref="CalculadorSaludBateria"/> y persiste la
/// prueba. Las muestras perdidas en la conmutación se registran y no se interpolan (CL-13). El
/// lanzamiento real por NUT está diferido: si el adaptador no acepta el comando, la prueba no corre.
/// </summary>
public sealed class ServicioPruebaBateria(
    IRepositorioMonitoreo repositorio,
    IAdaptadorConexion adaptador,
    OpcionesPrueba opciones)
{
    private static readonly string[] VariablesEsperadas = [.. Variables.ProcedenciaCanonica.Keys];

    /// <summary>Ejecuta una prueba de batería manual sobre el dispositivo en servicio.</summary>
    public async Task<ResultadoPrueba> EjecutarAsync(CancellationToken ct)
    {
        var dispositivo = await repositorio.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return ResultadoPrueba.Error("SIN_DISPOSITIVO");
        }

        var ahora = DateTimeOffset.UtcNow;

        // Precondición: tiempo mínimo en flotación tras el último corte (CL-25, CA-05).
        var ultimoCorte = await repositorio.UltimoCorteAsync(dispositivo.Codigo, ct);
        if (ultimoCorte is not null && (ahora - ultimoCorte.Instante).TotalSeconds < opciones.FlotacionMinimaSeg)
        {
            return ResultadoPrueba.Error("FLOTACION_INSUFICIENTE");
        }

        // Congela el montaje vigente (I-15, RC-07).
        var montaje = await repositorio.MontajeVigenteAsync(dispositivo.Codigo, ct);
        if (montaje is null)
        {
            return ResultadoPrueba.Error("SIN_BATERIA");
        }

        // Lanza el autotest. Si el adaptador no lo acepta (NUT diferido), la prueba no corre.
        var accion = await adaptador.LanzarTestBateriaAsync(ct);
        if (!accion.Aceptada)
        {
            return ResultadoPrueba.Error("TEST_NO_DISPONIBLE");
        }

        var (serie, sesionDensa, nuevaFuente) = await RecogerSerieAsync(dispositivo.Codigo, ahora, ct);

        // Línea base y comparables previas del montaje (para el veredicto y la confianza).
        var pruebasPrevias = await repositorio.PruebasDeMontajeAsync(montaje.Codigo, ct);
        var comparablesPrevias = pruebasPrevias.Count(p => p.Comparable);
        var lineaBase = pruebasPrevias.FirstOrDefault(p => p.Comparable);

        var resultado = CalculadorSaludBateria.Evaluar(
            serie,
            cargaConcurrente: CargaDeLaSerie(serie),
            caidaLineaBase: lineaBase?.CaidaTension.Contenido,
            cargaLineaBase: lineaBase?.CargaPorcentaje,
            toleranciaCargaPct: opciones.ToleranciaCargaPct,
            comparablesPrevias: comparablesPrevias);

        var prueba = PruebaBateria.Registrar($"prb-{Guid.NewGuid():N}", dispositivo.Codigo, montaje.Codigo, ahora, resultado);
        await repositorio.GuardarPruebaConSerieAsync(prueba, serie, sesionDensa, nuevaFuente, ct);

        return new ResultadoPrueba(true, null, prueba, serie.Count, serie.Count(m => m.Calidad == CalidadMuestra.Perdida));
    }

    /// <summary>Historial de las últimas pruebas del dispositivo en servicio (para el panel).</summary>
    public async Task<IReadOnlyList<PruebaBateria>> HistorialAsync(CancellationToken ct)
    {
        var dispositivo = await repositorio.DispositivoEnServicioAsync(ct);
        return dispositivo is null ? [] : await repositorio.PruebasDeDispositivoAsync(dispositivo.Codigo, 10, ct);
    }

    private async Task<(List<Muestra> Serie, SesionSondeo SesionDensa, FuenteDatos? NuevaFuente)> RecogerSerieAsync(
        string dispositivoCodigo, DateTimeOffset ahora, CancellationToken ct)
    {
        var fuenteExiste = await repositorio.ExisteFuenteAsync(ServicioMonitoreo.CodigoFuenteNut, ct);
        var nuevaFuente = fuenteExiste
            ? null
            : new FuenteDatos(ServicioMonitoreo.CodigoFuenteNut, ConfianzaFuente.Media, "Herramienta de acceso (NUT)");

        var finVentana = ahora.AddMilliseconds((opciones.NumeroMuestras + 1) * Math.Max(1, opciones.IntervaloMuestraMs));
        var sesionDensa = new SesionSondeo(
            $"ses-prueba-{Guid.NewGuid():N}", dispositivoCodigo, ServicioMonitoreo.CodigoFuenteNut,
            driver: "nut", intervaloSeg: 1, vigencia: new Vigencia(ahora, finVentana),
            mapaVariableOrigen: Variables.ProcedenciaCanonica);

        var serie = new List<Muestra>(opciones.NumeroMuestras);
        for (var i = 0; i < opciones.NumeroMuestras; i++)
        {
            var estado = await adaptador.LeerEstadoAsync(ct);
            serie.Add(Muestra.Registrar(
                $"mue-p-{Guid.NewGuid():N}", dispositivoCodigo, sesionDensa.Codigo,
                estado.MarcaTiempoUtc, estado.Alcanzable, MapeoLecturas.DesdeEstado(estado), VariablesEsperadas));

            if (i < opciones.NumeroMuestras - 1 && opciones.IntervaloMuestraMs > 0)
            {
                await Task.Delay(opciones.IntervaloMuestraMs, ct);
            }
        }

        return (serie, sesionDensa, nuevaFuente);
    }

    private static int? CargaDeLaSerie(IReadOnlyList<Muestra> serie)
    {
        var carga = serie
            .Select(m => m.Valor(Variables.CargaSalida))
            .LastOrDefault(v => v.HasValue);
        return carga is { } c ? (int)Math.Round(c) : null;
    }
}
