using FluentAssertions;
using SAI.Service.Core.Infrastructure.Adaptadores.Nut;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Parseo del protocolo de red de NUT (<see cref="ProtocoloNut"/>) contra los formatos reales del
/// cable capturados del servidor upsd. Es puro: no toca socket.
/// </summary>
public class ProtocoloNutTests
{
    [Fact]
    public void LeerValorDeVarExtraeElValorEntreComillas()
    {
        ProtocoloNut.LeerValorDeVar("VAR sai ups.status \"OL\"", "sai", "ups.status").Should().Be("OL");
        ProtocoloNut.LeerValorDeVar("VAR sai input.voltage \"228.7\"", "sai", "input.voltage").Should().Be("228.7");
    }

    [Fact]
    public void LeerValorDeVarDevuelveNullSiLaLineaNoEsDeEsaVariable()
    {
        ProtocoloNut.LeerValorDeVar("VAR sai ups.status \"OL\"", "sai", "battery.charge").Should().BeNull();
        ProtocoloNut.LeerValorDeVar("ERR VAR-NOT-SUPPORTED", "sai", "device.mfr").Should().BeNull();
    }

    [Theory]
    [InlineData("ERR VAR-NOT-SUPPORTED", true)]
    [InlineData("ERR ACCESS-DENIED", true)]
    [InlineData("VAR sai ups.status \"OL\"", false)]
    [InlineData("BEGIN LIST VAR sai", false)]
    public void EsErrorReconoceLasRespuestasDeError(string linea, bool esError)
    {
        ProtocoloNut.EsError(linea).Should().Be(esError);
    }

    [Fact]
    public void ParsearLineaVarDevuelveVariableYValor()
    {
        ProtocoloNut.ParsearLineaVar("VAR sai battery.charge \"100\"", "sai")
            .Should().Be(("battery.charge", "100"));
    }

    [Fact]
    public void ParsearLineaVarDevuelveNullSiNoEsLineaVarDelUps()
    {
        ProtocoloNut.ParsearLineaVar("END LIST VAR sai", "sai").Should().BeNull();
        ProtocoloNut.ParsearLineaVar("VAR otro battery.charge \"100\"", "sai").Should().BeNull();
    }

    [Fact]
    public void ParsearLineaUpsDevuelveNombreYDescripcion()
    {
        ProtocoloNut.ParsearLineaUps("UPS sai \"UPS INNO TECH / Voltronic Qx - relevamiento\"")
            .Should().Be(("sai", "UPS INNO TECH / Voltronic Qx - relevamiento"));
    }

    [Fact]
    public void DesescapaComillasYBarrasInvertidasEnLosValores()
    {
        // NUT escapa " como \" y \ como \\ dentro del valor entrecomillado.
        ProtocoloNut.LeerValorDeVar("VAR sai x \"a\\\"b\"", "sai", "x").Should().Be("a\"b");
        ProtocoloNut.LeerValorDeVar("VAR sai y \"c\\\\d\"", "sai", "y").Should().Be("c\\d");
    }
}
