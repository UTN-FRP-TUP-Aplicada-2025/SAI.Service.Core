using FluentAssertions;
using SAI.Service.Core.Domain.Vinculos;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Resolución temporal de la batería/dispositivo vigente (RC-07). La historia guarda dispositivo +
/// instante; la unidad vigente se resuelve por el vínculo que contiene el instante, con contención
/// semiabierta. Cubre los casos de CL-17 (retiro y reinstalación; movimiento a otro SAI).
/// </summary>
public class ResolutorTemporalTests
{
    private static readonly TimeSpan Ar = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset T0 = new(2024, 11, 20, 0, 0, 0, Ar);
    private static readonly DateTimeOffset Corte = new(2026, 9, 5, 10, 30, 0, Ar);
    private static readonly DateTimeOffset T2 = new(2027, 3, 1, 0, 0, 0, Ar);

    [Fact]
    public void ResuelveElMontajeQueContieneElInstante()
    {
        var montaje = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, Corte));

        var resuelto = ResolutorTemporal.ResolverMontaje([montaje], "disp-1", "principal", T0.AddDays(30));

        resuelto.Should().BeSameAs(montaje);
    }

    [Fact]
    public void EnElInstanteDeCorteResuelveElMontajeSucesorNoElQueCierra()
    {
        var previo = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, Corte));
        var sucesor = new MontajeBateria("mnt-002", "bat-b", "disp-1", "principal", new Vigencia(Corte, null));

        var resuelto = ResolutorTemporal.ResolverMontaje([previo, sucesor], "disp-1", "principal", Corte);

        resuelto.Should().BeSameAs(sucesor, "la contención es semiabierta: el instante de cierre pertenece al vínculo nuevo (RC-07)");
        resuelto!.BateriaCodigo.Should().Be("bat-b");
    }

    // CL-17: retiro para prueba y reinstalación de la MISMA batería en dos períodos.
    [Fact]
    public void RetiroYReinstalacionResuelveLaBateriaCorrectaEnCadaPeriodo()
    {
        var primerPeriodo = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, Corte));
        var reinstalada = new MontajeBateria("mnt-002", "bat-a", "disp-1", "principal", new Vigencia(T2, null));
        var montajes = new[] { primerPeriodo, reinstalada };

        // Entre el retiro (Corte) y la reinstalación (T2) no hay batería montada: hueco legítimo.
        ResolutorTemporal.ResolverMontaje(montajes, "disp-1", "principal", Corte.AddDays(1)).Should().BeNull();
        ResolutorTemporal.ResolverMontaje(montajes, "disp-1", "principal", T0.AddDays(1)).Should().BeSameAs(primerPeriodo);
        ResolutorTemporal.ResolverMontaje(montajes, "disp-1", "principal", T2.AddDays(1)).Should().BeSameAs(reinstalada);
    }

    // CL-17: la misma batería se mueve a otro SAI.
    [Fact]
    public void MovimientoAOtroSaiResuelvePorDispositivo()
    {
        var enDisp1 = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, Corte));
        var enDisp2 = new MontajeBateria("mnt-002", "bat-a", "disp-2", "principal", new Vigencia(Corte, null));
        var montajes = new[] { enDisp1, enDisp2 };

        ResolutorTemporal.ResolverMontaje(montajes, "disp-1", "principal", T0.AddDays(1)).Should().BeSameAs(enDisp1);
        ResolutorTemporal.ResolverMontaje(montajes, "disp-2", "principal", T2).Should().BeSameAs(enDisp2);
        ResolutorTemporal.ResolverMontaje(montajes, "disp-1", "principal", T2).Should().BeNull("bat-a ya no está en disp-1");
    }

    [Fact]
    public void SinMontajeEnElInstanteDevuelveNull()
    {
        var montaje = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, Corte));

        ResolutorTemporal.ResolverMontaje([montaje], "disp-1", "principal", T0.AddDays(-1)).Should().BeNull();
    }

    [Fact]
    public void DosMontajesQueContienenElMismoInstanteEsUnSolapeYLanza()
    {
        // Estado imposible que la exclusividad temporal (I-1) debió impedir al escribir.
        var a = new MontajeBateria("mnt-001", "bat-a", "disp-1", "principal", new Vigencia(T0, null));
        var b = new MontajeBateria("mnt-002", "bat-b", "disp-1", "principal", new Vigencia(Corte, null));

        var acto = () => ResolutorTemporal.ResolverMontaje([a, b], "disp-1", "principal", T2);

        acto.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ResuelveLaCoberturaVigenteDelHost()
    {
        var cobertura = new CoberturaHost("cob-001", "disp-1", "host-1", new Vigencia(T0, null));

        var resuelta = ResolutorTemporal.ResolverCobertura([cobertura], "host-1", T2);

        resuelta.Should().BeSameAs(cobertura);
    }

    [Fact]
    public void HostSinCoberturaEnElInstanteDevuelveNull()
    {
        var cobertura = new CoberturaHost("cob-001", "disp-1", "host-1", new Vigencia(T0, Corte));

        ResolutorTemporal.ResolverCobertura([cobertura], "host-1", T2)
            .Should().BeNull("tras el cierre el host queda sin protección: días sin protección");
    }
}
