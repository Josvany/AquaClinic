using AquaClinic.Application.Features.Servicios.Queries.GetServiciosList;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Queries.GetServiciosTodosAdmin
{
    /// <summary>
    /// Listado administrativo del catalogo COMPLETO: incluye los servicios
    /// inactivos (Activo = false), que no aparecen en el catalogo publico.
    /// Lo usa la pantalla de Servicios para poder verlos y filtrarlos.
    /// </summary>
    public class GetServiciosTodosAdminQuery : IRequest<List<ServicioListVm>>
    {
    }
}