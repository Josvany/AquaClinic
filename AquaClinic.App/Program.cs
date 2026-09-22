using AquaClinic.App;
using AquaClinic.App.Auth;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Blazor Web App con render interactivo Server por pagina
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// AutoMapper (perfiles de mapeo de la App)
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// Necesario para que CookieForwardingHandler lea las cookies del request entrante
builder.Services.AddHttpContextAccessor();

//Agregando radzen
builder.Services.AddRadzenComponents();

// HttpClient hacia la API con CookieForwardingHandler:
// - SSR/prerender: reenvia la cookie del navegador del request entrante
// - Circuito interactivo: la cookie ya fue capturada en el prerender
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7081/";

builder.Services.AddScoped<CookieForwardingHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<CookieForwardingHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<IClient>(sp => new Client(sp.GetRequiredService<HttpClient>()));

// Servicios de datos (consumen la API via REST)
builder.Services.AddScoped<IUsuarioDataService, UsuarioDataService>();
builder.Services.AddScoped<ICitaDataService, CitaDataService>();
builder.Services.AddScoped<IEspecialidadDataService, EspecialidadDataService>();

// Autenticacion via API: consulta /api/usuarios/actual con la cookie del navegador
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthenticationStateProvider>());

// Cliente nombrado para los endpoints de autenticacion minimal API
// (sin CookieForwardingHandler porque las cookies se manejan manualmente)
builder.Services.AddHttpClient("ApiAuth", client => client.BaseAddress = new Uri(apiBaseUrl));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.UseAntiforgery();

// ============================================================
// Endpoints minimal API para el flujo de autenticacion.
// Copian la Set-Cookie de la API directamente al navegador (response)
// y hacen redirect HTTP estandar — sin pasar por el pipeline de Blazor.
// ============================================================

app.MapPost("/auth/login", async (HttpContext ctx, IHttpClientFactory httpClientFactory) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();

    using var client = httpClientFactory.CreateClient("ApiAuth");
    var response = await client.PostAsJsonAsync(
        "api/usuarios/login?useCookies=true",
        new { email, password });

    if (!response.IsSuccessStatusCode)
    {
        var error = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "credenciales",
            System.Net.HttpStatusCode.TooManyRequests => "ratelimit",
            _ => "api"
        };
        return Results.Redirect($"/login?error={error}");
    }

    // Copia la Set-Cookie de la API a la respuesta del navegador
    CopiarCookies(response, ctx);
    return Results.Redirect("/");
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

    // 2) Login automatico: la API emite la cookie
    var login = await client.PostAsJsonAsync(
        "api/usuarios/login?useCookies=true",
        new { email, password });

    if (login.IsSuccessStatusCode)
    {
        CopiarCookies(login, ctx);
        return Results.Redirect("/");
    }

    // Registro exitoso pero login fallo: ir a login manual
    return Results.Redirect("/login");
});

app.MapPost("/logout", async (HttpContext ctx, IHttpClientFactory httpClientFactory) =>
{
    using var client = httpClientFactory.CreateClient("ApiAuth");

    // Reenvia la cookie del navegador para que la API la limpie
    var cookieHeader = ctx.Request.Headers.Cookie.ToString();
    if (!string.IsNullOrEmpty(cookieHeader))
    {
        client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
    }

    var response = await client.PostAsync("logout", null);

    // Copia el Set-Cookie EXPIRADO de la API al navegador para que
    // la cookie de autenticacion se borre realmente (sin esto el
    // navegador conserva la cookie valida y el usuario sigue logueado)
    CopiarCookies(response, ctx);

    return Results.LocalRedirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

/// <summary>
/// Copia los headers Set-Cookie de la respuesta de la API a la respuesta del navegador.
/// Sin esto, la cookie de autenticacion queda en el HttpClient del servidor
/// y nunca llega al navegador.
/// </summary>
static void CopiarCookies(HttpResponseMessage apiResponse, HttpContext browserContext)
{
    if (apiResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
    {
        foreach (var cookie in cookies)
        {
            browserContext.Response.Headers.Append("Set-Cookie", cookie);
        }
    }
}
