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

    /// <summary>Se disparó una prueba física que exige reiniciar el host antes de repetirse (Etapa 4·E).</summary>
    PruebaDisparada = 5,
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

    /// <summary>Paso 1: presupuesto de apagado, cronometrado <b>a mano</b> bajo carga (180 días).</summary>
    /// <remarks>
    /// El tiempo se carga a mano (CU-10): por ADR-25 el servicio corre en el contenedor que el propio
    /// apagado detiene, así que no puede cronometrarlo desde adentro. Nunca se rotula como "medido".
    /// </remarks>
    public Task<ResultadoVerificacion> VerificarPresupuestoAsync(int segundos, CancellationToken ct) =>
        VerificarAsync(Supuesto.PresupuestoDeApagado, $"apagado cronometrado a mano en {segundos} s bajo carga", ct);

    /// <summary>
    /// Etapa 4·E — dispara el apagado ordenado del host para la prueba de tiempo de apagado y deja la
    /// verificación <b>esperando el reinicio</b> (freno para no re-disparar el apagado "a lo loco"). El
    /// tiempo se cronometra y se carga a mano después (ver <see cref="VerificarPresupuestoAsync"/>). Reusa
    /// el write path de apagado con retorno (el host baja limpio y reenciende al volver la energía); en
    /// desarrollo el simulado lo acepta de forma inerte.
    /// </summary>
    public async Task<ResultadoVerificacion> DispararPruebaPresupuestoAsync(CancellationToken ct)
    {
        var verificacion = await repositorio.VerificacionDeSupuestoAsync(Supuesto.PresupuestoDeApagado, ct);
        if (verificacion is null)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.SinVerificacion, "no hay una verificación para el supuesto");
        }

        if (verificacion.EsperandoReinicio)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.PruebaDisparada, "ya hay una prueba disparada: se activará cuando reinicie el equipo");
        }

        // El retardo es el tiempo que el SAI espera antes de cortar la salida, para que el host baje limpio.
        var accion = await adaptador.OrdenarApagadoConRetornoAsync(TimeSpan.FromSeconds(30), ct);
        if (!accion.Aceptada)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.EfectoNoConfirmado, $"el apagado no se disparó: {accion.Motivo}");
        }

        verificacion.IniciarPrueba(DateTimeOffset.UtcNow);
        await repositorio.ActualizarVerificacionAsync(verificacion, ct);
        return new ResultadoVerificacion(CodigoResultadoVerificacion.PruebaDisparada,
            "apagado disparado: cronometrá el tiempo a mano y cargalo cuando el host reinicie");
    }

    /// <summary>
    /// Etapa 4·E — rearma las pruebas que quedaron esperando un reinicio (limpia
    /// <see cref="Verificacion.PruebaEnCursoDesde"/>). Lo invoca el servicio de arranque tras un reinicio
    /// del host (ADR-25: el contenedor se reinicia con el host), habilitando de nuevo la acción en el panel.
    /// </summary>
    public async Task RearmarPruebasPendientesAsync(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var verificaciones = await repositorio.ListarVerificacionesAsync(ct);
        foreach (var verificacion in verificaciones)
        {
            if (!verificacion.EsperandoReinicio)
            {
                continue;
            }

            verificacion.RearmarPorReinicio(ahora);
            await repositorio.ActualizarVerificacionAsync(verificacion, ct);
        }
    }

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

    /// <summary>
    /// Paso 4: encendido por presencia de energía. Si el host arranca solo, verifica y deja la prueba
    /// esperando el reinicio (Etapa 4·E: mismo freno que el presupuesto). Si no arranca, refuta (bloqueo
    /// permanente).
    /// </summary>
    public async Task<ResultadoVerificacion> RegistrarReencendidoAsync(bool arrancoSolo, CancellationToken ct)
    {
        var verificacion = await repositorio.VerificacionDeSupuestoAsync(Supuesto.ReencendidoPorPlaca, ct);
        if (verificacion is null)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.SinVerificacion, "no hay una verificación para el supuesto");
        }

        var ahora = DateTimeOffset.UtcNow;
        if (!arrancoSolo)
        {
            verificacion.Refutar(Metodo, "el host NO arrancó solo: el autoencendido de placa no está soportado", ahora);
            await repositorio.ActualizarVerificacionAsync(verificacion, ct);
            return new ResultadoVerificacion(CodigoResultadoVerificacion.Refutado, "supuesto refutado: bloqueo permanente hasta resolución");
        }

        try
        {
            verificacion.Verificar(Metodo, "el host arrancó solo al restaurarse la energía",
                VigenciasSupuesto.VigenciaHasta(Supuesto.ReencendidoPorPlaca, ahora), ahora);
        }
        catch (InvalidOperationException)
        {
            return new ResultadoVerificacion(CodigoResultadoVerificacion.Refutado, "el supuesto está refutado: no puede reverificarse");
        }

        // Gate hasta el próximo reinicio: mismo comportamiento que el botón del presupuesto.
        verificacion.IniciarPrueba(ahora);
        await repositorio.ActualizarVerificacionAsync(verificacion, ct);
        return new ResultadoVerificacion(CodigoResultadoVerificacion.Verificado, "supuesto verificado");
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
