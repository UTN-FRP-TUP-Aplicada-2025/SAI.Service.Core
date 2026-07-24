using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Ejercicio guiado (P-7): acompaña las cuatro pruebas como un solo ejercicio. La sesión se persiste
/// (no vive en el circuito Blazor), el paso se deriva de las verificaciones —única verdad— y salir del
/// ejercicio no pierde nada de lo ya verificado.
/// </summary>
public class EjercicioGuiadoIntegracionTests
{
    [Fact]
    public async Task IniciarAbreUnaSesionYElPasoEsElPrimeroDeLaSecuencia()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var ejercicio = sp.GetRequiredService<ServicioEjercicioGuiado>();

        var estado = await ejercicio.IniciarAsync(CancellationToken.None);

        estado.HayEjercicio.Should().BeTrue();
        estado.PasoActual.Should().Be(Supuesto.SenalEnBateria);
        estado.NumeroPaso.Should().Be(1);
        estado.Total.Should().Be(4);
    }

    [Fact]
    public async Task NoSeAbrenDosSesionesEnParalelo()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var ejercicio = sp.GetRequiredService<ServicioEjercicioGuiado>();

        var primera = await ejercicio.IniciarAsync(CancellationToken.None);
        var segunda = await ejercicio.IniciarAsync(CancellationToken.None);

        segunda.Sesion!.Codigo.Should().Be(primera.Sesion!.Codigo, "iniciar es idempotente");
    }

    [Fact]
    public async Task ElPasoAvanzaAlVerificarSinTocarLaSesion()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<SAI.Service.Core.Infrastructure.Adaptadores.AdaptadorConexionSimulado>()
            .SimularEnBateria = true;
        var ejercicio = sp.GetRequiredService<ServicioEjercicioGuiado>();
        await ejercicio.IniciarAsync(CancellationToken.None);

        await sp.GetRequiredService<ServicioVerificacion>().VerificarSenalBateriaAsync(CancellationToken.None);
        var estado = await ejercicio.EstadoAsync(CancellationToken.None);

        estado.PasoActual.Should().Be(Supuesto.PresupuestoDeApagado, "el paso se deriva de las verificaciones");
        estado.NumeroPaso.Should().Be(2);
        estado.HayEjercicio.Should().BeTrue();
    }

    [Fact]
    public async Task LaSesionSobreviveAlReinicioDelServicio()
    {
        using var fabrica = new FabricaSai();
        string codigo;

        // Alcance 1: se inicia el ejercicio (simula la sesión antes del apagón).
        using (var scope = fabrica.Services.CreateScope())
        {
            await DarDeAlta(scope.ServiceProvider);
            var estado = await scope.ServiceProvider.GetRequiredService<ServicioEjercicioGuiado>()
                .IniciarAsync(CancellationToken.None);
            codigo = estado.Sesion!.Codigo;
        }

        // Alcance 2: contexto nuevo, como al volver el servicio tras el reinicio del host.
        using (var scope = fabrica.Services.CreateScope())
        {
            var estado = await scope.ServiceProvider.GetRequiredService<ServicioEjercicioGuiado>()
                .EstadoAsync(CancellationToken.None);

            estado.HayEjercicio.Should().BeTrue("la sesión está persistida, no en el circuito");
            estado.Sesion!.Codigo.Should().Be(codigo);
        }
    }

    [Fact]
    public async Task SalirDelEjercicioNoPierdeLoYaVerificado()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<SAI.Service.Core.Infrastructure.Adaptadores.AdaptadorConexionSimulado>()
            .SimularEnBateria = true;
        var ejercicio = sp.GetRequiredService<ServicioEjercicioGuiado>();
        await ejercicio.IniciarAsync(CancellationToken.None);
        await sp.GetRequiredService<ServicioVerificacion>().VerificarSenalBateriaAsync(CancellationToken.None);

        await ejercicio.AbandonarAsync(CancellationToken.None);

        var estado = await ejercicio.EstadoAsync(CancellationToken.None);
        estado.HayEjercicio.Should().BeFalse();
        var verificaciones = await sp.GetRequiredService<ServicioVerificacion>().EstadoAsync(CancellationToken.None);
        verificaciones.Single(v => v.Supuesto == Supuesto.SenalEnBateria).Estado
            .Should().Be(EstadoVerificacion.Verificado, "lo verificado vive en las verificaciones, no en la sesión");
    }

    [Fact]
    public async Task ConLasCuatroVigentesLaSesionSeCierraSola()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<SAI.Service.Core.Infrastructure.Adaptadores.AdaptadorConexionSimulado>()
            .SimularEnBateria = true;
        var ejercicio = sp.GetRequiredService<ServicioEjercicioGuiado>();
        await ejercicio.IniciarAsync(CancellationToken.None);

        var verificacion = sp.GetRequiredService<ServicioVerificacion>();
        await verificacion.VerificarPresupuestoAsync(120, CancellationToken.None);
        await verificacion.VerificarSenalBateriaAsync(CancellationToken.None);
        await verificacion.VerificarCorteConRetornoAsync(CancellationToken.None);
        await verificacion.RegistrarReencendidoAsync(arrancoSolo: true, CancellationToken.None);

        var estado = await ejercicio.EstadoAsync(CancellationToken.None);

        estado.PasoActual.Should().BeNull();
        estado.HayEjercicio.Should().BeFalse("al quedar los cuatro vigentes el ejercicio se completa solo");
    }

    private static async Task DarDeAlta(IServiceProvider sp) =>
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

    private static SolicitudAltaEquipos SolicitudValida() => new(
        new DateTimeOffset(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3)),
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");
}
