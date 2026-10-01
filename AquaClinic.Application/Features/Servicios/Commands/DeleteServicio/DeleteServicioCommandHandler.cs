using AutoMapper;
using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Servicios.Commands.DeleteServicio
{
    /// <summary>
    /// "Eliminar" un servicio es un borrado LOGICO: queda Activo = false y
    /// desaparece del catalogo publico y de la agenda, pero la fila se conserva
    /// con su historial de citas (las citas pasadas siguen teniendo su servicio).
    /// </summary>
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
            var servicioToDelete = await _servicioRepository.GetByIdAsync(request.ServicioId)
                ?? throw new NotFoundException(nameof(Servicio), request.ServicioId);

            servicioToDelete.Activo = false;

            await _servicioRepository.UpdateAsync(servicioToDelete);
        }
    }
}
