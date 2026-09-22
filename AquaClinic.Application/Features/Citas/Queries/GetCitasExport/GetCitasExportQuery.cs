using MediatR;

namespace AquaClinic.Application.Features.Citas.Queries.GetCitasExport
{
    public class GetCitasExportQuery : IRequest<CitaExportFileVm>
    {
    }
}
