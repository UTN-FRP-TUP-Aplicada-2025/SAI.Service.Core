using FluentAssertions;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Recambio de batería: cuadre de costos (RN-08), importes con moneda y fecha (RN-07) y proyección de
/// la ficha de vida útil con el costo por año normalizado a USD (US-19). La ficha es append-only.
/// </summary>
public class IntervencionesTests
{
    private static readonly DateOnly Hoy = new(2026, 9, 5);

    [Fact]
    public void LosCostosCuadranCuandoElTotalIgualaLaSuma()
    {
        var costos = new Costos(
            new Dinero(52000, "ARS", Hoy),
            new Dinero(15000, "ARS", Hoy),
            new Dinero(67000, "ARS", Hoy));

        costos.Cuadra().Should().BeTrue("52.000 + 15.000 = 67.000 (RN-08)");
    }

    [Fact]
    public void LosCostosNoCuadranConUnTotalDistinto()
    {
        var costos = new Costos(
            new Dinero(52000, "ARS", Hoy),
            new Dinero(15000, "ARS", Hoy),
            new Dinero(60000, "ARS", Hoy));

        costos.Cuadra().Should().BeFalse("60.000 ≠ 52.000 + 15.000 (RN-08)");
    }

    [Fact]
    public void LosCostosNoCuadranConMonedasDistintas()
    {
        var costos = new Costos(
            new Dinero(52000, "ARS", Hoy),
            new Dinero(15000, "USD", Hoy),
            new Dinero(67000, "ARS", Hoy));

        costos.Cuadra().Should().BeFalse("no se suman importes de monedas distintas");
    }

    [Fact]
    public void DineroExigeMoneda()
    {
        var acto = () => new Dinero(1000, " ", Hoy);
        acto.Should().Throw<ArgumentException>("todo importe declara su moneda (RN-07)");
    }

    [Fact]
    public void LaFichaProyectaVidaYCostoPorAnioConEquivalenteUsdDerivado()
    {
        var desde = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero); // 366 días (2024 bisiesto)

        var ficha = FichaVidaUtil.Proyectar(
            "fic-1", "int-1", "ups", "bat-vieja", desde, hasta,
            vidaEsperadaDias: 1095, costoTotal: new Dinero(67000, "ARS", Hoy),
            tasaAUsd: 0.001m, fuenteCotizacion: "BCRA");

        ficha.Should().BeAssignableTo<IEntidadHistoria>();
        ficha.DiasEnServicio.Should().Be(366);
        ficha.CumplioExpectativa.Should().BeFalse("366 < 1095 días esperados");
        ficha.DesvioDias.Should().Be(366 - 1095);
        ficha.CostoPorAnioServicio.Moneda.Should().Be("ARS", "el valor original conserva su moneda");
        ficha.CostoPorAnioServicioUsd.Moneda.Should().Be("USD");
        ficha.CostoPorAnioServicioUsd.Monto.Should().Be(decimal.Round(ficha.CostoPorAnioServicio.Monto * 0.001m, 2));
        ficha.FuenteCotizacion.Should().Be("BCRA");
    }

    [Fact]
    public void LaFichaExigeUnaCotizacionPositiva()
    {
        var desde = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var acto = () => FichaVidaUtil.Proyectar("fic-1", "int-1", "ups", "bat", desde, desde.AddDays(10), 100, new Dinero(1, "ARS", Hoy), 0m, "x");

        acto.Should().Throw<ArgumentOutOfRangeException>();
    }
}
