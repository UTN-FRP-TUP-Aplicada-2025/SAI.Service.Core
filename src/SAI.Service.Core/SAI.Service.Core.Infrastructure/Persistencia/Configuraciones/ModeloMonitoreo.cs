using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core de la historia de monitoreo (FuenteDatos, SesionSondeo, Muestra, Agregado, Etapa 3).
/// Todas son append-only (<c>IEntidadHistoria</c>): el interceptor ya cableado las protege sin
/// cambios. Los diccionarios (lecturas de la muestra, mapa variable→origen de la sesión) se guardan
/// como JSON en una columna. Coherente con el resto del modelo, la clave es el <c>Codigo</c>.
/// </summary>
internal static class ModeloMonitoreo
{
    private static readonly JsonSerializerOptions Json = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Configurar(ModelBuilder builder)
    {
        ConfigurarEventos(builder);

        builder.Entity<FuenteDatos>(e =>
        {
            e.ToTable("FuenteDatos");
            e.HasKey(f => f.Codigo);
            e.Property(f => f.Codigo);
            e.Property(f => f.ConfianzaBase).HasConversion<string>().IsRequired();
            e.Property(f => f.Descripcion);
        });

        builder.Entity<SesionSondeo>(e =>
        {
            e.ToTable("SesionSondeo");
            e.HasKey(s => s.Codigo);
            e.Property(s => s.Codigo);
            e.Property(s => s.DispositivoCodigo).IsRequired();
            e.Property(s => s.FuenteDatosCodigo).IsRequired();
            e.Property(s => s.Driver).IsRequired();
            e.Property(s => s.DriverVersion);
            e.Property(s => s.Dialecto);
            e.Property(s => s.IntervaloSeg).IsRequired();
            e.Property(s => s.MapaVariableOrigen)
                .HasColumnName("MapaVariableOrigen")
                .HasConversion(ConversorMapaOrigen())
                .Metadata.SetValueComparer(ComparadorMapaOrigen());
            e.ComplexProperty(s => s.Vigencia, v =>
            {
                v.Property(p => p.Desde).HasColumnName("Desde");
                v.Property(p => p.Hasta).HasColumnName("Hasta");
            });

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(s => s.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FuenteDatos>().WithMany().HasPrincipalKey(f => f.Codigo)
                .HasForeignKey(s => s.FuenteDatosCodigo).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => new { s.DispositivoCodigo });
        });

        builder.Entity<Muestra>(e =>
        {
            e.ToTable("Muestra", t => t.HasCheckConstraint(
                "CK_Muestra_Calidad", "\"Calidad\" IN ('Completa','Parcial','Perdida')"));
            e.HasKey(m => m.Codigo);
            e.Property(m => m.Codigo);
            e.Property(m => m.DispositivoCodigo).IsRequired();
            e.Property(m => m.SesionSondeoCodigo).IsRequired();
            e.Property(m => m.Instante).IsRequired();
            e.Property(m => m.Calidad).HasConversion<string>().IsRequired();
            e.Property(m => m.Lecturas)
                .HasColumnName("Valores")
                .HasConversion(ConversorLecturas())
                .Metadata.SetValueComparer(ComparadorLecturas());

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(m => m.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<SesionSondeo>().WithMany().HasPrincipalKey(s => s.Codigo)
                .HasForeignKey(m => m.SesionSondeoCodigo).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(m => new { m.DispositivoCodigo, m.Instante });
            e.HasIndex(m => m.SesionSondeoCodigo);
        });

        builder.Entity<Agregado>(e =>
        {
            e.ToTable("Agregado", t => t.HasCheckConstraint(
                "CK_Agregado_Cobertura", "\"Cobertura\" >= 0 AND \"Cobertura\" <= 1"));
            e.HasKey(a => a.Codigo);
            e.Property(a => a.Codigo);
            e.Property(a => a.DispositivoCodigo).IsRequired();
            e.Property(a => a.Variable).IsRequired();
            e.Property(a => a.VentanaInicio).IsRequired();
            e.Property(a => a.VentanaDuracion).IsRequired();
            e.Property(a => a.NMuestras).IsRequired();
            e.Property(a => a.Cobertura).IsRequired();
            e.Property(a => a.Advertencia);
            e.Property(a => a.Promedio);
            e.Property(a => a.Minimo);
            e.Property(a => a.Maximo);

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(a => a.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);

            // Un agregado por (dispositivo, variable, ventana) — RC-04.
            e.HasIndex(a => new { a.DispositivoCodigo, a.Variable, a.VentanaInicio }).IsUnique();
        });
    }

    private static void ConfigurarEventos(ModelBuilder builder)
    {
        builder.Entity<ReglaDerivacion>(e =>
        {
            e.ToTable("ReglaDerivacion");
            e.HasKey(r => new { r.Codigo, r.Version }); // la versión es parte de la identidad (RC-09)
            e.Property(r => r.Codigo);
            e.Property(r => r.Version);
            e.Property(r => r.Descripcion);
            e.Property(r => r.VigenteDesde).IsRequired();
            e.Property(r => r.Parametros)
                .HasColumnName("Parametros")
                .HasConversion(ConversorParametros())
                .Metadata.SetValueComparer(ComparadorParametros());
        });

        builder.Entity<Evento>(e =>
        {
            e.ToTable("Evento");
            e.HasKey(v => v.Codigo);
            e.Property(v => v.Codigo);
            e.Property(v => v.DispositivoCodigo).IsRequired();
            e.Property(v => v.Tipo).HasConversion<string>().IsRequired();
            e.Property(v => v.Instante).IsRequired();
            e.Property(v => v.DuracionSeg);
            e.Property(v => v.IncertidumbreDuracionSeg);
            e.Property(v => v.ReglaDerivacionCodigo).IsRequired();
            e.Property(v => v.ReglaVersion).IsRequired();

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(v => v.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<ReglaDerivacion>().WithMany()
                .HasForeignKey(v => new { v.ReglaDerivacionCodigo, v.ReglaVersion })
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(v => new { v.DispositivoCodigo, v.Instante });
            e.HasIndex(v => new { v.ReglaDerivacionCodigo, v.ReglaVersion });
        });
    }

    private static ValueConverter<IReadOnlyDictionary<string, double>, string> ConversorParametros() =>
        new(
            d => JsonSerializer.Serialize(d, Json),
            s => JsonSerializer.Deserialize<Dictionary<string, double>>(s, Json) ?? new Dictionary<string, double>());

    private static ValueComparer<IReadOnlyDictionary<string, double>> ComparadorParametros() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, Json) == JsonSerializer.Serialize(b, Json),
            d => JsonSerializer.Serialize(d, Json).GetHashCode(StringComparison.Ordinal),
            d => new Dictionary<string, double>(d, StringComparer.Ordinal));

