using System.Globalization;

namespace AquaClinic.App
{
    /// <summary>
    /// Formato regional unico de la clinica: la moneda se muestra siempre en CAD
    /// (en-CA -> "$250.00"), incluso cuando la interfaz se cambia a español.
    /// La cultura de interfaz determina los formatos de fecha y hora.
    /// </summary>
    public static class FormatoClinica
    {
        /// <summary>Cultura de formato para montos en dolares canadienses.</summary>
        public static readonly CultureInfo CulturaDinero = CultureInfo.GetCultureInfo("en-CA");
    }
}
