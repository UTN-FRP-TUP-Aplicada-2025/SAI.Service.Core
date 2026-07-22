using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core de la historia de intervenciones (Etapa 4·C): <see cref="Intervencion"/> y
/// <see cref="FichaVidaUtil"/>, ambas append-only (<c>IEntidadHistoria</c>: el interceptor las
/// protege). El value object <see cref="Domain.Valores.Dinero"/> se mapea como <b>complex type</b>
/// (monto, moneda, fecha), igual que <c>Valor&lt;T&gt;</c>; los <see cref="Costos"/> anidan tres
/// importes. Coherente con el resto del modelo, la clave es el <c>Codigo</c>.
/// </summary>
internal static class ModeloIntervenciones
{
    public static void Configurar(ModelBuilder builder)
    {
        builder.Entity<Intervencion>(e =>
        {
            e.ToTable("Intervencion");
            e.HasKey(i => i.Codigo);
            e.Property(i => i.Codigo);
            e.Property(i => i.DispositivoCodigo).IsRequired();
            e.Property(i => i.Posicion).IsRequired();
            e.Property(i => i.BateriaSalienteCodigo).IsRequired();
            e.Property(i => i.BateriaEntranteCodigo).IsRequired();
            e.Property(i => i.InstanteOcurrido).IsRequired();
            e.Property(i => i.InstanteRegistrado).IsRequired();
            e.Property(i => i.Proveedor).IsRequired();
            e.Property(i => i.Ejecutor).IsRequired();
            e.Property(i => i.Hallazgos).IsRequired();

            // Los tres importes se mapean directos (Dinero es un complex type con ctor de escalares).
            // Costos es un value object calculado sobre ellos y no se persiste.
            e.Ignore(i => i.Costos);
            MapearDinero(e.ComplexProperty(i => i.Repuestos), "Repuestos");
            MapearDinero(e.ComplexProperty(i => i.ManoDeObra), "ManoObra");
            MapearDinero(e.ComplexProperty(i => i.Total), "Total");
            e.ComplexProperty(i => i.Disposicion, d =>
            {
                d.Property(x => x.Destino).HasColumnName("DisposicionDestino");
                d.Property(x => x.Receptor).HasColumnName("DisposicionReceptor");
            });

            e.HasOne<UnidadFisica>().WithMany().HasPrincipalKey(u => u.Codigo)
                .HasForeignKey(i => i.DispositivoCodigo).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(i => new { i.DispositivoCodigo, i.InstanteOcurrido });
        });

        builder.Entity<FichaVidaUtil>(e =>
        {
            e.ToTable("FichaVidaUtil");
            e.HasKey(f => f.Codigo);
            e.Property(f => f.Codigo);
            e.Property(f => f.IntervencionCodigo).IsRequired();
            e.Property(f => f.DispositivoCodigo).IsRequired();
            e.Property(f => f.BateriaCodigo).IsRequired();
            e.Property(f => f.DiasEnServicio).IsRequired();
            e.Property(f => f.VidaEsperadaDias).IsRequired();
            e.Property(f => f.CumplioExpectativa).IsRequired();
            e.Property(f => f.DesvioDias).IsRequired();
            e.Property(f => f.FuenteCotizacion).IsRequired();

            MapearDinero(e.ComplexProperty(f => f.CostoPorAnioServicio), "CostoAnio");
            MapearDinero(e.ComplexProperty(f => f.CostoPorAnioServicioUsd), "CostoAnioUsd");

            e.HasOne<Intervencion>().WithMany().HasPrincipalKey(i => i.Codigo)
                .HasForeignKey(f => f.IntervencionCodigo).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(f => new { f.DispositivoCodigo, f.IntervencionCodigo });
        });
    }

    // Mapea un Dinero (complex type) con el prefijo de columna dado: <prefijo>Monto/Moneda/Fecha.
    private static void MapearDinero(ComplexPropertyBuilder<Domain.Valores.Dinero> dinero, string prefijo)
    {
        dinero.Property(x => x.Monto).HasColumnName($"{prefijo}Monto");
        dinero.Property(x => x.Moneda).HasColumnName($"{prefijo}Moneda");
        dinero.Property(x => x.Fecha).HasColumnName($"{prefijo}Fecha");
    }
}
