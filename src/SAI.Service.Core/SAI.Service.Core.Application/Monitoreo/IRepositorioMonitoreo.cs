using SAI.Service.Core.Domain.Inventario;
using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Vinculos;

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

    /// <summary>Últimas <paramref name="cantidad"/> muestras del dispositivo, más reciente primero.</summary>
    Task<IReadOnlyList<Muestra>> MuestrasRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct);

    /// <summary>Reglas de derivación vigentes en el instante (la mayor versión con vigencia previa), por código.</summary>
    Task<IReadOnlyDictionary<string, ReglaDerivacion>> ReglasVigentesAsync(DateTimeOffset instante, CancellationToken ct);

    /// <summary>Agrega los eventos derivados (append-only).</summary>
    Task GuardarEventosAsync(IReadOnlyList<Evento> eventos, CancellationToken ct);

    /// <summary>Últimos <paramref name="cantidad"/> eventos del dispositivo, más reciente primero (panel en vivo).</summary>
    Task<IReadOnlyList<Evento>> EventosRecientesAsync(string dispositivoCodigo, int cantidad, CancellationToken ct);

    /// <summary>Muestras del dispositivo en un período (histórico), ordenadas por instante ascendente.</summary>
    Task<IReadOnlyList<Muestra>> MuestrasPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct);

    /// <summary>Agregados persistidos de una variable en un período, ordenados por inicio de ventana.</summary>
    Task<IReadOnlyList<Agregado>> AgregadosPorPeriodoAsync(string dispositivoCodigo, string variable, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct);

    /// <summary>Eventos del dispositivo en un período (marcas y conteo de microcortes), por instante ascendente.</summary>
    Task<IReadOnlyList<Evento>> EventosPorPeriodoAsync(string dispositivoCodigo, DateTimeOffset desde, DateTimeOffset hasta, CancellationToken ct);

    /// <summary>Montaje de batería vigente (fin abierto) del dispositivo, o <c>null</c>. Se congela en la prueba (I-15).</summary>
    Task<MontajeBateria?> MontajeVigenteAsync(string dispositivoCodigo, CancellationToken ct);

    /// <summary>Último evento de corte de suministro del dispositivo, o <c>null</c> (para la precondición de flotación).</summary>
    Task<Evento?> UltimoCorteAsync(string dispositivoCodigo, CancellationToken ct);

    /// <summary>Pruebas de un montaje (para la línea base y el conteo de comparables), más antigua primero.</summary>
    Task<IReadOnlyList<PruebaBateria>> PruebasDeMontajeAsync(string montajeCodigo, CancellationToken ct);

    /// <summary>Últimas pruebas de un dispositivo, más reciente primero (historial del panel).</summary>
    Task<IReadOnlyList<PruebaBateria>> PruebasDeDispositivoAsync(string dispositivoCodigo, int cantidad, CancellationToken ct);

    /// <summary>Guarda la prueba junto con su serie densa y la sesión densa (y su fuente, si es nueva), transaccional.</summary>
    Task GuardarPruebaConSerieAsync(PruebaBateria prueba, IReadOnlyList<Muestra> serie, SesionSondeo sesionDensa, FuenteDatos? nuevaFuente, CancellationToken ct);
}
