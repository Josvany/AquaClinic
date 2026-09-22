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
    }
}
