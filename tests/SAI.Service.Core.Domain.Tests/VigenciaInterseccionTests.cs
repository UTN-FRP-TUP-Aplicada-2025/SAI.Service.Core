using FluentAssertions;
using SAI.Service.Core.Domain.Vinculos;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Recorte de una vigencia a un período <c>[desde, hasta)</c> (CU-12): base de los intervalos recortados y
/// de los días con protección del informe de período. El fin abierto se trata como el fin del período.
/// </summary>
public class VigenciaInterseccionTests
{
    private static readonly DateTimeOffset Ini2026 = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fin2026 = new(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Recambio = new(2026, 9, 5, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void RecortaLaVigenciaQueSaleDelPeriodoPorAmbosBordes()
    {
        // Montada desde 2024 hasta el recambio 2026-09-05: recorta al inicio del período.
        var vigencia = new Vigencia(new DateTimeOffset(2024, 11, 20, 0, 0, 0, TimeSpan.Zero), Recambio);

        var recorte = vigencia.Intersecar(Ini2026, Fin2026);

        recorte.Should().NotBeNull();
        recorte!.Value.Desde.Should().Be(Ini2026);
        recorte.Value.Hasta.Should().Be(Recambio);
    }

    [Fact]
    public void ElFinAbiertoSeRecortaAlFinDelPeriodo()
    {
        var vigente = new Vigencia(Recambio, null); // sigue vigente

        var recorte = vigente.Intersecar(Ini2026, Fin2026);

        recorte!.Value.Desde.Should().Be(Recambio);
        recorte.Value.Hasta.Should().Be(Fin2026, "una cobertura aún vigente cuenta hasta el corte del informe");
    }

    [Fact]
    public void UnaVigenciaFueraDelPeriodoNoInterseca()
    {
        var previa = new Vigencia(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                  new DateTimeOffset(2023, 6, 1, 0, 0, 0, TimeSpan.Zero));

        previa.Intersecar(Ini2026, Fin2026).Should().BeNull();
    }

    [Fact]
    public void LosDiasRecortadosDeDosMontajesSucesivosSumanElPeriodoSinSolapar()
    {
        // CA-01: bat-2024-a hasta el recambio, bat-2026-a desde el recambio (aún vigente).
        var saliente = new Vigencia(new DateTimeOffset(2024, 11, 20, 0, 0, 0, TimeSpan.Zero), Recambio);
        var entrante = new Vigencia(Recambio, null);

        var dias = saliente.DiasEnPeriodo(Ini2026, Fin2026) + entrante.DiasEnPeriodo(Ini2026, Fin2026);

        dias.Should().BeApproximately(365, 0.001, "los intervalos recortados cubren el período sin solaparse");
    }

    [Fact]
    public void DiasEnPeriodoEsCeroFueraDelPeriodo()
    {
        var previa = new Vigencia(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                                  new DateTimeOffset(2020, 2, 1, 0, 0, 0, TimeSpan.Zero));

        previa.DiasEnPeriodo(Ini2026, Fin2026).Should().Be(0);
    }
}
