using AquaClinic.Api.Authorization;
using AquaClinic.Application.Features.Bloqueos.Commands.CreateBloqueo;
using AquaClinic.Application.Features.Bloqueos.Commands.DeleteBloqueo;
using AquaClinic.Application.Features.Bloqueos.Commands.UpdateBloqueo;
using AquaClinic.Application.Features.Bloqueos.Queries.GetBloqueosList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// CRUD de bloqueos de toda la clinica (cierres, feriados, capacitaciones):
    /// el rango queda cerrado y no se puede agendar dentro. Exige permisos de
    /// administracion (bloqueos:ver/crear/editar/eliminar).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BloqueosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BloqueosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Name = "GetAllBloqueos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.BloqueosVer)]
        public async Task<ActionResult<List<BloqueoListVm>>> GetAllBloqueos()
        {
            var result = await _mediator.Send(new GetBloqueosListQuery());
            return Ok(result);
        }

        [HttpPost(Name = "AddBloqueo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.BloqueosCrear)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateBloqueoCommand createBloqueoCommand)
        {
            var id = await _mediator.Send(createBloqueoCommand);
            return Ok(id);
        }

        [HttpPut(Name = "UpdateBloqueo")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.BloqueosEditar)]
        public async Task<ActionResult> Update([FromBody] UpdateBloqueoCommand updateBloqueoCommand)
        {
            await _mediator.Send(updateBloqueoCommand);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteBloqueo")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.BloqueosEliminar)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleteBloqueoCommand = new DeleteBloqueoCommand() { BloqueoId = id };
            await _mediator.Send(deleteBloqueoCommand);
            return NoContent();
        }
    }
}
