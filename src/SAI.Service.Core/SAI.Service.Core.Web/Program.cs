using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
using SAI.Service.Core.Api;
using SAI.Service.Core.Infrastructure;
using SAI.Service.Core.Web;
using SAI.Service.Core.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// --- Kestrel / TLS ---------------------------------------------------------
// Los endpoints (HTTP 8080 y, en Development, HTTPS 8443 con el certificado de
// desarrollo de ASP.NET Core) se configuran por "Kestrel:Endpoints" en
// appsettings. La terminacion TLS de produccion es la ranura pendiente P-04
// (ADR-20): en el contenedor se inyecta el certificado autofirmado por
// configuracion/variables de entorno. Kestrel toma esa seccion automaticamente.

// --- Sello de version (barra superior) -------------------------------------
builder.Services.Configure<SelloVersion>(builder.Configuration.GetSection("Sello"));

// --- Blazor interactive server + MudBlazor (§17.P.1) -----------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// --- Composition root: se componen las capas Api e Infrastructure (ADR-15) --
builder.Services.AddApi();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Ranura de autenticacion de administrador unico (ADR-16) ---------------
// Sprint 0 deja el andamiaje de authn/authz por cookie. El proveedor Identity
// completo (store sobre SaiDbContext, alta inicial del admin, cambiar contrasena
// y la exigencia de login en todo el panel salvo /health) se cablea en BT-10.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/acceso";
        options.Cookie.Name = "sai.auth";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// --- Pipeline HTTP ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints de la API: /health (anonimo) y /api/v1 (placeholder).
app.MapApi();

// Panel Blazor.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Punto de entrada del host. Se declara parcial y publica para que
/// <c>WebApplicationFactory&lt;Program&gt;</c> (pruebas de integracion) lo referencie.
/// </summary>
public partial class Program;
