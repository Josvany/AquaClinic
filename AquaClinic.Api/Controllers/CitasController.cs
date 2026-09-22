using AquaClinic.Api.Authorization;
using AquaClinic.Api.Utility;
using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using AquaClinic.Application.Features.Citas.Queries.GetCitasList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// Agenda de citas de la clinica. La consulta y la exportacion exigen el
    /// permiso citas:ver (Administrador, Recepcion y Esteticista).
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
