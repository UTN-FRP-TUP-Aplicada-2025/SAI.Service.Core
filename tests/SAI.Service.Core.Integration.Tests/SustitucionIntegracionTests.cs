using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la sustitución/reparación del SAI (CU-09, US-20): un solo acto cierra la cobertura
/// vigente, cambia el estado del equipo saliente y —si hay suplente— abre una cobertura nueva sin
/// solapar; un hueco es legítimo (días sin protección) y sustituir por otro modelo reinicia las
/// verificaciones (FA-2). Cubre los criterios CA-01..CA-04, los errores y el append-only.
/// </summary>
public class SustitucionIntegracionTests
{
    private static readonly DateTimeOffset Momento = new(2027, 1, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CierraLaCoberturaDejaElSalienteEnReparacionYAbreLaDelSuplente()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);

        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(Solicitud(), CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.Registrado);

        var db = sp.GetRequiredService<SaiDbContext>();
        var coberturas = await db.Coberturas.Where(c => c.HostCodigo == "host").ToListAsync();
        coberturas.Should().HaveCount(2, "la vieja cerrada y la del suplente");
        coberturas.Count(c => c.Vigencia.Hasta == null).Should().Be(1, "a lo sumo una cobertura vigente por host (I-4)");

        (await db.Unidades.OfType<Dispositivo>().SingleAsync(d => d.Codigo == "ups")).Estado
            .Should().Be(EstadoUnidad.EnReparacion);
        (await db.Unidades.OfType<Dispositivo>().SingleAsync(d => d.Codigo == "ups-2")).Estado
            .Should().Be(EstadoUnidad.EnServicio);
        (await db.Sustituciones.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UnSuplenteQueEmpiezaDespuesDejaDiasSinProteccion()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var solicitud = Solicitud() with { InstanteInicioCobertura = Momento.AddDays(2) };

        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.Registrado);
        resultado.DiasSinProteccion.Should().Be(2);
    }

    [Fact]
    public async Task SustituirPorOtroModeloReiniciaLasVerificaciones()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);

        // Un modelo distinto en el catálogo, y una verificación ya hecha (para observar el reinicio).
        var db = sp.GetRequiredService<SaiDbContext>();
        db.ModelosDispositivo.Add(new ModeloDispositivo("mod-disp-2", "fab", "Otro modelo"));
        await db.SaveChangesAsync();
        await sp.GetRequiredService<ServicioVerificacion>().VerificarPresupuestoAsync(120, CancellationToken.None);

        var solicitud = Solicitud() with { DispositivoEntranteCodigo = "ups-3", ModeloDispositivoEntrante = "mod-disp-2" };
        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.Registrado);
        resultado.FirmwareReiniciado.Should().BeTrue();
        var verificaciones = await sp.GetRequiredService<ServicioVerificacion>().EstadoAsync(CancellationToken.None);
        verificaciones.Should().OnlyContain(v => v.Estado == EstadoVerificacion.NuncaVerificado,
            "sustituir por otro modelo obliga a recaracterizar el firmware (FA-2/CA-04)");
    }

    [Fact]
    public async Task MismoModeloNoReiniciaLasVerificaciones()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        await sp.GetRequiredService<ServicioVerificacion>().VerificarPresupuestoAsync(120, CancellationToken.None);

        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(Solicitud(), CancellationToken.None);

        resultado.FirmwareReiniciado.Should().BeFalse("el suplente es del mismo modelo");
        var presupuesto = (await sp.GetRequiredService<ServicioVerificacion>().EstadoAsync(CancellationToken.None))
            .Single(v => v.Supuesto == Supuesto.PresupuestoDeApagado);
        presupuesto.Estado.Should().Be(EstadoVerificacion.Verificado, "no se reinició");
    }

    [Fact]
    public async Task SinCoberturaVigenteSeRechaza()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var servicio = sp.GetRequiredService<ServicioSustitucionSai>();
        // Reemplazo sin suplente: cierra la cobertura y el host queda descubierto.
        await servicio.RegistrarAsync(Solicitud() with
        {
            Tipo = TipoIntervencionSai.Reemplazo,
            DispositivoEntranteCodigo = null,
            DestinoDisposicion = "chatarra electrónica",
            ReceptorDisposicion = "gestor habilitado",
        }, CancellationToken.None);

        var resultado = await servicio.RegistrarAsync(Solicitud(), CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.SinCoberturaVigente);
    }

    [Fact]
    public async Task UnaFechaAnteriorAlInicioDeLaCoberturaSeRechazaSinEfectos()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var solicitud = Solicitud() with { InstanteOcurrido = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero) };

        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.CoherenciaTemporal);
        var db = sp.GetRequiredService<SaiDbContext>();
        (await db.Coberturas.CountAsync(c => c.HostCodigo == "host")).Should().Be(1, "no se aplicó nada");
        (await db.Sustituciones.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UnCostoSinMonedaSeRechaza()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        var solicitud = Solicitud() with { Costo = new ImporteEntrada(150000m, null, null) };

        var resultado = await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(solicitud, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoSustitucion.DineroSinMonedaOFecha);
    }

    [Fact]
    public async Task LaSustitucionEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await DarDeAlta(sp);
        await sp.GetRequiredService<ServicioSustitucionSai>().RegistrarAsync(Solicitud(), CancellationToken.None);

        var db = sp.GetRequiredService<SaiDbContext>();
        db.Sustituciones.Remove(await db.Sustituciones.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("la historia de intervenciones es append-only (ADR-04)");
    }

    private static SolicitudSustitucion Solicitud() => new(
        InstanteOcurrido: Momento,
        InstanteRegistrado: Momento,
        HostCodigo: "host",
        DispositivoSalienteCodigo: "ups",
        Tipo: TipoIntervencionSai.Reparacion,
        DispositivoEntranteCodigo: "ups-2",
        ModeloDispositivoEntrante: "mod-disp",
        NumeroSerieEntrante: null,
        InstanteInicioCobertura: null,
        Proveedor: "Proveedor SA",
        Ejecutor: "Técnico",
        Hallazgos: "revisión general",
        Costo: null,
        DestinoDisposicion: null,
        ReceptorDisposicion: null);

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
