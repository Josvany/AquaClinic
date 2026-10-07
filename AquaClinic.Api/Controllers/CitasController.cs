using AquaClinic.Api.Authorization;
using AquaClinic.Api.Utility;
using AquaClinic.Identity;
using AquaClinic.Application.Features.Citas.Commands.CancelarCita;
using AquaClinic.Application.Features.Citas.Commands.CreateCita;
using AquaClinic.Application.Features.Citas.Commands.MoverCita;
using AquaClinic.Application.Features.Citas.Queries.GetAgendaSemana;
using AquaClinic.Application.Features.Citas.Queries.GetCitaById;
using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using AquaClinic.Application.Features.Citas.Queries.GetCitasList;
using AquaClinic.Application.Features.Citas.Queries.GetCitasPaginadas;
using AquaClinic.Application.Features.Citas.Queries.GetMisCitas;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// Agenda de citas de la clinica. La consulta de agenda semanal y la creacion
    /// de citas son publicas (el visitante reserva desde /reservar); el listado
    /// interno y la exportacion exigen el permiso citas:ver.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CitasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Agenda publica de una semana para el scheduler de reservas: bloques
        /// "Ocupado" (citas activas) y "Reservando" (reserva suave en curso).
        /// No expone datos de clientes.
        /// </summary>
        [HttpGet("agenda", Name = "GetAgendaSemana")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<AgendaSemanaVm>> GetAgendaSemana(
            [FromQuery] DateTime semanaInicio,
            [FromQuery] DateTime semanaFin)
        {
            var result = await _mediator.Send(new GetAgendaSemanaQuery
            {
                SemanaInicioLocal = semanaInicio,
                SemanaFinLocal = semanaFin
            });

            return Ok(result);
        }

        /// <summary>
        /// Crea una cita desde el scheduler publico. Devuelve 409 si el horario
        /// no esta disponible (horario, bloqueo u otra cita); el handler notifica
        /// en tiempo real via SignalR cuando la cita se confirma.
        /// </summary>
        [HttpPost(Name = "CreateCita")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<Guid>> CreateCita([FromBody] CreateCitaCommand command)
        {
            // Control contra manipulacion de parametros: el identity del token manda
            // sobre el UsuarioId del body. Un cliente autenticado solo puede crear
            // citas para SI mismo (se ignora el UsuarioId recibido) y un visitante
            // anonimo queda siempre como invitado. El personal (flujo del back office)
            // si puede indicar a que cliente se le agenda.
            var autenticado = User.Identity?.IsAuthenticated == true;
            var esPersonal = autenticado && User.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                c.Value is RolesClinica.Administrador or RolesClinica.Recepcion or RolesClinica.Esteticista);

            command.UsuarioId = esPersonal
                ? command.UsuarioId
                : autenticado ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            try
            {
                var citaId = await _mediator.Send(command);
                return Ok(citaId);
            }
            catch (Application.Exceptions.BadRequestException ex)
            {
                // Horario tomado o clinica cerrada: conflicto para el scheduler
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Mueve una cita pendiente o confirmada a otro horario.
        /// Valida disponibilidad (horario de la clinica, bloqueos y
        /// solapamiento con otras citas); 409 si el nuevo horario ya
        /// fue tomado. El identity del token manda: solo personal con
        /// permiso citas:gestionar puede mover citas.
        /// </summary>
        [HttpPut("{id}/mover", Name = "MoverCita")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasGestionar)]
        public async Task<ActionResult<Guid>> MoverCita(Guid id, [FromBody] MoverCitaCommand command)
        {
            command.CitaId = id;

            try
            {
                var citaId = await _mediator.Send(command);
                return Ok(citaId);
            }
            catch (Application.Exceptions.BadRequestException ex)
            {
                // Horario tomado o clinica cerrada: conflicto
                return Conflict(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancela una cita pendiente o confirmada (el horario vuelve
        /// a quedar disponible). Exige permiso citas:gestionar.
        /// </summary>
        [HttpPost("{id}/cancelar", Name = "CancelarCita")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasGestionar)]
        public async Task<ActionResult> CancelarCita(Guid id)
        {
            await _mediator.Send(new CancelarCitaCommand { CitaId = id });
            return Ok();
        }

        /// <summary>
        /// Citas del usuario autenticado (rol Cliente) para su pantalla
        /// "Mis citas": su historial completo, sin exponer datos de otros.
        /// </summary>
        [HttpGet("mias", Name = "GetMisCitas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.MisCitasVer)]
        public async Task<ActionResult<List<MiCitaVm>>> GetMisCitas()
        {
            var result = await _mediator.Send(new GetMisCitasQuery());
            return Ok(result);
        }

        /// <summary>
        /// Pagina del listado de citas (fecha descendente) para el DataGrid
        /// server-side de la agenda interna.
        /// </summary>
        [HttpGet("paginadas", Name = "GetCitasPaginadas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasVer)]
        public async Task<ActionResult<CitasPaginadasVm>> GetCitasPaginadas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanoPagina = 10)
        {
            var result = await _mediator.Send(new GetCitasPaginadasQuery
            {
                Pagina = pagina,
                TamanoPagina = tamanoPagina
            });
            return Ok(result);
        }

        /// <summary>
        /// Detalle de una cita para la pantalla /citadetails de la agenda interna.
        /// </summary>
        [HttpGet("{id}", Name = "GetCitaById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasVer)]
        public async Task<ActionResult<CitaDetailVm>> GetCitaById(Guid id)
        {
            var result = await _mediator.Send(new GetCitaByIdQuery { Id = id });
            return Ok(result);
        }

        [HttpGet(Name = "GetAllCitas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.CitasVer)]
        public async Task<ActionResult<List<CitaListVm>>> GetAllCitas()
        {
            var result = await _mediator.Send(new GetCitasListQuery());
            return Ok(result);
        }

        [HttpGet("export", Name = "ExportCitas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [FileResultContentType("text/csv")]
        [HasPermission(Permisos.CitasVer)]
        public async Task<FileResult> ExportCitas()
        {
            var fileDto = await _mediator.Send(new GetCitasExportQuery());

            return File(fileDto.Data, fileDto.ContentType, fileDto.CitaExportFileName);
        }
    }
}
