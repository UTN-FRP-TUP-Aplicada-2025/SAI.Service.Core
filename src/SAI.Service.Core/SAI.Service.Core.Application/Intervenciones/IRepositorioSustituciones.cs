using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Intervenciones;

/// <summary>
/// Puerto de persistencia de la sustitución del SAI (CU-09, US-20). La implementación (Infrastructure,
/// EF Core) aplica la sustitución de forma <b>transaccional</b> (un solo acto: cierre de la cobertura
/// vigente, apertura de la nueva, cambio de estado de los equipos, intervención y —si cambió el modelo—
/// reinicio de las verificaciones) y consulta las coberturas y el catálogo/inventario que necesita.
/// </summary>
public interface IRepositorioSustituciones
{
    /// <summary>Primera cobertura vigente (fin abierto), o <c>null</c> (para prellenar el formulario).</summary>
    Task<CoberturaHost?> CoberturaVigenteAsync(CancellationToken ct);

    /// <summary>Todas las coberturas del host (para validar el no solape I-4 y mostrar la sucesión).</summary>
    Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct);

    /// <summary>Dispositivo (SAI) de inventario por código, o <c>null</c>.</summary>
    Task<Dispositivo?> DispositivoAsync(string codigo, CancellationToken ct);

    /// <summary>Verdadero si existe el modelo de dispositivo en el catálogo (para registrar un suplente nuevo).</summary>
    Task<bool> ExisteModeloDispositivoAsync(string codigo, CancellationToken ct);

    /// <summary>Verdadero si ya existe una unidad física con ese código (para no pisar el suplente nuevo).</summary>
    Task<bool> ExisteUnidadAsync(string codigo, CancellationToken ct);

    /// <summary>Las verificaciones de los cuatro supuestos (para reiniciarlas si cambia el modelo, FA-2).</summary>
    Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct);

    /// <summary>
    /// Aplica la sustitución en una sola transacción: cierra la cobertura vigente, cambia el estado del
    /// equipo saliente, y —si hubo suplente— lo agrega/pone en servicio y abre la cobertura nueva; guarda
    /// la intervención (append-only) y persiste las verificaciones reiniciadas.
    /// </summary>
    Task GuardarSustitucionAsync(
        CoberturaHost coberturaCerrada,
        CoberturaHost? coberturaNueva,
        Dispositivo dispositivoSaliente,
        Dispositivo? dispositivoEntrante,
        bool entranteEsNuevo,
        SustitucionSai sustitucion,
        IReadOnlyList<Verificacion> verificacionesReiniciadas,
        CancellationToken ct);

    /// <summary>Últimas sustituciones del host, más reciente primero (panel).</summary>
    Task<IReadOnlyList<SustitucionSai>> SustitucionesDeHostAsync(string hostCodigo, int cantidad, CancellationToken ct);
}
