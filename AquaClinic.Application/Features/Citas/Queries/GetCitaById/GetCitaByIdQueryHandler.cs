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

            var servicio = cita.CitasServicios?.FirstOrDefault()?.Servicio;
            var fechaLocal = CitaFechas.ALocal(cita.FechaInicio);

            return new CitaDetailVm
            {
                CitaId = cita.CitaId,
                Nombre = cita.NombreInvitado ?? cita.EmailInvitado ?? "Cliente registrado",
                // La cita no tiene precio/doctor propios: vienen del servicio vinculado
                Precio = (int)Math.Round(servicio?.Precio ?? 0),
                Doctor = servicio?.Nombre,
                Fecha = fechaLocal,
                Descripcion = cita.Notas,
                ImagenUrl = servicio?.ImagenUrl,
                EspecialidadId = servicio?.EspecialidadId ?? Guid.Empty,
                Especialidad = servicio?.Especialidad == null ? null : new CitaDetalleEspecialidadDto
                {
                    EspecialidadId = servicio.Especialidad.EspecialidadId,
                    Nombre = servicio.Especialidad.Nombre
                }
            };
        }
    }
}
