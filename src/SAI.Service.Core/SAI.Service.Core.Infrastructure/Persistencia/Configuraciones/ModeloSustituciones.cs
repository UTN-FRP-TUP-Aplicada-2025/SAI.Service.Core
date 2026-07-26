using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core de la historia de sustituciones del SAI (CU-09): <see cref="SustitucionSai"/>,
/// append-only (<c>IEntidadHistoria</c>: el interceptor la protege). El costo y la disposición son
/// <b>opcionales</b>, así que se mapean como columnas <b>nullable</b> directas (no como complex types,
/// que no admiten nulabilidad); las propiedades calculadas <c>Costo</c> y <c>Disposicion</c> se ignoran.
/// Coherente con el resto del modelo, la clave es el <c>Codigo</c> y el <c>Tipo</c> se guarda como texto.
/// </summary>
internal static class ModeloSustituciones
{
    public static void Configurar(ModelBuilder builder)
    {
        builder.Entity<SustitucionSai>(e =>
        {
            e.ToTable("SustitucionSai");
            e.HasKey(s => s.Codigo);
            e.Property(s => s.Codigo);
            e.Property(s => s.HostCodigo).IsRequired();
            e.Property(s => s.DispositivoSalienteCodigo).IsRequired();
            e.Property(s => s.DispositivoEntranteCodigo);
            e.Property(s => s.Tipo).HasConversion<string>().IsRequired();
            e.Property(s => s.InstanteOcurrido).IsRequired();
            e.Property(s => s.InstanteRegistrado).IsRequired();
            e.Property(s => s.Proveedor).IsRequired();
            e.Property(s => s.Ejecutor).IsRequired();
            e.Property(s => s.Hallazgos).IsRequired();
            e.Property(s => s.FirmwareReiniciado).IsRequired();

            // Costo y disposición opcionales: columnas nullable. Las proyecciones a value object se ignoran.
            e.Ignore(s => s.Costo);
            e.Ignore(s => s.Disposicion);
            e.Property(s => s.CostoMonto);
            e.Property(s => s.CostoMoneda);
            e.Property(s => s.CostoFecha);
            e.Property(s => s.DisposicionDestino);
            e.Property(s => s.DisposicionReceptor);

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(s => s.DispositivoSalienteCodigo).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => new { s.HostCodigo, s.InstanteOcurrido });
        });
    }
}