    private static ValueConverter<IReadOnlyDictionary<string, double?>, string> ConversorLecturas() =>
        new(
            d => JsonSerializer.Serialize(d, Json),
            s => JsonSerializer.Deserialize<Dictionary<string, double?>>(s, Json) ?? new Dictionary<string, double?>());

    private static ValueComparer<IReadOnlyDictionary<string, double?>> ComparadorLecturas() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, Json) == JsonSerializer.Serialize(b, Json),
            d => JsonSerializer.Serialize(d, Json).GetHashCode(StringComparison.Ordinal),
            d => new Dictionary<string, double?>(d, StringComparer.Ordinal));

    private static ValueConverter<IReadOnlyDictionary<string, Origen>, string> ConversorMapaOrigen() =>
        new(
            d => JsonSerializer.Serialize(d, Json),
            s => JsonSerializer.Deserialize<Dictionary<string, Origen>>(s, Json) ?? new Dictionary<string, Origen>());

    private static ValueComparer<IReadOnlyDictionary<string, Origen>> ComparadorMapaOrigen() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, Json) == JsonSerializer.Serialize(b, Json),
            d => JsonSerializer.Serialize(d, Json).GetHashCode(StringComparison.Ordinal),
            d => new Dictionary<string, Origen>(d, StringComparer.Ordinal));
}
