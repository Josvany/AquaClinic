using AutoMapper;
using AquaClinic.Application.Contracts;
using AquaClinic.Application.Contracts.Authentication;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.GetUsuarioActual
{
    /// <summary>
    /// Obtiene el perfil del usuario autenticado actual. Retorna null (no lanza
    /// NotFoundException) si el usuario del cookie no existe en la BD — esto ocurre
    /// cuando la API reinicia y recrea la BD con usuarios nuevos (UUIDs distintos),
    /// pero el navegador aun tiene un cookie con el UUID anterior. El controller
    /// responde 401 para que el cliente re-autentique.
    /// </summary>
    public class GetUsuarioActualQueryHandler : IRequestHandler<GetUsuarioActualQuery, UsuarioVm?>
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

        public async Task<UsuarioVm?> Handle(GetUsuarioActualQuery request, CancellationToken cancellationToken)
        {
            var usuarioId = _loggedInUserService.UserId;

            if (string.IsNullOrEmpty(usuarioId))
            {
                return null;
            }

            var usuario = await _identityService.ObtenerPorIdAsync(usuarioId);

            return usuario is null ? null : _mapper.Map<UsuarioVm>(usuario);
        }
    }
}
