using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Informes;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración del informe de período (CU-12, EP-07): valida las consultas por período del repositorio EF
/// de extremo a extremo. Tras el alta de un equipo (que abre la cobertura del host y el montaje de la
/// batería), el informe de un período que los contiene reporta el dispositivo activo y la batería
/// interviniente; un período sin actividad devuelve PERIODO_SIN_DATOS; la comparación sin fichas avisa.
/// </summary>
public class InformesIntegracionTests
{
    private static readonly DateTimeOffset Alta = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));
    private static readonly DateTimeOffset IniAnio = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FinAnio = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ElInformeReportaLaCoberturaYLaBateriaTrasElAlta()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        var informe = await sp.GetRequiredService<ServicioInformePeriodo>().ArmarInformeAsync(IniAnio, FinAnio, CancellationToken.None);

        informe.Codigo.Should().Be(CodigoInforme.Ok);
        informe.Cobertura!.DispositivosActivos.Should().Contain("ups");
        informe.Cobertura.BateriasIntervinientes.Should().ContainSingle(b => b.BateriaCodigo == "bat");
        informe.Cobertura.DiasConProteccion.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UnPeriodoSinActividadDevuelvePeriodoSinDatos()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        // Período muy anterior al alta: sin coberturas, montajes, intervenciones ni muestras.
        var desde = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var informe = await sp.GetRequiredService<ServicioInformePeriodo>().ArmarInformeAsync(desde, hasta, CancellationToken.None);

        informe.Codigo.Should().Be(CodigoInforme.PeriodoSinDatos);
    }

    [Fact]
    public async Task LaComparacionSinFichasAvisa()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        var comparacion = await sp.GetRequiredService<ServicioInformePeriodo>().CompararMarcasAsync(CancellationToken.None);

        comparacion.Codigo.Should().Be(CodigoInforme.PeriodoSinDatos);
        comparacion.Concluyente.Should().BeFalse();
        comparacion.Modelos.Should().BeEmpty();
    }

    private static SolicitudAltaEquipos SolicitudValida() => new(
        Alta,
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");
}
