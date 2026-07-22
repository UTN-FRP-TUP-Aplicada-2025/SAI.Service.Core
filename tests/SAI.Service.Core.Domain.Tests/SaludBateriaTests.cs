using FluentAssertions;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Método de salud por caída de tensión (BT-21, ADR-13, US-13): tendencia relativa contra la línea
/// base a carga igualada, confianza que arranca baja y sube con cuatro pruebas comparables, y la
/// reserva de temperatura siempre presente. Usa solo battery.voltage medido; ignora las perdidas.
/// </summary>
public class SaludBateriaTests
{
    private const int Tolerancia = 5;
    private static readonly DateTimeOffset T = new(2026, 9, 5, 10, 0, 0, TimeSpan.FromHours(-3));
    private static readonly string[] Esperadas = [.. Variables.ProcedenciaCanonica.Keys];

    [Fact]
    public void LineaBaseComparableSinDegradacionYConfianzaBaja()
    {
        var serie = Serie(carga: 13, 13.2, 12.6, 13.1);

        var r = CalculadorSaludBateria.Evaluar(serie, cargaConcurrente: 13, caidaLineaBase: null, cargaLineaBase: null, Tolerancia, comparablesPrevias: 0);

        r.Comparable.Should().BeTrue("sin línea base, esta prueba la establece");
        r.Veredicto.Should().Be(VeredictoSalud.SinDegradacionDetectable);
        r.Confianza.Should().Be(ConfianzaVeredicto.Baja);
        r.Caida.Contenido.Should().BeApproximately(-0.6, 0.001);
        r.Caida.Origen.Should().Be(Origen.Derivado);
        r.Reserva.Should().Be(CalculadorSaludBateria.ReservaTemperatura);
    }

    [Fact]
    public void CaidaBastantePeorQueLaBaseEsDegradada()
    {
        var serie = Serie(carga: 13, 13.2, 12.5, 13.1); // caída -0.7

        var r = CalculadorSaludBateria.Evaluar(serie, cargaConcurrente: 13, caidaLineaBase: -0.5, cargaLineaBase: 13, Tolerancia, comparablesPrevias: 1);

        r.Comparable.Should().BeTrue();
        r.Veredicto.Should().Be(VeredictoSalud.Degradada, "-0,7 empeora más del 20 % respecto de -0,5");
    }

    [Fact]
    public void CargaConcurrenteFueraDeToleranciaNoEsComparableYNoTieneVeredicto()
    {
        var serie = Serie(carga: 25, 13.2, 12.6, 13.1);

        var r = CalculadorSaludBateria.Evaluar(serie, cargaConcurrente: 25, caidaLineaBase: -0.5, cargaLineaBase: 13, Tolerancia, comparablesPrevias: 1);

        r.Comparable.Should().BeFalse("25 % vs 13 % excede la tolerancia (RN-06)");
        r.Veredicto.Should().BeNull("una prueba no comparable se registra pero no entra en la tendencia");
    }

    [Fact]
    public void LasMuestrasPerdidasSeIgnoranYNoRompenElCalculo()
    {
        var serie = new List<Muestra> { M(13.2, 13), Perdida(), M(12.6, 13), M(13.1, 13) };

        var r = CalculadorSaludBateria.Evaluar(serie, 13, null, null, Tolerancia, 0);

        r.Caida.Contenido.Should().BeApproximately(-0.6, 0.001);
    }

    [Fact]
    public void LaConfianzaSubeConCuatroPruebasComparables()
    {
        var serie = Serie(carga: 13, 13.2, 12.6, 13.1);

        var r = CalculadorSaludBateria.Evaluar(serie, 13, caidaLineaBase: -0.6, cargaLineaBase: 13, Tolerancia, comparablesPrevias: 3);

        r.Confianza.Should().Be(ConfianzaVeredicto.Media, "con 3 previas + esta = 4 comparables");
    }

    [Fact]
    public void SerieInsuficienteEsNoCalculableSinCifraInventada()
    {
        var serie = Serie(carga: 13, 13.2, 12.6); // solo 2 muestras válidas

        var r = CalculadorSaludBateria.Evaluar(serie, 13, null, null, Tolerancia, 0);

        r.Caida.Contenido.Should().BeNull();
        r.Caida.Origen.Should().Be(Origen.NoCalculable);
        r.MotivoNoCalculable.Should().NotBeNull();
        r.Veredicto.Should().BeNull();
    }

    private static List<Muestra> Serie(int carga, params double[] tensionesBateria) =>
        [.. tensionesBateria.Select(t => M(t, carga))];

    private static Muestra M(double tensionBateria, double carga) =>
        Muestra.Registrar($"m{Guid.NewGuid():N}", "ups", "s", T, alcanzable: true,
            new Dictionary<string, double?>
            {
                [Variables.TensionEntrada] = 230,
                [Variables.TensionSalida] = 230,
                [Variables.CargaSalida] = carga,
                [Variables.CargaBateria] = 100,
                [Variables.EstadoUps] = Variables.CodigoEnLinea,
                [Variables.TensionBateria] = tensionBateria,
            },
            Esperadas);

    private static Muestra Perdida() =>
        Muestra.Registrar($"m{Guid.NewGuid():N}", "ups", "s", T, alcanzable: false, new Dictionary<string, double?>(), Esperadas);
}
