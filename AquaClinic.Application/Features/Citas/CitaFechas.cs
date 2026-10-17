namespace AquaClinic.Application.Features.Citas
{
    /// <summary>
    /// La clinica opera en America/Vancouver (PST/PDT); las citas se almacenan en UTC
    /// y la conversion a hora local es responsabilidad de esta capa de aplicacion.
    /// </summary>
    public static class CitaFechas
    {
        public const string TimeZoneId = "America/Vancouver";

        private static readonly TimeZoneInfo ZonaClinica = ObtenerZonaClinica();

        private static TimeZoneInfo ObtenerZonaClinica()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            }
            catch (Exception)
            {
                return TimeZoneInfo.Local;
            }
        }

        /// <summary>Instante actual en la hora local de la clinica.</summary>
        public static DateTime AhoraLocal()
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ZonaClinica).DateTime;
        }

        public static DateTime ALocal(DateTimeOffset fechaUtc)
        {
            return TimeZoneInfo.ConvertTime(fechaUtc, ZonaClinica).DateTime;
        }

        /// <summary>Offset (PST/PDT) vigente en la fecha local dada.</summary>
        public static TimeSpan OffsetLocal(DateTime fechaLocal)
        {
            return ZonaClinica.GetUtcOffset(fechaLocal);
        }

        /// <summary>Convierte una fecha/hora local de la clinica a DateTimeOffset UTC.
        /// Normaliza Kind (Local/Unspecified) para evitar ArgumentException cuando el
        /// offset de Vancouver no coincide con la zona de la maquina.</summary>
        public static DateTimeOffset AUtc(DateTime fechaLocal)
        {
            // DateTimeKind.Local exige que el offset argumento coincida con la zona de
            // la maquina; lo tratamos como Unspecified porque la hora es "local de la clinica".
            var sinZona = DateTime.SpecifyKind(fechaLocal, DateTimeKind.Unspecified);
            return new DateTimeOffset(sinZona, OffsetLocal(sinZona)).ToUniversalTime();
        }
    }
}
