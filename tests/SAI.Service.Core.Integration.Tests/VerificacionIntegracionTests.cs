using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Infrastructure.Adaptadores;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la ventana de mantenimiento (US-16, US-17, CU-10): la verificación de cada supuesto
/// por efecto observado (ADR-11) y el desbloqueo de la modalidad con los cuatro verificados; la
/// refutación del reencendido por placa como bloqueo permanente. El write path por NUT está diferido:
/// el camino se ejercita contra el adaptador simulado, que puede reportar el equipo en batería.
/// </summary>
public class VerificacionIntegracionTests
{
    [Fact]
    public async Task LaVentanaVerificaLosCuatroSupuestosYDesbloqueaLaModalidad()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<AdaptadorConexionSimulado>().SimularEnBateria = true; // corte simulado
        var servicio = sp.GetRequiredService<ServicioVerificacion>();

        (await servicio.VerificarPresupuestoAsync(120, CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Verificado);
        (await servicio.VerificarSenalBateriaAsync(CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Verificado);
        (await servicio.VerificarCorteConRetornoAsync(CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Verificado);
        (await servicio.RegistrarReencendidoAsync(arrancoSolo: true, CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Verificado);

        var ahora = DateTimeOffset.UtcNow;
        var estado = await servicio.EstadoAsync(CancellationToken.None);
        EvaluadorModalidad.Verificados(estado, ahora).Should().Be(4);
        EvaluadorModalidad.Efectiva(Modalidad.ApagarHostConRetorno, estado, ahora)
            .Should().Be(Modalidad.ApagarHostConRetorno, "con los cuatro supuestos verificados se desbloquea la acción");
    }

    [Fact]
    public async Task SinObservarElEstadoEnBateriaLaSenalNoSeConfirma()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp); // simulado en línea por defecto
        var servicio = sp.GetRequiredService<ServicioVerificacion>();

        var resultado = await servicio.VerificarSenalBateriaAsync(CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoResultadoVerificacion.EfectoNoConfirmado, "ADR-11: sin efecto observado no se supera");
        var estado = await servicio.EstadoAsync(CancellationToken.None);
        estado.Single(v => v.Supuesto == Supuesto.SenalEnBateria).Estado
            .Should().Be(EstadoVerificacion.NuncaVerificado);
    }

    [Fact]
    public async Task RefutarElReencendidoEsUnBloqueoPermanenteQueNoSeReVerifica()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var servicio = sp.GetRequiredService<ServicioVerificacion>();

        (await servicio.RegistrarReencendidoAsync(arrancoSolo: false, CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Refutado);
        (await servicio.RegistrarReencendidoAsync(arrancoSolo: true, CancellationToken.None)).Codigo
            .Should().Be(CodigoResultadoVerificacion.Refutado, "un supuesto refutado no puede reverificarse");

        var estado = await servicio.EstadoAsync(CancellationToken.None);
        estado.Single(v => v.Supuesto == Supuesto.ReencendidoPorPlaca).Estado
            .Should().Be(EstadoVerificacion.Refutado);
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
