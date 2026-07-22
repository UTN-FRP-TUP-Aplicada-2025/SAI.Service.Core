using FluentAssertions;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Muestreo (US-08, US-10, BT-18): la calidad de la muestra (completa/parcial/perdida), la
/// procedencia canónica por variable y la agregación con mínimo/máximo/promedio y cobertura.
/// </summary>
public class MonitoreoTests
{
    private static readonly DateTimeOffset Instante = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));
    private static readonly string[] Esperadas = [.. Variables.ProcedenciaCanonica.Keys];

    [Fact]
    public void MuestraConTodasLasVariablesEsCompleta()
    {
        var lecturas = new Dictionary<string, double?>
        {
            [Variables.TensionEntrada] = 228.7,
            [Variables.TensionSalida] = 228.7,
            [Variables.CargaSalida] = 13,
            [Variables.CargaBateria] = 100,
        };

        var muestra = Muestra.Registrar("m1", "ups", "s1", Instante, alcanzable: true, lecturas, Esperadas);

        muestra.Calidad.Should().Be(CalidadMuestra.Completa);
        muestra.Valor(Variables.TensionEntrada).Should().Be(228.7);
    }

    [Fact]
    public void MuestraConUnaVariableAusenteEsParcial()
    {
        var lecturas = new Dictionary<string, double?>
        {
            [Variables.TensionEntrada] = 228.7,
            [Variables.TensionSalida] = 228.7,
            [Variables.CargaSalida] = null, // faltó
            [Variables.CargaBateria] = 100,
        };

        var muestra = Muestra.Registrar("m1", "ups", "s1", Instante, alcanzable: true, lecturas, Esperadas);

        muestra.Calidad.Should().Be(CalidadMuestra.Parcial);
        muestra.Valor(Variables.CargaSalida).Should().BeNull();
    }

    [Fact]
    public void MuestraNoAlcanzableEsPerdidaYSinValores()
    {
        var muestra = Muestra.Registrar("m1", "ups", "s1", Instante, alcanzable: false,
            new Dictionary<string, double?>(), Esperadas);

        muestra.Calidad.Should().Be(CalidadMuestra.Perdida);
        muestra.Lecturas.Should().BeEmpty();
    }

    [Fact]
    public void LaProcedenciaDeLaCargaDeBateriaEsDerivadaNoMedida()
    {
        Variables.ProcedenciaCanonica[Variables.CargaBateria].Should().Be(Origen.Derivado);
        Variables.ProcedenciaCanonica[Variables.TensionEntrada].Should().Be(Origen.Medido);
    }

    [Fact]
    public void AgregadoConservaMinimoMaximoYPromedioConCoberturaTotal()
    {
        var agregado = CalculadorAgregado.Calcular("a1", "ups", Variables.TensionEntrada, Instante,
            new double?[] { 220, 225, 218 });

        agregado.Should().NotBeNull();
        agregado!.Minimo.Should().Be(218);
        agregado.Maximo.Should().Be(225);
        agregado.Promedio.Should().BeApproximately(221, 0.01);
        agregado.Cobertura.Should().Be(1);
        agregado.Advertencia.Should().BeNull();
    }

    [Fact]
    public void AgregadoConHuecosBajaLaCoberturaYLlevaAdvertencia()
    {
        var agregado = CalculadorAgregado.Calcular("a1", "ups", Variables.TensionEntrada, Instante,
            new double?[] { 220, null, 218 });

        agregado!.NMuestras.Should().Be(2);
        agregado.Cobertura.Should().BeApproximately(2d / 3d, 0.001);
        agregado.Minimo.Should().Be(218);
        agregado.Maximo.Should().Be(220);
        agregado.Advertencia.Should().NotBeNull();
    }

    [Fact]
    public void AgregadoSinMuestrasEsNulo()
    {
        CalculadorAgregado.Calcular("a1", "ups", Variables.TensionEntrada, Instante, [])
            .Should().BeNull();
    }
}
