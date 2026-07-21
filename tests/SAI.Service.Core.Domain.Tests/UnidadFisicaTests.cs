using FluentAssertions;
using SAI.Service.Core.Domain.Inventario;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Invariantes del ciclo de vida de las unidades físicas: baja lógica consultable (I-5), la máquina
/// de estados no salta pasos (I-6) y la coherencia temporal de la baja (RC-08). Cada uno corre como
/// prueba (mitigación del riesgo R-10).
/// </summary>
public class UnidadFisicaTests
{
    private static readonly TimeSpan Ar = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset FechaBaja = new(2026, 9, 5, 10, 30, 0, Ar);

    private static Bateria NuevaBateria() => new("bat-2024-a", "mod-bat-1");

    [Fact]
    public void NuevaUnidadArrancaEnStock()
    {
        NuevaBateria().Estado.Should().Be(EstadoUnidad.EnStock);
    }

    // I-6: EnStock -> EnServicio es válida.
    [Fact]
    public void PonerEnServicioDesdeStockEsValido()
    {
        var bateria = NuevaBateria();

        bateria.PonerEnServicio();

        bateria.Estado.Should().Be(EstadoUnidad.EnServicio);
    }

    // I-6: una transición que salta pasos falla (EnStock -> EnReparacion).
    [Fact]
    public void TransicionQueSaltaPasosLanza()
    {
        var bateria = NuevaBateria();

        var acto = () => bateria.EnviarAReparacion();

        acto.Should().Throw<InvalidOperationException>("no se puede ir de EnStock a EnReparacion sin pasar por EnServicio (I-6)");
    }

    // I-6: ciclo de reparación válido EnServicio -> EnReparacion -> EnServicio.
    [Fact]
    public void CicloDeReparacionEsValido()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();

        bateria.EnviarAReparacion();
        bateria.Estado.Should().Be(EstadoUnidad.EnReparacion);

        bateria.PonerEnServicio();
        bateria.Estado.Should().Be(EstadoUnidad.EnServicio);
    }

    // RC-08: la baja es coherente — DadoDeBaja implica fecha y motivo.
    [Fact]
    public void DarDeBajaRegistraEstadoFechaYMotivoDeFormaCoherente()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();

        bateria.DarDeBaja(FechaBaja, "FinDeVidaUtil");

        bateria.Estado.Should().Be(EstadoUnidad.DadoDeBaja);
        bateria.FechaBaja.Should().Be(FechaBaja);
        bateria.MotivoBaja.Should().Be("FinDeVidaUtil");
    }

    [Fact]
    public void DarDeBajaExigeMotivo()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();

        var acto = () => bateria.DarDeBaja(FechaBaja, "  ");

        acto.Should().Throw<ArgumentException>();
    }

    // I-5: una unidad dada de baja sigue siendo consultable con toda su información.
    [Fact]
    public void UnidadDadaDeBajaSigueConsultable()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();
        bateria.DarDeBaja(FechaBaja, "FinDeVidaUtil");

        bateria.Codigo.Should().Be("bat-2024-a");
        bateria.ModeloBateriaCodigo.Should().Be("mod-bat-1");
        bateria.MotivoBaja.Should().Be("FinDeVidaUtil");
    }

    // DadoDeBaja es terminal: no admite más transiciones (I-6).
    [Fact]
    public void UnaUnidadDadaDeBajaNoTransicionaMas()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();
        bateria.DarDeBaja(FechaBaja, "FinDeVidaUtil");

        var acto = () => bateria.PonerEnServicio();

        acto.Should().Throw<InvalidOperationException>();
    }

    // RC-08: coherencia temporal — no se admiten operaciones fechadas después de la baja.
    [Fact]
    public void NoAdmiteOperacionFechadaDespuesDeLaBaja()
    {
        var bateria = NuevaBateria();
        bateria.PonerEnServicio();
        bateria.DarDeBaja(FechaBaja, "FinDeVidaUtil");

        bateria.AdmiteOperacionEn(FechaBaja.AddDays(1)).Should().BeFalse();
        bateria.AdmiteOperacionEn(FechaBaja.AddDays(-1)).Should().BeTrue("una operación anterior a la baja es válida");
    }

    [Fact]
    public void UnaUnidadEnServicioAdmiteOperacionesEnCualquierInstante()
    {
        var host = new Host("host-1");
        host.PonerEnServicio();

        host.AdmiteOperacionEn(FechaBaja.AddYears(5)).Should().BeTrue();
    }
}
