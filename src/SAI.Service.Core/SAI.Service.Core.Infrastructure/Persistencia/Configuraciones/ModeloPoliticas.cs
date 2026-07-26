using Microsoft.EntityFrameworkCore;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Politicas;

namespace SAI.Service.Core.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Mapeo EF Core de las políticas de apagado versionadas (Etapa 4·D, CU-03, EP-04):
/// <see cref="VersionPolitica"/>, append-only (<c>IEntidadHistoria</c>: el interceptor la protege). La
/// modalidad se guarda como texto; el techo duro de <see cref="Accion.TechoDuroApagadoSeg"/> (RN-04,
/// I-10) se defiende también en base con un check constraint. La clave es el <c>Codigo</c> y el
/// <c>Numero</c> es único (la vigente es la de mayor número).
/// </summary>
internal static class ModeloPoliticas
{
    public static void Configurar(ModelBuilder builder)
    {
        builder.Entity<VersionPolitica>(e =>
        {
            e.ToTable("VersionPolitica", t => t.HasCheckConstraint(
                "CK_VersionPolitica_TechoApagado",
                $"\"TiempoReservadoApagadoSeg\" >= 0 AND \"TiempoReservadoApagadoSeg\" <= {Accion.TechoDuroApagadoSeg}"));
            e.HasKey(p => p.Codigo);
            e.Property(p => p.Codigo);
            e.Property(p => p.Numero).IsRequired();
            e.Property(p => p.ModalidadSolicitada).HasConversion<string>().IsRequired();
            e.Property(p => p.UmbralDisparoSegundos).IsRequired();
            e.Property(p => p.TiempoReservadoApagadoSeg).IsRequired();
            e.Property(p => p.VigenteDesde).IsRequired();

            e.HasIndex(p => p.Numero).IsUnique();
        });
    }
}
