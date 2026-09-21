using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.DeleteServicio
{
    public class DeleteServicioCommandHandler : IRequestHandler<DeleteServicioCommand>
    {
        private readonly IServicioRepository _servicioRepository;
        private readonly IMapper _mapper;

        public DeleteServicioCommandHandler(IMapper mapper, IServicioRepository servicioRepository)
        {
            _mapper = mapper;
            _servicioRepository = servicioRepository;
        }

        public async Task Handle(DeleteServicioCommand request, CancellationToken cancellationToken)
        {
            var servicioToDelete = await _servicioRepository.GetByIdAsync(request.ServicioId);

            await _servicioRepository.DeleteAsync(servicioToDelete);
        }
    }
}
