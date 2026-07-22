using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración del recambio de batería (CU-08, US-18, US-19): un solo acto cierra la vigencia vieja,
/// abre la nueva sin hueco, da de baja la retirada y pone en servicio la entrante, y proyecta la
/// ficha; con validación de cuadre (RN-08) y de moneda/fecha (RN-07) antes de aplicar efectos. La
/// intervención y la ficha son append-only.
/// </summary>
public class RecambioIntegracionTests
{
    private static readonly DateOnly FechaCosto = new(2027, 3, 1);
    private static readonly DateTimeOffset Recambio = new(2027, 3, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ElRecambioCierraYAbreVigenciaDaDeBajaYProyectaLaFicha()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);

        var resultado = await sp.GetRequiredService<ServicioRecambioBateria>().RegistrarAsync(Solicitud(), CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoRecambio.Registrado);
        resultado.Ficha.Should().NotBeNull();

        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.Intervenciones.CountAsync()).Should().Be(1);
        (await db.FichasVidaUtil.CountAsync()).Should().Be(1);

        var montajes = await db.Montajes.Where(m => m.DispositivoCodigo == "ups" && m.Posicion == "principal").ToListAsync();
        montajes.Should().HaveCount(2, "el viejo cerrado y el nuevo vigente");
        montajes.Count(m => m.Vigencia.Hasta == null).Should().Be(1, "a lo sumo uno vigente (I-2)");

        var bateriaVieja = await db.Unidades.OfType<Bateria>().SingleAsync(b => b.Codigo == "bat");
        bateriaVieja.Estado.Should().Be(EstadoUnidad.DadoDeBaja);
        var bateriaNueva = await db.Unidades.OfType<Bateria>().SingleAsync(b => b.Codigo == "bat-2");
        bateriaNueva.Estado.Should().Be(EstadoUnidad.EnServicio);
    }

    [Fact]
    public async Task UnTotalQueNoCuadraSeRechazaSinAplicarEfectos()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var solicitud = Solicitud() with { Total = new ImporteEntrada(60000, "ARS", FechaCosto) };

        var resultado = await sp.GetRequiredService<ServicioRecambioBateria>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoRecambio.CostosNoCuadran);
        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.Intervenciones.CountAsync()).Should().Be(0, "postcondición de fallo: no se aplica ningún efecto");
        (await db.Unidades.OfType<Bateria>().SingleAsync(b => b.Codigo == "bat")).Estado.Should().Be(EstadoUnidad.EnServicio);
    }

    [Fact]
    public async Task UnImporteSinMonedaOFechaSeRechaza()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var solicitud = Solicitud() with { ManoDeObra = new ImporteEntrada(15000, null, FechaCosto) };

        var resultado = await sp.GetRequiredService<ServicioRecambioBateria>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoRecambio.DineroSinMonedaOFecha);
        (await sp.GetRequiredService<SaiDbContext>().Intervenciones.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task LaIntervencionEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        await sp.GetRequiredService<ServicioRecambioBateria>().RegistrarAsync(Solicitud(), CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        db.Intervenciones.Remove(await db.Intervenciones.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("las intervenciones son historia append-only (ADR-04)");
    }

    private static SolicitudRecambio Solicitud() => new(
        Recambio, Recambio,
        DispositivoCodigo: "ups", Posicion: "principal",
        BateriaEntranteCodigo: "bat-2", ModeloBateriaEntranteCodigo: "mod-bat",
        FechaFabricacionEntrante: null, FechaCompraEntrante: null,
        Proveedor: "Baterías del Sur", Ejecutor: "técnico",
        Repuestos: new ImporteEntrada(52000, "ARS", FechaCosto),
        ManoDeObra: new ImporteEntrada(15000, "ARS", FechaCosto),
        Total: new ImporteEntrada(67000, "ARS", FechaCosto),
        Hallazgos: "batería hinchada, tensión de flotación baja",
        DestinoDisposicion: "gestor de residuos habilitado", ReceptorDisposicion: "EcoBaterías SA",
        TasaAUsd: 0.001m, FuenteCotizacion: "BCRA");

    private static async Task DarDeAlta(IServiceProvider sp) =>
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudAlta(), CancellationToken.None);

    private static SolicitudAltaEquipos SolicitudAlta() => new(
        new DateTimeOffset(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3)),
        Fabricante: new DatosFabricante("fab", "INNO TECH"),
        ModeloDispositivo: new DatosModeloDispositivo("mod-disp", "Voltronic Qx"),
        ModeloBateria: new DatosModeloBateria("mod-bat", "12V 9Ah", TemperaturaReferenciaC: 25, VidaFlotacionAniosMin: 3),
        Host: new DatosHost("host", "alta"),
        Dispositivo: new DatosDispositivo("ups", NumeroSerie: null),
        Bateria: new DatosBateria("bat"),
        Posicion: "principal");
}
