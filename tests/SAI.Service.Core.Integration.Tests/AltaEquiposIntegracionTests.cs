using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración del alta de equipos (CU-02, US-04, US-05): persiste catálogo, inventario, vínculos
/// abiertos y las cuatro verificaciones sembradas en solo aviso, de forma transaccional, sobre la
/// base real (migración aplicada). Valida también el índice único parcial de "una vigente".
/// </summary>
public class AltaEquiposIntegracionTests
{
    private static readonly DateTimeOffset Instante = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public async Task AltaCreaElEquipoCompletoYSiembraCuatroVerificacionesEnSoloAlerta()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioAltaEquipos>();
        var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();

        var resultado = await servicio.RegistrarAsync(SolicitudValida(), CancellationToken.None);

        resultado.Exito.Should().BeTrue();
        resultado.ModalidadEfectiva.Should().Be(Modalidad.SoloAlerta);
        resultado.SupuestosVerificados.Should().Be(0);
        resultado.SupuestosTotales.Should().Be(4);

        (await db.Unidades.CountAsync()).Should().Be(3, "host + dispositivo + batería");
        (await db.Montajes.CountAsync()).Should().Be(1);
        (await db.Coberturas.CountAsync()).Should().Be(1);
        (await db.Verificaciones.CountAsync()).Should().Be(4);
        (await db.Verificaciones.CountAsync(v => v.Estado == EstadoVerificacion.NuncaVerificado)).Should().Be(4);
    }

    [Fact]
    public async Task ModeloBateriaSinTemperaturaDevuelveErrorYNoPersisteNada()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioAltaEquipos>();
        var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();

        var solicitud = SolicitudValida() with
        {
            ModeloBateria = new DatosModeloBateria("mod-bat", "12V", VidaFlotacionAniosMin: 3, TemperaturaReferenciaC: null),
        };

        var resultado = await servicio.RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Exito.Should().BeFalse();
        resultado.CodigoError.Should().Be("VIDA_FLOTACION_SIN_TEMPERATURA");
        (await db.Unidades.CountAsync()).Should().Be(0, "no se persiste nada si el alta falla");
    }

    [Fact]
    public async Task ElIndiceUnicoParcialRechazaUnSegundoMontajeVigenteEnLaMismaPosicion()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioAltaEquipos>();
        var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();

        await servicio.RegistrarAsync(SolicitudValida(), CancellationToken.None);

        // Segundo montaje vigente para (dispositivo, posición) ya ocupada: lo debe rechazar la base.
        db.Montajes.Add(new MontajeBateria("mnt-duplicado", "bat", "ups", "principal", new Vigencia(Instante)));
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<DbUpdateException>("el índice único parcial impone a lo sumo un montaje vigente (RC-02)");
    }

    [Fact]
    public async Task ElEstadoDePuestaEnMarchaQuedaDegradadoTrasElAlta()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioAltaEquipos>();

        await servicio.RegistrarAsync(SolicitudValida(), CancellationToken.None);
        var estado = await servicio.ConsultarEstadoAsync(CancellationToken.None);

        estado.Degradado.Should().BeTrue();
        estado.ModalidadEfectiva.Should().Be(Modalidad.SoloAlerta);
        estado.SupuestosVerificados.Should().Be(0);
        estado.SupuestosTotales.Should().Be(4);
        estado.HayEquipos.Should().BeTrue();
    }

    private static SolicitudAltaEquipos SolicitudValida() => new(
        Instante,
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx 1200"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3, VidaFlotacionAniosMax: 5),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");
}
