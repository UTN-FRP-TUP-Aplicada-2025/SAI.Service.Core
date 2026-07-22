using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Acciones;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Infrastructure.Adaptadores;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la ejecución del apagado ordenado (CU-05, US-14, US-15): el bloqueo por
/// verificación degrada a solo aviso (RN-02); con los cuatro supuestos verificados la acción se
/// ejecuta y se confirma por efecto observado (ADR-11) contra el adaptador simulado; toda decisión
/// deja una acción append-only. Nunca se dispara un apagado real.
/// </summary>
public class ApagadoIntegracionTests
{
    [Fact]
    public async Task ConLaModalidadDeAccionSinVerificarLaAccionQuedaBloqueada()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<OpcionesApagado>().ModalidadSolicitada = Modalidad.ApagarHostConRetorno;

        var accion = await sp.GetRequiredService<ServicioApagadoOrdenado>().EjecutarDisparoAsync("evt-disparo", CancellationToken.None);

        accion!.Estado.Should().Be(EstadoAccion.BloqueadaPorVerificacion);
        accion.ModalidadEfectiva.Should().Be(Modalidad.SoloAlerta, "sin los cuatro supuestos degrada a solo aviso (RN-02)");
        (await sp.GetRequiredService<SaiDbContext>().Acciones.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ConLosCuatroSupuestosVerificadosLaAccionSeEjecuta()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        sp.GetRequiredService<AdaptadorConexionSimulado>().SimularEnBateria = true;
        sp.GetRequiredService<OpcionesApagado>().ModalidadSolicitada = Modalidad.ApagarHostConRetorno;
        await VerificarLosCuatro(sp);

        var accion = await sp.GetRequiredService<ServicioApagadoOrdenado>().EjecutarDisparoAsync("evt-disparo", CancellationToken.None);

        accion!.Estado.Should().Be(EstadoAccion.Ejecutada, "el simulado admite la orden de apagado con retorno");
        accion.ModalidadEfectiva.Should().Be(Modalidad.ApagarHostConRetorno);
        accion.TiempoReservadoSeg.Should().BeLessThanOrEqualTo(Accion.TechoDuroApagadoSeg);
    }

    [Fact]
    public async Task EnSoloAvisoElDisparoNoApaga()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp); // OpcionesApagado por defecto: SoloAlerta

        var accion = await sp.GetRequiredService<ServicioApagadoOrdenado>().EjecutarDisparoAsync("evt-disparo", CancellationToken.None);

        accion!.Estado.Should().Be(EstadoAccion.SoloAviso, "la modalidad base segura solo alerta (RN-01)");
    }

    [Fact]
    public async Task LaAccionEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        await sp.GetRequiredService<ServicioApagadoOrdenado>().EjecutarDisparoAsync(null, CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        db.Acciones.Remove(await db.Acciones.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("las acciones son historia append-only (ADR-04)");
    }

    private static async Task VerificarLosCuatro(IServiceProvider sp)
    {
        var servicio = sp.GetRequiredService<ServicioVerificacion>();
        await servicio.VerificarPresupuestoAsync(120, CancellationToken.None);
        await servicio.VerificarSenalBateriaAsync(CancellationToken.None);
        await servicio.VerificarCorteConRetornoAsync(CancellationToken.None);
        await servicio.RegistrarReencendidoAsync(arrancoSolo: true, CancellationToken.None);
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
