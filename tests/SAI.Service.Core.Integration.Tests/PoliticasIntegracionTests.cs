using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SAI.Service.Core.Application.Politicas;
using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// Integración de la configuración de políticas versionadas (CU-03, US-06, EP-04): el arranque siembra
/// la versión inicial; crear una versión persiste, incrementa el número y deja la nueva vigente sin tocar
/// las anteriores (append-only); el historial las devuelve de la más reciente a la más vieja.
/// </summary>
public class PoliticasIntegracionTests
{
    [Fact]
    public async Task ElArranqueSiembraLaVersionInicial()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var servicio = scope.ServiceProvider.GetRequiredService<ServicioPoliticas>();

        var vigente = await servicio.VigenteAsync(CancellationToken.None);

        vigente.Should().NotBeNull();
        vigente!.Numero.Should().Be(1);
        vigente.ModalidadSolicitada.Should().Be(Modalidad.SoloAlerta, "el arranque es seguro en solo aviso (RN-01)");
    }

    [Fact]
    public async Task CrearUnaVersionPersisteYQuedaVigente()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var servicio = sp.GetRequiredService<ServicioPoliticas>();

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.CicloForzado, 200, 300), CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoPolitica.Creada);
        var vigente = await servicio.VigenteAsync(CancellationToken.None);
        vigente!.Numero.Should().Be(2);
        vigente.ModalidadSolicitada.Should().Be(Modalidad.CicloForzado);

        var historial = await servicio.HistorialAsync(CancellationToken.None);
        historial.Should().HaveCount(2);
        historial[0].Numero.Should().Be(2, "el historial va de la más reciente a la más vieja");
        historial[1].Numero.Should().Be(1);

        (await sp.GetRequiredService<SaiDbContext>().Politicas.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task UnaVersionQueExcedeElTechoNoSePersiste()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var servicio = sp.GetRequiredService<ServicioPoliticas>();

        var resultado = await servicio.CrearVersionAsync(new PropuestaPolitica(Modalidad.ApagarHostConRetorno, 300, 600), CancellationToken.None);

        resultado.Codigo.Should().Be(CodigoPolitica.TiempoApagadoExcedeTecho);
        (await sp.GetRequiredService<SaiDbContext>().Politicas.CountAsync())
            .Should().Be(1, "solo la versión sembrada al arranque; la postcondición de fallo no crea versión");
    }

    [Fact]
    public async Task LaVersionEsAppendOnly()
    {
        using var fabrica = new FabricaSai();
        using var scope = fabrica.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();

        db.Politicas.Remove(await db.Politicas.FirstAsync());
        var acto = async () => await db.SaveChangesAsync(CancellationToken.None);

        await acto.Should().ThrowAsync<EscrituraDestructivaProhibidaException>("las versiones son historia append-only (ADR-04)");
    }
}
