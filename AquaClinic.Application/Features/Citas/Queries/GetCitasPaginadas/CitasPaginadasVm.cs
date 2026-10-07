namespace AquaClinic.Application.Features.Citas.Queries.GetCitasPaginadas
{
    /// <summary>
    /// Pagina del listado de citas para el DataGrid de la agenda interna.
    /// La ordenacion (fecha desc) se aplica en la consulta.
    /// </summary>
    public class CitasPaginadasVm
    {
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int Total { get; set; }
        public int TotalPaginas => TamanoPagina > 0
            ? (int)Math.Ceiling(Total / (double)TamanoPagina)
            : 0;
        public List<CitasPaginadasItemVm> Items { get; set; } = [];
    }

    public class CitasPaginadasItemVm
    {
        public Guid CitaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        /// <summary>Estado legible (Pendiente/Confirmada/Cancelada/Completada).</summary>
        public string Estado { get; set; } = string.Empty;
    }
}
