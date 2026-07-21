using FluentAssertions;
using SAI.Service.Core.Domain.Vinculos;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Invariantes de vigencia I-1, I-2, I-3 e I-4 (ADR-05, RC-02, RC-03) más la semántica del
/// intervalo semiabierto. Cada invariante corre como prueba (mitigación del riesgo R-10).
/// </summary>
public class VigenciaTests
{
    private static readonly TimeSpan Ar = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset Inicio = new(2024, 11, 20, 0, 0, 0, Ar);
    // Instante exacto de cierre/apertura de TC-03.
    private static readonly DateTimeOffset Corte = new(2026, 9, 5, 10, 30, 0, Ar);
    private static readonly DateTimeOffset Despues = new(2027, 1, 1, 0, 0, 0, Ar);

    [Fact]
    public void ConstruirConHastaAnteriorADesdeLanza()
    {
        var acto = () => new Vigencia(Corte, Inicio);

        acto.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ContieneEsSemiabiertoDesdeInclusiveHastaExclusivo()
    {
        var vigencia = new Vigencia(Inicio, Corte);

        vigencia.Contiene(Inicio).Should().BeTrue("el inicio es inclusivo");
        vigencia.Contiene(Corte).Should().BeFalse("el fin es exclusivo: el instante de cierre pertenece al vínculo siguiente (RC-07)");
        vigencia.Contiene(Inicio.AddDays(-1)).Should().BeFalse();
        vigencia.Contiene(Inicio.AddDays(1)).Should().BeTrue();
    }

    [Fact]
    public void UnaVigenciaAbiertaContieneTodoInstantePosteriorADesde()
    {
        var abierta = new Vigencia(Inicio);

        abierta.EsVigente.Should().BeTrue();
        abierta.Contiene(Despues).Should().BeTrue();
    }

    // I-3: cierre y apertura sin hueco — tocarse en el borde NO es solape.
    [Fact]
    public void DosVigenciasQueSeTocanEnElBordeNoSolapan()
    {
        var primera = new Vigencia(Inicio, Corte);
        var segunda = new Vigencia(Corte, Despues);

        primera.Solapa(segunda).Should().BeFalse();
        primera.Hasta.Should().Be(segunda.Desde, "es exactamente la sucesión sin hueco de RC-03 (I-3)");
    }

    [Fact]
    public void DosVigenciasQueSeSuperponenSolapan()
    {
        var primera = new Vigencia(Inicio, Despues);
        var segunda = new Vigencia(Corte, Despues.AddDays(10));

        primera.Solapa(segunda).Should().BeTrue();
    }

    [Fact]
    public void CerrarEnCierraLaVigenciaAbierta()
    {
        var abierta = new Vigencia(Inicio);

        var cerrada = abierta.CerrarEn(Corte);

        cerrada.EsVigente.Should().BeFalse();
        cerrada.Hasta.Should().Be(Corte);
    }

    [Fact]
    public void CerrarUnaVigenciaYaCerradaLanza()
    {
        var cerrada = new Vigencia(Inicio, Corte);

        var acto = () => cerrada.CerrarEn(Despues);

        acto.Should().Throw<InvalidOperationException>();
    }

    // I-1 / I-2: montajes sin solape por (dispositivo, posición); a lo sumo uno vigente.
    [Fact]
    public void MontajeSolapadoEnLaMismaPosicionNoEsAdmitido()
    {
        var existente = Montaje("mnt-001", new Vigencia(Inicio, Despues));
        var solapado = Montaje("mnt-002", new Vigencia(Corte, Despues.AddDays(30)));

        Vigencias.AdmiteNuevo(solapado, [existente]).Should().BeFalse();
    }

    [Fact]
    public void SegundoMontajeVigenteEnLaMismaPosicionNoEsAdmitido()
    {
        var abierto = Montaje("mnt-001", new Vigencia(Inicio));
        var otroAbierto = Montaje("mnt-002", new Vigencia(Corte));

        Vigencias.AdmiteNuevo(otroAbierto, [abierto]).Should().BeFalse("a lo sumo un montaje vigente por posición (I-2)");
    }

    [Fact]
    public void MontajeSucesorSinHuecoEsAdmitido()
    {
        var previo = Montaje("mnt-001", new Vigencia(Inicio, Corte));
        var sucesor = Montaje("mnt-002", new Vigencia(Corte, Despues));

        Vigencias.AdmiteNuevo(sucesor, [previo]).Should().BeTrue("cerrar y abrir en el mismo instante no solapa (I-3)");
    }

    [Fact]
    public void MontajeEnOtraPosicionNoCompiteConElExistente()
    {
        var principal = Montaje("mnt-001", new Vigencia(Inicio), posicion: "principal");
        var secundaria = Montaje("mnt-002", new Vigencia(Inicio), posicion: "secundaria");

        Vigencias.AdmiteNuevo(secundaria, [principal]).Should().BeTrue("la exclusividad es por (dispositivo, posición)");
    }

    // I-4: coberturas sin solape por host; a lo sumo una vigente.
    [Fact]
    public void CoberturaSolapadaDelMismoHostNoEsAdmitida()
    {
        var existente = Cobertura("cob-001", "host-1", new Vigencia(Inicio));
        var otra = Cobertura("cob-002", "host-1", new Vigencia(Corte));

        Vigencias.AdmiteNuevo(otra, [existente]).Should().BeFalse();
    }

    [Fact]
    public void CoberturaDeOtroHostNoCompite()
    {
        var host1 = Cobertura("cob-001", "host-1", new Vigencia(Inicio));
        var host2 = Cobertura("cob-002", "host-2", new Vigencia(Inicio));

        Vigencias.AdmiteNuevo(host2, [host1]).Should().BeTrue();
    }

    private static MontajeBateria Montaje(string codigo, Vigencia vigencia, string posicion = "principal") =>
        new(codigo, bateriaCodigo: "bat-1", dispositivoCodigo: "disp-1", posicion, vigencia);

    private static CoberturaHost Cobertura(string codigo, string host, Vigencia vigencia) =>
        new(codigo, dispositivoCodigo: "disp-1", hostCodigo: host, vigencia);
}
