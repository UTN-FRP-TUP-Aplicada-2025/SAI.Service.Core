namespace SAI.Service.Core.Infrastructure.Adaptadores.Nut;

/// <summary>
/// Cliente de lectura del servidor NUT (upsd). Abstracción del transporte para que
/// <see cref="AdaptadorConexionNut"/> se pueda probar con un cliente falso, sin socket. La
/// implementación real es <see cref="ClienteNut"/>.
/// </summary>
public interface IClienteNut
{
    /// <summary>Nombre del UPS configurado.</summary>
    string Ups { get; }

    /// <summary>Punto final legible del servidor NUT (para diagnóstico).</summary>
    string PuntoFinal { get; }

    /// <summary>Devuelve el banner de versión del servidor (<c>VER</c>).</summary>
    Task<string> ObtenerVersionAsync(CancellationToken ct);

    /// <summary>Enumera los UPS declarados en el servidor (<c>LIST UPS</c>).</summary>
    Task<IReadOnlyList<(string Nombre, string Descripcion)>> ListarUpsAsync(CancellationToken ct);

    /// <summary>Lee todas las variables del UPS configurado (<c>LIST VAR</c>).</summary>
    Task<IReadOnlyDictionary<string, string>> LeerVariablesAsync(CancellationToken ct);

    /// <summary>Verdadero si el cliente tiene credenciales de escritura (para ordenar apagado/test).</summary>
    bool TieneCredencialesEscritura { get; }

    /// <summary>
    /// Ejecuta un comando instantáneo de escritura (US-14) en una sesión autenticada
    /// (<c>USERNAME</c>/<c>PASSWORD</c>/<c>LOGIN</c>): fija primero los <paramref name="ajustesPrevios"/>
    /// (<c>SET VAR</c>, p. ej. retardos de apagado/retorno) y luego emite <c>INSTCMD &lt;ups&gt; &lt;comando&gt;</c>.
    /// Lanza <see cref="NutException"/> si el servidor no responde <c>OK</c> a cualquier paso.
    /// </summary>
    Task EnviarComandoInstantaneoAsync(string comando, IReadOnlyList<(string Variable, string Valor)> ajustesPrevios, CancellationToken ct);
}
