using FluentAssertions;
using MudBlazor;
using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Retroalimentación en vivo del panel (mejora UX): traduce la lectura del SAI a un cartel legible, sin
/// jerga. No verifica nada; solo informa lo que el sistema ve, para que confirmar la señal no sea a ciegas.
/// </summary>
public class DescriptorEnVivoTests
{
    private static EstadoSai Estado(bool alcanzable, EstadoUps? ups, double? entrada = 220, double? bateria = 100) =>
        new(alcanzable, entrada, 220, 35, bateria, ups, 13.2, DateTimeOffset.UnixEpoch);

    [Fact]
    public void SinLecturaTodaviaMuestraQueEstaLeyendo()
    {
        var vista = DescriptorEnVivo.Describir(null);

        vista.Titulo.Should().Contain("Leyendo");
        vista.EnBateria.Should().BeFalse();
    }

    [Fact]
    public void EquipoNoAlcanzableSeMuestraComoSinLectura()
    {
        var vista = DescriptorEnVivo.Describir(Estado(alcanzable: false, ups: null));

        vista.Titulo.Should().Be("Sin lectura del equipo");
        vista.Tono.Should().Be(Color.Error);
    }

    [Fact]
    public void EnBateriaLoDiceEnLenguajeLlanoYEnAmbar()
    {
        var vista = DescriptorEnVivo.Describir(Estado(alcanzable: true, ups: EstadoUps.EnBateria, entrada: 8.5, bateria: 71));

        vista.Titulo.Should().Be("El equipo está en batería ahora");
        vista.EnBateria.Should().BeTrue();
        vista.Tono.Should().Be(Color.Warning, "el ámbar codifica el estado (5.1)");
        vista.Detalle.Should().Contain("8").And.Contain("71");
    }

    [Fact]
    public void EnLineaSeMuestraEnVerde()
    {
        var vista = DescriptorEnVivo.Describir(Estado(alcanzable: true, ups: EstadoUps.EnLinea));

        vista.Titulo.Should().Be("El equipo está en línea");
        vista.EnBateria.Should().BeFalse();
        vista.Tono.Should().Be(Color.Success);
    }
}
