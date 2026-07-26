using FluentAssertions;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Intervención ingresada por la API externa (CU-11). Es historia append-only con la clave de idempotencia
/// y la huella del cuerpo; exige por construcción que los costos cuadren (RN-08) y conserva los dos tiempos.
/// </summary>
public class IntervencionIngeridaTests
{
    private static readonly DateTimeOffset Valido = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Registrado = new(2026, 6, 2, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Fecha = new(2026, 6, 1);

    private static Costos CostosCuadrados(decimal total) =>
        new(new Dinero(total, "ARS", Fecha), new Dinero(0m, "ARS", Fecha), new Dinero(total, "ARS", Fecha));

    private static IntervencionIngerida Construir(Costos costos, DisposicionFinal? disposicion = null) =>
        new("ing-1", "gmao-ext-ot-88213", "sha256:abc", "fd-gmao-externo", ConfianzaFuente.Media,
            "ti-inspeccion", "ups-01", ["bat-1"], "prov-taller", costos, "inspección de rutina", disposicion, Valido, Registrado);

    [Fact]
    public void EsHistoriaAppendOnly()
    {
        Construir(CostosCuadrados(12000m)).Should().BeAssignableTo<IEntidadHistoria>();
    }

    [Fact]
    public void ConservaClaveHuellaConfianzaYLosDosTiempos()
    {
        var i = Construir(CostosCuadrados(12000m));

        i.ClaveIdempotencia.Should().Be("gmao-ext-ot-88213");
        i.HuellaCuerpo.Should().Be("sha256:abc");
        i.Confianza.Should().Be(ConfianzaFuente.Media);
        i.TiempoValido.Should().Be(Valido);
        i.TiempoRegistrado.Should().Be(Registrado);
        i.Total.Monto.Should().Be(12000m);
        i.Baterias.Should().Equal("bat-1");
    }

    [Fact]
    public void RechazaCostosQueNoCuadran()
    {
        var noCuadra = new Costos(new Dinero(52000m, "ARS", Fecha), new Dinero(15000m, "ARS", Fecha), new Dinero(60000m, "ARS", Fecha));

        var acto = () => Construir(noCuadra);

        acto.Should().Throw<ArgumentException>("los costos deben cuadrar (RN-08)");
    }

    [Fact]
    public void LaDisposicionSeComponeDeSusColumnas()
    {
        var conDisposicion = Construir(CostosCuadrados(12000m), new DisposicionFinal("reciclado", "gestor-sur"));
        conDisposicion.Disposicion!.Value.Destino.Should().Be("reciclado");

        var sinDisposicion = Construir(CostosCuadrados(12000m));
        sinDisposicion.Disposicion.Should().BeNull();
    }
}
