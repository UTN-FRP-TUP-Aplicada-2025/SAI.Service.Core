using SAI.Service.Core.Domain.Monitoreo;

namespace SAI.Service.Core.Application.Monitoreo;

/// <summary>
/// Arma la instantánea de lectura del panel en vivo (US-07) a partir de la última muestra, su sesión
/// de sondeo (para la procedencia por variable, US-10) y los eventos recientes. Es de solo lectura y
/// siempre consulta la base: el panel refleja el estado vigente sin cachear.
/// </summary>
public sealed class ServicioPanelEnVivo(IRepositorioMonitoreo repositorio)
{
    private const int EventosAMostrar = 10;

    /// <summary>Obtiene la instantánea de estado en vivo del dispositivo en servicio.</summary>
    public async Task<EstadoEnVivo> ObtenerAsync(CancellationToken ct)
    {
        var dispositivo = await repositorio.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return new EstadoEnVivo(false, false, null, null, [], []);
        }

        var recientes = await repositorio.MuestrasRecientesAsync(dispositivo.Codigo, 1, ct);
        var ultima = recientes.Count > 0 ? recientes[0] : null;
        var sesion = await repositorio.SesionActivaDeAsync(dispositivo.Codigo, ct);
        var eventos = await repositorio.EventosRecientesAsync(dispositivo.Codigo, EventosAMostrar, ct);

        var lecturas = ultima is null
            ? (IReadOnlyList<LecturaEnVivo>)[]
            : [.. ultima.Lecturas.Select(kv => new LecturaEnVivo(kv.Key, kv.Value, sesion?.OrigenDe(kv.Key)))];

        return new EstadoEnVivo(
            HayDispositivo: true,
            Conectado: ultima is not null && ultima.Calidad != CalidadMuestra.Perdida,
            InstanteUltimaMuestra: ultima?.Instante,
            CalidadUltima: ultima?.Calidad,
            Lecturas: lecturas,
            EventosRecientes: [.. eventos.Select(e => new EventoEnVivo(
                e.Tipo, e.Instante, e.DuracionSeg, e.IncertidumbreDuracionSeg, e.ReglaDerivacionCodigo, e.ReglaVersion))]);
    }
}
