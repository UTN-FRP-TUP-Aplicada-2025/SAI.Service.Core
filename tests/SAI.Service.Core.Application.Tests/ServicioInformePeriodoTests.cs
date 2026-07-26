using FluentAssertions;
using SAI.Service.Core.Application.Informes;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using SAI.Service.Core.Domain.Vinculos;
using Xunit;

namespace SAI.Service.Core.Application.Tests;

/// <summary>
/// Informe de período y comparación de marcas (CU-12, US-11, EP-07). Interseca intervalos (cobertura,
/// baterías intervinientes), agrega costos por tipo con moneda y fecha (RN-07), incluye las bajas (RN-12) y
/// no arma informe ante un período sin datos. La comparación agrupa fichas cerradas por modelo y avisa
/// confianza baja con menos de dos modelos (FA-1).
/// </summary>
public class ServicioInformePeriodoTests
{
    private static readonly DateTimeOffset Ini = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fin = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Momento = new(2026, 9, 5, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Arranque = new(2024, 11, 20, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Fecha = new(2026, 9, 5);

    private static ServicioInformePeriodo Servicio(RepositorioInformesFalso repo) =>
        new(repo, new ServicioHistoricos(new RepositorioMonitoreoVacio()));

    // CA-01: baterías con intervalos recortados que suman 365 días sin solapamiento.
    [Fact]
    public async Task ElInformeRecortaLosIntervalosDeLasBateriasAlPeriodo()
    {
        var repo = new RepositorioInformesFalso
        {
            Coberturas = [Cobertura("cob-1", "ups-01", new Vigencia(Arranque, null))],
            Montajes =
            [
                Montaje("mnt-1", "bat-2024-a", "ups-01", new Vigencia(Arranque, Momento)),
                Montaje("mnt-2", "bat-2026-a", "ups-01", new Vigencia(Momento, null)),
            ],
        };

        var informe = await Servicio(repo).ArmarInformeAsync(Ini, Fin, default);

        informe.Codigo.Should().Be(CodigoInforme.Ok);
        var c = informe.Cobertura!;
        c.DispositivosActivos.Should().ContainSingle().Which.Should().Be("ups-01");
        c.DiasConProteccion.Should().Be(365);
        c.DiasSinProteccion.Should().Be(0);
        c.BateriasIntervinientes.Select(b => b.BateriaCodigo).Should().Equal("bat-2024-a", "bat-2026-a");
        c.BateriasIntervinientes.Sum(b => b.Dias).Should().Be(365);
    }

    // CA-02: una batería dada de baja que intervino aparece igual en el informe (RN-12).
    [Fact]
    public async Task ElInformeIncluyeLasBateriasDadasDeBaja()
    {
        // El servicio no filtra por estado: el montaje de la batería (aunque esté dada de baja) se lista.
        var repo = new RepositorioInformesFalso
        {
            Coberturas = [Cobertura("cob-1", "ups-01", new Vigencia(Ini, null))],
            Montajes = [Montaje("mnt-1", "bat-2024-a", "ups-01", new Vigencia(Arranque, Momento))],
        };

        var informe = await Servicio(repo).ArmarInformeAsync(Ini, Fin, default);

        informe.Cobertura!.BateriasIntervinientes.Should().ContainSingle(b => b.BateriaCodigo == "bat-2024-a");
    }

    // FA-2: un tramo sin cobertura se reporta como días sin protección.
    [Fact]
    public async Task ElInformeReportaLosDiasSinProteccionDelHueco()
    {
        var repo = new RepositorioInformesFalso
        {
            Coberturas =
            [
                Cobertura("cob-1", "ups-01", new Vigencia(Ini, new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero))),
                Cobertura("cob-2", "ups-02", new Vigencia(new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero), null)),
            ],
        };

        var informe = await Servicio(repo).ArmarInformeAsync(Ini, Fin, default);

        // Hueco de junio y julio (61 días) sin protección.
        informe.Cobertura!.DiasSinProteccion.Should().Be(61);
        informe.Cobertura.DispositivosActivos.Should().Equal("ups-01", "ups-02");
    }

    // Intervenciones y costos por tipo: recambios sumados y sustituciones con/sin costo.
    [Fact]
    public async Task ElInformeAgregaLosCostosPorTipoConMonedaYFecha()
    {
        var repo = new RepositorioInformesFalso
        {
            Coberturas = [Cobertura("cob-1", "ups-01", new Vigencia(Ini, null))],
            Intervenciones = [Recambio("int-1", 30000m), Recambio("int-2", 20000m)],
            Sustituciones =
            [
                Sustitucion("sus-1", TipoIntervencionSai.Reparacion, 15000m),
                Sustitucion("sus-2", TipoIntervencionSai.Reemplazo, null),
            ],
        };

        var informe = await Servicio(repo).ArmarInformeAsync(Ini, Fin, default);

        var porTipo = informe.Costos!.PorTipo;
        var recambios = porTipo.Single(t => t.Tipo == "Recambio de batería");
        recambios.Cantidad.Should().Be(2);
        recambios.Total!.Value.Monto.Should().Be(50000m);
        recambios.Total.Value.Moneda.Should().Be("ARS");

        porTipo.Single(t => t.Tipo == "Reparación del SAI").Total!.Value.Monto.Should().Be(15000m);
        porTipo.Single(t => t.Tipo == "Sustitución del SAI").Total.Should().BeNull("esa sustitución no llevó costo");
    }

