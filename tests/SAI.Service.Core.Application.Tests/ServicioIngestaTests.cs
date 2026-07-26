using FluentAssertions;
using SAI.Service.Core.Application.Ingesta;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Application.Tests;

/// <summary>
/// Ingesta idempotente de intervenciones (CU-11, US-21/US-22). Cubre los seis códigos de resultado que el
/// endpoint mapea a 201/200/409/422: creado, reintento, conflicto por huella (RN-09), cuadre y dinero
/// incompleto (RN-08/RN-07), coherencia temporal (RN-12) y fuente no registrada.
/// </summary>
public class ServicioIngestaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 6, 2, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Valido = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Fecha = new(2026, 6, 1);

    private static DineroEntrada D(decimal monto, string? moneda = "ARS", DateOnly? fecha = null) =>
        new(monto, moneda, fecha ?? Fecha);

    private static EntradaIngesta Entrada(CostosEntrada? costos = null) =>
        new("ti-inspeccion", "ups-01", ["bat-1"], "prov-taller", Valido,
            costos ?? new CostosEntrada([], D(12000m), D(12000m)), "inspección", null);

    // CA-01: clave nueva y cuerpo válido → creado con confianza media.
    [Fact]
    public async Task ClaveNuevaYCuerpoValidoCrea()
    {
        var repo = new RepositorioFalso();
        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(), "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.Creado);
        resultado.Id.Should().NotBeNullOrEmpty();
        resultado.Confianza.Should().Be("media");
        repo.Guardadas.Should().ContainSingle();
    }

    // CA-02: misma clave con la misma huella → reintento, mismo id, sin duplicar.
    [Fact]
    public async Task MismaClaveMismaHuellaEsReintento()
    {
        var repo = new RepositorioFalso();
        await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(), "sha256:a", Ahora, default);
        var idOriginal = repo.Guardadas[0].Codigo;

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(), "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.Reintento);
        resultado.Id.Should().Be(idOriginal);
        repo.Guardadas.Should().ContainSingle("no se duplica el registro (RN-09)");
    }

    // CA-03: misma clave con huella distinta → conflicto con ambas huellas, nunca aplicado.
    [Fact]
    public async Task MismaClaveHuellaDistintaEsConflicto()
    {
        var repo = new RepositorioFalso();
        await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(), "sha256:a", Ahora, default);

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(), "sha256:b", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.ConflictoIdempotencia);
        resultado.HuellaOriginal.Should().Be("sha256:a");
        resultado.HuellaRecibida.Should().Be("sha256:b");
        resultado.AccionSugerida.Should().NotBeNullOrEmpty();
        repo.Guardadas.Should().ContainSingle("un conflicto no crea un segundo registro");
    }

    // CA-04: costos que no cuadran → validación (RN-08).
    [Fact]
    public async Task CostosQueNoCuadranSeRechazan()
    {
        var repo = new RepositorioFalso();
        var costos = new CostosEntrada([D(52000m)], D(15000m), D(60000m)); // 52000+15000 ≠ 60000

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(costos), "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.Validacion);
        resultado.Invariante.Should().Be("validacion");
        repo.Guardadas.Should().BeEmpty();
    }

    // Dinero sin moneda → validación (RN-07).
    [Fact]
    public async Task DineroSinMonedaSeRechaza()
    {
        var repo = new RepositorioFalso();
        var costos = new CostosEntrada([], D(12000m, moneda: ""), D(12000m));

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", Entrada(costos), "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.Validacion);
        repo.Guardadas.Should().BeEmpty();
    }

    // CA-05: intervención fechada después de la baja de una batería → coherencia temporal (RN-12).
    [Fact]
    public async Task IntervencionPosteriorALaBajaSeRechaza()
    {
        var bateria = new Bateria("bat-1", "mod-bat");
        bateria.DarDeBaja(new DateTimeOffset(2026, 9, 5, 0, 0, 0, TimeSpan.Zero), "agotada");
        var repo = new RepositorioFalso { Unidades = { ["bat-1"] = bateria } };
        var entrada = Entrada() with { TiempoValido = new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero) };

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-gmao-externo", entrada, "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.CoherenciaTemporal);
        resultado.Invariante.Should().Be("coherencia_temporal");
        repo.Guardadas.Should().BeEmpty();
    }

    // Fuente del encabezado no registrada → rechazo.
    [Fact]
    public async Task FuenteNoRegistradaSeRechaza()
    {
        var repo = new RepositorioFalso { FuenteRegistrada = false };

        var resultado = await new ServicioIngesta(repo).IngerirAsync("k1", "fd-desconocida", Entrada(), "sha256:a", Ahora, default);

        resultado.Codigo.Should().Be(CodigoIngesta.FuenteNoRegistrada);
        resultado.Campo.Should().Be("X-Fuente-Datos");
        repo.Guardadas.Should().BeEmpty();
    }

    private sealed class RepositorioFalso : IRepositorioIngesta
    {
        public List<IntervencionIngerida> Guardadas { get; } = [];
        public Dictionary<string, UnidadFisica> Unidades { get; } = [];
        public bool FuenteRegistrada { get; init; } = true;

        public Task<IntervencionIngerida?> BuscarPorClaveAsync(string clave, CancellationToken ct) =>
            Task.FromResult(Guardadas.FirstOrDefault(i => i.ClaveIdempotencia == clave));

        public Task<FuenteDatos?> FuenteAsync(string codigo, CancellationToken ct) =>
            Task.FromResult(FuenteRegistrada ? new FuenteDatos(codigo, ConfianzaFuente.Media) : null);

        public Task<UnidadFisica?> UnidadAsync(string codigo, CancellationToken ct) =>
            Task.FromResult(Unidades.GetValueOrDefault(codigo));

        public Task AgregarAsync(IntervencionIngerida intervencion, CancellationToken ct)
        {
            Guardadas.Add(intervencion);
            return Task.CompletedTask;
        }
    }
}
