using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Especialidades.Commands.DeleteEspecialidad
{
    public class DeleteEspecialidadCommandHandler : IRequestHandler<DeleteEspecialidadCommand>
    {
        private readonly IEspecialidadRepository _especialidadRepository;

        public DeleteEspecialidadCommandHandler(IEspecialidadRepository especialidadRepository)
        {
            _especialidadRepository = especialidadRepository;
        }

        public async Task Handle(DeleteEspecialidadCommand request, CancellationToken cancellationToken)
        {
            var especialidad = await _especialidadRepository.GetByIdAsync(request.EspecialidadId)
                ?? throw new NotFoundException(nameof(Especialidad), request.EspecialidadId);

            // Una especialidad con servicios no se puede borrar: dejaria servicios
            // huerfanos y sin especialidad en el catalogo. Hay que moverlos antes.
            if (await _especialidadRepository.TieneServiciosAsync(especialidad.EspecialidadId))
            {
                throw new BadRequestException(
                    "No se puede eliminar: la especialidad tiene servicios asignados. Reasígnalos primero.");
            }

            await _especialidadRepository.DeleteAsync(especialidad);
        }
    }
}