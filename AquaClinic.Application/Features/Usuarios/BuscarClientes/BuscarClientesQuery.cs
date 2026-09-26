using AquaClinic.Application.Contracts.Authentication;
using MediatR;

namespace AquaClinic.Application.Features.Usuarios.BuscarClientes
{
    /// <summary>
    /// Busca clientes registrados (rol Cliente) para agendarles una cita desde el
    /// back office. Sin texto devuelve los primeros clientes ordenados por nombre.
    /// </summary>
    public class BuscarClientesQuery : IRequest<List<ClienteBusquedaVm>>
    {
        /// <summary>Nombre, apellidos o correo a buscar.</summary>
        public string? Texto { get; set; }

        /// <summary>Maximo de resultados a devolver.</summary>
        public int Limite { get; set; } = 20;
    }

    /// <summary>Cliente registrado que se puede seleccionar al crear una cita.</summary>
    public class ClienteBusquedaVm
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
    }

    public class BuscarClientesQueryHandler : IRequestHandler<BuscarClientesQuery, List<ClienteBusquedaVm>>
    {
        private readonly IIdentityService _identityService;

        public BuscarClientesQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<List<ClienteBusquedaVm>> Handle(BuscarClientesQuery request, CancellationToken cancellationToken)
        {
            var limite = request.Limite <= 0 ? 20 : request.Limite;
            var clientes = await _identityService.BuscarClientesAsync(request.Texto, limite);

            return clientes
                .Select(c => new ClienteBusquedaVm
                {
                    UsuarioId = c.UsuarioId,
                    Nombre = c.NombreCompleto,
                    Email = c.Email,
                    Telefono = c.Telefono
                })
                .ToList();
        }
    }
}
