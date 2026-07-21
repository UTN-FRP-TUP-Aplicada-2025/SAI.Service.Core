using System.Net;
using FluentAssertions;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Prueba de integración: el host arranca de punta a punta (aplicando el esquema por
/// migraciones, ADR-18) y el endpoint de salud anónimo responde 200 sin autenticación
/// (ADR-16). Valida el criterio de cierre "el esquema se aplica al arranque".
/// </summary>
public class SaludEndpointTests : IClassFixture<FabricaSai>
{
    private readonly FabricaSai _fabrica;

    public SaludEndpointTests(FabricaSai fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task HealthRespondeOkSinAutenticacion()
    {
        var cliente = _fabrica.CreateClient();

        var respuesta = await cliente.GetAsync("/health");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
