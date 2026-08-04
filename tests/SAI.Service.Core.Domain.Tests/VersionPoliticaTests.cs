using FluentAssertions;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Políticas de apagado versionadas (CU-03, US-06, EP-04). Cada versión es historia append-only
/// (<see cref="IEntidadHistoria"/>): se configura creando una <b>versión nueva</b>, no editando la
/// vigente. Defiende por construcción el techo duro de 540 s (RN-04, I-10) y el umbral positivo.
/// </summary>
public class VersionPoliticaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void LaVersionEsHistoriaAppendOnly()
    {
        VersionPolitica.Inicial(Modalidad.SoloAlerta, 300, 120, 180, Ahora).Should().BeAssignableTo<IEntidadHistoria>();
    }

    [Fact]
    public void LaVersionInicialArrancaEnUno()
    {
        var version = VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, 120, 180, Ahora);

        version.Numero.Should().Be(1);
        version.ModalidadSolicitada.Should().Be(Modalidad.ApagarHostConRetorno);
        version.UmbralDisparoSegundos.Should().Be(300);
        version.TiempoReservadoApagadoSeg.Should().Be(120);
        version.VigenteDesde.Should().Be(Ahora);
    }

    [Fact]
    public void SiguienteIncrementaElNumeroSinTocarLaAnterior()
    {
        var v1 = VersionPolitica.Inicial(Modalidad.SoloAlerta, 300, 120, 180, Ahora);

        var v2 = v1.Siguiente(Modalidad.CicloForzado, 200, 300, 240, Ahora.AddDays(1));

        v2.Numero.Should().Be(2);
        v2.ModalidadSolicitada.Should().Be(Modalidad.CicloForzado);
        v1.Numero.Should().Be(1, "la versión anterior no se modifica (append-only)");
        v1.ModalidadSolicitada.Should().Be(Modalidad.SoloAlerta);
    }

    [Fact]
    public void ElTiempoReservadoNoPuedeSuperarElTechoDuro()
    {
        var acto = () => VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, 541, 180, Ahora);

        acto.Should().Throw<ArgumentOutOfRangeException>("el techo duro del apagado es 540 s (RN-04, I-10)");
    }

    [Fact]
    public void ElTiempoReservadoAceptaJustoElTechoDuro()
    {
        var version = VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, Accion.TechoDuroApagadoSeg, 180, Ahora);

        version.TiempoReservadoApagadoSeg.Should().Be(540);
    }

    [Fact]
    public void ElUmbralDeDisparoDebeSerPositivo()
    {
        var acto = () => VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 0, 120, 180, Ahora);

        acto.Should().Throw<ArgumentOutOfRangeException>("el umbral de disparo debe ser positivo");
    }

    [Fact]
    public void ConservaElTiempoDeRetorno()
    {
        VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, 120, 200, Ahora)
            .TiempoRetornoSeg.Should().Be(200);
    }

    [Fact]
    public void ElTiempoDeRetornoDebeSerPositivo()
    {
        var acto = () => VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, 120, 0, Ahora);

        acto.Should().Throw<ArgumentOutOfRangeException>("el tiempo de retorno debe ser positivo");
    }
}
