using FluentAssertions;
using SAI.Service.Core.Domain.Catalogo;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Invariante I-21 (RN-13): la vida de flotación de un modelo de batería solo tiene sentido a una
/// temperatura de referencia. Corre como prueba (mitigación del riesgo R-10).
/// </summary>
public class ModeloBateriaTests
{
    [Fact]
    public void DeclararVidaDeFlotacionSinTemperaturaDeReferenciaLanza()
    {
        var acto = () => new ModeloBateria(
            "mod-bat-1", "fab-1", "12V 9Ah",
            vidaFlotacionAniosMin: 3, vidaFlotacionAniosMax: 5,
            temperaturaReferenciaC: null);

        acto.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeclararSoloElMinimoDeVidaDeFlotacionSinTemperaturaTambienLanza()
    {
        var acto = () => new ModeloBateria(
            "mod-bat-1", "fab-1", "12V 9Ah",
            vidaFlotacionAniosMin: 3,
            temperaturaReferenciaC: null);

        acto.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void VidaDeFlotacionConTemperaturaDeReferenciaEsValida()
    {
        var acto = () => new ModeloBateria(
            "mod-bat-1", "fab-1", "12V 9Ah",
            vidaFlotacionAniosMin: 3, vidaFlotacionAniosMax: 5,
            temperaturaReferenciaC: 25);

        acto.Should().NotThrow();
    }

    [Fact]
    public void UnModeloSinVidaDeFlotacionNoRequiereTemperatura()
    {
        var acto = () => new ModeloBateria("mod-bat-1", "fab-1", "12V 9Ah");

        acto.Should().NotThrow();
    }
}
