using SAI.Service.Core.Domain.Catalogo;
using SAI.Service.Core.Domain.Intervenciones;
using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Vinculos;

namespace SAI.Service.Core.Application.Intervenciones;

/// <summary>
/// Puerto de persistencia del recambio de batería (CU-08, US-18). La implementación (Infrastructure,
/// EF Core) aplica el recambio de forma <b>transaccional</b> (un solo acto: cierre de la vigencia
/// vieja, apertura de la nueva, baja/servicio de las baterías, intervención y ficha) y consulta el
/// montaje vigente y el catálogo/inventario que necesita el servicio.
/// </summary>
public interface IRepositorioIntervenciones
{
    /// <summary>Primer dispositivo (SAI) en servicio, o <c>null</c> (para prellenar el formulario).</summary>
    Task<Dispositivo?> DispositivoEnServicioAsync(CancellationToken ct);

    /// <summary>Montaje vigente (fin abierto) del dispositivo en la posición, o <c>null</c>.</summary>
    Task<MontajeBateria?> MontajeVigenteAsync(string dispositivoCodigo, string posicion, CancellationToken ct);

    /// <summary>Todos los montajes del dispositivo en la posición (para validar el no solape, I-1/I-2).</summary>
    Task<IReadOnlyList<MontajeBateria>> MontajesDeDispositivoAsync(string dispositivoCodigo, string posicion, CancellationToken ct);

    /// <summary>Batería de inventario por código, o <c>null</c>.</summary>
    Task<Bateria?> BateriaAsync(string codigo, CancellationToken ct);

    /// <summary>Modelo de batería del catálogo por código, o <c>null</c>.</summary>
    Task<ModeloBateria?> ModeloBateriaAsync(string codigo, CancellationToken ct);

    /// <summary>Verdadero si ya existe una unidad física con ese código (para no pisar la batería nueva).</summary>
    Task<bool> ExisteUnidadAsync(string codigo, CancellationToken ct);

    /// <summary>
    /// Aplica el recambio en una sola transacción: cierra el montaje viejo, da de baja la batería
    /// retirada, agrega la batería nueva y su montaje, y guarda la intervención y la ficha (append-only).
    /// </summary>
    Task GuardarRecambioAsync(
        MontajeBateria montajeCerrado,
        Bateria bateriaRetirada,
        Bateria bateriaNueva,
        MontajeBateria montajeNuevo,
        Intervencion intervencion,
        FichaVidaUtil ficha,
        CancellationToken ct);

    /// <summary>Últimas intervenciones del dispositivo, más reciente primero (panel).</summary>
    Task<IReadOnlyList<Intervencion>> IntervencionesDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct);

    /// <summary>Últimas fichas de vida útil del dispositivo, más reciente primero (panel).</summary>
    Task<IReadOnlyList<FichaVidaUtil>> FichasDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct);
}
