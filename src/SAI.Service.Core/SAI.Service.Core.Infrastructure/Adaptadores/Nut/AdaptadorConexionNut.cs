using System.Diagnostics;
using System.Globalization;
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
public sealed class AdaptadorConexionNut : IAdaptadorConexion, IDescubridorSai
{
    // Variables NUT que alimentan el estado, con su procedencia (Matriz-Sensado §5, RN-05).
    private const string VarEstado = "ups.status";
    private const string VarTensionEntrada = "input.voltage";   // Medido
    private const string VarTensionSalida = "output.voltage";   // Medido
    private const string VarCargaSalida = "ups.load";           // Medido
    private const string VarCargaBateria = "battery.charge";    // Estimado por driver, nunca medido
    private const string VarTensionBateria = "battery.voltage"; // Medido

    private readonly IClienteNut _cliente;

    /// <summary>Crea el adaptador sobre un cliente NUT.</summary>
    public AdaptadorConexionNut(IClienteNut cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);
        _cliente = cliente;
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

        try
        {
            await _cliente.EnviarComandoInstantaneoAsync(CmdApagadoConRetorno, ajustes, ct);

            // Efecto observado (ADR-11): el equipo admitió la orden (respondió OK, no ERR). El corte
            // físico ocurre tras el retardo; no se cancela aunque vuelva la red (ciclo forzado, ADR-09).
            return new ResultadoAccion(
                Aceptada: true,
                Motivo: $"El SAI admitió '{CmdApagadoConRetorno}' (retardo de corte {retardoApagadoSeg} s, retorno {RetardoRetornoSeg} s). "
                    + "El corte ocurrirá al vencer el retardo y no se cancelará (ciclo forzado).",
                MarcaTiempoUtc: ahora);
        }
        catch (NutException e)
        {
            // Falla de transporte o rechazo del servidor: no se observó el efecto (ADR-11).
            return new ResultadoAccion(false, e.Message, ahora);
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoAccion> LanzarTestBateriaAsync(CancellationToken ct)
    {
        var ahora = DateTimeOffset.UtcNow;
        try
        {
            await _cliente.EnviarComandoInstantaneoAsync(CmdTestBateria, [], ct);
            return new ResultadoAccion(
                Aceptada: true,
                Motivo: $"El SAI admitió '{CmdTestBateria}': autotest rápido de batería iniciado.",
                MarcaTiempoUtc: ahora);
        }
        catch (NutException e)
        {
            return new ResultadoAccion(false, e.Message, ahora);
        }
    }

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
