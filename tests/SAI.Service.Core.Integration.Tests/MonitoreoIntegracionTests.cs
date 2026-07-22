using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración del sondeo (US-08, US-10): una ronda contra el adaptador simulado persiste una
/// muestra con su calidad y abre la sesión de sondeo con la procedencia por variable; las muestras
/// son append-only. El planificador (hosted service) está deshabilitado en pruebas; se ejercita el
/// orquestador de sondeo directamente.
/// </summary>
public class MonitoreoIntegracionTests
{
    private static readonly DateTimeOffset Instante = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public async Task SondearPersisteUnaMuestraYAbreLaSesionConLaProcedencia()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        var resultado = await sp.GetRequiredService<ServicioMonitoreo>().SondearAsync(5, CancellationToken.None);

        resultado.Should().Be(ResultadoSondeo.Registrada);
        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.Muestras.CountAsync()).Should().Be(1);

        var sesion = await db.SesionesSondeo.SingleAsync();
        sesion.MapaVariableOrigen[Variables.CargaBateria].Should().Be(Origen.Derivado, "US-10: la carga de batería es derivada");
        sesion.MapaVariableOrigen[Variables.TensionEntrada].Should().Be(Origen.Medido);

        var muestra = await db.Muestras.SingleAsync();
        muestra.Calidad.Should().Be(CalidadMuestra.Completa, "el adaptador simulado devuelve todas las variables");
        muestra.Valor(Variables.CargaBateria).Should().Be(100);
    }

    [Fact]
    public async Task RondasSucesivasReusanLaMismaSesion()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        var monitoreo = sp.GetRequiredService<ServicioMonitoreo>();

        await monitoreo.SondearAsync(5, CancellationToken.None);
        await monitoreo.SondearAsync(5, CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.SesionesSondeo.CountAsync()).Should().Be(1, "las rondas reusan la sesión activa");
        (await db.Muestras.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task SinDispositivoEnServicioNoSondea()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var resultado = await sp.GetRequiredService<ServicioMonitoreo>().SondearAsync(5, CancellationToken.None);

        resultado.Should().Be(ResultadoSondeo.SinDispositivo);
        (await sp.GetRequiredService<SaiDbContext>().Muestras.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LaMuestraEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        await sp.GetRequiredService<ServicioMonitoreo>().SondearAsync(5, CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        db.Muestras.Remove(await db.Muestras.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("las muestras son historia append-only (ADR-04)");
    }

    [Fact]
    public async Task ElPanelEnVivoDevuelveLaUltimaMuestraConProcedencia()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        await sp.GetRequiredService<ServicioMonitoreo>().SondearAsync(5, CancellationToken.None);

        var vivo = await sp.GetRequiredService<ServicioPanelEnVivo>().ObtenerAsync(CancellationToken.None);

        vivo.HayDispositivo.Should().BeTrue();
        vivo.Conectado.Should().BeTrue();
        vivo.CalidadUltima.Should().Be(CalidadMuestra.Completa);
        vivo.Lecturas.Should().Contain(l => l.Variable == Variables.CargaBateria && l.Origen == Origen.Derivado,
            "US-10: la carga de batería se muestra derivada");
        vivo.Lecturas.Should().Contain(l => l.Variable == Variables.TensionEntrada && l.Origen == Origen.Medido);
    }

    [Fact]
    public async Task LasReglasDeDerivacionSeSembranAlArranque()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();

        (await db.Reglas.CountAsync()).Should().Be(4, "corte, tensión, desconexión y disparo (BT-19)");
    }

    [Fact]
    public async Task ElSondeoConEstadoEstableNoGeneraEventos()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        var monitoreo = sp.GetRequiredService<ServicioMonitoreo>();

        await monitoreo.SondearAsync(5, CancellationToken.None);
        await monitoreo.SondearAsync(5, CancellationToken.None);

        // El adaptador simulado siempre está en línea y en rango: no hay eventos.
        (await sp.GetRequiredService<SaiDbContext>().Eventos.CountAsync()).Should().Be(0);
    }

    private static SolicitudAltaEquipos SolicitudValida() => new(
        Instante,
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");
}
