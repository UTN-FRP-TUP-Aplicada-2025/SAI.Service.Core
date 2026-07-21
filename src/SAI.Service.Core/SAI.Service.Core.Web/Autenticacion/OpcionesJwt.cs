namespace SAI.Service.Core.Web.Autenticacion;

/// <summary>
/// Configuración del token JWT que emite y valida la API REST (<c>/api/v1</c>).
/// Se enlaza desde la sección <c>Jwt</c> de la configuración. La clave de firma es un
/// secreto: en Development viene de <c>appsettings.Development.json</c>; en producción se
/// inyecta por variable de entorno <c>Jwt__ClaveFirma</c> (gestión de secretos de 09 /
/// ADR-20). No se debe hornear una clave real en el repositorio.
/// </summary>
public sealed class OpcionesJwt
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Seccion = "Jwt";

    /// <summary>Emisor del token (claim <c>iss</c>).</summary>
    public string Emisor { get; init; } = "sai-service-core";

    /// <summary>Audiencia del token (claim <c>aud</c>).</summary>
    public string Audiencia { get; init; } = "sai-service-core-api";

    /// <summary>
    /// Clave simétrica de firma (HMAC-SHA256). Debe tener al menos 32 bytes.
    /// Ranura de secreto: se sobreescribe por entorno en producción.
    /// </summary>
    public string ClaveFirma { get; init; } = string.Empty;

    /// <summary>Vigencia del token en minutos.</summary>
    public int MinutosVigencia { get; init; } = 60;
}
