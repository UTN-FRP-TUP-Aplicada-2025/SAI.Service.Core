using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SAI.Service.Core.Application.Abstractions;

namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Adaptador de conexión real contra el SAI a través de <b>NUT</b> (Network UPS Tools), la
/// herramienta de acceso adoptada (ADR-01). Habla con <c>upsd</c> por su protocolo de red (no se
/// escribe un traductor de dialecto propio, ADR-01 E-04); el driver que posee el USB corre en el
/// mismo contenedor y recibe el nodo por ruta física (ADR-03/ADR-25, config de despliegue, no de
/// este código).
/// <para>
/// Toda lectura se valida por efecto observado (ADR-11): una excepción de transporte no es un
/// veredicto sobre el equipo, se traduce a "no alcanzable / no conectado" (ADR-27 §2).
/// </para>
/// </summary>
public sealed partial class AdaptadorConexionNut : IAdaptadorConexion, IDescubridorSai
{
    // --- Mensajes para el operador (UI). En términos de permiso/operación, sin jerga de NUT ni claves
    // de configuración: el detalle técnico (comando, punto final, causa) va al log, no a la pantalla. ---
    private const string MensajeSinPermiso =
        "El sistema no tiene permiso para enviarle órdenes al SAI, así que la prueba no se ejecutó. "
        + "Es un ajuste de configuración del servicio —sus credenciales de operación del SAI—, no un "
        + "problema del equipo ni de esta prueba. Consultá con quien administra el sistema.";

    private const string MensajeEquipoNoRespondio =
        "El SAI no respondió la orden o la rechazó, así que la prueba no se ejecutó. "
        + "Revisá que el equipo esté conectado y en línea, y reintentá.";

    // Variables NUT que alimentan el estado, con su procedencia (Matriz-Sensado §5, RN-05).
    private const string VarEstado = "ups.status";
    private const string VarTensionEntrada = "input.voltage";   // Medido
    private const string VarTensionSalida = "output.voltage";   // Medido
    private const string VarCargaSalida = "ups.load";           // Medido
    private const string VarCargaBateria = "battery.charge";    // Estimado por driver, nunca medido
    private const string VarTensionBateria = "battery.voltage"; // Medido

    private readonly IClienteNut _cliente;
    private readonly ILogger<AdaptadorConexionNut> _registro;

    /// <summary>Crea el adaptador sobre un cliente NUT. El logger es opcional (útil en pruebas).</summary>
    public AdaptadorConexionNut(IClienteNut cliente, ILogger<AdaptadorConexionNut>? registro = null)
    {
        ArgumentNullException.ThrowIfNull(cliente);
        _cliente = cliente;
        _registro = registro ?? NullLogger<AdaptadorConexionNut>.Instance;
    }

