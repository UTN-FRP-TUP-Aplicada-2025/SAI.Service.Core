using FluentAssertions;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// RN-07 / invariante I-18: todo importe monetario declara su moneda y su fecha.
/// Un <see cref="Dinero"/> no se puede construir sin moneda; la fecha es obligatoria por
/// firma (no hay sobrecarga sin fecha).
/// </summary>
public class DineroTests
{
    [Fact]
    public void ConstruirConMonedaYFechaConservaLosDatos()
    {
        var fecha = new DateOnly(2026, 7, 21);

        var dinero = new Dinero(1500.50m, "ARS", fecha);

        dinero.Monto.Should().Be(1500.50m);
        dinero.Moneda.Should().Be("ARS");
        dinero.Fecha.Should().Be(fecha);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ConstruirSinMonedaLanza(string? moneda)
    {
        var acto = () => new Dinero(100m, moneda!, new DateOnly(2026, 7, 21));

        acto.Should().Throw<ArgumentException>();
    }
}
