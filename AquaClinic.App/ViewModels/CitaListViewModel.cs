namespace AquaClinic.App.ViewModels
{
    public class CitaListViewModel
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        /// <summary>Estado legible (Pendiente/Confirmada/Cancelada/Completada).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
