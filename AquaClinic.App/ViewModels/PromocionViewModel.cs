namespace AquaClinic.App.ViewModels
{
    /// <summary>
    /// Promocion del catalogo para el CRUD administrativo. La vigencia se edita en
    /// hora local de la clinica (America/Vancouver); el data service la convierte a
    /// y desde UTC como habla la API.
    /// </summary>
    public class PromocionViewModel
    {
        public Guid PromocionId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// <summary>Porcentaje (1) o monto fijo en CAD (2).</summary>
        public int TipoDescuento { get; set; } = 1;

        /// <summary>Porcentaje (ej. 15.00 = 15%) o monto en CAD, segun TipoDescuento.</summary>
        public decimal Valor { get; set; }

        /// <summary>Inicio de la vigencia en hora local de la clinica; null = sin limite.</summary>
        public DateTime? VigenciaDesde { get; set; }

        /// <summary>Fin de la vigencia en hora local de la clinica; null = sin limite.</summary>
        public DateTime? VigenciaHasta { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Servicios a los que aplica. Vacio = promocion global (a todos). El
        /// data service lo viaja como servicioIds.
        /// </summary>
        public List<Guid> ServicioIds { get; set; } = [];

        /// <summary>True cuando no tiene alcance: aplica a todos los servicios.</summary>
        public bool EsGlobal => ServicioIds.Count == 0;

        /// <summary>Texto del descuento tal como se muestra en la tabla ("15%" o "$20.00 CAD").</summary>
        public string DescuentoTexto => TipoDescuento == 1
            ? string.Format("{0}%", Valor.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
            : string.Format("{0} CAD", Valor.ToString("0.00", FormatoClinica.CulturaDinero));
    }
}