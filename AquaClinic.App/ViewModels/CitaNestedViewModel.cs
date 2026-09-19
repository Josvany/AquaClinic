using System;

namespace AquaClinic.App.ViewModels
{
    public class CitaNestedViewModel
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string? Doctor { get; set; }
        public DateTime Fecha { get; set; }
        public Guid EspecialidadId { get; set; }
    }
}
