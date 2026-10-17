using AquaClinic.Api.Authorization;
using AquaClinic.Api.Utility;
using AquaClinic.Application.Features.Servicios.Commands.CreateServicio;
using AquaClinic.Application.Features.Servicios.Commands.DeleteServicio;
using AquaClinic.Application.Features.Servicios.Commands.UpdateServicio;
using AquaClinic.Application.Features.Servicios.Queries.GetServicioDetail;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosTodosAdmin;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosExport;
using AquaClinic.Application.Features.Servicios.Queries.GetServiciosList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// Cat&#225;logo de servicios de la clinica. La consulta es p&#250;blica
    /// (el cliente puede ver los servicios sin registrarse); las escrituras
    /// exigen permisos administrativos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ServiciosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiciosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Name = "GetAllServicios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<List<ServicioListVm>>> GetAllServicios()
        {
            var result = await _mediator.Send(new GetServiciosListQuery());
            return Ok(result);
        }

        /// <summary>
        /// Catalogo completo para la administracion, INCLUYENDO los servicios
        /// inactivos (los que no aparecen en la consulta publica). Exige permiso
        /// de ver servicios.
        /// </summary>
        [HttpGet("todos", Name = "GetAllServiciosAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.ServiciosVer)]
        public async Task<ActionResult<List<ServicioListVm>>> GetAllServiciosAdmin()
        {
            var result = await _mediator.Send(new GetServiciosTodosAdminQuery());
            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetServicioById")]
        public async Task<ActionResult<ServicioDetailVm>> GetServicioById(Guid id)
        {
            var getServicioDetailQuery = new GetServicioDetailQuery() { Id = id };
            return Ok(await _mediator.Send(getServicioDetailQuery));
        }

        [HttpPost(Name = "AddServicio")]
        [HasPermission(Permisos.ServiciosCrear)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateServicioCommand createServicioCommand)
        {
            var id = await _mediator.Send(createServicioCommand);
            return Ok(id);
        }

        [HttpPut(Name = "UpdateServicio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.ServiciosEditar)]
        public async Task<ActionResult> Update([FromBody] UpdateServicioCommand updateServicioCommand)
        {
            await _mediator.Send(updateServicioCommand);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteServicio")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.ServiciosEliminar)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleteServicioCommand = new DeleteServicioCommand() { ServicioId = id };
            await _mediator.Send(deleteServicioCommand);
            return NoContent();
        }

        [HttpGet("export", Name = "ExportServicios")]
        [FileResultContentType("text/csv")]
        public async Task<FileResult> ExportServicios()
        {
            var fileDto = await _mediator.Send(new GetServiciosExportQuery());

            return File(fileDto.Data, fileDto.ContentType, fileDto.ServicioExportFileName);
        }
    }
}
