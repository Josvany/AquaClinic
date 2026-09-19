using Microsoft.AspNetCore.Components;

namespace AquaClinic.App.Pages
{
    public partial class Index
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        protected void NavigateToLogin()
        {
            NavigationManager.NavigateTo("login");
        }

        protected void NavigateToRegister()
        {
            NavigationManager.NavigateTo("register");
        }
    }
}
