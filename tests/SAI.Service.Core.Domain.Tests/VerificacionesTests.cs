using FluentAssertions;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Verificaciones de supuestos y derivación de la modalidad efectiva (ADR-10, RN-01, RN-02). Al
/// sembrarse quedan en <see cref="EstadoVerificacion.NuncaVerificado"/> y fuerzan
/// <see cref="Modalidad.SoloAlerta"/>; solo con los cuatro verificados se habilita una acción.
/// </summary>
public class VerificacionesTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void SembrarDejaLaVerificacionNuncaVerificada()
    {
        var verificacion = Verificacion.Sembrar("ver-1", Supuesto.PresupuestoDeApagado, Ahora);

        verificacion.Estado.Should().Be(EstadoVerificacion.NuncaVerificado);
        verificacion.Supuesto.Should().Be(Supuesto.PresupuestoDeApagado);
    }

    [Fact]
    public void ConAlgunSupuestoSinVerificarLaModalidadEfectivaEsSoloAlerta()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones[0].Verificar("prueba", "ok", null, Ahora); // solo uno verificado

        var efectiva = EvaluadorModalidad.Efectiva(Modalidad.ApagarHostConRetorno, verificaciones);

        efectiva.Should().Be(Modalidad.SoloAlerta);
        EvaluadorModalidad.Verificados(verificaciones).Should().Be(1);
    }

    [Fact]
    public void ConLosCuatroVerificadosLaModalidadEfectivaEsLaSolicitada()
    {
        var verificaciones = SembrarLasCuatro();
        foreach (var v in verificaciones)
        {
            v.Verificar("prueba", "ok", null, Ahora);
        }

        var efectiva = EvaluadorModalidad.Efectiva(Modalidad.ApagarHostConRetorno, verificaciones);

        efectiva.Should().Be(Modalidad.ApagarHostConRetorno);
    }

    [Fact]
    public void SinVerificacionesLaModalidadEfectivaEsSoloAlerta()
    {
        EvaluadorModalidad.Efectiva(Modalidad.ApagarHostConRetorno, []).Should().Be(Modalidad.SoloAlerta);
    }

    [Fact]
    public void VerificarMarcaElSupuestoComoVerificado()
    {
        var verificacion = Verificacion.Sembrar("ver-1", Supuesto.CorteConRetorno, Ahora);

        verificacion.Verificar("prueba-de-corte", "el host reencendió", null, Ahora);

        verificacion.Estado.Should().Be(EstadoVerificacion.Verificado);
        verificacion.Metodo.Should().Be("prueba-de-corte");
    }

    [Fact]
    public void RefutarBloqueaLaReVerificacion()
    {
        var verificacion = Verificacion.Sembrar("ver-1", Supuesto.ReencendidoPorPlaca, Ahora);

        verificacion.Refutar("ventana", "el host no arrancó solo", Ahora);

        verificacion.Estado.Should().Be(EstadoVerificacion.Refutado);
        var acto = () => verificacion.Verificar("ventana", "reintento", null, Ahora);
        acto.Should().Throw<InvalidOperationException>("un supuesto refutado es un bloqueo permanente");
    }

    [Fact]
    public void UnaVerificacionVencidaNoCuentaYSeVeVencida()
    {
        var verificacion = Verificacion.Sembrar("ver-1", Supuesto.PresupuestoDeApagado, Ahora);
        verificacion.Verificar("ventana", "ok", vigenciaHasta: Ahora.AddDays(-1), Ahora.AddDays(-181));

        verificacion.CuentaComoVerificada(Ahora).Should().BeFalse("la vigencia venció");
        verificacion.EstadoEfectivo(Ahora).Should().Be(EstadoVerificacion.Vencido);
    }

    [Fact]
    public void UnaVerificacionSinCaducidadSigueContando()
    {
        var verificacion = Verificacion.Sembrar("ver-1", Supuesto.CorteConRetorno, Ahora);
        verificacion.Verificar("ventana", "ok", vigenciaHasta: null, Ahora.AddDays(-1000));

        verificacion.CuentaComoVerificada(Ahora).Should().BeTrue("el corte con retorno no caduca");
    }

    [Fact]
    public void SiUnSupuestoEstaVencidoLaModalidadDegrada()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones[0].Verificar("ventana", "ok", vigenciaHasta: Ahora.AddDays(-1), Ahora.AddDays(-2)); // vencida
        for (var i = 1; i < verificaciones.Count; i++)
        {
            verificaciones[i].Verificar("ventana", "ok", null, Ahora);
        }

        EvaluadorModalidad.Efectiva(Modalidad.ApagarHostConRetorno, verificaciones, Ahora).Should().Be(Modalidad.SoloAlerta);
        EvaluadorModalidad.Verificados(verificaciones, Ahora).Should().Be(3);
    }

    private static List<Verificacion> SembrarLasCuatro() =>
        Enum.GetValues<Supuesto>().Select(s => Verificacion.Sembrar($"ver-{s}", s, Ahora)).ToList();
}
