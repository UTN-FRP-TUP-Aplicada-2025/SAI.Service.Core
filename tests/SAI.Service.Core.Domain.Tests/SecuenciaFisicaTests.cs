using FluentAssertions;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Secuencia física del ejercicio (P-2/P-7): el orden en que los cuatro supuestos ocurren realmente, y la
/// derivación del paso pendiente, que es la posición de un ejercicio guiado (no se persiste).
/// </summary>
public class SecuenciaFisicaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    private static List<Verificacion> SembrarLasCuatro() =>
        Enum.GetValues<Supuesto>().Select(s => Verificacion.Sembrar($"ver-{s}", s, Ahora)).ToList();

    [Fact]
    public void ElOrdenEsElFisicoNoElDeDeclaracionDelEnum()
    {
        SecuenciaFisica.Orden.Should().Equal(
            Supuesto.SenalEnBateria,
            Supuesto.PresupuestoDeApagado,
            Supuesto.CorteConRetorno,
            Supuesto.ReencendidoPorPlaca);
    }

    [Fact]
    public void NumeroDevuelveLaPosicionEnLaSecuencia()
    {
        SecuenciaFisica.Numero(Supuesto.SenalEnBateria).Should().Be(1);
        SecuenciaFisica.Numero(Supuesto.PresupuestoDeApagado).Should().Be(2);
        SecuenciaFisica.Numero(Supuesto.CorteConRetorno).Should().Be(3);
        SecuenciaFisica.Numero(Supuesto.ReencendidoPorPlaca).Should().Be(4);
    }

    [Fact]
    public void SinNadaVerificadoElPasoPendienteEsElPrimero()
    {
        SecuenciaFisica.PrimeroPendiente(SembrarLasCuatro(), Ahora)
            .Should().Be(Supuesto.SenalEnBateria);
    }

    [Fact]
    public void ElPasoPendienteAvanzaAlVerificarEnOrden()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones.Single(v => v.Supuesto == Supuesto.SenalEnBateria)
            .Verificar("ventana", "ok", Ahora.AddDays(365), Ahora);

        SecuenciaFisica.PrimeroPendiente(verificaciones, Ahora)
            .Should().Be(Supuesto.PresupuestoDeApagado);
    }

    [Fact]
    public void UnaVerificacionVencidaVuelveASerElPasoPendiente()
    {
        var verificaciones = SembrarLasCuatro();
        foreach (var v in verificaciones)
        {
            v.Verificar("ventana", "ok", Ahora.AddDays(365), Ahora);
        }
        // La segunda del orden físico vence: el ejercicio vuelve a ese paso.
        verificaciones.Single(v => v.Supuesto == Supuesto.PresupuestoDeApagado)
            .Verificar("ventana", "ok", Ahora.AddDays(-1), Ahora.AddDays(-200));

        SecuenciaFisica.PrimeroPendiente(verificaciones, Ahora)
            .Should().Be(Supuesto.PresupuestoDeApagado);
    }

    [Fact]
    public void ConLasCuatroVigentesNoHayPasoPendiente()
    {
        var verificaciones = SembrarLasCuatro();
        foreach (var v in verificaciones)
        {
            v.Verificar("ventana", "ok", Ahora.AddDays(365), Ahora);
        }

        SecuenciaFisica.PrimeroPendiente(verificaciones, Ahora).Should().BeNull();
    }
}
