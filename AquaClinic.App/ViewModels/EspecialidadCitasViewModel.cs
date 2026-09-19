namespace AquaClinic.App.ViewModels
{
    public class EspecialidadCitasViewModel
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<CitaNestedViewModel>? Citas { get; set; }
    }
}
