using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Application.Features.Citas.Commands.CancelarCita;
using AquaClinic.Domain.Entities;
using AquaClinic.Domain.Enums;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.CancelarMisCita
{
    /// <summary>
    /// Cancelacion de cita por parte del cliente. Verifica que la cita
    /// pertenezca al usuario autenticado y que no haya pasado su fecha de
    /// atencion, y delega en CancelarCitaCommandHandler.
    /// </summary>
    public class CancelarMisCitaCommandHandler : IRequestHandler<CancelarMisCitaCommand>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly ISender _sender;

        public CancelarMisCitaCommandHandler(
            ICitaRepository citaRepository,
            ILoggedInUserService loggedInUserService,
            ISender sender)
        {
            _citaRepository = citaRepository;
            _loggedInUserService = loggedInUserService;
            _sender = sender;
        }

        public async Task Handle(CancelarMisCitaCommand request, CancellationToken cancellationToken)
        {
            var usuarioId = _loggedInUserService.UserId;

            if (string.IsNullOrEmpty(usuarioId))
            {
                throw new BadRequestException("No se pudo identificar al usuario.");
            }

            var cita = await _citaRepository.GetByIdWithServiciosAsync(request.CitaId)
                ?? throw new NotFoundException(nameof(Cita), request.CitaId);

            // 404 (no 403) para no revelar que la cita de otra persona existe.
            if (!string.Equals(cita.UsuarioId, usuarioId, StringComparison.Ordinal))
            {
                throw new NotFoundException(nameof(Cita), request.CitaId);
            }

            // Cita ya atendida: no tiene sentido cancelarla.
            if (CitaFechas.ALocal(cita.FechaInicio) <= CitaFechas.AhoraLocal())
            {
                throw new BadRequestException("No puedes cancelar una cita que ya paso.");
            }

            await _sender.Send(new CancelarCitaCommand { CitaId = request.CitaId }, cancellationToken);
        }
    }
}