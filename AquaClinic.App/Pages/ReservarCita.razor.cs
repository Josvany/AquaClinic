using Microsoft.AspNetCore.Components;
using Radzen;

namespace AquaClinic.App.Pages
{
    public partial class ReservarCita
    {
        [Inject]
        public DialogService DialogService { get; set; } = default!;
    }
}
