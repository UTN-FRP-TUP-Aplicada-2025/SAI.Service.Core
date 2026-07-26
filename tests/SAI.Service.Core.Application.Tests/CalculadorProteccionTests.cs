using FluentAssertions;
using SAI.Service.Core.Application.Intervenciones;
using SAI.Service.Core.Domain.Vinculos;
using Xunit;

namespace SAI.Service.Core.Application.Tests;

/// <summary>
/// Sucesión de coberturas y días sin protección de un host (CU-09 §4.6): a partir de los vínculos
/// temporales, la línea de tiempo intercala los huecos legítimos (RC-03) y suma sus días.
/// </summary>
public class CalculadorProteccionTests
{
    private static readonly DateTimeOffset T0 = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static CoberturaHost Cobertura(string codigo, string disp, int desdeDia, int? hastaDia) =>
        new(codigo, disp, "host", new Vigencia(T0.AddDays(desdeDia), hastaDia is { } h ? T0.AddDays(h) : null));

    [Fact]
    public void UnaCoberturaVigenteNoDejaDiasSinProteccion()
    {
        var coberturas = new[] { Cobertura("cob-1", "ups-1", 0, null) };

        CalculadorProteccion.DiasSinProteccion(coberturas, T0.AddDays(30)).Should().Be(0);
    }

    [Fact]
    public void UnHuecoEntreCoberturasSeCuentaComoDiasSinProteccion()
    {
        // ups-1 cubre [día 0, día 10); ups-2 cubre [día 12, abierto): hueco de 2 días.
        var coberturas = new[]
        {
            Cobertura("cob-1", "ups-1", 0, 10),
            Cobertura("cob-2", "ups-2", 12, null),
        };

        CalculadorProteccion.DiasSinProteccion(coberturas, T0.AddDays(30)).Should().Be(2);
    }

    [Fact]
    public void SucesionIntercalaElTramoSinProteccion()
    {
        var coberturas = new[]
        {
            Cobertura("cob-1", "ups-1", 0, 10),
            Cobertura("cob-2", "ups-2", 12, null),
        };

        var tramos = CalculadorProteccion.Sucesion(coberturas);

        tramos.Should().HaveCount(3);
        tramos[0].DispositivoCodigo.Should().Be("ups-1");
        tramos[1].SinProteccion.Should().BeTrue();
        tramos[2].DispositivoCodigo.Should().Be("ups-2");
        tramos[2].Hasta.Should().BeNull("sigue vigente");
    }

    [Fact]
    public void SinHuecoNoHayDiasSinProteccion()
    {
        var coberturas = new[]
        {
            Cobertura("cob-1", "ups-1", 0, 10),
            Cobertura("cob-2", "ups-2", 10, null),
        };

        CalculadorProteccion.DiasSinProteccion(coberturas, T0.AddDays(30)).Should().Be(0);
        CalculadorProteccion.Sucesion(coberturas).Should().NotContain(t => t.SinProteccion);
    }

    [Fact]
    public void UnHuecoFinalAbiertoSeCuentaHastaElInstanteDeReferencia()
    {
        // Solo hubo una cobertura cerrada al día 10; el host quedó descubierto desde entonces.
        var coberturas = new[] { Cobertura("cob-1", "ups-1", 0, 10) };

        CalculadorProteccion.DiasSinProteccion(coberturas, T0.AddDays(15)).Should().Be(5);
    }
}
