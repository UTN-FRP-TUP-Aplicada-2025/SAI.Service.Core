using FluentAssertions;
using SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Formato de fechas del panel (SPEC 5.3). Además del formato en sí, cubre la regresión que rompió el
/// panel en producción: la solución compila con <c>InvariantGlobalization=true</c>, así que el formato
/// <b>no puede depender de una cultura nombrada</b> —pedir "es-AR" lanza <c>CultureNotFoundException</c>
/// y, al estar en un inicializador estático, tumbaba el render de toda la página.
/// </summary>
public class FormatoRelativoTests
{
    private static readonly DateTimeOffset Enero = new(2027, 1, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FechaCortaNoDependeDeUnaCulturaNombrada()
    {
        var acto = () => FormatoRelativo.FechaCorta(Enero);

        acto.Should().NotThrow("la solución corre en modo globalization-invariant");
        FormatoRelativo.FechaCorta(Enero).Should().Be("18 ene 2027");
    }

    [Theory]
    [InlineData(1, "12 ene 2027")]
    [InlineData(7, "12 jul 2027")]
    [InlineData(12, "12 dic 2027")]
    public void LosMesesSeEscribenEnEspanol(int mes, string esperado)
    {
        var fecha = new DateTimeOffset(2027, mes, 12, 0, 0, 0, TimeSpan.Zero);

        FormatoRelativo.FechaCorta(fecha).Should().Be(esperado);
    }

    [Fact]
    public void ElIsoQuedaSoloParaElTooltip()
    {
        FormatoRelativo.Iso(Enero).Should().Be("2027-01-18");
    }

    [Theory]
    [InlineData(0, "vence hoy")]
    [InlineData(1, "falta 1 día")]
    [InlineData(20, "faltan 20 días")]
    [InlineData(180, "faltan 6 meses")]
    [InlineData(365, "falta 1 año")]
    public void ElTiempoRelativoUsaLaGranularidadQueCorresponde(int dias, string esperado)
    {
        FormatoRelativo.Relativo(dias).Should().Be(esperado);
    }

    [Fact]
    public void UnaVigenciaPasadaSeDiceComoVencida()
    {
        FormatoRelativo.Relativo(-3).Should().Be("venció hace 3 días");
    }

    [Fact]
    public void SinFechaElVencimientoEsSinCaducidad()
    {
        FormatoRelativo.Vencimiento(null, null).Should().Be("sin caducidad");
    }

    [Fact]
    public void ConFechaElVencimientoCombinaAbsolutaYRelativa()
    {
        FormatoRelativo.Vencimiento(Enero, 180).Should().Be("vigente hasta el 18 ene 2027 (faltan 6 meses)");
    }
}
