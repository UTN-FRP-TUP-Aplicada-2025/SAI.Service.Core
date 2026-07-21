using FluentAssertions;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Invariante I-7 (procedencia obligatoria, ADR-06 / RN-05): mitigación del riesgo R-10
/// (los invariantes corren como pruebas). Un <see cref="Valor{T}"/> no se puede construir
/// sin declarar su <see cref="Origen"/>.
/// <para>
/// La exigencia "de menos" (omitir el origen) es de tiempo de compilación: no existe una
/// sobrecarga <c>new Valor&lt;T&gt;(contenido)</c>. Acá se prueba el camino de tiempo de
/// ejecución (origen no definido) y el camino feliz.
/// </para>
/// </summary>
public class ValorConOrigenTests
{
    [Fact]
    public void ConstruirConOrigenValidoConservaContenidoYOrigen()
    {
        var valor = new Valor<double>(23.5, Origen.Medido);

        valor.Contenido.Should().Be(23.5);
        valor.Origen.Should().Be(Origen.Medido);
    }

    [Fact]
    public void ConstruirConOrigenNoDefinidoLanza()
    {
        // El 0 (default del enum) no corresponde a ninguna procedencia: I-7 lo rechaza.
        var origenInvalido = (Origen)0;

        var acto = () => new Valor<int>(10, origenInvalido);

        acto.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConstruirConValorFueraDelRangoLanza()
    {
        var acto = () => new Valor<int>(10, (Origen)99);

        acto.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(Origen.Medido)]
    [InlineData(Origen.Derivado)]
    [InlineData(Origen.Declarado)]
    [InlineData(Origen.Imputado)]
    [InlineData(Origen.EstimadoPorDriver)]
    [InlineData(Origen.NoCalculable)]
    public void AceptaTodasLasProcedenciasCanonicas(Origen origen)
    {
        var acto = () => new Valor<string>("dato", origen);

        acto.Should().NotThrow();
    }

    [Fact]
    public void DosValoresConMismoContenidoYOrigenSonIguales()
    {
        var a = new Valor<int>(5, Origen.Declarado);
        var b = new Valor<int>(5, Origen.Declarado);

        a.Should().Be(b);
    }
}
