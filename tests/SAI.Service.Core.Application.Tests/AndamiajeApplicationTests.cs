using FluentAssertions;
using SAI.Service.Core.Application.Abstractions;
using Xunit;

namespace SAI.Service.Core.Application.Tests;

/// <summary>
/// Prueba trivial de andamiaje (Sprint 0): confirma que el puerto del adaptador y
/// sus records de resultado existen y son referenciables. La logica de casos de uso
/// y del planificador llega en etapas posteriores.
/// </summary>
public class AndamiajeApplicationTests
{
    [Fact]
    public void ElPuertoDelAdaptadorDeclaraLasCuatroOperaciones()
    {
        var metodos = typeof(IAdaptadorConexion).GetMethods();
        metodos.Should().HaveCount(4);
    }

    [Fact]
    public void LosRecordsDeResultadoSonConstruibles()
    {
        var estado = new EstadoSai(true, 220.0, 220.0, 35.0, 100.0, EstadoUps.EnLinea, 13.2, DateTimeOffset.UtcNow);
        estado.Alcanzable.Should().BeTrue();
    }
}
