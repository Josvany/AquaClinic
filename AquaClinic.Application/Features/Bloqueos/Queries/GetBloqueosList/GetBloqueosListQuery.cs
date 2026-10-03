using MediatR;

namespace AquaClinic.Application.Features.Bloqueos.Queries.GetBloqueosList
{
    /// <summary>
    /// Bloqueos de toda la clinica (PersonalId = null) ordenados por fecha de inicio:
    /// son los que cierran la agenda en /reservar y /citaoverview y los que administra
    /// el CRUD.
    /// </summary>
    public class GetBloqueosListQuery : IRequest<List<BloqueoListVm>>
    {
    }
}
