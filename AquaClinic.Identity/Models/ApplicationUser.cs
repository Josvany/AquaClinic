using Microsoft.AspNetCore.Identity;

namespace AquaClinic.Identity.Models
{
    /// <summary>
    /// Usuario de la aplicacion. Sirve tanto para clientes como para personal
    /// administrativo (diferenciados por roles). El Id se genera con UUID v7.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Id = Guid.CreateVersion7().ToString();
        }

        public string? Nombre { get; set; }

        public string? Apellidos { get; set; }
    }
}
