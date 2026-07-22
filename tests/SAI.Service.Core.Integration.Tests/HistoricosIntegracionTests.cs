using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la consulta histórica (US-11, CU-06): dentro de la retención sirve muestras con su
/// procedencia; un período sin datos responde PERIODO_SIN_DATOS sin dibujar una serie vacía.
/// </summary>
public class HistoricosIntegracionTests
{
    private static readonly DateTimeOffset Instante = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public async Task DentroDeRetencionDevuelveMuestrasConProcedencia()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);
        var monitoreo = sp.GetRequiredService<ServicioMonitoreo>();
        await monitoreo.SondearAsync(5, CancellationToken.None);
        await monitoreo.SondearAsync(5, CancellationToken.None);

        var desde = DateTimeOffset.UtcNow.AddHours(-1);
        var hasta = DateTimeOffset.UtcNow.AddMinutes(1);
        var resultado = await sp.GetRequiredService<ServicioHistoricos>()
            .ConsultarAsync([Variables.TensionEntrada], desde, hasta, resolucionForzada: null, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoResultadoHistorico.Ok);
        resultado.Resolucion.Should().Be(ResolucionSerie.Muestras);
        var serie = resultado.Series.Single();
        serie.Variable.Should().Be(Variables.TensionEntrada);
        serie.Procedencia.Should().Be(Origen.Medido);
        serie.Puntos.Should().HaveCount(2);
        serie.Puntos.Should().OnlyContain(p => p.Valor != null);
    }

    [Fact]
    public async Task PeriodoSinDatosNoDibujaSerieVacia()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<ServicioAltaEquipos>().RegistrarAsync(SolicitudValida(), CancellationToken.None);

        var desde = DateTimeOffset.UtcNow.AddHours(-2);
        var hasta = DateTimeOffset.UtcNow.AddHours(-1);
        var resultado = await sp.GetRequiredService<ServicioHistoricos>()
            .ConsultarAsync([Variables.TensionEntrada], desde, hasta, resolucionForzada: null, CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoResultadoHistorico.PeriodoSinDatos);
        resultado.Series.Should().BeEmpty();
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
