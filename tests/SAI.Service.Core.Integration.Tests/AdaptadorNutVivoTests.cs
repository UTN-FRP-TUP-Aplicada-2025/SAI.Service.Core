using FluentAssertions;
using SAI.Service.Core.Infrastructure.Adaptadores.Nut;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Pruebas en vivo del adaptador NUT contra el SAI real (omitidas salvo <c>SAI_NUT_LIVE=1</c>, ver
/// <see cref="HechoNutVivoAttribute"/>). Validan de punta a punta el diálogo con upsd: lectura de
/// estado, prueba de conexión por efecto observado y descubrimiento con descriptores (US-03).
/// </summary>
public class AdaptadorNutVivoTests
{
    private static AdaptadorConexionNut AdaptadorVivo()
    {
        var opciones = new OpcionesNut
        {
            Host = Environment.GetEnvironmentVariable("SAI_NUT_HOST") ?? "127.0.0.1",
            Puerto = int.TryParse(Environment.GetEnvironmentVariable("SAI_NUT_PORT"), out var puerto) ? puerto : 3493,
            Ups = Environment.GetEnvironmentVariable("SAI_NUT_UPS") ?? "sai",
        };
        return new AdaptadorConexionNut(new ClienteNut(opciones));
    }

    [HechoNutVivo]
    public async Task LeeElEstadoRealDelSai()
    {
        var estado = await AdaptadorVivo().LeerEstadoAsync(CancellationToken.None);

        estado.Alcanzable.Should().BeTrue();
        estado.TensionEntradaVoltios.Should().BeGreaterThan(0, "el SAI real expone input.voltage");
        estado.CargaBateriaPorcentaje.Should().NotBeNull();
    }

    [HechoNutVivo]
    public async Task LaPruebaDeConexionRealConfirmaPorEfectoObservado()
    {
        var resultado = await AdaptadorVivo().ProbarConectividadAsync(CancellationToken.None);

        resultado.Conectado.Should().BeTrue();
        resultado.LatenciaMilisegundos.Should().NotBeNull();
        resultado.Detalle.Should().Contain("ups.status");
    }

    [HechoNutVivo]
    public async Task DescubreElSaiRealConSuDescriptor()
    {
        var descubiertos = await AdaptadorVivo().DescubrirAsync(CancellationToken.None);

        descubiertos.Should().NotBeEmpty();
        descubiertos[0].Descriptor.Should().Contain(":", "el descriptor incluye el id USB vendor:product");
    }
}
