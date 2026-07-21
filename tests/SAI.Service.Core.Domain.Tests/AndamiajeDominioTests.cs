using FluentAssertions;
using SAI.Service.Core.Domain;
using Xunit;

namespace SAI.Service.Core.Domain.Tests;

/// <summary>
/// Prueba trivial de andamiaje (Sprint 0): confirma que la suite corre y que el
/// assembly de dominio se referencia. Los 21 invariantes I-1 a I-21 llegan en EP-03+.
/// </summary>
public class AndamiajeDominioTests
{
    [Fact]
    public void ElAssemblyDeDominioEsReferenciable()
    {
        DominioMarcador.Capa.Should().Be("Domain");
    }
}
