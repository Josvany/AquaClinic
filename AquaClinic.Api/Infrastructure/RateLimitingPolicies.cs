namespace AquaClinic.Api.Infrastructure
{
    /// <summary>
    /// Politicas de rate limiting registradas en la aplicacion.
    /// </summary>
    public static class RateLimitingPolicies
    {
        /// <summary>
        /// Politica estricta para endpoints de autenticacion (frena fuerza bruta).
        /// Usar con <c>RequireRateLimiting</c> / <c>[EnableRateLimiting]</c> en endpoints propios.
        /// </summary>
        public const string Authentication = "authentication";
    }
}
