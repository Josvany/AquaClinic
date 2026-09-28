using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetMisCitas
{
    /// <summary>
    /// Citas del usuario autenticado (rol Cliente): su historial completo de
    /// citas agendadas, ordenadas de la mas reciente a la mas antigua.
    /// </summary>
    public class GetMisCitasQuery : IRequest<List<MiCitaVm>>
    {
    }
}
