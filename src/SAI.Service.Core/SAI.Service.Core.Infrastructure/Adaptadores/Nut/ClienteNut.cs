using System.Net.Sockets;
using System.Text;

namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Cliente TCP mínimo del protocolo de red de NUT (upsd). Abre una conexión efímera por operación
/// (los sondeos son poco frecuentes; no hace falta pool ni keepalive) y usa solo los comandos de
/// <b>lectura</b> anónima (<c>VER</c>, <c>LIST UPS</c>, <c>LIST VAR</c>), que no requieren login.
/// Los comandos de escritura (apagado, test de batería) llegan en la Etapa 4 con credenciales de
/// administrador. El parseo del protocolo vive en <see cref="ProtocoloNut"/> (probado sin socket).
/// </summary>
public sealed class ClienteNut : IClienteNut
{
    private readonly OpcionesNut _opciones;

    /// <summary>Crea el cliente con la configuración del servidor NUT.</summary>
    public ClienteNut(OpcionesNut opciones)
    {
        ArgumentNullException.ThrowIfNull(opciones);
        _opciones = opciones;
    }

    /// <summary>Nombre del UPS configurado.</summary>
    public string Ups => _opciones.Ups;

    /// <summary>Punto final legible del servidor NUT (para diagnóstico).</summary>
    public string PuntoFinal => $"{_opciones.Host}:{_opciones.Puerto}";

    /// <summary>Devuelve el banner de versión del servidor (<c>VER</c>): confirma que responde.</summary>
    /// <exception cref="NutException">Si no se pudo conectar o el servidor no respondió.</exception>
    public Task<string> ObtenerVersionAsync(CancellationToken ct) =>
        ConConexionAsync(async (lector, escritor, token) =>
        {
            await escritor.WriteLineAsync("VER".AsMemory(), token);
            var linea = await lector.ReadLineAsync(token);
            if (linea is null || ProtocoloNut.EsError(linea))
            {
                throw new NutException($"El servidor NUT en {PuntoFinal} no devolvió su versión: {linea ?? "(sin respuesta)"}.");
            }

            return linea;
        }, ct);

    /// <summary>Enumera los UPS declarados en el servidor (<c>LIST UPS</c>): nombre y descripción.</summary>
    /// <exception cref="NutException">Si no se pudo conectar o el listado falló.</exception>
    public Task<IReadOnlyList<(string Nombre, string Descripcion)>> ListarUpsAsync(CancellationToken ct) =>
        ConConexionAsync(async (lector, escritor, token) =>
        {
            var lineas = await LeerListaAsync(escritor, lector, "LIST UPS", "END LIST UPS", token);
            IReadOnlyList<(string, string)> ups = lineas
                .Select(ProtocoloNut.ParsearLineaUps)
                .Where(p => p is not null)
                .Select(p => p!.Value)
                .ToList();
            return ups;
        }, ct);

    /// <summary>
    /// Lee todas las variables del UPS configurado en una sola pasada (<c>LIST VAR &lt;ups&gt;</c>).
    /// </summary>
    /// <exception cref="NutException">Si no se pudo conectar o el listado falló.</exception>
    public Task<IReadOnlyDictionary<string, string>> LeerVariablesAsync(CancellationToken ct) =>
        ConConexionAsync(async (lector, escritor, token) =>
        {
            var lineas = await LeerListaAsync(escritor, lector, $"LIST VAR {_opciones.Ups}", "END LIST VAR", token);
            var variables = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var linea in lineas)
            {
                if (ProtocoloNut.ParsearLineaVar(linea, _opciones.Ups) is { } par)
                {
                    variables[par.Variable] = par.Valor;
                }
            }

            IReadOnlyDictionary<string, string> resultado = variables;
            return resultado;
        }, ct);

    private static async Task<List<string>> LeerListaAsync(
        StreamWriter escritor,
        StreamReader lector,
        string comando,
        string prefijoFin,
        CancellationToken ct)
    {
        await escritor.WriteLineAsync(comando.AsMemory(), ct);

        var primera = await lector.ReadLineAsync(ct);
        if (primera is null || ProtocoloNut.EsError(primera))
        {
            throw new NutException($"El servidor NUT rechazó '{comando}': {primera ?? "(sin respuesta)"}.");
        }

        var lineas = new List<string>();
        string? linea;
        while ((linea = await lector.ReadLineAsync(ct)) is not null)
        {
            if (linea.StartsWith(prefijoFin, StringComparison.Ordinal))
            {
                return lineas;
            }

            lineas.Add(linea);
        }

        throw new NutException($"El servidor NUT cerró la conexión sin cerrar el listado '{comando}'.");
    }

    private async Task<T> ConConexionAsync<T>(
        Func<StreamReader, StreamWriter, CancellationToken, Task<T>> operacion,
        CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_opciones.TimeoutSegundos));
        var token = cts.Token;

        using var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(_opciones.Host, _opciones.Puerto, token);

            await using var flujo = tcp.GetStream();
            using var lector = new StreamReader(flujo, Encoding.Latin1, false, 1024, leaveOpen: true);
            await using var escritor = new StreamWriter(flujo, Encoding.Latin1, 1024, leaveOpen: true)
            {
                NewLine = "\n",
                AutoFlush = true,
            };

            var resultado = await operacion(lector, escritor, token);

            // Cierre cortés del protocolo; si falla, no altera el resultado ya obtenido.
            try
            {
                await escritor.WriteLineAsync("LOGOUT".AsMemory(), token);
            }
            catch (Exception e) when (e is IOException or OperationCanceledException)
            {
                // Ignorado: la operación ya devolvió su resultado.
            }

            return resultado;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new NutException(
                $"Tiempo de espera agotado ({_opciones.TimeoutSegundos}s) hablando con NUT en {PuntoFinal}.");
        }
        catch (Exception e) when (e is SocketException or IOException)
        {
            throw new NutException($"No se pudo hablar con NUT en {PuntoFinal}: {e.Message}", e);
        }
    }
}
