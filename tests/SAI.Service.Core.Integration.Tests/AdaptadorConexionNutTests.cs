using FluentAssertions;
using SAI.Service.Core.Infrastructure.Adaptadores.Nut;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Pruebas del <see cref="AdaptadorConexionNut"/> con un cliente NUT falso (sin socket): mapeo de
/// variables a <c>EstadoSai</c>, validación por efecto observado (ADR-11) y descubrimiento (US-03).
/// Los datos del cliente falso reproducen los del SAI real (0665:5161, nutdrv_qx).
/// </summary>
public class AdaptadorConexionNutTests
{
    private static Dictionary<string, string> VariablesReales() => new(StringComparer.Ordinal)
    {
        ["ups.status"] = "OL",
        ["input.voltage"] = "228.7",
        ["output.voltage"] = "228.7",
        ["ups.load"] = "13",
        ["battery.charge"] = "100",
        ["driver.name"] = "nutdrv_qx",
        ["driver.parameter.vendorid"] = "0665",
        ["driver.parameter.productid"] = "5161",
    };

    [Fact]
    public async Task LeerEstadoMapeaLasVariablesYEsAlcanzable()
    {
        var adaptador = new AdaptadorConexionNut(new ClienteNutFalso { Variables = VariablesReales() });

        var estado = await adaptador.LeerEstadoAsync(CancellationToken.None);

        estado.Alcanzable.Should().BeTrue();
        estado.TensionEntradaVoltios.Should().Be(228.7);
        estado.TensionSalidaVoltios.Should().Be(228.7);
        estado.CargaSalidaPorcentaje.Should().Be(13);
        estado.CargaBateriaPorcentaje.Should().Be(100);
    }

    [Fact]
    public async Task LeerEstadoAnteFallaDeTransporteEsNoAlcanzable()
    {
        var adaptador = new AdaptadorConexionNut(new ClienteNutFalso { Falla = true });

        var estado = await adaptador.LeerEstadoAsync(CancellationToken.None);

        estado.Alcanzable.Should().BeFalse("una excepción de transporte no es un veredicto sobre el equipo (ADR-11)");
        estado.TensionEntradaVoltios.Should().BeNull();
    }

    [Fact]
    public async Task ProbarConectividadConfirmaPorEfectoObservado()
    {
        var adaptador = new AdaptadorConexionNut(new ClienteNutFalso { Variables = VariablesReales() });

        var resultado = await adaptador.ProbarConectividadAsync(CancellationToken.None);

        resultado.Conectado.Should().BeTrue();
        resultado.LatenciaMilisegundos.Should().NotBeNull();
        resultado.Detalle.Should().Contain("ups.status=OL");
    }

    [Fact]
    public async Task ProbarConectividadInformaFallaSinPropagarExcepcion()
    {
        var adaptador = new AdaptadorConexionNut(new ClienteNutFalso { Falla = true });

        var resultado = await adaptador.ProbarConectividadAsync(CancellationToken.None);

        resultado.Conectado.Should().BeFalse();
        resultado.Detalle.Should().Contain("PRUEBA_CONEXION_FALLIDA");
    }

    [Fact]
    public async Task DescubrirComponeElDescriptorConIdUsbYDescripcion()
    {
        var cliente = new ClienteNutFalso
        {
            Variables = VariablesReales(),
            Ups = "sai",
            Candidatos = [("sai", "UPS INNO TECH / Voltronic Qx - relevamiento")],
        };
        var adaptador = new AdaptadorConexionNut(cliente);

        var descubiertos = await adaptador.DescubrirAsync(CancellationToken.None);

        descubiertos.Should().ContainSingle();
        var d = descubiertos[0];
        d.NombreNut.Should().Be("sai");
        d.VendorId.Should().Be("0665");
        d.ProductId.Should().Be("5161");
        d.Driver.Should().Be("nutdrv_qx");
        d.Descriptor.Should().Contain("0665:5161").And.Contain("INNO TECH");
    }

