using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Domain.Historia;
using SAI.Service.Core.Infrastructure.Persistencia;
using Xunit;

namespace SAI.Service.Core.Integration.Tests;

/// <summary>
/// BT-09 / ADR-04: la disciplina append-only. El interceptor rechaza modificar o borrar
/// entidades que implementan <see cref="IEntidadHistoria"/>; permite insertarlas.
/// Se usa un contexto mínimo con una entidad de historia de prueba (en la Etapa 1 el
/// modelo real todavía no tiene tablas de hechos).
/// </summary>
public class InterceptorAppendOnlyTests
{
    private sealed class Hecho : IEntidadHistoria
    {
        public int Id { get; set; }
        public string Nota { get; set; } = string.Empty;
    }

    private sealed class ContextoHechos(DbContextOptions<ContextoHechos> opciones) : DbContext(opciones)
    {
        public DbSet<Hecho> Hechos => Set<Hecho>();
    }

    private static ContextoHechos CrearContexto(SqliteConnection conexion)
    {
        var opciones = new DbContextOptionsBuilder<ContextoHechos>()
            .UseSqlite(conexion)
            .AddInterceptors(new InterceptorAppendOnly())
            .Options;

        var contexto = new ContextoHechos(opciones);
        contexto.Database.EnsureCreated();
        return contexto;
    }

    [Fact]
    public void InsertarUnaEntidadDeHistoriaEstaPermitido()
    {
        using var conexion = new SqliteConnection("DataSource=:memory:");
        conexion.Open();
        using var contexto = CrearContexto(conexion);

        contexto.Hechos.Add(new Hecho { Nota = "alta" });

        var acto = () => contexto.SaveChanges();

        acto.Should().NotThrow();
    }

    [Fact]
    public void ModificarUnaEntidadDeHistoriaEsRechazado()
    {
        using var conexion = new SqliteConnection("DataSource=:memory:");
        conexion.Open();
        using var contexto = CrearContexto(conexion);

        var hecho = new Hecho { Nota = "original" };
        contexto.Hechos.Add(hecho);
        contexto.SaveChanges();

        hecho.Nota = "modificada";

        var acto = () => contexto.SaveChanges();

        acto.Should().Throw<EscrituraDestructivaProhibidaException>();
    }

    [Fact]
    public void BorrarUnaEntidadDeHistoriaEsRechazado()
    {
        using var conexion = new SqliteConnection("DataSource=:memory:");
        conexion.Open();
        using var contexto = CrearContexto(conexion);

        var hecho = new Hecho { Nota = "original" };
        contexto.Hechos.Add(hecho);
        contexto.SaveChanges();

        contexto.Hechos.Remove(hecho);

        var acto = () => contexto.SaveChanges();

        acto.Should().Throw<EscrituraDestructivaProhibidaException>();
    }
}
