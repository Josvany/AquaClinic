using AquaClinic.App.Localization;
using System.ComponentModel.DataAnnotations;

namespace AquaClinic.App.ViewModels
{
    public class CitaDetailViewModel
    {
        public Guid CitaId { get; set; }

        [RequiredUi("Este campo es obligatorio")]
        [StringLengthUi(50, "El nombre de la cita no debe exceder 50 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [RequiredUi("Este campo es obligatorio")]
        [RangeUi(0, int.MaxValue, "El precio debe ser un valor positivo")]
        public int Precio { get; set; }

        [StringLengthUi(50, "El nombre de la cita no debe exceder 50 caracteres")]
        public string? Doctor { get; set; }

        public DateTime Fecha { get; set; }
        
        [StringLengthUi(500, "La descripción no puede superar los 500 caracteres")]
        public string? Descripcion { get; set; }

        public string? ImagenUrl { get; set; }

        [RequiredUi("Este campo es obligatorio")]
        public Guid EspecialidadId { get; set; }

        public EspecialidadViewModel Especialidad { get; set; } = default!;
    }
}
