using FluentAssertions;
using SAI.Service.Core.Application.Politicas;
using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;
using Xunit;

namespace SAI.Service.Core.Application.Tests;

/// <summary>
/// Configuración de políticas versionadas (CU-03, US-06, EP-04). Crear una versión valida el techo duro
/// (RN-04) y los parámetros antes de persistir, incrementa el número y deja la nueva vigente sin tocar las
/// anteriores (append-only). La previsualización deriva la modalidad efectiva con el bloqueo por
/// verificación (RN-02) sin ejecutar nada.
/// </summary>
public class ServicioPoliticasTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 5, 10, 30, 0, TimeSpan.FromHours(-3));

    // CA-01: crear una versión con modalidad y tiempo deja v2 vigente y v1 intacta.
    [Fact]
    public async Task CrearUnaSegundaVersionLaDejaVigenteYConservaLaPrimera()
    {
        var repo = new RepositorioFalso();
        await repo.AgregarVersionAsync(VersionPolitica.Inicial(Modalidad.SoloAlerta, 300, 120, Ahora), default);
        var servicio = new ServicioPoliticas(repo);

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 300, 240), default);

        resultado.Codigo.Should().Be(CodigoPolitica.Creada);
        resultado.Version!.Numero.Should().Be(2);
        resultado.Version.ModalidadSolicitada.Should().Be(Modalidad.ApagarHostConRetorno);
        resultado.Version.TiempoReservadoApagadoSeg.Should().Be(240);

        var vigente = await servicio.VigenteAsync(default);
        vigente!.Numero.Should().Be(2);
        repo.Versiones.Should().HaveCount(2, "la v1 queda intacta y consultable (append-only)");
        repo.Versiones[0].ModalidadSolicitada.Should().Be(Modalidad.SoloAlerta);
    }

    // CA: sin versión previa, la primera creada es la número 1.
    [Fact]
    public async Task LaPrimeraVersionCreadaEsLaNumeroUno()
    {
        var servicio = new ServicioPoliticas(new RepositorioFalso());

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.SoloAlerta, 300, 120), default);

        resultado.Codigo.Should().Be(CodigoPolitica.Creada);
        resultado.Version!.Numero.Should().Be(1);
    }

    // CA-02: un tiempo reservado que excede el techo duro se rechaza y no crea versión.
    [Fact]
    public async Task UnTiempoQueExcedeElTechoNoCreaVersion()
    {
        var repo = new RepositorioFalso();
        var servicio = new ServicioPoliticas(repo);

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 300, 600), default);

        resultado.Codigo.Should().Be(CodigoPolitica.TiempoApagadoExcedeTecho);
        resultado.Version.Should().BeNull();
        repo.Versiones.Should().BeEmpty("la postcondición de fallo no crea versión");
    }

    // CA: un umbral no positivo se rechaza como parámetro inválido.
    [Fact]
    public async Task UnUmbralNoPositivoSeRechaza()
    {
        var repo = new RepositorioFalso();
        var servicio = new ServicioPoliticas(repo);

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 0, 120), default);

        resultado.Codigo.Should().Be(CodigoPolitica.ParametroInvalido);
        repo.Versiones.Should().BeEmpty();
    }

    // CA-04: cambiar solo el umbral crea la versión siguiente conservando el resto.
    [Fact]
    public async Task CambiarSoloElUmbralCreaLaVersionSiguiente()
    {
        var repo = new RepositorioFalso();
        await repo.AgregarVersionAsync(VersionPolitica.Inicial(Modalidad.ApagarHostConRetorno, 300, 120, Ahora), default);
        var servicio = new ServicioPoliticas(repo);

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 180, 120), default);

        resultado.Version!.Numero.Should().Be(2);
        resultado.Version.UmbralDisparoSegundos.Should().Be(180);
        resultado.Version.ModalidadSolicitada.Should().Be(Modalidad.ApagarHostConRetorno);
    }

    // CA-03: previsualizar una modalidad de acción con supuestos sin verificar avisa degradación.
    [Fact]
    public async Task PrevisualizarConSupuestosSinVerificarAvisaDegradacion()
    {
        var repo = new RepositorioFalso { Verificaciones = SembrarLasCuatro() };
        var servicio = new ServicioPoliticas(repo);

        var previa = await servicio.PrevisualizarAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 300, 120), default);

        previa.Degradada.Should().BeTrue("los cuatro supuestos no están verificados (RN-02)");
        previa.ModalidadEfectiva.Should().Be(Modalidad.SoloAlerta);
        previa.Verificados.Should().Be(0);
        previa.Requeridos.Should().Be(4);
    }

    // Con los cuatro supuestos verificados, la previsualización no degrada.
    [Fact]
    public async Task PrevisualizarConLosCuatroVerificadosNoDegrada()
    {
        var verificaciones = SembrarLasCuatro();
        foreach (var v in verificaciones)
        {
            v.Verificar("prueba", "ok", null, Ahora);
        }

        var repo = new RepositorioFalso { Verificaciones = verificaciones };
        var servicio = new ServicioPoliticas(repo);

        var previa = await servicio.PrevisualizarAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 300, 120), default);

        previa.Degradada.Should().BeFalse();
        previa.ModalidadEfectiva.Should().Be(Modalidad.ApagarHostConRetorno);
    }

    // Solo aviso nunca cuenta como degradada (es el estado base, no una degradación).
    [Fact]
    public async Task PrevisualizarSoloAvisoNoEsDegradacion()
    {
        var servicio = new ServicioPoliticas(new RepositorioFalso());

        var previa = await servicio.PrevisualizarAsync(new PropuestaPolitica(Modalidad.SoloAlerta, 300, 120), default);

        previa.Degradada.Should().BeFalse();
    }

    private static List<Verificacion> SembrarLasCuatro() =>
        Enum.GetValues<Supuesto>().Select(s => Verificacion.Sembrar($"ver-{s}", s, Ahora)).ToList();

    // Repositorio en memoria: la vigente es la de mayor número; agregar es append.
    private sealed class RepositorioFalso : IRepositorioPoliticas
    {
        public List<VersionPolitica> Versiones { get; } = [];

        public IReadOnlyList<Verificacion> Verificaciones { get; init; } = [];

        public Task<VersionPolitica?> VigenteAsync(CancellationToken ct) =>
            Task.FromResult(Versiones.OrderByDescending(v => v.Numero).FirstOrDefault());

        public Task<IReadOnlyList<VersionPolitica>> HistorialAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<VersionPolitica>>(Versiones.OrderByDescending(v => v.Numero).ToList());

        public Task AgregarVersionAsync(VersionPolitica version, CancellationToken ct)
        {
            Versiones.Add(version);
            return Task.CompletedTask;
        }

        public Task<bool> ExisteAlgunaAsync(CancellationToken ct) => Task.FromResult(Versiones.Count > 0);

        public Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct) =>
            Task.FromResult(Verificaciones);
    }
}
