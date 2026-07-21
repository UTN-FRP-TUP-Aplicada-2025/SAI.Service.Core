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

    private static List<Verificacion> SembrarLasCuatro() =>
        Enum.GetValues<Supuesto>().Select(s => Verificacion.Sembrar($"ver-{s}", s, Ahora)).ToList();
}
