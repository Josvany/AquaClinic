using AquaClinic.Domain.Common;

namespace AquaClinic.Domain.Entities
{
    /// <summary>
    /// Servicio que ofrece la clinica, con su duracion en minutos y precio en CAD.
    /// </summary>
    public class Servicio : AuditableEntity
    {
        public Servicio()
        {
            ServicioId = Guid.CreateVersion7();
        }

        public Guid ServicioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// <summary>Precio en dolares canadienses (CAD).</summary>
        public decimal Precio { get; set; }

        /// <summary>Duracion del servicio en minutos.</summary>
        public int DuracionMinutos { get; set; }

        public string? ImagenUrl { get; set; }

        /// <summary>Visibilidad del servicio en el catalogo publico.</summary>
        public bool Activo { get; set; } = true;

        public Guid EspecialidadId { get; set; }

        public Especialidad Especialidad { get; set; } = default!;
    }
}