    [Fact]
    public async Task DescubrirSinCandidatosDevuelveVacio()
    {
        var adaptador = new AdaptadorConexionNut(new ClienteNutFalso { Candidatos = [] });

        var descubiertos = await adaptador.DescubrirAsync(CancellationToken.None);

        descubiertos.Should().BeEmpty("sin candidatos en el bus se informa DISPOSITIVO_NO_DESCUBIERTO, no se inventa uno");
    }

    [Fact]
    public async Task ElApagadoOrdenadoEmiteShutdownReturnConLosRetardosYNuncaCancela()
    {
        var cliente = new ClienteNutFalso { Variables = VariablesReales(), ConCredenciales = true };
        var adaptador = new AdaptadorConexionNut(cliente);

        var resultado = await adaptador.OrdenarApagadoConRetornoAsync(TimeSpan.FromSeconds(30), CancellationToken.None);

        resultado.Aceptada.Should().BeTrue("el equipo admitió la orden (efecto observado, ADR-11)");
        var comando = cliente.Comandos.Should().ContainSingle().Subject;
        comando.Comando.Should().Be("shutdown.return");
        comando.Ajustes.Should().Contain(("ups.delay.shutdown", "30")).And.Contain(("ups.delay.start", "180"));
        cliente.Comandos.Should().NotContain(c => c.Comando.Contains("stop"), "el ciclo forzado no se cancela (ADR-09): nunca shutdown.stop");
    }

    [Fact]
    public async Task ElApagadoSinCredencialesDeEscrituraNoSeConfirma()
    {
        var cliente = new ClienteNutFalso { Variables = VariablesReales(), ConCredenciales = false };
        var adaptador = new AdaptadorConexionNut(cliente);

        var resultado = await adaptador.OrdenarApagadoConRetornoAsync(TimeSpan.FromSeconds(30), CancellationToken.None);

        resultado.Aceptada.Should().BeFalse("sin credenciales de escritura el efecto no se confirma (ADR-11)");
        cliente.Comandos.Should().BeEmpty();
    }

    [Fact]
    public async Task ElTestDeBateriaEmiteElComandoRapido()
    {
        var cliente = new ClienteNutFalso { Variables = VariablesReales(), ConCredenciales = true };
        var adaptador = new AdaptadorConexionNut(cliente);

        var resultado = await adaptador.LanzarTestBateriaAsync(CancellationToken.None);

        resultado.Aceptada.Should().BeTrue();
        cliente.Comandos.Should().ContainSingle(c => c.Comando == "test.battery.start.quick");
    }

    private sealed class ClienteNutFalso : IClienteNut
    {
        public string Ups { get; init; } = "sai";

        public string PuntoFinal => "falso:3493";

        public IReadOnlyDictionary<string, string> Variables { get; init; } = new Dictionary<string, string>();

        public IReadOnlyList<(string Nombre, string Descripcion)> Candidatos { get; init; } = [];

        public bool Falla { get; init; }

        public bool ConCredenciales { get; init; }

        public bool TieneCredencialesEscritura => ConCredenciales;

        // Comandos de escritura recibidos (para verificar el write path sin socket).
        public List<(string Comando, IReadOnlyList<(string Variable, string Valor)> Ajustes)> Comandos { get; } = [];

        public Task<string> ObtenerVersionAsync(CancellationToken ct) =>
            Falla ? throw new NutException("falla simulada") : Task.FromResult("upsd de prueba");

        public Task<IReadOnlyList<(string Nombre, string Descripcion)>> ListarUpsAsync(CancellationToken ct) =>
            Falla ? throw new NutException("falla simulada") : Task.FromResult(Candidatos);

        public Task<IReadOnlyDictionary<string, string>> LeerVariablesAsync(CancellationToken ct) =>
            Falla ? throw new NutException("falla simulada") : Task.FromResult(Variables);

        public Task EnviarComandoInstantaneoAsync(string comando, IReadOnlyList<(string Variable, string Valor)> ajustesPrevios, CancellationToken ct)
        {
            if (Falla)
            {
                throw new NutException("falla simulada");
            }

            if (!ConCredenciales)
            {
                throw new NutException("sin credenciales de escritura (rechazo simulado)");
            }

            Comandos.Add((comando, ajustesPrevios));
            return Task.CompletedTask;
        }
    }
}
