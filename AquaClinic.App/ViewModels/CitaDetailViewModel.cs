using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.ViewModels
{
    public class CitaDetailViewModel
    {
        public Guid CitaId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage ="El nombre de la cita no debe exceder 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage ="El precio debe ser un valor positivo")]
        public int Precio { get; set; }

        [StringLength(50, ErrorMessage = "El nombre de la cita no debe exceder 50 caracteres")]
        public string? Doctor { get; set; }

        public DateTime Fecha { get; set; }
        
        [StringLength(500, ErrorMessage = "The description can't be longer than 500 characters")] 
        public string? Descripcion { get; set; }

        public string? ImagenUrl { get; set; }

        [Required]
        public Guid EspecialidadId { get; set; }

        public EspecialidadViewModel Especialidad { get; set; } = default!;
    }
}