    /// <inheritdoc />
    public async Task<EstadoSai> LeerEstadoAsync(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        try
        {
            var variables = await _cliente.LeerVariablesAsync(ct);

            // Alcanzable por efecto observado: el equipo expuso su estado, no por ausencia de excepción.
            var alcanzable = variables.ContainsKey(VarEstado);

            return new EstadoSai(
                Alcanzable: alcanzable,
                TensionEntradaVoltios: LeerNumero(variables, VarTensionEntrada),
                TensionSalidaVoltios: LeerNumero(variables, VarTensionSalida),
                CargaSalidaPorcentaje: LeerNumero(variables, VarCargaSalida),
                CargaBateriaPorcentaje: LeerNumero(variables, VarCargaBateria),
                EstadoUps: InterpretarEstado(variables.GetValueOrDefault(VarEstado)),
                TensionBateriaVoltios: LeerNumero(variables, VarTensionBateria),
                MarcaTiempoUtc: ahora);
        }
        catch (NutException)
        {
            // Falla de transporte: no se observó el equipo (ADR-11).
            return new EstadoSai(false, null, null, null, null, null, null, ahora);
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoConectividad> ProbarConectividadAsync(CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        try
        {
            var variables = await _cliente.LeerVariablesAsync(ct);
            cronometro.Stop();

            // Efecto observado: se leyó una variable real del equipo (ADR-11, RN-03).
            if (variables.TryGetValue(VarEstado, out var estado))
            {
                var driver = variables.GetValueOrDefault("driver.name");
                var detalle = $"Conectado a {_cliente.PuntoFinal} (UPS {_cliente.Ups}); {VarEstado}={estado}"
                    + (driver is null ? string.Empty : $", driver={driver}");
                return new ResultadoConectividad(true, cronometro.Elapsed.TotalMilliseconds, detalle);
            }

            return new ResultadoConectividad(
                false,
                cronometro.Elapsed.TotalMilliseconds,
                $"PRUEBA_CONEXION_FALLIDA: el UPS {_cliente.Ups} respondió pero no expuso {VarEstado}.");
        }
        catch (NutException e)
        {
            return new ResultadoConectividad(false, null, $"PRUEBA_CONEXION_FALLIDA: {e.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DispositivoDescubierto>> DescubrirAsync(CancellationToken ct)
    {
        IReadOnlyList<(string Nombre, string Descripcion)> candidatos;
        try
        {
            candidatos = await _cliente.ListarUpsAsync(ct);
        }
        catch (NutException)
        {
            // No se pudo hablar con NUT: se informa como sin candidatos (DISPOSITIVO_NO_DESCUBIERTO).
            return [];
        }

        if (candidatos.Count == 0)
        {
            return [];
        }

        // Enriquecemos el UPS configurado (el equipo real) con sus variables identificatorias.
        IReadOnlyDictionary<string, string> variables;
        try
        {
            variables = await _cliente.LeerVariablesAsync(ct);
        }
        catch (NutException)
        {
            variables = new Dictionary<string, string>();
        }

        var descubiertos = new List<DispositivoDescubierto>(candidatos.Count);
        foreach (var (nombre, descripcion) in candidatos)
        {
            var esConfigurado = string.Equals(nombre, _cliente.Ups, StringComparison.Ordinal);
            var vars = esConfigurado ? variables : new Dictionary<string, string>();

            var vendorId = vars.GetValueOrDefault("driver.parameter.vendorid");
            var productId = vars.GetValueOrDefault("driver.parameter.productid");
            var driver = vars.GetValueOrDefault("driver.name");
            var serie = vars.GetValueOrDefault("device.serial") ?? vars.GetValueOrDefault("ups.serial");

            descubiertos.Add(new DispositivoDescubierto(
                NombreNut: nombre,
                Descriptor: ComponerDescriptor(vendorId, productId, descripcion, serie),
                VendorId: vendorId,
                ProductId: productId,
                Driver: driver,
                NumeroSerie: serie));
        }

        return descubiertos;
    }

    // Comando INSTCMD de apagado con retorno del SAI (ADR-09): corta la salida y la repone al volver
    // la energía. NUNCA se emite shutdown.stop (el ciclo forzado no se cancela).
    private const string CmdApagadoConRetorno = "shutdown.return";

    // Comando INSTCMD del autotest rápido de batería (US-12).
    private const string CmdTestBateria = "test.battery.start.quick";

    // Variables NUT de temporización del apagado con retorno (ADR-27 §6.2): retardo de corte y de
    // reposición de la salida. El retorno fijo de 180 s da la transición ausencia→presencia (ADR-09).
    private const string VarRetardoApagado = "ups.delay.shutdown";
    private const string VarRetardoRetorno = "ups.delay.start";
    private const int RetardoRetornoSeg = 180;

    /// <inheritdoc />
    public async Task<ResultadoAccion> OrdenarApagadoConRetornoAsync(TimeSpan retardo, CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        var retardoApagadoSeg = Math.Max(0, (int)Math.Round(retardo.TotalSeconds));
        var ajustes = new[]
        {
            (VarRetardoApagado, retardoApagadoSeg.ToString(CultureInfo.InvariantCulture)),
            (VarRetardoRetorno, RetardoRetornoSeg.ToString(CultureInfo.InvariantCulture)),
        };

        if (!_cliente.TieneCredencialesEscritura)
        {
            SinCredencialesEscritura(CmdApagadoConRetorno);
            return new ResultadoAccion(false, MensajeSinPermiso, ahora);
        }

        try
        {
            await _cliente.EnviarComandoInstantaneoAsync(CmdApagadoConRetorno, ajustes, ct);

            // Efecto observado (ADR-11): el equipo admitió la orden (respondió OK, no ERR). El corte
            // físico ocurre tras el retardo; no se cancela aunque vuelva la red (ciclo forzado, ADR-09).
            return new ResultadoAccion(
                Aceptada: true,
                Motivo: "El SAI aceptó la orden: cortará su salida tras el tiempo de espera configurado "
                    + "y la repondrá cuando vuelva la energía de red.",
                MarcaTiempoUtc: ahora);
        }
        catch (NutException e)
        {
            // Falla de transporte o rechazo del servidor: no se observó el efecto (ADR-11).
            OrdenNoConfirmada(CmdApagadoConRetorno, e);
            return new ResultadoAccion(false, MensajeEquipoNoRespondio, ahora);
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        if (!_cliente.TieneCredencialesEscritura)
        {
            SinCredencialesEscritura(CmdTestBateria);
            return new ResultadoAccion(false, MensajeSinPermiso, ahora);
        }

        try
        {
            await _cliente.EnviarComandoInstantaneoAsync(CmdTestBateria, [], ct);
            return new ResultadoAccion(
                Aceptada: true,
                Motivo: "El SAI aceptó la orden: inició el autotest de batería.",
                MarcaTiempoUtc: ahora);
        }
        catch (NutException e)
        {
            OrdenNoConfirmada(CmdTestBateria, e);
            return new ResultadoAccion(false, MensajeEquipoNoRespondio, ahora);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "Sin credenciales de escritura para operar el SAI (comando {Comando}). Configurar Sai:Nut:Usuario y Sai:Nut:Password (rol de escritura).")]
    private partial void SinCredencialesEscritura(string comando);

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "El SAI no confirmó la orden (comando {Comando}): no se observó el efecto.")]
    private partial void OrdenNoConfirmada(string comando, Exception causa);

    // ups.status puede traer varios flags (p. ej. "OL CHRG", "OB DISCHRG"). La presencia de "OB"
    // indica que el equipo pasó a batería (DM-05).
    private static EstadoUps? InterpretarEstado(string? status) =>
        string.IsNullOrWhiteSpace(status)
            ? null
            : status.Contains("OB", StringComparison.Ordinal) ? EstadoUps.EnBateria : EstadoUps.EnLinea;

    private static double? LeerNumero(IReadOnlyDictionary<string, string> variables, string clave) =>
        variables.TryGetValue(clave, out var texto)
            && double.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;

    private static string ComponerDescriptor(string? vendorId, string? productId, string? descripcion, string? serie)
    {
        var idUsb = vendorId is not null && productId is not null
            ? $"{vendorId}:{productId}"
            : vendorId ?? productId ?? "id USB desconocido";

        var marca = string.IsNullOrWhiteSpace(descripcion) ? "sin marca ni modelo" : descripcion;
        var serieTexto = string.IsNullOrWhiteSpace(serie) ? "vacío" : serie;

        return $"{idUsb} · {marca} · serie: {serieTexto}";
    }
}
