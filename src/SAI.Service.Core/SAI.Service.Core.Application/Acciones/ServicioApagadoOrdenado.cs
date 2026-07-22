using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Application.Monitoreo;
using SAI.Service.Core.Domain.Acciones;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Acciones;

/// <summary>Situación de apagado para el panel: modalidad, bloqueo y últimas acciones.</summary>
public sealed record SituacionApagado(
    Modalidad Solicitada,
    Modalidad Efectiva,
    int Verificados,
    int Requeridos,
    int TiempoReservadoSeg,
    IReadOnlyList<Accion> Acciones);

/// <summary>
/// Ejecuta el apagado ordenado ante un disparo (CU-05, US-14, US-15): pipeline C del flujo de
/// ejecución. Deriva la <b>modalidad efectiva</b> con el bloqueo por verificación (RN-02, ADR-10) y,
/// solo si habilita una acción, ordena el apagado con retorno por el adaptador y lo registra
/// <b>por efecto observado</b> (ADR-11, RN-03): la acción se da por ejecutada solo si el equipo
/// admitió la orden, nunca por ausencia de excepción. Toda decisión —incluido el bloqueo y el solo
/// aviso— deja una <see cref="Accion"/> append-only. El ciclo forzado no se cancela (ADR-09): este
/// servicio nunca emite un <c>shutdown.stop</c>.
/// </summary>
public sealed class ServicioApagadoOrdenado(
    IRepositorioMonitoreo repositorioMonitoreo,
    IRepositorioEquipos repositorioEquipos,
    IAdaptadorConexion adaptador,
    OpcionesApagado opciones)
{
    /// <summary>
    /// Evalúa y ejecuta la acción de apagado sobre el dispositivo en servicio ante un disparo. Devuelve
    /// la acción registrada, o <c>null</c> si no hay dispositivo en servicio.
    /// </summary>
    /// <param name="eventoDisparoCodigo">Código del evento de disparo que la originó (si lo hubo).</param>
    /// <param name="ct">Token de cancelación.</param>
    public async Task<Accion?> EjecutarDisparoAsync(string? eventoDisparoCodigo, CancellationToken ct)
    {
        var dispositivo = await repositorioMonitoreo.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return null;
        }

        var ahora = DateTimeOffset.UtcNow;
        var verificaciones = await repositorioEquipos.ListarVerificacionesAsync(ct);
        var solicitada = opciones.ModalidadSolicitada;
        var efectiva = EvaluadorModalidad.Efectiva(solicitada, verificaciones, ahora);
        var verificados = EvaluadorModalidad.Verificados(verificaciones, ahora);
        var codigo = $"acc-{Guid.NewGuid():N}";

        var accion = await DecidirAsync(codigo, dispositivo.Codigo, solicitada, efectiva, verificados, ahora, eventoDisparoCodigo, ct);
        await repositorioMonitoreo.GuardarAccionAsync(accion, ct);
        return accion;
    }

    /// <summary>Situación actual para el panel: modalidad efectiva, bloqueo y últimas acciones.</summary>
    public async Task<SituacionApagado?> SituacionAsync(CancellationToken ct)
    {
        var dispositivo = await repositorioMonitoreo.DispositivoEnServicioAsync(ct);
        if (dispositivo is null)
        {
            return null;
        }

        var ahora = DateTimeOffset.UtcNow;
        var verificaciones = await repositorioEquipos.ListarVerificacionesAsync(ct);
        var solicitada = opciones.ModalidadSolicitada;
        var efectiva = EvaluadorModalidad.Efectiva(solicitada, verificaciones, ahora);
        var verificados = EvaluadorModalidad.Verificados(verificaciones, ahora);
        var acciones = await repositorioMonitoreo.AccionesRecientesAsync(dispositivo.Codigo, 10, ct);

        return new SituacionApagado(solicitada, efectiva, verificados, EvaluadorModalidad.SupuestosRequeridos, opciones.TiempoReservadoSeg, acciones);
    }

    private async Task<Accion> DecidirAsync(
        string codigo, string dispositivoCodigo, Modalidad solicitada, Modalidad efectiva, int verificados,
        DateTimeOffset ahora, string? eventoDisparoCodigo, CancellationToken ct)
    {
        // Modalidad base segura: solo aviso, no se apaga nada (RN-01).
        if (solicitada == Modalidad.SoloAlerta)
        {
            return Accion.SoloAviso(codigo, dispositivoCodigo, ahora,
                "Modalidad solicitada en solo aviso: se alerta, no se apaga.", eventoDisparoCodigo);
        }

        // Bloqueo por verificación: la modalidad de acción degradó a solo aviso (RN-02, ADR-10).
        if (efectiva == Modalidad.SoloAlerta)
        {
            return Accion.Bloqueada(codigo, dispositivoCodigo, solicitada, ahora,
                $"Bloqueada por verificación: {verificados}/{EvaluadorModalidad.SupuestosRequeridos} supuestos verificados y vigentes. Degrada a solo aviso.",
                eventoDisparoCodigo);
        }

        // Habilitada: se ordena el apagado con retorno y se confirma por efecto observado (ADR-11).
        var reservado = opciones.TiempoReservadoSeg;
        var resultado = await adaptador.OrdenarApagadoConRetornoAsync(TimeSpan.FromSeconds(reservado), ct);
        return resultado.Aceptada
            ? Accion.Ejecutada(codigo, dispositivoCodigo, solicitada, efectiva, reservado, ahora,
                $"Apagado con retorno ordenado y admitido por el equipo: {resultado.Motivo}", eventoDisparoCodigo)
            : Accion.EfectoNoConfirmado(codigo, dispositivoCodigo, solicitada, efectiva, reservado, ahora,
                $"El apagado no se confirmó por efecto observado: {resultado.Motivo}", eventoDisparoCodigo);
    }
}
