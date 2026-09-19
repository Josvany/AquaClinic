namespace AquaClinic.Application.Features.Especialidades.Commands.CreateEspecialidad
{
    public class CreateEspecialidadDto
    {
        public Guid EspecialidadId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
