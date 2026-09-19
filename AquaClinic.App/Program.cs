using System.Security.Claims;
using AquaClinic.App;
using AquaClinic.App.Auth;
using AquaClinic.App.Contracts;
using AquaClinic.App.Services;
using AquaClinic.Identity;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Blazor Web App con render interactivo Server por pagina
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// AutoMapper (perfiles de mapeo de la App)
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// HttpClient hacia la API: uno por circuito (scoped), con reenvio de la cookie
// de la API para los endpoints protegidos con [Authorize]
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7081/";

builder.Services.AddScoped<ApiCookieHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<ApiCookieHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<IClient>(sp => new Client(sp.GetRequiredService<HttpClient>()));

// Cliente nombrado para el flujo de login/logout contra la API (sin estado de usuario)
builder.Services.AddHttpClient("Authentication", client => client.BaseAddress = new Uri(apiBaseUrl));

// Servicios de datos (consumen la API via REST)
builder.Services.AddScoped<ICitaDataService, CitaDataService>();
builder.Services.AddScoped<IEspecialidadDataService, EspecialidadDataService>();
builder.Services.AddScoped<IConsultaDataService, ConsultaDataService>();

// Identidad local: la App autentica contra la misma base de datos de identidad
// que usa la API (AquaClinicIdentityDb)
builder.Services.AddDbContext<AquaClinicIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AquaClinicIdentityConnectionString")));

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorizationBuilder();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AquaClinicIdentityDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Proveedor de estado de autenticacion para circuitos interactivos (revalida cada 30 min)
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<IdentityRedirectManager>();

// Almacen de la cookie de la API por usuario (para reenviarla desde el servidor)
builder.Services.AddSingleton<UserCookieStore>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/logout", async (SignInManager<ApplicationUser> signInManager, UserCookieStore cookieStore, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId is not null)
    {
        cookieStore.Remove(userId);
    }

    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
