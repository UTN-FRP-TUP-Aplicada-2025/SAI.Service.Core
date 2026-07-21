using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace SAI.Service.Core.Web.Autenticacion;

/// <summary>
/// Proveedor de estado de autenticación para el panel Blazor (Server). Recibe el usuario
/// autenticado de la solicitud/cookie a través de
/// <see cref="IHostEnvironmentAuthenticationStateProvider"/> (lo llama la infraestructura
/// de render tanto en SSR estático como al iniciar el circuito interactivo) y lo expone a
/// <c>AuthorizeRouteView</c> / <c>AuthorizeView</c>.
/// <para>
/// Es la variante mínima (sin revalidación periódica). Alcanza para el administrador único
/// (ADR-16): la cookie es la fuente de verdad de la sesión.
/// </para>
/// </summary>
public sealed class ProveedorEstadoAutenticacionServidor
    : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider
{
    private static readonly Task<AuthenticationState> Anonimo =
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

    private Task<AuthenticationState> _estado = Anonimo;

    /// <inheritdoc />
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _estado;

    /// <inheritdoc />
    public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
    {
        ArgumentNullException.ThrowIfNull(authenticationStateTask);
        _estado = authenticationStateTask;
        NotifyAuthenticationStateChanged(_estado);
    }
}
