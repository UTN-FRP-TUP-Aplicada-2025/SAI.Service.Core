using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Puerto de persistencia del monitoreo (ADR-15). La implementación (Infrastructure, EF Core)
/// guarda las muestras y sesiones append-only y resuelve el dispositivo y la sesión activos que
/// alimenta el planificador (BT-17, US-08).
/// </summary>
public interface IRepositorioMonitoreo
{
    /// <summary>Primer dispositivo (SAI) en servicio, o <c>null</c> si no hay ninguno para sondear.</summary>
    Task<Dispositivo?> DispositivoEnServicioAsync(CancellationToken ct);

    /// <summary>Sesión de sondeo activa (vigencia abierta) del dispositivo, o <c>null</c> si no hay.</summary>
    Task<SesionSondeo?> SesionActivaDeAsync(string dispositivoCodigo, CancellationToken ct);

    /// <summary>Verdadero si existe la fuente de datos indicada.</summary>
    Task<bool> ExisteFuenteAsync(string codigo, CancellationToken ct);

    /// <summary>Guarda una sesión de sondeo nueva (y su fuente, si es nueva).</summary>
    Task GuardarSesionAsync(FuenteDatos? nuevaFuente, SesionSondeo sesion, CancellationToken ct);

    /// <summary>Agrega una muestra (append-only).</summary>
    Task GuardarMuestraAsync(Muestra muestra, CancellationToken ct);
}
