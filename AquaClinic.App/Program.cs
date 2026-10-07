using AquaClinic.App;
using AquaClinic.App.Auth;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Radzen;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var culturaPredeterminada = new CultureInfo("en-CA");
CultureInfo.DefaultThreadCurrentCulture = culturaPredeterminada;
CultureInfo.DefaultThreadCurrentUICulture = culturaPredeterminada;

var culturasSoportadas = new[] { "en-CA", "es-ES" };
var opcionesLocalizacion = new RequestLocalizationOptions()
    .SetDefaultCulture("en-CA")
    .AddSupportedCultures(culturasSoportadas)
    .AddSupportedUICultures(culturasSoportadas);
opcionesLocalizacion.RequestCultureProviders = [new CookieRequestCultureProvider()];
builder.Services.AddLocalization();

// Blazor Web App con render interactivo Server por pagina
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// AutoMapper (perfiles de mapeo de la App)
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// Necesario para que servicios como el saludo accedan al HttpContext del request
builder.Services.AddHttpContextAccessor();

// Cache del saludo del nav (nombre real o derivado del correo), compartido
// entre las islas interactivas de un mismo circuito (NavSaludo <-> MiPerfil)
builder.Services.AddSingleton<UsuarioSaludoCache>();

//Agregando radzen
builder.Services.AddRadzenComponents();

// HttpClient hacia la API con AuthHeaderHandler:
// inyecta Authorization: Bearer con el JWT de la sesion (TokenStore) y
// renueva el token automaticamente ante un 401 (api/usuarios/refresh).
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7022/";

// Almacen del JWT/refresh token por circuito (scoped)
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped<TokenStoreInitializer>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<IClient>(sp => new Client(sp.GetRequiredService<HttpClient>()));

// Servicios de datos (consumen la API via REST)
builder.Services.AddScoped<IUsuarioDataService, UsuarioDataService>();
builder.Services.AddScoped<ICitaDataService, CitaDataService>();
builder.Services.AddScoped<IEspecialidadDataService, EspecialidadDataService>();
builder.Services.AddScoped<IServicioDataService, ServicioDataService>();
builder.Services.AddScoped<IBloqueoDataService, BloqueoDataService>();

// Notificaciones globales de la UI (toasts Radzen)
builder.Services.AddScoped<NotificadorUi>();

// Agenda en tiempo real (SignalR hacia la API)
builder.Services.AddScoped<AgendaRealtimeService>();
builder.Services.AddScoped<IReservasDataService, ReservasDataService>();
builder.Services.AddScoped<IServiciosPublicosDataService, ReservasDataService>();

// Servicio elegido + re-anclaje de hora compartidos entre /reservar y el dialogo
builder.Services.AddScoped<SeleccionReservaEstado>();

// Autenticacion via JWT: el principal se construye desde el access token del TokenStore
builder.Services.AddScoped<ApiAuthenticationStateProvider>();

// Esquema de autenticacion de la App (para el [Authorize] del middleware):
// reconstruye el principal desde la sesion del ticket (igual que el circuito
// interactivo). La sesion real vive en el JWT; este esquema nunca emite cookies.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "SesionTicket";
        options.DefaultChallengeScheme = "SesionTicket";
    })
    .AddScheme<AuthenticationSchemeOptions, SesionTicketAuthHandler>("SesionTicket", null);

builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());

// Cliente nombrado para los endpoints de autenticacion minimal API
// (sin AuthHeaderHandler: login/refresh no llevan Authorization)
builder.Services.AddHttpClient("ApiAuth", client => client.BaseAddress = new Uri(apiBaseUrl));

var app = builder.Build();

app.UseRequestLocalization(opcionesLocalizacion);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// ============================================================
// Endpoints minimal API para el flujo de autenticacion JWT.
// Reciben el form POST del navegador, piden el JWT a la API y GUARDAN los
// tokens en el TokenStore del circuito. No hay cookies de sesion: el
// principal se construye desde el JWT (ApiAuthenticationStateProvider).
// ============================================================

