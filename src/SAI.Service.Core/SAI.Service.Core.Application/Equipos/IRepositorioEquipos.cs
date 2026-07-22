using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Verificaciones;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>
/// Puerto de persistencia del alta de equipos (ADR-15). La implementación (Infrastructure, EF Core)
/// guarda el conjunto del alta de forma <b>transaccional</b> (todo o nada, CU-02 postcondición de
/// fallo) y consulta los vínculos vigentes para validar el no solapamiento (RC-02).
/// </summary>
public interface IRepositorioEquipos
{
    /// <summary>Montajes existentes para una posición de un dispositivo (para validar solape, I-1/I-2).</summary>
    Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivoAsync(string dispositivoCodigo, string posicion, CancellationToken ct);

    /// <summary>Coberturas existentes de un host (para validar solape, I-4).</summary>
    Task<IReadOnlyList<CoberturaHost>> CoberturasDeHostAsync(string hostCodigo, CancellationToken ct);

    /// <summary>Guarda todo el conjunto del alta en una sola transacción.</summary>
    Task GuardarAltaAsync(ConjuntoAlta alta, CancellationToken ct);

    /// <summary>Lista todas las verificaciones sembradas (para el estado de puesta en marcha).</summary>
    Task<IReadOnlyList<Verificacion>> ListarVerificacionesAsync(CancellationToken ct);

    /// <summary>Verificación de un supuesto, o <c>null</c> si no existe (ventana de mantenimiento).</summary>
    Task<Verificacion?> VerificacionDeSupuestoAsync(Supuesto supuesto, CancellationToken ct);

    /// <summary>Persiste el cambio de estado de una verificación (update in-place; no es append-only).</summary>
    Task ActualizarVerificacionAsync(Verificacion verificacion, CancellationToken ct);

    /// <summary>Verdadero si ya hay al menos una unidad física dada de alta.</summary>
    Task<bool> HayEquiposAsync(CancellationToken ct);
}

/// <summary>
/// Conjunto de entidades que compone un alta (CU-02): catálogo, inventario, vínculos abiertos y las
/// cuatro verificaciones sembradas. Se persiste como una unidad transaccional.
/// </summary>
public sealed record ConjuntoAlta(
    Fabricante Fabricante,
    ModeloDispositivo ModeloDispositivo,
    ModeloBateria ModeloBateria,
    Host Host,
    Dispositivo Dispositivo,
    Bateria Bateria,
    MontajeBateria Montaje,
    CoberturaHost Cobertura,
    IReadOnlyList<Verificacion> Verificaciones);
