using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Personal de la clinica (esteticistas y administrativos). Puede vincularse
    /// de forma opcional a una cuenta de usuario (ApplicationUser) para iniciar
    /// sesion y consultar su agenda.
    /// </summary>
    public class Personal : AuditableEntity
    {
        public Personal()
        {
            PersonalId = Guid.CreateVersion7();
        }

        public Guid PersonalId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>Cuenta de inicio de sesion vinculada (Id en AspNetUsers); &#250;nica si se asigna.</summary>
        public string? ApplicationUserId { get; set; }

        public ICollection<PersonalEspecialidad>? PersonalEspecialidades { get; set; }
    }
}