app.MapPost("/auth/login", async (HttpContext ctx, IHttpClientFactory httpClientFactory, TokenStore tokens) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();

    // El modal de login envia via fetch y espera JSON (LoginDialog.razor);
    // la pagina /login usa el POST tradicional y espera redirecciones.
    // El header Fetch-Metadata (sec-fetch-* marca a todos los fetch del
    // mismo origen con "cors"... se usa X-Requested-With, estandar simple.
    var esFetch = string.Equals(ctx.Request.Headers["X-Requested-With"].ToString(), "fetch", StringComparison.OrdinalIgnoreCase);

    // Respuesta JSON para el modal: el cliente decide (mostrar error o navegar)
    IResult RedirigirOJson(string url)
    {
        return esFetch ? Results.Json(new { redirect = url }) : Results.Redirect(url);
    }

    using var client = httpClientFactory.CreateClient("ApiAuth");
    var response = await client.PostAsJsonAsync(
        "api/usuarios/login",
        new { email, password });

    if (!response.IsSuccessStatusCode)
    {
        var error = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "credenciales",
            System.Net.HttpStatusCode.TooManyRequests => "ratelimit",
            _ => "api"
        };
        return RedirigirOJson($"/login?error={error}");
    }

    var sesion = await response.Content.ReadFromJsonAsync<SesionJwt>();

    if (sesion is null || string.IsNullOrEmpty(sesion.AccessToken))
    {
        return RedirigirOJson("/login?error=api");
    }

    // La sesion (tokens) vive en el SERVIDOR; al navegador solo se le entrega
    // un ticket opaco en una cookie httpOnly. Cada circuito nuevo la recupera
    // con ese ticket (TokenStoreInitializer), sobreviviendo a recargas.
    var ticket = SesionPendiente.Registrar(sesion);
    ctx.Response.Cookies.Append(SesionPendiente.NombreCookie, ticket, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = true,
        MaxAge = TimeSpan.FromDays(7)
    });

    // El personal aterriza directo en Citas; clientes y resto van a la home
    var esPersonal = sesion.Usuario.Roles.Any(r =>
        r is "Administrador" or "Recepcion" or "Esteticista");

    return RedirigirOJson(esPersonal ? "/citaoverview" : "/");
});

app.MapPost("/auth/register", async (HttpContext ctx, IHttpClientFactory httpClientFactory) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();

    using var client = httpClientFactory.CreateClient("ApiAuth");

    // 1) Registrar
    var registro = await client.PostAsJsonAsync(
        "api/usuarios/registro",
        new { email, password });

    if (!registro.IsSuccessStatusCode)
    {
        var error = registro.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "duplicado",
            System.Net.HttpStatusCode.TooManyRequests => "ratelimit",
            _ => "api"
        };
        return Results.Redirect($"/register?error={error}");
    }

    // 2) Login automatico (el login guarda los tokens en /auth/login)
    var login = await client.PostAsJsonAsync(
        "api/usuarios/login",
        new { email, password });

    if (login.IsSuccessStatusCode)
    {
        return Results.Redirect("/");
    }

    // Registro exitoso pero login fallo: ir a login manual
    return Results.Redirect("/login");
});

app.MapPost("/logout", async (HttpContext ctx, IHttpClientFactory httpClientFactory, TokenStore tokens) =>
{
    // 1) Revocar el refresh token en la API (best-effort, con el access token)
    if (tokens.TieneToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient("ApiAuth");
            using var peticion = new HttpRequestMessage(HttpMethod.Post, "api/usuarios/logout");
            peticion.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", tokens.AccessToken);
            await client.SendAsync(peticion);
        }
        catch
        {
            // La sesion local se cierra aunque la API no este disponible
        }
    }

    // 2) Cerrar la sesion del servidor y borrar el ticket del navegador
    var ticket = ctx.Request.Cookies[SesionPendiente.NombreCookie];
    SesionPendiente.Eliminar(ticket);
    tokens.Limpiar();
    ctx.Response.Cookies.Delete(SesionPendiente.NombreCookie);

    return Results.LocalRedirect("/");
});

app.MapPost("/culture", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var culture = form["culture"].ToString();
    if (culture is not ("en-CA" or "es-ES"))
    {
        return Results.BadRequest();
    }

    ctx.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Secure = ctx.Request.IsHttps
        });

    var returnUrl = form["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//"))
    {
        returnUrl = "/";
    }

    return Results.LocalRedirect(returnUrl);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
