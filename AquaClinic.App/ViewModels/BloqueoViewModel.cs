namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Bloqueo de toda la clinica para el CRUD administrativo (cierres, feriados,
    /// capacitaciones). Las fechas se manejan en hora local de la clinica
    /// (America/Vancouver); el data service las convierte a/desde UTC como habla
    /// la API.
    /// </summary>
    public class BloqueoViewModel
    {
        public Guid BloqueoId { get; set; }

        public string Motivo { get; set; } = string.Empty;

        /// <summary>Inicio del bloqueo en hora local de la clinica.</summary>
        public DateTime Inicio { get; set; }

        /// <summary>Fin del bloqueo en hora local de la clinica (exclusivo).</summary>
        public DateTime Fin { get; set; }
    }
}
