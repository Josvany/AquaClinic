using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetAgendaSemana
{
    /// <summary>
    /// Agenda publica de una semana para el scheduler de reservas: slots ocupados
    /// (citas activas), en proceso de reserva (reservas suaves) y bloqueos de la
    /// clinica (cierres no reservables). No expone datos de clientes, solo los
    /// rangos de tiempo.
    /// </summary>
    public class GetAgendaSemanaQuery : IRequest<AgendaSemanaVm>
    {
        /// <summary>Inicio de la semana solicitada, en hora local de la clinica.</summary>
        public DateTime SemanaInicioLocal { get; set; }

        /// <summary>Fin de la semana solicitada (exclusivo), en hora local de la clinica.</summary>
        public DateTime SemanaFinLocal { get; set; }
    }

    public class GetAgendaSemanaQueryHandler : IRequestHandler<GetAgendaSemanaQuery, AgendaSemanaVm>
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IAgendaRepository _agendaRepository;
        private readonly IReservasSuavesStore _reservasSuaves;

        public GetAgendaSemanaQueryHandler(
            ICitaRepository citaRepository,
            IAgendaRepository agendaRepository,
            IReservasSuavesStore reservasSuaves)
        {
            _citaRepository = citaRepository;
            _agendaRepository = agendaRepository;
            _reservasSuaves = reservasSuaves;
        }

        public async Task<AgendaSemanaVm> Handle(GetAgendaSemanaQuery request, CancellationToken cancellationToken)
        {
            var inicioLocal = request.SemanaInicioLocal.Date;
            var finLocal = request.SemanaFinLocal.Date;

            var inicioUtc = CitaFechas.AUtc(inicioLocal);
            var finUtc = CitaFechas.AUtc(finLocal);

            var ocupaciones = new List<OcupacionVm>();

            // Citas activas filtradas directamente en base de datos por rango UTC (no carga toda la tabla en memoria)
            var citasActivas = await _agendaRepository.GetCitasActivasEntreAsync(inicioUtc, finUtc);
            foreach (var cita in citasActivas)
            {
                ocupaciones.Add(new OcupacionVm
                {
                    FechaInicio = cita.FechaInicio,
                    FechaFin = cita.FechaFin,
                    Estado = nameof(EstadoSlot.Ocupado)
                });
            }

            foreach (var reserva in _reservasSuaves.ObtenerVigentes())
            {
                if (reserva.FechaFin > inicioUtc && reserva.FechaInicio < finUtc)
                {
                    ocupaciones.Add(new OcupacionVm
                    {
                        FechaInicio = reserva.FechaInicio,
                        FechaFin = reserva.FechaFin,
                        Estado = nameof(EstadoSlot.Reservando)
                    });
                }
            }

            // Bloqueos de toda la clinica (PersonalId = null): se muestran como
            // rangos cerrados para que el cliente vea el dia/hora no disponible
            // y no descubra el cierre recien al confirmar la cita.
            foreach (var bloqueo in await _agendaRepository.GetBloqueosClinicaEntreAsync(inicioUtc, finUtc))
            {
                ocupaciones.Add(new OcupacionVm
                {
                    FechaInicio = bloqueo.FechaInicio,
                    FechaFin = bloqueo.FechaFin,
                    Estado = nameof(EstadoSlot.Bloqueado)
                });
            }

            return new AgendaSemanaVm
            {
                Ocupaciones = ocupaciones
                    .OrderBy(o => o.FechaInicio)
                    .ToList()
            };
        }
    }
}