    // PERIODO_SIN_DATOS: sin actividad no se arma un informe engañoso.
    [Fact]
    public async Task SinActividadDevuelvePeriodoSinDatos()
    {
        var informe = await Servicio(new RepositorioInformesFalso()).ArmarInformeAsync(Ini, Fin, default);

        informe.Codigo.Should().Be(CodigoInforme.PeriodoSinDatos);
        informe.Cobertura.Should().BeNull();
    }

    // CA-04: el costo por año de servicio se muestra normalizado a USD, marcado como derivado con su fuente.
    [Fact]
    public async Task LaComparacionMuestraElCostoPorAnioNormalizadoAUsdDerivado()
    {
        var repo = new RepositorioInformesFalso
        {
            Fichas =
            [
                new FichaConModelo(Ficha("fic-1", "bat-2024-a", 37430m, 0.000788m), "mod-a", "Modelo A", "INNO TECH"),
                new FichaConModelo(Ficha("fic-2", "bat-2020-x", 52000m, 0.000788m), "mod-b", "Modelo B", "Otra Marca"),
            ],
        };

        var comparacion = await Servicio(repo).CompararMarcasAsync(default);

        comparacion.Codigo.Should().Be(CodigoInforme.Ok);
        comparacion.Concluyente.Should().BeTrue("hay dos modelos con ficha cerrada");
        comparacion.Modelos.Should().HaveCount(2);
        var modeloA = comparacion.Modelos.Single(m => m.ModeloCodigo == "mod-a");
        modeloA.CostoPorAnioUsd.Moneda.Should().Be("USD");
        modeloA.CostoPorAnioUsd.Monto.Should().BeApproximately(29.5m, 0.6m);
        modeloA.FuenteCotizacion.Should().NotBeNullOrWhiteSpace();
        // Orden ascendente por costo: el más barato primero.
        comparacion.Modelos[0].CostoPorAnioUsd.Monto.Should().BeLessThanOrEqualTo(comparacion.Modelos[1].CostoPorAnioUsd.Monto);
    }

    // FA-1: con un solo modelo con ficha cerrada, la comparación avisa confianza baja.
    [Fact]
    public async Task LaComparacionConUnSoloModeloAvisaConfianzaBaja()
    {
        var repo = new RepositorioInformesFalso
        {
            Fichas = [new FichaConModelo(Ficha("fic-1", "bat-2024-a", 37430m, 0.000788m), "mod-a", "Modelo A", "INNO TECH")],
        };

        var comparacion = await Servicio(repo).CompararMarcasAsync(default);

        comparacion.Modelos.Should().ContainSingle();
        comparacion.Concluyente.Should().BeFalse();
        comparacion.Aviso.Should().Contain("al menos dos modelos");
    }

    // --- Helpers de construcción ---

    private static CoberturaHost Cobertura(string codigo, string dispositivo, Vigencia vigencia) =>
        new(codigo, dispositivo, "host-i7infra", vigencia);

    private static MontajeBateria Montaje(string codigo, string bateria, string dispositivo, Vigencia vigencia) =>
        new(codigo, bateria, dispositivo, "principal", vigencia);

    private static Intervencion Recambio(string codigo, decimal total)
    {
        var repuestos = new Dinero(total, "ARS", Fecha);
        var cero = new Dinero(0m, "ARS", Fecha);
        var costos = new Costos(repuestos, cero, new Dinero(total, "ARS", Fecha));
        return new Intervencion(codigo, "ups-01", "principal", "bat-vieja", "bat-nueva", Momento, Momento,
            "prov", "ejec", costos, "ok", new DisposicionFinal("reciclado", "gestor"));
    }

    private static SustitucionSai Sustitucion(string codigo, TipoIntervencionSai tipo, decimal? costo) =>
        new(codigo, "host-i7infra", "ups-01", tipo == TipoIntervencionSai.Reemplazo ? "ups-02" : null,
            tipo, Momento, Momento, "prov", "ejec", "hallazgos", firmwareReiniciado: false,
            costo is { } m ? new Dinero(m, "ARS", Fecha) : null, disposicion: null);

