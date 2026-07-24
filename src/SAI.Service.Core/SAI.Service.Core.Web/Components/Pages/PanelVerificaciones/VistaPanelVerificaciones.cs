using SAI.Service.Core.Application.Acciones;
using SAI.Service.Core.Application.Equipos;
using SAI.Service.Core.Domain.Verificaciones;

namespace SAI.Service.Core.Web.Components.Pages.PanelVerificaciones;

/// <summary>Modo efectivo del servicio para el encabezado (P-1).</summary>
public enum ModoServicio
{
    /// <summary>El servicio solo avisa; no apaga el host automáticamente.</summary>
    SoloAviso,

    /// <summary>El servicio apaga el host automáticamente ante un corte.</summary>
    ApagadoAutomatico,
}

/// <summary>Vista del estado del servicio para el encabezado del panel (P-1).</summary>
public sealed record ModoServicioVista(ModoServicio Modo, int Verificados, int Total, bool HayRefutado);

/// <summary>
/// View model de una tarjeta de verificación (SPEC 4.3): presentación pura, un solo cálculo alimenta
/// badge/color/orden/acciones. El estado es el <b>efectivo</b> (incluye «por vencer»). La evidencia de la
/// prueba de apagado se expone como valores (medido/reservado), no como cadena preformateada (H-7).
/// </summary>
public sealed record VistaVerificacion(
    Supuesto Supuesto,
    int Numero,
    string Titulo,
    string? Subtitulo,
    EstadoVerificacion Estado,
    string Actor,
    string? AccionOperador,
    bool RequierePresencia,
    bool CortaCorrienteReal,
    bool DisparaApagado,
    string QueSePrueba,
    string EsperasVer,
    IReadOnlyList<string> Pasos,
    string? EvidenciaTexto,
    int? MedidoSeg,
    int? ReservadoSeg,
    DateTimeOffset? FechaVencimiento,
    int? DiasRestantes,
    bool EsperandoReinicio)
{
    /// <summary>Verdadero si la tarjeta se muestra colapsada (vigente / por vencer), P-4.</summary>
    public bool Colapsada => Estado is EstadoVerificacion.Verificado or EstadoVerificacion.PorVencer;

    /// <summary>Verdadero si el margen de apagado es holgado (medido &lt; reservado), para el color de la evidencia.</summary>
    public bool MargenHolgado => MedidoSeg is { } m && ReservadoSeg is { } r && m < r;
}

/// <summary>
/// Construye la vista del panel a partir de las verificaciones de dominio (SPEC 4.1/4.4): ordena por la
/// secuencia física (P-2), calcula el estado efectivo con el umbral de preaviso, arma la evidencia
/// comparada y deriva el modo del servicio. Es presentación pura y unitariamente testeable.
/// </summary>
public sealed class ConstructorVistaPanel(OpcionesVerificacion opcionesVerificacion, OpcionesApagado opcionesApagado)
{
    // Orden físico fijo (P-2): lo define el dominio (SecuenciaFisica), no la vista.
    private static IReadOnlyList<Supuesto> OrdenFisico => SecuenciaFisica.Orden;

    /// <summary>Arma el encabezado y las tarjetas ordenadas para un instante dado.</summary>
    public (ModoServicioVista Modo, IReadOnlyList<VistaVerificacion> Tarjetas) Construir(
        IReadOnlyList<Verificacion> verificaciones, DateTimeOffset ahora)
    {
        var preaviso = opcionesVerificacion.DiasPreavisoVencimiento;
        var porSupuesto = verificaciones
            .GroupBy(v => v.Supuesto)
            .ToDictionary(g => g.Key, g => g.First());

        var tarjetas = new List<VistaVerificacion>();
        for (var i = 0; i < OrdenFisico.Count; i++)
        {
            var supuesto = OrdenFisico[i];
            if (!porSupuesto.TryGetValue(supuesto, out var v))
            {
                continue;
            }

            var meta = Meta[supuesto];
            int? dias = v.VigenciaHasta is { } hasta ? (int)Math.Ceiling((hasta - ahora).TotalDays) : null;
            tarjetas.Add(new VistaVerificacion(
                supuesto,
                Numero: i + 1,
                meta.Titulo,
                meta.Subtitulo,
                Estado: v.EstadoEfectivo(ahora, preaviso),
                meta.Actor,
                meta.AccionOperador,
                meta.RequierePresencia,
                meta.CortaCorriente,
                meta.DisparaApagado,
                meta.QueSePrueba,
                meta.EsperasVer,
                meta.Pasos,
                EvidenciaTexto: v.Evidencia,
                MedidoSeg: v.MedicionSegundos,
                ReservadoSeg: supuesto == Supuesto.PresupuestoDeApagado ? opcionesApagado.TiempoReservadoSeg : null,
                FechaVencimiento: v.VigenciaHasta,
                DiasRestantes: dias,
                EsperandoReinicio: v.EsperandoReinicio));
        }

        var verificados = EvaluadorModalidad.Verificados(verificaciones, ahora);
        var hayRefutado = verificaciones.Any(v => v.Estado == EstadoVerificacion.Refutado);
        var modo = verificados >= EvaluadorModalidad.SupuestosRequeridos
            ? ModoServicio.ApagadoAutomatico
            : ModoServicio.SoloAviso;

        return (new ModoServicioVista(modo, verificados, EvaluadorModalidad.SupuestosRequeridos, hayRefutado), tarjetas);
    }

