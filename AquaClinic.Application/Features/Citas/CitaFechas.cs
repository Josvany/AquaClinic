namespace AquaClinic.Application.Features.Citas
{
    /// <summary>
    /// La clinica opera en America/Vancouver (PST/PDT); las citas se almacenan en UTC
    /// y la conversion a hora local es responsabilidad de esta capa de aplicacion.
    /// </summary>
    internal static class CitaFechas
    {
        public static DateTime ALocal(DateTimeOffset fechaUtc)
        {
            try
            {
                return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(fechaUtc, "America/Vancouver").DateTime;
            }
            catch (TimeZoneNotFoundException)
            {
                return fechaUtc.DateTime;
            }
            catch (InvalidTimeZoneException)
            {
                return fechaUtc.DateTime;
            }
        }

        /// <summary>Offset (PST/PDT) vigente en la fecha local dada.</summary>
        public static TimeSpan OffsetLocal(DateTime fechaLocal)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Vancouver");
                return tz.GetUtcOffset(fechaLocal);
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeSpan.Zero;
            }
            catch (InvalidTimeZoneException)
            {
                return TimeSpan.Zero;
            }
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
