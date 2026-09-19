using AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesList;
using AquaClinic.Application.Features.Especialidades.Queries.GetEspecialidadesListWithCitas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<ActionResult<List<EspecialidadListVm>>> GetAllEspecialidades()
        {
            var dtos = await _mediator.Send(new GetEspecialidadesListQuery());
            return Ok(dtos);
        }

        [HttpGet("allwithcitas", Name = "GetEspecialidadesWithCitas")]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public async Task<ActionResult<List<EspecialidadCitaListVm>>> GetEspecialidadesWithCitas(bool incluirHistorial)
        {
            GetEspecialidadesListWithCitasQuery getEspecialidadesListWithCitasQuery = new GetEspecialidadesListWithCitasQuery() { IncluirHistorial = incluirHistorial };

            var dtos = await _mediator.Send(getEspecialidadesListWithCitasQuery);
            return Ok(dtos);
        }

        [HttpPost(Name = "AddEspecialidad")]
        public async Task<ActionResult<CreateEspecialidadCommandResponse>> Create([FromBody] CreateEspecialidadCommand createEspecialidadCommand)
        {
            var response = await _mediator.Send(createEspecialidadCommand);
            return Ok(response);
        }
    }
}
