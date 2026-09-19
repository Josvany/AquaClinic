using System.Net.Http.Json;
using AquaClinic.App.Auth;
using AquaClinic.App.ViewModels;
using AquaClinic.Identity.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;

namespace AquaClinic.App.Pages
{
    /// <summary>
    /// P&#225;gina con renderizado est&#225;tico (SSR): el env&#237;o del formulario produce una
    /// petici&#243;n HTTP completa, lo que permite que la cookie de autenticaci&#243;n local
    /// quede establecida en el navegador y que se capture la cookie de la API.
    /// </summary>
    public partial class Login
    {
        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;

        [Inject]
        public SignInManager<ApplicationUser> SignInManager { get; set; } = default!;

        [Inject]
        public IdentityRedirectManager RedirectManager { get; set; } = default!;

        [Inject]
        public UserCookieStore CookieStore { get; set; } = default!;

        [Inject]
        public IHttpClientFactory HttpClientFactory { get; set; } = default!;

        [SupplyParameterFromForm]
        private LoginViewModel? FormInput { get; set; }

        private LoginViewModel Input { get; set; } = new();

        private string? Message { get; set; }

        protected override void OnInitialized() => Input = FormInput ?? new LoginViewModel();

        private async Task HandleLogin()
        {
            var user = await UserManager.FindByEmailAsync(Input.Email);
            if (user is null || !await UserManager.CheckPasswordAsync(user, Input.Password))
            {
                Message = "Correo y/o contrase&#241;a incorrectos.";
                return;
            }

            // Cookie local (navegador) para las p&#225;ginas de la App
            await SignInManager.SignInAsync(user, isPersistent: false);

            // Cookie de la API para los endpoints protegidos (guardada en servidor)
            await GuardarCookieDeLaApiAsync(user, Input.Password);

            RedirectManager.RedirectTo("/");
        }

        private async Task GuardarCookieDeLaApiAsync(ApplicationUser user, string password)
        {
            try
            {
                var client = HttpClientFactory.CreateClient("Authentication");
                var response = await client.PostAsJsonAsync("login?useCookies=true", new { email = user.Email, password });
                if (response.IsSuccessStatusCode)
                {
                    var identityCookie = response.Headers.GetValues("Set-Cookie")
                        .FirstOrDefault(c => c.StartsWith(".AspNetCore.Identity.Application="));

                    if (identityCookie is not null)
                    {
                        CookieStore.Set(user.Id, identityCookie);
                    }
                }
            }
            catch
            {
                // La App funciona localmente aunque la API no est&#233; disponible;
                // los endpoints protegidos fallar&#225;n hasta el pr&#243;ximo inicio de sesi&#243;n.
            }
        }
    }
}
