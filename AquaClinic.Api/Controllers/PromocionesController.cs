using AquaClinic.Api.Authorization;
using AquaClinic.Application.Features.Promociones.Commands.CreatePromocion;
using AquaClinic.Application.Features.Promociones.Commands.DeletePromocion;
using AquaClinic.Application.Features.Promociones.Commands.UpdatePromocion;
using AquaClinic.Application.Features.Promociones.Queries.GetPromocionesList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AquaClinic.Api.Controllers
{
    /// <summary>
    /// CRUD del catalogo de promociones de la clinica. Solo administra los datos
    /// de la promocion (descuento y vigencia); no affecta el precio de las citas.
    /// Exige permisos de administracion (promociones:ver/crear/editar/eliminar).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PromocionesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PromocionesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet(Name = "GetAllPromociones")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.PromocionesVer)]
        public async Task<ActionResult<List<PromocionListVm>>> GetAllPromociones()
        {
            var result = await _mediator.Send(new GetPromocionesListQuery());
            return Ok(result);
        }

        [HttpPost(Name = "AddPromocion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.PromocionesCrear)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePromocionCommand createPromocionCommand)
        {
            var id = await _mediator.Send(createPromocionCommand);
            return Ok(id);
        }

        [HttpPut(Name = "UpdatePromocion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.PromocionesEditar)]
        public async Task<ActionResult> Update([FromBody] UpdatePromocionCommand updatePromocionCommand)
        {
            await _mediator.Send(updatePromocionCommand);
            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeletePromocion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [HasPermission(Permisos.PromocionesEliminar)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deletePromocionCommand = new DeletePromocionCommand() { PromocionId = id };
            await _mediator.Send(deletePromocionCommand);
            return NoContent();
        }
    }
}