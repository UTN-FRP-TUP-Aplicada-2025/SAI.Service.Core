namespace SAI.Service.Core.Domain.Historia;

/// <summary>
/// Marcador de entidad de <b>historia append-only</b> (ADR-04). Las entidades que la
/// implementan representan hechos registrados: se insertan, pero <b>nunca</b> se
/// modifican ni se borran físicamente. Una corrección se expresa como un hecho nuevo,
/// no reescribiendo el pasado.
/// <para>
/// La disciplina se hace cumplir en la capa de infraestructura con un interceptor de
/// <c>SaveChanges</c> (BT-09) que rechaza cualquier entidad marcada que aparezca en el
/// <c>ChangeTracker</c> en estado <c>Modified</c> o <c>Deleted</c>. El marcador vive en
/// Domain (framework-free) para que el modelo declare la intención sin depender de EF.
/// </para>
/// </summary>
public interface IEntidadHistoria
{
}
