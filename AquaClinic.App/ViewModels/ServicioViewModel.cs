namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Servicio del catalogo para el CRUD del personal. Reutiliza la validacion
    /// de la API (nombre unico, precio/duracion positivos, especialidad obligatoria).
    /// </summary>
    public class ServicioViewModel
    {
        public Guid ServicioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// <summary>Precio en CAD.</summary>
        public decimal Precio { get; set; }

        /// <summary>Duracion de la sesion en minutos.</summary>
        public int DuracionMinutos { get; set; }

        /// <summary>URL de imagen opcional para el banner del catalogo.</summary>
        public string? ImagenUrl { get; set; }

        /// <summary>Vigente en el catalogo publico (la portada solo muestra activos).</summary>
        public bool Activo { get; set; } = true;

        public Guid EspecialidadId { get; set; }

        /// <summary>Nombre de la especialidad (solo lectura, para la tabla).</summary>
        public string? EspecialidadNombre { get; set; }
    }
}
