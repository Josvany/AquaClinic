using System.Globalization;

namespace AquaClinic.App
{
    /// <summary>
    /// Formato regional unico de la clinica: la moneda se muestra siempre en CAD
    /// (en-CA -> "$250.00") sin importar la cultura del sistema operativo del
    /// servidor (es-NI en pruebas locales, en-CA en produccion). La cultura de
    /// la UI (nombres de dia/mes) se fija en los Program.cs de App y Api.
    /// </summary>
    public static class FormatoClinica
    {
        /// <summary>Cultura de formato para montos en dolares canadienses.</summary>
        public static readonly CultureInfo CulturaDinero = CultureInfo.GetCultureInfo("en-CA");
    }
}