    private static FichaVidaUtil Ficha(string codigo, string bateria, decimal costoAnualArs, decimal tasa) =>
        FichaVidaUtil.Proyectar(codigo, "int-x", "ups-01", bateria,
            new DateTimeOffset(2025, 9, 5, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero),
            vidaEsperadaDias: 1095, new Dinero(costoAnualArs, "ARS", Fecha), tasa, "BNA 2026-09-05");

    // Repositorio de informes en memoria.
    private sealed class RepositorioInformesFalso : IRepositorioInformes
    {
        public IReadOnlyList<CoberturaHost> Coberturas { get; init; } = [];
        public IReadOnlyList<MontajeBateria> Montajes { get; init; } = [];
        public IReadOnlyList<Intervencion> Intervenciones { get; init; } = [];
        public IReadOnlyList<SustitucionSai> Sustituciones { get; init; } = [];
        public IReadOnlyList<FichaConModelo> Fichas { get; init; } = [];

        public Task<Host?> HostAsync(CancellationToken ct) => Task.FromResult<Host?>(new Host("host-i7infra"));
        public Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct) => Task.FromResult(Coberturas);
        public Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivosAsync(IReadOnlyList<string> dispositivoCodigos, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<MontajeBateria>>(Montajes.Where(m => dispositivoCodigos.Contains(m.DispositivoCodigo)).ToList());
        public Task<IReadOnlyList<Intervencion>> IntervencionesPorPeriodoAsync(DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) => Task.FromResult(Intervenciones);
        public Task<IReadOnlyList<SustitucionSai>> SustitucionesPorPeriodoAsync(string hostCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) => Task.FromResult(Sustituciones);
        public Task<IReadOnlyList<FichaConModelo>> FichasCerradasConModeloAsync(CancellationToken ct) => Task.FromResult(Fichas);
    }

    // Repositorio de monitoreo vacío: sin dispositivo en servicio, ServicioHistoricos devuelve PeriodoSinDatos.
    private sealed class RepositorioMonitoreoVacio : IRepositorioMonitoreo
    {
        public Task<Dispositivo?> DispositivoEnServicioAsync(CancellationToken ct) => Task.FromResult<Dispositivo?>(null);
        public Task<SesionSondeo?> SesionActivaDeAsync(string dispositivoCodigo, CancellationToken ct) => Task.FromResult<SesionSondeo?>(null);
        public Task<bool> ExisteFuenteAsync(string codigo, CancellationToken ct) => Task.FromResult(false);
        public Task GuardarSesionAsync(FuenteDatos? nuevaFuente, SesionSondeo sesion, CancellationToken ct) => Task.CompletedTask;
        public Task GuardarMuestraAsync(Muestra muestra, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<Muestra>> MuestrasRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) => Task.FromResult<IReadOnlyList<Muestra>>([]);
        public Task<IReadOnlyDictionary<string, ReglaDerivacion>> ReglasVigentesAsync(DateTimeOffset instante, CancellationToken ct) => Task.FromResult<IReadOnlyDictionary<string, ReglaDerivacion>>(new Dictionary<string, ReglaDerivacion>());
        public Task GuardarEventosAsync(IReadOnlyList<Evento> eventos, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<Evento>> EventosRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) => Task.FromResult<IReadOnlyList<Evento>>([]);
        public Task<IReadOnlyList<Muestra>> MuestrasPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) => Task.FromResult<IReadOnlyList<Muestra>>([]);
        public Task<IReadOnlyList<Agregado>> AgregadosPorPeriodoAsync(string dispositivoCodigo, string variable, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) => Task.FromResult<IReadOnlyList<Agregado>>([]);
        public Task<IReadOnlyList<Evento>> EventosPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct) => Task.FromResult<IReadOnlyList<Evento>>([]);
        public Task<MontajeBateria?> MontajeVigenteAsync(string dispositivoCodigo, CancellationToken ct) => Task.FromResult<MontajeBateria?>(null);
        public Task<Evento?> UltimoCorteAsync(string dispositivoCodigo, CancellationToken ct) => Task.FromResult<Evento?>(null);
        public Task<IReadOnlyList<PruebaBateria>> PruebasDeMontajeAsync(string montajeCodigo, CancellationToken ct) => Task.FromResult<IReadOnlyList<PruebaBateria>>([]);
        public Task<IReadOnlyList<PruebaBateria>> PruebasDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) => Task.FromResult<IReadOnlyList<PruebaBateria>>([]);
        public Task GuardarPruebaConSerieAsync(PruebaBateria prueba, IReadOnlyList<Muestra> serie, SesionSondeo sesionDensa, FuenteDatos? nuevaFuente, CancellationToken ct) => Task.CompletedTask;
        public Task GuardarAccionAsync(Accion accion, CancellationToken ct) => Task.CompletedTask;
        public Task<IReadOnlyList<Accion>> AccionesRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct) => Task.FromResult<IReadOnlyList<Accion>>([]);
    }
}
