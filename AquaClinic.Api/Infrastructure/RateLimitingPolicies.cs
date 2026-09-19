namespace AquaClinic.Api.Infrastructure
{
    /// <summary>
    /// Pol&#237;ticas de rate limiting registradas en la aplicaci&#243;n.
    /// </summary>
    public static class RateLimitingPolicies
    {
        /// <summary>
        /// Pol&#237;tica estricta para endpoints de autenticaci&#243;n (frena fuerza bruta).
        /// Usar con <c>RequireRateLimiting</c> / <c>[EnableRateLimiting]</c> en endpoints propios.
        /// </summary>
        public const string Authentication = "authentication";
    }
}
