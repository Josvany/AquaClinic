using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasPaginadas
{
    /// <summary>
    /// Pagina del listado de citas (fecha descendente) para el DataGrid
    /// server-side de la agenda interna.
    /// </summary>
    public class GetCitasPaginadasQuery : IRequest<CitasPaginadasVm>
    {
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
    }
}
