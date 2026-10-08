using AquaClinic.Api.Authorization;
using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Features.Especialidades.Commands.DeleteEspecialidad;
using AquaClinic.Application.Features.Especialidades.Commands.UpdateEspecialidad;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithServicios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspecialidadesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EspecialidadesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("all", Name = "GetAllEspecialidades")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HasPermission(Permisos.EspecialidadesVer)]
        public async Task<ActionResult<List<EspecialidadListVm>>> GetAllEspecialidades()
        {
            var dtos = await _mediator.Send(new GetEspecialidadesListQuery());
            return Ok(dtos);
        }

        [HttpGet("allwithservicios", Name = "GetEspecialidadesWithServicios")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HasPermission(Permisos.EspecialidadesVer)]
        public async Task<ActionResult<List<EspecialidadServicioListVm>>> GetEspecialidadesWithServicios()
        {
            GetEspecialidadesListWithServiciosQuery getEspecialidadesListWithServiciosQuery = new GetEspecialidadesListWithServiciosQuery();

            var dtos = await _mediator.Send(getEspecialidadesListWithServiciosQuery);
            return Ok(dtos);
        }

        [HttpPost(Name = "AddEspecialidad")]
        [HasPermission(Permisos.EspecialidadesCrear)]
        public async Task<ActionResult<CreateEspecialidadCommandResponse>> Create([FromBody] CreateEspecialidadCommand createEspecialidadCommand)
        {
            var response = await _mediator.Send(createEspecialidadCommand);
            return Ok(response);
        }

        [HttpPut(Name = "UpdateEspecialidad")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HasPermission(Permisos.EspecialidadesEditar)]
        public async Task<ActionResult<UpdateEspecialidadCommandResponse>> Update(
            [FromBody] UpdateEspecialidadCommand updateEspecialidadCommand)
        {
            var response = await _mediator.Send(updateEspecialidadCommand);
            return Ok(response);
        }

        /// <summary>
        /// Elimina una especialidad. 400 si tiene servicios asignados: habria que
        /// reasignarlos antes para no dejar servicios huerfanos.
        /// </summary>
        [HttpDelete("{id}", Name = "DeleteEspecialidad")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HasPermission(Permisos.EspecialidadesEliminar)]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteEspecialidadCommand { EspecialidadId = id });
            return NoContent();
        }
    }
}
