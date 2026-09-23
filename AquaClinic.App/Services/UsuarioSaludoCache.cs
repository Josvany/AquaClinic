using System.Collections.Concurrent;

namespace AquaClinic.App.Services
{
    /// <summary>
    /// Cache singleton (por circuito de Blazor Server) con el nombre para el saludo
    /// del nav, indexado por correo. Permite que MiPerfil (isla interactiva) avise
    /// al saludo (otra isla) cuando el perfil cambia, sin recargar la pagina.
    /// </summary>
    public class UsuarioSaludoCache
    {
        private readonly ConcurrentDictionary<string, string> _nombres = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Se dispara con el correo cuyo nombre para saludo cambio.</summary>
        public event Action<string>? NombreActualizado;

        /// <summary>
        /// Deriva el nombre a mostrar: nombre real si existe; si no, el prefijo
        /// del correo capitalizado (comportamiento anterior).
        /// </summary>
        public static string DerivarNombre(string correo, string? nombre)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                return nombre;
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                return "";
            }

            var prefijo = correo.Split('@')[0];
            return char.ToUpper(prefijo[0]) + prefijo[1..];
        }

        public string? Obtener(string correo)
        {
            return _nombres.TryGetValue(correo, out var nombre) ? nombre : null;
        }

        /// <summary>
        /// Guarda el nombre para saludo del correo y notifica a los suscriptores.
        /// </summary>
        public void Establecer(string correo, string nombreParaSaludo)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(nombreParaSaludo))
            {
                return;
            }

            _nombres[correo] = nombreParaSaludo;
            NombreActualizado?.Invoke(correo);
        }
    }
}
