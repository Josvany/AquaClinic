using AquaClinic.Api.Utility;
using AquaClinic.Application.Features.Citas.Commands.CreateCita;
using AquaClinic.Application.Features.Citas.Commands.DeleteCita;
using AquaClinic.Application.Features.Citas.Commands.UpdateCita;
using AquaClinic.Application.Features.Citas.Queries.GetCitaDetail;
using AquaClinic.Application.Features.Citas.Queries.GetCitasExport;
using AquaClinic.Application.Features.Citas.Queries.GetCitasList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : Controller
    {
        private readonly IMediator _mediator;

        public CitasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Name = "GetAllCitas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<List<CitaListVm>>> GetAllCitas()
        {
            var result = await _mediator.Send(new GetCitasListQuery());
            return Ok(result);
        }

        [HttpGet("{id}", Name = "GetCitaById")]
        public async Task<ActionResult<CitaDetailVm>> GetCitaById(Guid id)
        {
            var getCitaDetailQuery = new GetCitaDetailQuery() { Id = id };
            return Ok(await _mediator.Send(getCitaDetailQuery));
        }

        [HttpPost(Name = "AddCita")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCitaCommand createCitaCommand)
        {
            var id = await _mediator.Send(createCitaCommand);
            return Ok(id);
        }

        [HttpPut(Name = "UpdateCita")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Update([FromBody] UpdateCitaCommand updateCitaCommand)
        {
            await _mediator.Send(updateCitaCommand);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteCita")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deleteCitaCommand = new DeleteCitaCommand() { CitaId = id };
            await _mediator.Send(deleteCitaCommand);
            return NoContent();
        }

        [HttpGet("export", Name = "ExportCitas")]
        [FileResultContentType("text/csv")]
        public async Task<FileResult> ExportCitas()
        {
            var fileDto = await _mediator.Send(new GetCitasExportQuery());

            return File(fileDto.Data, fileDto.ContentType, fileDto.CitaExportFileName);
        }
    }
}
