namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Representacion de dominio del usuario de la aplicacion (cliente o
    /// personal administrativo). La tabla fisica es AspNetUsers, creada por
    /// IdentityDbContext<ApplicationUser>; esta entidad NO esta mapeada por EF:
    /// se materializa desde UserManager en la capa de Identity y viaja por las
    /// queries/commands de Application.
    /// </summary>
    public class Usuario
    {
        /// <summary>Id en AspNetUsers (UUID v7 en string).</summary>
        public string UsuarioId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Nombre { get; set; }

        public string? Apellidos { get; set; }

        public string? Telefono { get; set; }

        public IReadOnlyCollection<string> Roles { get; set; } = [];

        public string NombreCompleto
        {
            get
            {
                var completo = $"{Nombre} {Apellidos}".Trim();
                return string.IsNullOrWhiteSpace(completo) ? Email : completo;
            }
        }
    }
}