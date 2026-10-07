using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CancelarCita
{
    /// <summary>
    /// Cancela la cita cambiando su estado a Cancelada. Solo se pueden
    /// cancelar citas pendientes o confirmadas: una cita cancelada no se
    /// cancela dos veces y una completada ya se dio por atendida.
    /// </summary>
    public class CancelarCitaCommandHandler : IRequestHandler<CancelarCitaCommand>
    {
        private readonly ICitaRepository _citaRepository;

        public CancelarCitaCommandHandler(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;
        }

        public async Task Handle(CancelarCitaCommand request, CancellationToken cancellationToken)
        {
            var cita = await _citaRepository.GetByIdWithServiciosAsync(request.CitaId)
                ?? throw new NotFoundException(nameof(Cita), request.CitaId);

            if (cita.Estado == EstadoCita.Cancelada)
            {
                throw new BadRequestException("La cita ya esta cancelada.");
            }

            if (cita.Estado == EstadoCita.Completada)
            {
                throw new BadRequestException("La cita ya fue completada y no se puede cancelar.");
            }

            cita.Estado = EstadoCita.Cancelada;

            await _citaRepository.UpdateAsync(cita);
        }
    }
}
