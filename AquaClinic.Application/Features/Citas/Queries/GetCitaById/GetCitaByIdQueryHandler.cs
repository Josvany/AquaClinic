using AquaClinic.Application.Contracts.Persistence;
using AquaClinic.Domain.Entities;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitaById
{
    public class GetCitaByIdQueryHandler : IRequestHandler<GetCitaByIdQuery, CitaDetailVm>
    {
        private readonly ICitaRepository _citaRepository;

        public GetCitaByIdQueryHandler(ICitaRepository citaRepository)
        {
            _citaRepository = citaRepository;
        }

        public async Task<CitaDetailVm> Handle(GetCitaByIdQuery request, CancellationToken cancellationToken)
        {
            var cita = await _citaRepository.GetByIdWithServiciosAsync(request.Id)
                ?? throw new AquaClinic.Application.Exceptions.NotFoundException(nameof(Cita), request.Id);

            var (clienteRegistrado, emailRegistrado, personal) =
                await _citaRepository.ObtenerContactoAsync(cita.UsuarioId, cita.PersonalId);

            var servicios = (cita.CitasServicios ?? [])
                .Where(cs => cs.Servicio is not null)
                .Select(cs => new CitaDetalleServicioDto
                {
                    ServicioId = cs.Servicio!.ServicioId,
                    Nombre = cs.Servicio.Nombre,
                    DuracionMinutos = cs.Servicio.DuracionMinutos,
                    Precio = cs.Servicio.Precio,
                    EspecialidadNombre = cs.Servicio.Especialidad?.Nombre
                })
                .ToList();

            // Cliente visible: el usuario registrado; si no, el invitado
            // (nombre o, de ultimo, su correo).
            var cliente = clienteRegistrado
                ?? (!string.IsNullOrWhiteSpace(cita.NombreInvitado)
                    ? cita.NombreInvitado
                    : (!string.IsNullOrWhiteSpace(cita.EmailInvitado) ? cita.EmailInvitado : "Cliente"));

            return new CitaDetailVm
            {
                CitaId = cita.CitaId,
                EsRegistrado = !string.IsNullOrEmpty(cita.UsuarioId),
                Cliente = cliente,
                EmailContacto = emailRegistrado ?? cita.EmailInvitado,
                TelefonoContacto = cita.TelefonoInvitado,
                PersonalNombre = personal,
                FechaInicioLocal = CitaFechas.ALocal(cita.FechaInicio),
                FechaFinLocal = CitaFechas.ALocal(cita.FechaFin),
                Estado = cita.Estado.ToString(),
                EstadoValor = (int)cita.Estado,
                Notas = cita.Notas,
                Servicios = servicios
            };
        }
    }
}
