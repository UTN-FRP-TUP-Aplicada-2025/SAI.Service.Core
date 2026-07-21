using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using SAI.Service.Core.Api;
using SAI.Service.Core.Infrastructure;
using SAI.Service.Core.Infrastructure.Persistencia;
using SAI.Service.Core.Web;
using SAI.Service.Core.Web.Autenticacion;
using SAI.Service.Core.Web.Components;
using SAI.Service.Core.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// --- Kestrel / TLS ---------------------------------------------------------
// Los endpoints (HTTP 8080 y, en Development, HTTPS 8443 con el certificado de
// desarrollo) se configuran por "Kestrel:Endpoints" en appsettings. La terminacion
// TLS de produccion es la ranura P-04 (ADR-20).

// --- Sello de version (barra superior) -------------------------------------
builder.Services.Configure<SelloVersion>(builder.Configuration.GetSection("Sello"));

// --- Blazor interactive server + MudBlazor (§17.P.1) -----------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// --- Composition root: se componen las capas Api e Infrastructure (ADR-15) --
builder.Services.AddApi();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Identity: administrador unico sobre SaiDbContext (ADR-16) -------------
// Politica de contrasena de la Linea-Base-Visual (EST-03): >= 12 caracteres, con
// letras y numeros.
builder.Services
    .AddIdentity<AdministradorUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 1;

        options.User.RequireUniqueEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<SaiDbContext>()
    .AddDefaultTokenProviders();

// --- DataProtection: keyring persistente (cookie de sesion + antiforgery) ----
// Sin persistir, el keyring es efimero por contenedor: al reiniciar o redesplegar
// se invalidan la cookie de sesion (obliga a re-loguear) y, peor, los tokens
// antiforgery de los formularios SSR (POST -> HTTP 400 "token could not be
// decrypted"). Se persiste en el directorio de 'DataProtection:RutaLlaves' (un
// volumen persistente en produccion) y se fija SetApplicationName para que el
// "proposito" del keyring sea estable entre instancias de la misma app. Si la ruta
// no se configura, el keyring queda efimero (aceptable solo en desarrollo).
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("SAI.Service.Core");
var rutaLlaves = builder.Configuration["DataProtection:RutaLlaves"];
if (!string.IsNullOrWhiteSpace(rutaLlaves))
{
    Directory.CreateDirectory(rutaLlaves);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(rutaLlaves));
}

// Cookie de sesion del panel (ADR-16): la superficie de acceso es /acceso.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "sai.auth";
    options.LoginPath = "/acceso";
    options.AccessDeniedPath = "/acceso";
    options.SlidingExpiration = true;
});

// --- Autenticacion Bearer para la API REST /api/v1 -------------------------
// Doble esquema: la cookie de Identity queda como esquema por defecto (panel) y el
// Bearer se exige explicitamente en los endpoints de API por la policy "Api".
builder.Services.Configure<OpcionesJwt>(builder.Configuration.GetSection(OpcionesJwt.Seccion));
var opcionesJwt = builder.Configuration.GetSection(OpcionesJwt.Seccion).Get<OpcionesJwt>() ?? new OpcionesJwt();

if (Encoding.UTF8.GetByteCount(opcionesJwt.ClaveFirma) < 32)
{
    throw new InvalidOperationException(
        "La clave de firma JWT (Jwt:ClaveFirma) debe tener al menos 32 bytes. " +
        "En Development se define en appsettings.Development.json; en produccion se " +
        "inyecta por la variable de entorno Jwt__ClaveFirma (ADR-20).");
}

builder.Services.AddSingleton<GeneradorTokens>();

builder.Services
    .AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

