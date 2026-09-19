using System.Collections.Concurrent;

namespace AquaClinic.App.Auth
{
    /// <summary>
    /// Almacena en servidor la cookie de autenticaci&#243;n emitida por la API para cada
    /// usuario, de modo que el servidor Blazor pueda reenviarla en las llamadas a los
    /// endpoints protegidos ([Authorize]) de la API.
    /// </summary>
    public class UserCookieStore
    {
        private readonly ConcurrentDictionary<string, string> _cookies = new();

        public void Set(string userId, string cookie) => _cookies[userId] = cookie;

        public string? Get(string userId) => _cookies.TryGetValue(userId, out var cookie) ? cookie : null;

        public void Remove(string userId) => _cookies.TryRemove(userId, out _);
    }
}
