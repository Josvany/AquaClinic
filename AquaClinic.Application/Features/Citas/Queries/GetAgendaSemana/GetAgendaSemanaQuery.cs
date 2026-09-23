using AquaClinic.Application.Contracts.Citas;
using AquaClinic.Application.Contracts.Persistence;
using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetAgendaSemana
{
    /// <summary>
    /// Agenda publica de una semana para el scheduler de reservas: slots ocupados
    /// (citas activas) y en proceso de reserva (reservas suaves). No expone datos
    /// de clientes, solo los rangos de tiempo.
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

            var inicioUtc = LocalAUtc(new DateTimeOffset(inicioLocal, CitaFechas.OffsetLocal(inicioLocal)));
            var finUtc = LocalAUtc(new DateTimeOffset(finLocal, CitaFechas.OffsetLocal(finLocal)));

            var ocupaciones = new List<OcupacionVm>();

            foreach (var cita in await _citaRepository.GetCitasWithServiciosAsync())
            {
                if (cita.Estado == Domain.Enums.EstadoCita.Cancelada)
                {
                    continue;
                }

                if (cita.FechaFin > inicioUtc && cita.FechaInicio < finUtc)
                {
                    ocupaciones.Add(new OcupacionVm
                    {
                        FechaInicio = cita.FechaInicio,
                        FechaFin = cita.FechaFin,
                        Estado = nameof(EstadoSlot.Ocupado)
                    });
                }
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

            return new AgendaSemanaVm
            {
                Ocupaciones = ocupaciones
                    .OrderBy(o => o.FechaInicio)
                    .ToList()
            };
        }

        private static DateTimeOffset LocalAUtc(DateTimeOffset local)
        {
            return local.ToUniversalTime();
        }
    }
}
