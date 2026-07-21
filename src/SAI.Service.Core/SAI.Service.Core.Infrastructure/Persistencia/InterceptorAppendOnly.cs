using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SAI.Service.Core.Domain.Historia;

namespace SAI.Service.Core.Infrastructure.Persistencia;

/// <summary>
/// Interceptor de <c>SaveChanges</c> que hace cumplir la disciplina <b>append-only</b>
/// (ADR-04, BT-09): recorre el <c>ChangeTracker</c> y, si alguna entidad que implementa
/// <see cref="IEntidadHistoria"/> aparece en estado <see cref="EntityState.Modified"/> o
/// <see cref="EntityState.Deleted"/>, aborta el guardado lanzando
/// <see cref="EscrituraDestructivaProhibidaException"/>.
/// <para>
/// Las tablas de hechos solo admiten inserciones; una corrección se registra como un
/// hecho nuevo, nunca reescribiendo o borrando el pasado. Se registra en las opciones
/// del contexto (ver <c>AddInfrastructure</c>).
/// </para>
/// </summary>
public sealed class InterceptorAppendOnly : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        VerificarAppendOnly(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        VerificarAppendOnly(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void VerificarAppendOnly(DbContext? contexto)
    {
        if (contexto is null)
        {
            return;
        }

        foreach (EntityEntry entrada in contexto.ChangeTracker.Entries())
        {
            if (entrada.Entity is not IEntidadHistoria)
            {
                continue;
            }

            if (entrada.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new EscrituraDestructivaProhibidaException(
                    entrada.Entity.GetType(),
                    entrada.State);
            }
        }
    }
}

/// <summary>
/// Se lanza cuando se intenta modificar o borrar una entidad de historia append-only
/// (ADR-04). Señala una violación de la disciplina de escritura, no un error de datos
/// del usuario.
/// </summary>
public sealed class EscrituraDestructivaProhibidaException : InvalidOperationException
{
    /// <summary>Crea la excepción indicando el tipo de entidad y el estado prohibido.</summary>
    /// <param name="tipoEntidad">Tipo de la entidad de historia afectada.</param>
    /// <param name="estado">Estado destructivo detectado (<c>Modified</c> o <c>Deleted</c>).</param>
    public EscrituraDestructivaProhibidaException(Type tipoEntidad, EntityState estado)
        : base($"La entidad de historia '{tipoEntidad.Name}' es append-only (ADR-04): " +
               $"no se admite el estado '{estado}'. Registre un hecho nuevo en lugar de " +
               "modificar o borrar el existente.")
    {
        TipoEntidad = tipoEntidad;
        Estado = estado;
    }

    /// <summary>Tipo de la entidad de historia que provocó el rechazo.</summary>
    public Type TipoEntidad { get; }

    /// <summary>Estado destructivo detectado.</summary>
    public EntityState Estado { get; }
}
