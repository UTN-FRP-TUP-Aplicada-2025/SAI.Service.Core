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
}
