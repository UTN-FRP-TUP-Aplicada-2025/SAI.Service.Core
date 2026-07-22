using FluentAssertions;
using SAI.Service.Core.Domain.Monitoreo;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Derivación de eventos por reglas versionadas (BT-19, BT-20, US-09). Cada escenario arma una
/// ventana de muestras (más reciente primero) y verifica los eventos derivados. El disparo (BT-20)
/// se decide por tiempo en batería y tensión, nunca por el flag LB / battery.charge (ADR-12).
/// </summary>
public class DerivadorEventosTests
{
    private static readonly DateTimeOffset T0 = new(2026, 9, 5, 10, 0, 0, TimeSpan.FromHours(-3));
    private static readonly string[] Esperadas = [.. Variables.ProcedenciaCanonica.Keys];

    private static readonly IReadOnlyDictionary<string, ReglaDerivacion> Reglas =
        new Dictionary<string, ReglaDerivacion>
        {
            [DerivadorEventos.ReglaCorte] = new(DerivadorEventos.ReglaCorte, 1, T0, new Dictionary<string, double> { ["microcorteMaxSeg"] = 60 }),
            [DerivadorEventos.ReglaTension] = new(DerivadorEventos.ReglaTension, 1, T0, new Dictionary<string, double> { ["min"] = 198, ["max"] = 242, ["sostenidoSeg"] = 30 }),
            [DerivadorEventos.ReglaDesconexion] = new(DerivadorEventos.ReglaDesconexion, 1, T0, new Dictionary<string, double> { ["sondeosPerdidos"] = 3 }),
            [DerivadorEventos.ReglaDisparo] = new(DerivadorEventos.ReglaDisparo, 1, T0, new Dictionary<string, double> { ["umbralObSeg"] = 300, ["batVoltMin"] = 13.3, ["batVoltMax"] = 14.5 }),
        };

    private static int _n;
    private static IReadOnlyList<Evento> Derivar(params Muestra[] recientes) =>
        DerivadorEventos.Derivar("ups", recientes, Reglas, () => $"evt-{_n++}");

    [Fact]
    public void TransicionAOBGeneraCorteSuministro()
    {
        var eventos = Derivar(EnBateria(5), EnLinea(0));

        eventos.Should().ContainSingle(e => e.Tipo == TipoEvento.CorteSuministro);
        eventos.Single().ReglaVersion.Should().Be(1, "el evento graba la versión de la regla (RC-09)");
    }

    [Fact]
    public void CorteBreveGeneraRetornoRedYMicrocorte()
    {
        // OB desde t=0 hasta t=10 (10 s < 60 s = microcorte).
        var eventos = Derivar(EnLinea(10), EnBateria(5), EnBateria(0), EnLinea(-5));

        eventos.Should().Contain(e => e.Tipo == TipoEvento.RetornoRed);
        var micro = eventos.Single(e => e.Tipo == TipoEvento.Microcorte);
        micro.DuracionSeg.Should().Be(10);
        micro.IncertidumbreDuracionSeg.Should().NotBeNull("un microcorte lleva incertidumbre estructural (CL-10)");
    }

    [Fact]
    public void CorteLargoGeneraRetornoRedSinMicrocorte()
    {
        // OB de 120 s (> 60): retorno sin microcorte.
        var eventos = Derivar(EnLinea(120), EnBateria(60), EnBateria(0), EnLinea(-5));

        eventos.Should().Contain(e => e.Tipo == TipoEvento.RetornoRed);
        eventos.Should().NotContain(e => e.Tipo == TipoEvento.Microcorte);
    }

    [Fact]
    public void TresSondeosPerdidosGeneranDesconexionUnaSolaVez()
    {
        Derivar(Perdida(15), Perdida(10), Perdida(5), EnLinea(0))
            .Should().ContainSingle(e => e.Tipo == TipoEvento.DesconexionUsb);

        // Cuarto sondeo perdido: no se re-emite.
        Derivar(Perdida(20), Perdida(15), Perdida(10), Perdida(5), EnLinea(0))
            .Should().NotContain(e => e.Tipo == TipoEvento.DesconexionUsb);
    }

    [Fact]
    public void TensionFueraDeRangoSostenidaGeneraEvento()
    {
        // input.voltage = 250 V (> 242) desde t=5; a t=35 lleva 30 s sostenido (cruza el umbral).
        var eventos = Derivar(
            EnLinea(35, tension: 250), EnLinea(30, tension: 250), EnLinea(25, tension: 250),
            EnLinea(20, tension: 250), EnLinea(15, tension: 250), EnLinea(10, tension: 250),
            EnLinea(5, tension: 250), EnLinea(0, tension: 230));

        eventos.Should().ContainSingle(e => e.Tipo == TipoEvento.TensionFueraDeRango);
    }

    [Fact]
    public void ElDisparoSeDecidePorTiempoEnBateriaNoPorElFlagLb()
    {
        // OB sostenido 300 s con battery.charge = 100 (¡nunca LB!) y battery.voltage sano: dispara igual.
        var muestras = new List<Muestra>();
        for (var t = 300; t >= 0; t -= 5)
        {
            muestras.Add(EnBateria(t, tension: 0, batVolt: 13.5));
        }
        muestras.Add(EnLinea(-5));

        var eventos = DerivadorEventos.Derivar("ups", muestras, Reglas, () => $"evt-{_n++}");

        eventos.Should().ContainSingle(e => e.Tipo == TipoEvento.DisparoApagado,
            "el disparo decide por tiempo en batería + tensión, nunca por el flag LB (ADR-12, BT-20)");
    }

    private static Muestra EnLinea(double segundos, double tension = 230) =>
        Construir(segundos, Variables.CodigoEnLinea, tension, 13.5);

    private static Muestra EnBateria(double segundos, double tension = 0, double batVolt = 13.5) =>
        Construir(segundos, Variables.CodigoEnBateria, tension, batVolt);

    private static Muestra Perdida(double segundos) =>
        Muestra.Registrar($"m{_n++}", "ups", "s", T0.AddSeconds(segundos), alcanzable: false,
            new Dictionary<string, double?>(), Esperadas);

    private static Muestra Construir(double segundos, double estado, double tension, double batVolt) =>
        Muestra.Registrar($"m{_n++}", "ups", "s", T0.AddSeconds(segundos), alcanzable: true,
            new Dictionary<string, double?>
            {
                [Variables.EstadoUps] = estado,
                [Variables.TensionEntrada] = tension,
                [Variables.TensionSalida] = tension,
                [Variables.CargaSalida] = 13,
                [Variables.CargaBateria] = 100,
                [Variables.TensionBateria] = batVolt,
            },
            Esperadas);
}
