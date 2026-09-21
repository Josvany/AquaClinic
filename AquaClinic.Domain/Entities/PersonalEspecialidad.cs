namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Relacion N:N entre personal y las especialidades que atiende.
    /// </summary>
    public class PersonalEspecialidad
    {
        public Guid PersonalId { get; set; }
        public Personal Personal { get; set; } = default!;

        public Guid EspecialidadId { get; set; }
        public Especialidad Especialidad { get; set; } = default!;
    }
}
