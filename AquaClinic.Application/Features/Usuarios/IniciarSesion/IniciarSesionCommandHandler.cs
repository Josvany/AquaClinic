using AutoMapper;
using AquaClinic.Application.Contracts.Authentication;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.IniciarSesion
{
    public class IniciarSesionCommandHandler : IRequestHandler<IniciarSesionCommand, UsuarioVm?>
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public IniciarSesionCommandHandler(IIdentityService identityService, IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        public async Task<UsuarioVm?> Handle(IniciarSesionCommand request, CancellationToken cancellationToken)
        {
            var usuario = await _identityService.ValidarCredencialesAsync(request.Email, request.Password);

            return usuario is null ? null : _mapper.Map<UsuarioVm>(usuario);
        }
    }
}
