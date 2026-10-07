using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasPaginadas
{
    /// <summary>
    /// Pagina del listado de citas (fecha descendente) para el DataGrid
    /// server-side de la agenda interna, con filtros opcionales de
    /// texto (cliente) y estado.
    /// </summary>
    public class GetCitasPaginadasQuery : IRequest<CitasPaginadasVm>
    {
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;

        /// <summary>Texto libre: nombre/correo del invitado o nombre/apellidos/correo del cliente registrado.</summary>
        public string? Texto { get; set; }

        /// <summary>Nombre del estado a filtrar (Pendiente/Confirmada/Cancelada/Completada); null = todas.</summary>
        public string? Estado { get; set; }

        /// <summary>Dia local inicial del filtro por fecha (inclusive); null = sin limite.</summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>Dia local final del filtro por fecha (inclusive, dia completo); null = sin limite.</summary>
        public DateTime? FechaHasta { get; set; }
    }
}
