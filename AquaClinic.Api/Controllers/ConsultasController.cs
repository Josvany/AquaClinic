using AquaClinic.Application.Features.Consultas.Queries.GetConsultasForMonth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsultasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("/getpagedconsultasformonth", Name = "GetPagedConsultasForMonth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<PagedConsultasForMonthVm>> GetPagedConsultasForMonth(DateTime date, int page, int size)
        {
            var getConsultasForMonthQuery = new GetConsultasForMonthQuery() { Fecha = date, Page = page, Size = size };
            var dtos = await _mediator.Send(getConsultasForMonthQuery);

            return Ok(dtos);
        }
    }
}
