using SAI.Service.Core.Domain.Monitoreo;
using SAI.Service.Core.Domain.Valores;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Instantánea de lectura para el panel de estado en vivo (US-07): la última muestra con sus valores
/// y procedencia, la conectividad y los eventos recientes. Siempre se relee de la base (no se cachea
/// en el cliente), para no mostrar un valor viejo como actual al reconectarse el circuito (US-07 CA-3).
/// </summary>
/// <param name="HayDispositivo">Verdadero si hay un dispositivo en servicio para monitorear.</param>
/// <param name="Conectado">Verdadero si la última muestra tuvo datos (no perdida).</param>
/// <param name="InstanteUltimaMuestra">Instante de la última muestra, o nulo si aún no hay.</param>
/// <param name="CalidadUltima">Calidad de la última muestra.</param>
/// <param name="Lecturas">Valores de la última muestra con su procedencia (US-10).</param>
/// <param name="EventosRecientes">Eventos recientes, más reciente primero.</param>
public sealed record EstadoEnVivo(
    bool HayDispositivo,
    bool Conectado,
    DateTimeOffset? InstanteUltimaMuestra,
    CalidadMuestra? CalidadUltima,
    IReadOnlyList<LecturaEnVivo> Lecturas,
    IReadOnlyList<EventoEnVivo> EventosRecientes);

/// <summary>Valor de una variable con su procedencia, para el panel (US-10).</summary>
public sealed record LecturaEnVivo(string Variable, double? Valor, Origen? Origen);

/// <summary>Evento reciente para el panel, con su duración/incertidumbre y la regla versionada.</summary>
public sealed record EventoEnVivo(
    TipoEvento Tipo,
    DateTimeOffset Instante,
    double? DuracionSeg,
    double? IncertidumbreDuracionSeg,
    string ReglaCodigo,
    int ReglaVersion);
