using FluentAssertions;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Registro de acciones de apagado (CU-05, US-14, US-15). Cada disparo deja una <see cref="Accion"/>
/// append-only que guarda la modalidad solicitada, la efectiva tras el bloqueo por verificación
/// (RN-02) y el resultado por efecto observado (ADR-11). El tiempo reservado nunca supera 540 s (RN-04).
/// </summary>
public class AccionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void LaAccionEsHistoriaAppendOnly()
    {
        Accion.SoloAviso("acc-1", "ups", Ahora, "aviso").Should().BeAssignableTo<IEntidadHistoria>();
    }

    [Fact]
    public void BloqueadaDegradaLaEfectivaASoloAlerta()
    {
        var accion = Accion.Bloqueada("acc-1", "ups", Modalidad.ApagarHostConRetorno, Ahora, "3/4 verificados");

        accion.Estado.Should().Be(EstadoAccion.BloqueadaPorVerificacion);
        accion.ModalidadSolicitada.Should().Be(Modalidad.ApagarHostConRetorno);
        accion.ModalidadEfectiva.Should().Be(Modalidad.SoloAlerta, "el bloqueo por verificación degrada a solo aviso (RN-02)");
        accion.TiempoReservadoSeg.Should().Be(0);
    }

    [Fact]
    public void EjecutadaConservaLaModalidadYElTiempoReservado()
    {
        var accion = Accion.Ejecutada("acc-1", "ups", Modalidad.CicloForzado, Modalidad.CicloForzado, 120, Ahora, "el equipo admitió la orden");

        accion.Estado.Should().Be(EstadoAccion.Ejecutada);
        accion.ModalidadEfectiva.Should().Be(Modalidad.CicloForzado);
        accion.TiempoReservadoSeg.Should().Be(120);
    }

    [Fact]
    public void ElTiempoReservadoNoPuedeSuperarElTechoDuro()
    {
        var acto = () => Accion.Ejecutada("acc-1", "ups", Modalidad.ApagarHostConRetorno, Modalidad.ApagarHostConRetorno, 541, Ahora, "ok");

        acto.Should().Throw<ArgumentOutOfRangeException>("el techo duro del apagado es 540 s (RN-04, I-10)");
    }

    [Fact]
    public void ElTiempoReservadoAceptaJustoElTechoDuro()
    {
        var accion = Accion.Ejecutada("acc-1", "ups", Modalidad.ApagarHostConRetorno, Modalidad.ApagarHostConRetorno, Accion.TechoDuroApagadoSeg, Ahora, "ok");

        accion.TiempoReservadoSeg.Should().Be(540);
    }
}
