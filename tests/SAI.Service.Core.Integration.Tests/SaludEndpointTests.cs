using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Prueba de integracion de andamiaje (Sprint 0): levanta el host con
/// <see cref="WebApplicationFactory{TEntryPoint}"/> y confirma que el endpoint de
/// salud anonimo responde 200. Valida que la solucion arranca de punta a punta.
/// </summary>
public class SaludEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SaludEndpointTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task HealthRespondeOkSinAutenticacion()
    {
        var cliente = _factory.CreateClient();

        var respuesta = await cliente.GetAsync("/health");

        respuesta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
