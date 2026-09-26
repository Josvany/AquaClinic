namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Pagina del listado de citas consumida por el DataGrid server-side
    /// de la agenda interna.
    /// </summary>
    public class CitasPaginadasViewModel
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int Total { get; set; }
        public int TotalPaginas { get; set; }
        public List<CitaListViewModel> Items { get; set; } = [];
    }
}
