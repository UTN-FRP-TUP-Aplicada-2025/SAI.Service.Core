using FluentAssertions;
using SAI.Service.Core.Application.Acciones;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Constructor de la vista del panel (rediseño UX, SPEC 4.1/4.3): el orden físico de las pruebas (P-2),
/// el estado efectivo con preaviso, la evidencia comparada (H-7) y el modo del servicio se calculan en un
/// solo lugar, fuera del markup.
/// </summary>
public class ConstructorVistaPanelTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    private static ConstructorVistaPanel Constructor(int preaviso = 30, int reservado = 120) =>
        new(new OpcionesVerificacion { DiasPreavisoVencimiento = preaviso },
            new OpcionesApagado { TiempoReservadoSeg = reservado });

    private static List<Verificacion> SembrarLasCuatro() =>
        Enum.GetValues<Supuesto>().Select(s => Verificacion.Sembrar($"ver-ups-{s}", s, Ahora)).ToList();

    [Fact]
    public void LasTarjetasSiguenLaSecuenciaFisicaNoElOrdenDelEnum()
    {
        var (_, tarjetas) = Constructor().Construir(SembrarLasCuatro(), Ahora);

        tarjetas.Select(t => t.Supuesto).Should().Equal(
            Supuesto.SenalEnBateria,
            Supuesto.PresupuestoDeApagado,
            Supuesto.CorteConRetorno,
            Supuesto.ReencendidoPorPlaca);
        tarjetas.Select(t => t.Numero).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void ElOrdenNoDependeDelEstado()
    {
        var verificaciones = SembrarLasCuatro();
        // Se verifica la primera de la secuencia: no debe reordenarse.
        verificaciones.Single(v => v.Supuesto == Supuesto.SenalEnBateria)
            .Verificar("ventana", "ok", Ahora.AddDays(365), Ahora);

        var (_, tarjetas) = Constructor().Construir(verificaciones, Ahora);

        tarjetas[0].Supuesto.Should().Be(Supuesto.SenalEnBateria, "el orden físico es fijo (P-2)");
    }

    [Fact]
    public void SinLasCuatroVigentesElModoEsSoloAviso()
    {
        var (modo, _) = Constructor().Construir(SembrarLasCuatro(), Ahora);

        modo.Modo.Should().Be(ModoServicio.SoloAviso);
        modo.Verificados.Should().Be(0);
        modo.Total.Should().Be(4);
    }

    [Fact]
    public void ConLasCuatroVigentesElModoEsApagadoAutomatico()
    {
        var verificaciones = SembrarLasCuatro();
        foreach (var v in verificaciones)
        {
            v.Verificar("ventana", "ok", Ahora.AddDays(365), Ahora);
        }

        var (modo, _) = Constructor().Construir(verificaciones, Ahora);

        modo.Modo.Should().Be(ModoServicio.ApagadoAutomatico);
        modo.Verificados.Should().Be(4);
    }

    [Fact]
    public void LaEvidenciaDelApagadoSeExponeComparadaContraLaVentanaReservada()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones.Single(v => v.Supuesto == Supuesto.PresupuestoDeApagado)
            .Verificar("ventana", "cronometrado a mano", Ahora.AddDays(180), Ahora, medicionSegundos: 20);

        var (_, tarjetas) = Constructor(reservado: 120).Construir(verificaciones, Ahora);

        var apagado = tarjetas.Single(t => t.Supuesto == Supuesto.PresupuestoDeApagado);
        apagado.MedidoSeg.Should().Be(20);
        apagado.ReservadoSeg.Should().Be(120);
        apagado.MargenHolgado.Should().BeTrue();
    }

    [Fact]
    public void SoloElApagadoTieneVentanaReservada()
    {
        var (_, tarjetas) = Constructor().Construir(SembrarLasCuatro(), Ahora);

        tarjetas.Where(t => t.Supuesto != Supuesto.PresupuestoDeApagado)
            .Should().OnlyContain(t => t.ReservadoSeg == null);
    }

    [Fact]
    public void UnaVigenciaCercanaSeVePorVencerYLaTarjetaQuedaColapsada()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones.Single(v => v.Supuesto == Supuesto.SenalEnBateria)
            .Verificar("ventana", "ok", Ahora.AddDays(5), Ahora);

        var (_, tarjetas) = Constructor(preaviso: 30).Construir(verificaciones, Ahora);

        var senal = tarjetas.Single(t => t.Supuesto == Supuesto.SenalEnBateria);
        senal.Estado.Should().Be(EstadoVerificacion.PorVencer);
        senal.Colapsada.Should().BeTrue("vigente y por vencer se muestran colapsadas (P-4)");
        senal.DiasRestantes.Should().Be(5);
    }

    [Fact]
    public void UnaPendienteSeMuestraExpandida()
    {
        var (_, tarjetas) = Constructor().Construir(SembrarLasCuatro(), Ahora);

        tarjetas.Should().OnlyContain(t => !t.Colapsada, "sin verificar, todas van expandidas (P-4)");
    }

    [Fact]
    public void ElChipDeRiesgoSoloAplicaAlCorteConRetorno()
    {
        var (_, tarjetas) = Constructor().Construir(SembrarLasCuatro(), Ahora);

        tarjetas.Single(t => t.CortaCorrienteReal).Supuesto.Should().Be(Supuesto.CorteConRetorno);
        tarjetas.Single(t => t.DisparaApagado).Supuesto.Should().Be(Supuesto.PresupuestoDeApagado);
    }

    [Fact]
    public void UnRefutadoSeReportaEnElEncabezado()
    {
        var verificaciones = SembrarLasCuatro();
        verificaciones.Single(v => v.Supuesto == Supuesto.ReencendidoPorPlaca)
            .Refutar("ventana", "no arrancó solo", Ahora);

        var (modo, _) = Constructor().Construir(verificaciones, Ahora);

        modo.HayRefutado.Should().BeTrue();
        modo.Modo.Should().Be(ModoServicio.SoloAviso);
    }
}
