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
    /// petici&#243;n HTTP completa, lo que permite establecer la cookie local y capturar
    /// la cookie de la API tras crear el usuario.
    /// </summary>
    public partial class Register
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
        private RegisterViewModel? FormInput { get; set; }

        private RegisterViewModel Input { get; set; } = new();

        private string? Message { get; set; }

        protected override void OnInitialized() => Input = FormInput ?? new RegisterViewModel();

        private async Task HandleRegister()
        {
            var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                Message = string.Join(" ", result.Errors.Select(e => e.Description));
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
                // La App funciona localmente aunque la API no est&#233; disponible.
            }
        }
    }
}
