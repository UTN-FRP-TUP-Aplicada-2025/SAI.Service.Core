using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;

namespace SAI.Service.Core.Application.Ingesta;

/// <summary>
/// Puerto de persistencia de la ingesta (CU-11). La historia append-only de intervenciones ingresadas es el
/// almacén de idempotencia: se busca por <see cref="IntervencionIngerida.ClaveIdempotencia"/> (ADR-17). Se
/// consulta la fuente de datos (confianza base) y las unidades referenciadas (para la coherencia temporal,
/// RN-12), incluidas las dadas de baja.
/// </summary>
public interface IRepositorioIngesta
{
    /// <summary>El hecho ya ingresado con esa clave, o <c>null</c> si la clave es nueva.</summary>
    Task<IntervencionIngerida?> BuscarPorClaveAsync(string clave, CancellationToken ct);

    /// <summary>La fuente de datos registrada del encabezado, o <c>null</c> si no existe.</summary>
    Task<FuenteDatos?> FuenteAsync(string codigo, CancellationToken ct);

    /// <summary>La unidad física referenciada (para verificar que no operó después de su baja, RN-12).</summary>
    Task<UnidadFisica?> UnidadAsync(string codigo, CancellationToken ct);

    /// <summary>Agrega el hecho ingresado (append-only, un solo <c>SaveChanges</c>).</summary>
    Task AgregarAsync(IntervencionIngerida intervencion, CancellationToken ct);
}
