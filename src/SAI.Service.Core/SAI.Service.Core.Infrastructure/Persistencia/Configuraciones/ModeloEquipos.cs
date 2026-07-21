using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core del dominio de equipos (catálogo / inventario / vínculos / verificaciones,
/// ADR-07, ADR-18) sobre SQLite. Decisiones de mapeo:
/// <list type="bullet">
///   <item>La <b>clave</b> de cada entidad es su <c>Codigo</c> (el dominio se referencia por código,
///   no por Id numérico; desvío consciente del modelo lógico, que usaba Id int).</item>
///   <item>Las propiedades se mapean <b>explícitamente</b>: las entidades de dominio solo tienen
///   constructor con parámetros (sin ctor sin parámetros), y EF materializa por binding de
///   constructor sobre las propiedades mapeadas.</item>
///   <item>Los value objects <c>Vigencia</c> y <c>Valor&lt;T&gt;</c> se mapean como <b>complex types</b>
///   (mismas tablas); los enums, como texto (nombre .NET).</item>
///   <item><c>UnidadFisica</c> se mapea con <b>TPH</b> (discriminador <c>Tipo</c>).</item>
///   <item>La regla "a lo sumo uno vigente" es un <b>índice único parcial</b> <c>WHERE Hasta IS NULL</c>
///   (RC-02), complementado por la validación de dominio <c>Vigencias.AdmiteNuevo</c>.</item>
/// </list>
/// </summary>
internal static class ModeloEquipos
{
    public static void Configurar(ModelBuilder builder)
    {
        ConfigurarCatalogo(builder);
        ConfigurarInventario(builder);
        ConfigurarVinculos(builder);
        ConfigurarVerificaciones(builder);
    }

    private static void ConfigurarCatalogo(ModelBuilder builder)
    {
        builder.Entity<Fabricante>(e =>
        {
            e.ToTable("Fabricante");
            e.HasKey(f => f.Codigo);
            e.Property(f => f.Codigo);
            e.Property(f => f.Nombre).IsRequired();
            e.Property(f => f.Identificado).IsRequired();
        });

        builder.Entity<ModeloDispositivo>(e =>
        {
            e.ToTable("ModeloDispositivo");
            e.HasKey(m => m.Codigo);
            e.Property(m => m.Codigo);
            e.Property(m => m.FabricanteCodigo).IsRequired();
            e.Property(m => m.Nombre).IsRequired();
            e.Property(m => m.LineaTopologia);
            e.Property(m => m.TensionNominalV);
            e.ComplexProperty(m => m.PotenciaVaNominal, p =>
            {
                p.Property(v => v.Contenido).HasColumnName("PotenciaVaNominalValor");
                p.Property(v => v.Origen).HasColumnName("PotenciaVaNominalOrigen").HasConversion<string>();
            });

            e.HasOne<Fabricante>().WithMany().HasPrincipalKey(f => f.Codigo)
                .HasForeignKey(m => m.FabricanteCodigo).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ModeloBateria>(e =>
        {
            e.ToTable("ModeloBateria", t => t.HasCheckConstraint(
                "CK_ModeloBateria_VidaTemp",
                "\"VidaFlotacionAniosMin\" IS NULL AND \"VidaFlotacionAniosMax\" IS NULL OR \"TemperaturaReferenciaC\" IS NOT NULL"));
            e.HasKey(m => m.Codigo);
            e.Property(m => m.Codigo);
            e.Property(m => m.FabricanteCodigo).IsRequired();
            e.Property(m => m.Nombre).IsRequired();
            e.Property(m => m.Tecnologia);
            e.Property(m => m.CapacidadAh);
            e.Property(m => m.TensionNominalV);
            e.Property(m => m.VidaFlotacionAniosMin);
            e.Property(m => m.VidaFlotacionAniosMax);
            e.Property(m => m.TemperaturaReferenciaC);

            e.HasOne<Fabricante>().WithMany().HasPrincipalKey(f => f.Codigo)
                .HasForeignKey(m => m.FabricanteCodigo).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarInventario(ModelBuilder builder)
    {
        builder.Entity<UnidadFisica>(e =>
        {
            e.ToTable("UnidadFisica", t => t.HasCheckConstraint(
                "CK_UnidadFisica_Baja",
                "(\"Estado\" = 'DadoDeBaja') = (\"FechaBaja\" IS NOT NULL)"));
            e.HasKey(u => u.Codigo);
            e.Property(u => u.Codigo);
            e.Property(u => u.Estado).HasConversion<string>().IsRequired();
            e.Property(u => u.FechaBaja);
            e.Property(u => u.MotivoBaja);

            e.HasDiscriminator<string>("Tipo")
                .HasValue<Host>("Host")
                .HasValue<Dispositivo>("Dispositivo")
                .HasValue<Bateria>("Bateria");

            e.HasIndex("Tipo", nameof(UnidadFisica.Estado));
        });

        builder.Entity<Host>(e =>
        {
            e.Property(h => h.Criticidad);
            e.Property(h => h.EnServicioDesde);
        });

        builder.Entity<Dispositivo>(e =>
        {
            e.Property(d => d.ModeloDispositivoCodigo);
            e.Property(d => d.NumeroSerie);
            e.HasOne<ModeloDispositivo>().WithMany().HasPrincipalKey(m => m.Codigo)
                .HasForeignKey(d => d.ModeloDispositivoCodigo).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Bateria>(e =>
        {
            e.Property(b => b.ModeloBateriaCodigo);
            e.Property(b => b.FechaFabricacion);
            e.Property(b => b.FechaCompra);
            e.HasOne<ModeloBateria>().WithMany().HasPrincipalKey(m => m.Codigo)
                .HasForeignKey(b => b.ModeloBateriaCodigo).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurarVinculos(ModelBuilder builder)
    {
        builder.Entity<MontajeBateria>(e =>
        {
            e.ToTable("MontajeBateria", t => t.HasCheckConstraint(
                "CK_MontajeBateria_Intervalo", "\"Hasta\" IS NULL OR \"Hasta\" >= \"Desde\""));
            e.HasKey(m => m.Codigo);
            e.Property(m => m.Codigo);
            e.Property(m => m.BateriaCodigo).IsRequired();
            e.Property(m => m.DispositivoCodigo).IsRequired();
            e.Property(m => m.Posicion).IsRequired();
            e.ComplexProperty(m => m.Vigencia, v =>
            {
                v.Property(p => p.Desde).HasColumnName("Desde");
                v.Property(p => p.Hasta).HasColumnName("Hasta");
            });

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(m => m.BateriaCodigo).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(m => m.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);

            // A lo sumo un montaje vigente por (dispositivo, posición) — I-2, RC-02.
            e.HasIndex(m => new { m.DispositivoCodigo, m.Posicion }).IsUnique().HasFilter("\"Hasta\" IS NULL");
        });

        builder.Entity<CoberturaHost>(e =>
        {
            e.ToTable("CoberturaHost", t => t.HasCheckConstraint(
                "CK_CoberturaHost_Intervalo", "\"Hasta\" IS NULL OR \"Hasta\" >= \"Desde\""));
            e.HasKey(c => c.Codigo);
            e.Property(c => c.Codigo);
            e.Property(c => c.DispositivoCodigo).IsRequired();
            e.Property(c => c.HostCodigo).IsRequired();
            e.ComplexProperty(c => c.Vigencia, v =>
            {
                v.Property(p => p.Desde).HasColumnName("Desde");
                v.Property(p => p.Hasta).HasColumnName("Hasta");
            });

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(c => c.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(c => c.HostCodigo).OnDelete(DeleteBehavior.Restrict);

            // A lo sumo una cobertura vigente por host — I-4, RC-02.
            e.HasIndex(c => c.HostCodigo).IsUnique().HasFilter("\"Hasta\" IS NULL");
        });
    }

    private static void ConfigurarVerificaciones(ModelBuilder builder)
    {
        builder.Entity<Verificacion>(e =>
        {
            e.ToTable("Verificacion");
            e.HasKey(v => v.Codigo);
            e.Property(v => v.Codigo);
            e.Property(v => v.Supuesto).HasConversion<string>().IsRequired();
            e.Property(v => v.Estado).HasConversion<string>().IsRequired();
            e.Property(v => v.Metodo);
            e.Property(v => v.Evidencia);
            e.Property(v => v.VigenciaHasta);
            e.Property(v => v.ActualizadoEn).IsRequired();
        });
    }
}