    private sealed record MetaSupuesto(
        string Titulo,
        string? Subtitulo,
        string Actor,
        string? AccionOperador,
        bool RequierePresencia,
        bool CortaCorriente,
        bool DisparaApagado,
        string QueSePrueba,
        string EsperasVer,
        string[] Pasos);

    // Microcopy (SPEC sección 6): títulos cortos + subtítulo, término «pruebas», voseo.
    private static readonly Dictionary<Supuesto, MetaSupuesto> Meta = new()
    {
        [Supuesto.SenalEnBateria] = new(
            "Señal en batería",
            "El SAI avisa que pasó a batería (OB)",
            "Actúa el SAI",
            "Vos cortás la red",
            RequierePresencia: true, CortaCorriente: false, DisparaApagado: false,
            "que al quedarse sin energía de red, el SAI avise que está funcionando con su batería. Ese aviso es lo que dispara el apagado.",
            "el estado del equipo cambia a «en batería».",
            [
                "Cortás la energía de red que entra al SAI.",
                "El SAI queda alimentando con su batería y lo informa.",
                "El sistema lee ese aviso y confirma el estado «en batería» (no manda ninguna orden al equipo).",
            ]),
        [Supuesto.PresupuestoDeApagado] = new(
            "Apagado ordenado del host",
            "El host baja limpio y se cronometra el tiempo",
            "Actúa el host",
            "Vos cronometrás",
            RequierePresencia: true, CortaCorriente: false, DisparaApagado: true,
            "que el host baje todos sus servicios de forma ordenada, y cuánto tarda, para reservar bien la ventana de apagado.",
            "el host se apaga limpio; el tiempo medido tiene que ser holgadamente menor a la ventana reservada.",
            [
                "Con el host trabajando, esta prueba inicia su apagado ordenado (el sistema operativo cierra todos los servicios).",
                "Cronometrás cuánto tarda en quedar completamente apagado.",
                "Cargás los segundos que midió.",
            ]),
        [Supuesto.CorteConRetorno] = new(
            "Corte con retorno",
            "El SAI corta su salida y la repone",
            "Actúa el SAI",
            null,
            RequierePresencia: true, CortaCorriente: true, DisparaApagado: false,
            "que el SAI pueda cortar la corriente que alimenta al host y volver a dársela por sí solo. Es el mecanismo del apagado.",
            "el SAI corta la corriente del host y, al volver la energía de red, la reactiva.",
            [
                "Se le ordena al SAI apagar la salida que alimenta al host y volver a activarla.",
                "El SAI espera un tiempo configurado —para que el host termine de apagarse— y recién ahí corta la corriente de la salida.",
                "Cuando regresa la energía de red, el SAI se activa (tras una breve espera de estabilización) y vuelve a alimentar la salida del host.",
            ]),
        [Supuesto.ReencendidoPorPlaca] = new(
            "Arranque automático del host",
            "por presencia de energía en la alimentación del host",
            "Actúa el host / BIOS",
            "Vos observás",
            RequierePresencia: true, CortaCorriente: false, DisparaApagado: false,
            "que el host se encienda solo cuando le vuelve la corriente (por la configuración de la placa/BIOS). No se puede consultar por software: hay que verlo.",
            "el host enciende solo al volver la corriente, sin que nadie apriete el botón de encendido.",
            [
                "Después del corte con retorno, el SAI vuelve a alimentar la salida del host.",
                "Observás si el host arranca por su cuenta, sin que nadie toque el botón de encendido.",
                "Registrás lo que pasó: arrancó solo, o no arrancó.",
            ]),
    };
}
