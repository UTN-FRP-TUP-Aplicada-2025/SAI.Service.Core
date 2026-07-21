using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SAI.Service.Core.Infrastructure.Persistencia;

namespace SAI.Service.Core.Web.Autenticacion;

/// <summary>
/// Emite tokens JWT firmados para la API REST a partir de las credenciales validadas
/// del administrador (flujo ROPC del endpoint <c>POST /api/v1/token</c>). El token lleva
/// <c>sub</c>, nombre y el rol <c>administrador</c>, con vencimiento y emisor/audiencia de
/// configuración (<see cref="OpcionesJwt"/>).
/// </summary>
public sealed class GeneradorTokens(IOptions<OpcionesJwt> opciones)
{
    private readonly OpcionesJwt _opciones = opciones.Value;

    /// <summary>Rol único del sistema (ADR-16).</summary>
    public const string RolAdministrador = "administrador";

    /// <summary>
    /// Genera un token de acceso para el usuario indicado.
    /// </summary>
    /// <param name="usuarioId">Identificador del usuario (claim <c>sub</c>).</param>
    /// <param name="nombreUsuario">Nombre de usuario (claim <c>name</c>).</param>
    /// <returns>El token serializado y su vigencia en segundos.</returns>
    public (string AccessToken, int ExpiraEnSegundos) Generar(string usuarioId, string nombreUsuario)
    {
        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.ClaveFirma));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var ahora = DateTime.UtcNow;
        var vence = ahora.AddMinutes(_opciones.MinutosVigencia);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId),
            new(JwtRegisteredClaimNames.UniqueName, nombreUsuario),
            new(ClaimTypes.Name, nombreUsuario),
            new(ClaimTypes.Role, RolAdministrador),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            notBefore: ahora,
            expires: vence,
            signingCredentials: credenciales);

        var serializado = new JwtSecurityTokenHandler().WriteToken(token);
        return (serializado, _opciones.MinutosVigencia * 60);
    }

    /// <summary>Extensión de conveniencia para emitir el token de un <see cref="AdministradorUser"/>.</summary>
    /// <param name="usuario">Usuario autenticado.</param>
    /// <returns>El token serializado y su vigencia en segundos.</returns>
    public (string AccessToken, int ExpiraEnSegundos) Generar(AdministradorUser usuario) =>
        Generar(usuario.Id, usuario.UserName ?? string.Empty);
}