// Los parametros de validacion del Bearer se resuelven de forma DIFERIDA desde
// IOptions<OpcionesJwt>, el mismo origen que usa GeneradorTokens para firmar. Asi el
// firmado y la validacion comparten SIEMPRE la misma clave/emisor/audiencia, incluso
// cuando la configuracion se sobreescribe despues del registro (p. ej. la clave de
// pruebas que inyectan las pruebas de integracion con WebApplicationFactory). Leerlos
// eager aca disociaba la clave de validacion de la de firma -> 401 en la API.
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((bearer, jwt) =>
    {
        var o = jwt.Value;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = o.Emisor,
            ValidateAudience = true,
            ValidAudience = o.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.ClaveFirma)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

// Policy "Api": exige un Bearer valido (no la cookie del panel).
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Api", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

// Estado de autenticacion en cascada para el panel Blazor (<AuthorizeView>,
// AuthorizeRouteView). El proveedor recibe el usuario de la cookie/solicitud.
builder.Services.AddScoped<
    Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider,
    ProveedorEstadoAutenticacionServidor>();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// --- Migraciones al arranque (ADR-18) y semilla del rol unico (ADR-16) -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SaiDbContext>();
    await db.Database.MigrateAsync();

    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roles.RoleExistsAsync(GeneradorTokens.RolAdministrador))
    {
        await roles.CreateAsync(new IdentityRole(GeneradorTokens.RolAdministrador));
    }
}

// --- Pipeline HTTP ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- Guarda de primer arranque: capa de RUTEO (Design-Rules-Primer-Arranque §3) --
// Mientras no exista ningun administrador, las superficies del panel se desvian a
// /alta-inicial ANTES de la autenticacion, de modo que el primer arranque ofrece el
// alta y no el login. Es la primera de las tres capas (ruteo / superficie / accion);
// las superficies (Acceso/AltaInicial) y la accion de alta conservan sus propias
// guardas. Una vez creado el administrador, la comprobacion se apaga de forma
// permanente (bandera cacheada) para no consultar la base en cada solicitud.
var hayAdministrador = false;
app.Use(async (context, next) =>
{
    if (!hayAdministrador)
    {
        var usuarios = context.RequestServices.GetRequiredService<UserManager<AdministradorUser>>();
        if (await usuarios.Users.AnyAsync())
        {
            hayAdministrador = true;
        }
        else if (EsSuperficieDePanel(context.Request.Path))
        {
            context.Response.Redirect("/alta-inicial");
            return;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// La validacion antiforgery debe correr DESPUES de la autenticacion. Los tokens de los
// formularios SSR autenticados (p. ej. cambiar contrasena) se vinculan al usuario por su
// claim UID; si UseAntiforgery corriera antes de UseAuthentication, al validar el POST
// HttpContext.User todavia seria anonimo y el token no coincidiria con el usuario actual
// ("The provided antiforgery token was meant for a different claims-based user") -> HTTP 400.
// Los formularios anonimos (alta inicial, login) no lo notan porque no hay usuario que atar.
app.UseAntiforgery();

// Endpoints de la API: /health (anonimo), /api/v1 (informativo + ping Bearer)
// y los endpoints de acceso (token ROPC y cierre de sesion).
app.MapApi();
app.MapEndpointsAcceso();

// Panel Blazor.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Rutas exentas de la guarda de ruteo: la propia alta inicial, las superficies de
// acceso/cuenta (con su propia guarda), la API y /health (responden por su cuenta) y
// los activos e infraestructura del framework. El resto son superficies del panel y se
// desvian al alta inicial durante el primer arranque.
static bool EsSuperficieDePanel(PathString ruta)
{
    string[] exentas =
    [
        "/alta-inicial", "/acceso", "/cuenta",
        "/health", "/api",
        "/_blazor", "/_framework", "/_content",
    ];

    foreach (var prefijo in exentas)
    {
        if (ruta.StartsWithSegments(prefijo, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
    }

    // Activos estaticos: el ultimo segmento lleva extension (css, js, ico, woff...).
    var valor = ruta.Value;
    if (!string.IsNullOrEmpty(valor))
    {
        var ultimoSegmento = valor.AsSpan(valor.LastIndexOf('/') + 1);
        if (ultimoSegmento.Contains('.'))
        {
            return false;
        }
    }

    return true;
}

/// <summary>
/// Punto de entrada del host. Se declara parcial y publica para que
/// <c>WebApplicationFactory&lt;Program&gt;</c> (pruebas de integracion) lo referencie.
/// </summary>
public partial class Program;
