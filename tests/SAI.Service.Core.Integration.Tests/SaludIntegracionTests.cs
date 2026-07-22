using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la prueba de batería (CU-07, US-12, US-13): sobre el adaptador simulado, ejecuta
/// la prueba, recoge la serie densa, congela el montaje y persiste el veredicto. La precondición de
/// flotación y las demoras están desactivadas en pruebas (ver <see cref="FabricaSai"/>).
/// </summary>
public class SaludIntegracionTests
{
    private static readonly DateTimeOffset Instante = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public async Task LaPruebaRegistraElVeredictoYCongelaElMontaje()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        var resultado = await sp.GetRequiredService<ServicioPruebaBateria>().EjecutarAsync(CancellationToken.None);

        resultado.Exito.Should().BeTrue();
        resultado.Prueba.Should().NotBeNull();
        resultado.Prueba!.MontajeBateriaCodigo.Should().Be("mnt-ups-principal", "el montaje vigente queda congelado (I-15)");
        resultado.Prueba.Comparable.Should().BeTrue("primera prueba: establece la línea base");
        resultado.Prueba.Veredicto.Should().Be(VeredictoSalud.SinDegradacionDetectable);
        resultado.MuestrasTomadas.Should().Be(5);

        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.PruebasBateria.CountAsync()).Should().Be(1);
        (await db.Muestras.CountAsync()).Should().Be(5, "la serie densa de la prueba");
    }

    [Fact]
    public async Task LaPruebaEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        await sp.GetRequiredService<ServicioPruebaBateria>().EjecutarAsync(CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        db.PruebasBateria.Remove(await db.PruebasBateria.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("las pruebas son historia append-only (ADR-04)");
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
