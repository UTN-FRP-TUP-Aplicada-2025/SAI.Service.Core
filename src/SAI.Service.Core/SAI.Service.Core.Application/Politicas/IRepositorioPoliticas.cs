using SAI.Service.Core.Domain.Politicas;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Politicas;

/// <summary>
/// Puerto de persistencia de las políticas de apagado versionadas (CU-03, US-06). La implementación
/// (Infrastructure, EF Core) guarda las versiones como historia append-only (la vigente es la de mayor
/// número) y consulta las verificaciones para previsualizar la modalidad efectiva.
/// </summary>
public interface IRepositorioPoliticas
{
    /// <summary>Versión vigente (la de mayor número), o <c>null</c> si aún no hay ninguna.</summary>
    Task<VersionPolitica?> VigenteAsync(CancellationToken ct);

    /// <summary>Todas las versiones, de la más reciente a la más vieja (historial del panel).</summary>
    Task<IReadOnlyList<VersionPolitica>> HistorialAsync(CancellationToken ct);

    /// <summary>Agrega una versión nueva (append-only: nunca edita las anteriores).</summary>
    Task AgregarVersionAsync(VersionPolitica version, CancellationToken ct);

    /// <summary>Verdadero si ya existe alguna versión (para la semilla de la puesta en marcha).</summary>
    Task<bool> ExisteAlgunaAsync(CancellationToken ct);

    /// <summary>Las verificaciones de los cuatro supuestos (para previsualizar la modalidad efectiva).</summary>
    Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct);
}
