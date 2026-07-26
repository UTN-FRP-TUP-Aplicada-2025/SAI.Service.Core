using FluentAssertions;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Valores;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Registro de sustitución/reparación del SAI (CU-09): el costo y la disposición son opcionales y se
/// proyectan desde columnas nullable; el resto de los datos son obligatorios.
/// </summary>
public class SustitucionSaiTests
{
    private static readonly DateTimeOffset Ahora = new(2027, 5, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SinCostoNiDisposicionLasProyeccionesSonNulas()
    {
        var sustitucion = new SustitucionSai(
            "sus-1", "host", "ups-1", "ups-2", TipoIntervencionSai.Reparacion,
            Ahora, Ahora, "Prov", "Téc", "sin hallazgos", firmwareReiniciado: false,
            costo: null, disposicion: null);

        sustitucion.Costo.Should().BeNull();
        sustitucion.Disposicion.Should().BeNull();
        sustitucion.DispositivoEntranteCodigo.Should().Be("ups-2");
    }

    [Fact]
    public void ConCostoYDisposicionSeProyectanLosValueObjects()
    {
        var costo = new Dinero(150000m, "ARS", new DateOnly(2027, 5, 1));
        var disposicion = new DisposicionFinal("reciclado gestor habilitado", "GestorX");

        var sustitucion = new SustitucionSai(
            "sus-2", "host", "ups-1", null, TipoIntervencionSai.Reemplazo,
            Ahora, Ahora, "Prov", "Téc", "carcasa dañada", firmwareReiniciado: true,
            costo, disposicion);

        sustitucion.Costo.Should().Be(costo);
        sustitucion.Disposicion.Should().Be(disposicion);
        sustitucion.FirmwareReiniciado.Should().BeTrue();
        sustitucion.DispositivoEntranteCodigo.Should().BeNull("no hubo suplente");
    }

    [Fact]
    public void ExigeLosDatosObligatorios()
    {
        var acto = () => new SustitucionSai(
            "sus-3", "host", "  ", null, TipoIntervencionSai.Reparacion,
            Ahora, Ahora, "Prov", "Téc", "x", false, null, null);

        acto.Should().Throw<ArgumentException>("el equipo saliente es obligatorio");
    }
}
