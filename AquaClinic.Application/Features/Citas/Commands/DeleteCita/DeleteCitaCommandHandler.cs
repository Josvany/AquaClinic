using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Commands.DeleteCita
{
    public class DeleteCitaCommandHandler : IRequestHandler<DeleteCitaCommand>
    {
        private readonly IAsyncRepository<Cita> _citaRepository;
        private readonly IMapper _mapper;

        public DeleteCitaCommandHandler(IMapper mapper, IAsyncRepository<Cita> citaRepository)
        {
            _mapper = mapper;
            _citaRepository = citaRepository;
        }

        public async Task Handle(DeleteCitaCommand request, CancellationToken cancellationToken)
        {
            var citaToDelete = await _citaRepository.GetByIdAsync(request.CitaId);

            await _citaRepository.DeleteAsync(citaToDelete);
        }
    }
}
