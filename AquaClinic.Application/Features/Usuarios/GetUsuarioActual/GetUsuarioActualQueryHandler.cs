using AutoMapper;
using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Authentication;
using AquaClinic.Application.Exceptions;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.GetUsuarioActual
{
    public class GetUsuarioActualQueryHandler : IRequestHandler<GetUsuarioActualQuery, UsuarioVm>
    {
        private readonly IIdentityService _identityService;
        private readonly ILoggedInUserService _loggedInUserService;
        private readonly IMapper _mapper;

        public GetUsuarioActualQueryHandler(
            IIdentityService identityService,
            ILoggedInUserService loggedInUserService,
            IMapper mapper)
        {
            _identityService = identityService;
            _loggedInUserService = loggedInUserService;
            _mapper = mapper;
        }

        public async Task<UsuarioVm> Handle(GetUsuarioActualQuery request, CancellationToken cancellationToken)
        {
            var usuarioId = _loggedInUserService.UserId;

            var usuario = await _identityService.ObtenerPorIdAsync(usuarioId);

            if (usuario is null)
                throw new NotFoundException(nameof(Usuario), usuarioId);

            return _mapper.Map<UsuarioVm>(usuario);
        }
    }
}
