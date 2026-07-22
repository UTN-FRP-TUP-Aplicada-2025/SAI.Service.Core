using SAI.Service.Core.Application.Abstractions;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Application.Equipos;

/// <summary>Código de resultado de un paso de la ventana de mantenimiento.</summary>
public enum CodigoResultadoVerificacion
{
    /// <summary>El supuesto quedó verificado.</summary>
    Verificado = 1,

    /// <summary>La orden al equipo no se confirmó por efecto observado (ADR-11): el paso no se supera.</summary>
    EfectoNoConfirmado = 2,

    /// <summary>El supuesto quedó refutado: bloqueo permanente.</summary>
    Refutado = 3,

    /// <summary>No hay una verificación para el supuesto (falta el equipo).</summary>
    SinVerificacion = 4,
}

/// <summary>Resultado de un paso de la ventana de mantenimiento.</summary>
public sealed record ResultadoVerificacion(CodigoResultadoVerificacion Codigo, string Mensaje);

/// <summary>
/// Orquesta la ventana de mantenimiento guiada (US-16, CU-10): verifica cada uno de los cuatro
/// supuestos por evidencia observada, aplicando la vigencia que corresponde a cada uno, y refuta el
/// reencendido por placa si el host no arranca solo (bloqueo permanente). Ningún paso se da por
/// superado por ausencia de excepción: se valida por efecto observado (ADR-11, RN-03). El write path
/// real por NUT está diferido (Incremento B); el camino se ejercita con el adaptador simulado.
/// </summary>
public sealed class ServicioVerificacion(IRepositorioEquipos repositorio, IAdaptadorConexion adaptador)
{
    private const string Metodo = "ventana-mantenimiento";

    /// <summary>Estado actual de las verificaciones (para el panel).</summary>
    public Task<IReadOnlyList<Verificacion>> EstadoAsync(CancellationToken ct) =>
        repositorio.ListarVerificacionesAsync(ct);

    /// <summary>Paso 1: presupuesto de apagado, cronometrado a mano bajo carga (180 días).</summary>
    public Task<ResultadoVerificacion> VerificarPresupuestoAsync(int segundos, CancellationToken ct) =>
        VerificarAsync(Supuesto.PresupuestoDeApagado, $"apagado cronometrado en {segundos} s bajo carga con contenedores detenidos", ct);

    /// <summary>Paso 2: señal en batería, observando ups.status = OB al cortar la red (365 días).</summary>
    public async Task<ResultadoVerificacion> VerificarSenalBateriaAsync(CancellationToken ct)
    {
        var estado = await adaptador.LeerEstadoAsync(ct);
        return estado.EstadoUps == EstadoUps.EnBateria
            ? await VerificarAsync(Supuesto.SenalEnBateria, "ups.status = OB observado al cortar la red", ct)
            : new ResultadoVerificacion(CodigoResultadoVerificacion.EfectoNoConfirmado, "no se observó el estado en batería (ups.status ≠ OB): cortá la red y reintentá");
    }

    /// <summary>Paso 3: corte con retorno, ejecutando el apagado con retorno y confirmando el efecto (sin caducidad).</summary>
    public async Task<ResultadoVerificacion> VerificarCorteConRetornoAsync(CancellationToken ct)
    {
        var accion = await adaptador.OrdenarApagadoConRetornoAsync(TimeSpan.FromSeconds(30), ct);
        return accion.Aceptada
            ? await VerificarAsync(Supuesto.CorteConRetorno, "corte con retorno ejecutado: el SAI cortó y repuso la salida", ct)
            : new ResultadoVerificacion(CodigoResultadoVerificacion.EfectoNoConfirmado, $"el corte con retorno no se confirmó: {accion.Motivo}");
    }

    /// <summary>Paso 4: reencendido por placa. Si el host no arranca solo, refuta (bloqueo permanente).</summary>
    public async Task<ResultadoVerificacion> RegistrarReencendidoAsync(bool arrancoSolo, CancellationToken ct)
    {
        if (arrancoSolo)
        {
            return await VerificarAsync(Supuesto.ReencendidoPorPlaca, "el host arrancó solo al restaurarse la energía", ct);
        }

        var verificacion = await repositorio.VerificacionDeSupuestoAsync(Supuesto.ReencendidoPorPlaca, ct);
        if (verificacion is null)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.SinVerificacion, "no hay una verificación para el supuesto");
        }

        verificacion.Refutar(Metodo, "el host NO arrancó solo: el autoencendido de placa no está soportado", DateTimeOffset.UtcNow);
        await repositorio.ActualizarVerificacionAsync(verificacion, ct);
        return new ResultadoVerificacion(CodigoResultadoVerificacion.Refutado, "supuesto refutado: bloqueo permanente hasta resolución");
    }

    private async Task<ResultadoVerificacion> VerificarAsync(Supuesto supuesto, string evidencia, CancellationToken ct)
    {
        var verificacion = await repositorio.VerificacionDeSupuestoAsync(supuesto, ct);
        if (verificacion is null)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.SinVerificacion, "no hay una verificación para el supuesto");
        }

        var ahora = DateTimeOffset.UtcNow;
        try
        {
            verificacion.Verificar(Metodo, evidencia, VigenciasSupuesto.VigenciaHasta(supuesto, ahora), ahora);
        }
        catch (InvalidOperationException)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.Refutado, "el supuesto está refutado: no puede reverificarse");
        }

        await repositorio.ActualizarVerificacionAsync(verificacion, ct);
        return new ResultadoVerificacion(CodigoResultadoVerificacion.Verificado, "supuesto verificado");
    }
}
