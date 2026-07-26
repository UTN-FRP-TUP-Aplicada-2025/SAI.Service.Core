using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SAI.Service.Core.Domain.Intervenciones;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core de la ingesta de intervenciones externas (CU-11): <see cref="IntervencionIngerida"/>,
/// append-only (<c>IEntidadHistoria</c>). La <c>ClaveIdempotencia</c> lleva un <b>índice único</b>: es el
/// almacén de idempotencia (ADR-17). Los tres importes son <see cref="Domain.Valores.Dinero"/> (complex
/// types); las baterías afectadas se guardan como JSON; la disposición es opcional (columnas nullable, como
/// en <c>SustitucionSai</c>). La confianza se guarda como texto.
/// </summary>
internal static class ModeloIngesta
{
    public static void Configurar(ModelBuilder builder)
    {
        builder.Entity<IntervencionIngerida>(e =>
        {
            e.ToTable("IntervencionIngerida");
            e.HasKey(i => i.Codigo);
            e.Property(i => i.Codigo);
            e.Property(i => i.ClaveIdempotencia).IsRequired();
            e.Property(i => i.HuellaCuerpo).IsRequired();
            e.Property(i => i.FuenteDatosCodigo).IsRequired();
            e.Property(i => i.Confianza).HasConversion<string>().IsRequired();
            e.Property(i => i.TipoIntervencion).IsRequired();
            e.Property(i => i.DispositivoCodigo).IsRequired();
            e.Property(i => i.Proveedor);
            e.Property(i => i.Hallazgos);
            e.Property(i => i.TiempoValido).IsRequired();
            e.Property(i => i.TiempoRegistrado).IsRequired();

            e.Property(i => i.Baterias)
                .HasColumnName("Baterias")
                .HasConversion(ConversorBaterias())
                .Metadata.SetValueComparer(ComparadorBaterias());

            // Los tres importes se mapean como complex type (Dinero: monto/moneda/fecha).
            e.Ignore(i => i.Costos);
            MapearDinero(e.ComplexProperty(i => i.Repuestos), "Repuestos");
            MapearDinero(e.ComplexProperty(i => i.ManoDeObra), "ManoObra");
            MapearDinero(e.ComplexProperty(i => i.Total), "Total");

            // Disposición opcional: columnas nullable; la proyección a value object se ignora.
            e.Ignore(i => i.Disposicion);
            e.Property(i => i.DisposicionDestino);
            e.Property(i => i.DisposicionReceptor);

            // Sin FK a UnidadFisica a propósito: la ingesta registra un hecho EXTERNO de confianza media, que
            // puede referenciar un dispositivo aún no dado de alta localmente; no se rechaza por integridad
            // referencial. La coherencia temporal (RN-12) se valida en el servicio sobre las unidades que sí
            // existen (una intervención posterior a la baja de una unidad conocida se rechaza).

            // Idempotencia por clave: a lo sumo un registro por clave (I-19, RN-09).
            e.HasIndex(i => i.ClaveIdempotencia).IsUnique();
        });
    }

    private static void MapearDinero(ComplexPropertyBuilder<Domain.Valores.Dinero> dinero, string prefijo)
    {
        dinero.Property(x => x.Monto).HasColumnName($"{prefijo}Monto");
        dinero.Property(x => x.Moneda).HasColumnName($"{prefijo}Moneda");
        dinero.Property(x => x.Fecha).HasColumnName($"{prefijo}Fecha");
    }

    private static ValueConverter<IReadOnlyList<string>, string> ConversorBaterias() =>
        new(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            s => JsonSerializer.Deserialize<List<string>>(s, (JsonSerializerOptions?)null) ?? new List<string>());

    private static ValueComparer<IReadOnlyList<string>> ComparadorBaterias() =>
        new(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(StringComparison.Ordinal),
            v => v.ToList());
}
