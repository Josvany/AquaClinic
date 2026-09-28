namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Permisos granulares del sistema. Cada endpoint protegido declara el permiso
    /// que exige con [HasPermission(...)]. Los roles se mapean a permisos en
    /// <see cref="PermisosPorRol"/>.
    /// </summary>
    public static class Permisos
    {
        // ===== Catálogo de servicios =====
        public const string ServiciosVer = "servicios:ver";
        public const string ServiciosCrear = "servicios:crear";
        public const string ServiciosEditar = "servicios:editar";
        public const string ServiciosEliminar = "servicios:eliminar";

        // ===== Especialidades =====
        public const string EspecialidadesVer = "especialidades:ver";
        public const string EspecialidadesCrear = "especialidades:crear";

        // ===== Citas (fase de agendado) =====
        public const string CitasVer = "citas:ver";
        public const string CitasGestionar = "citas:gestionar";

        // ===== Citas propias del cliente =====
        public const string MisCitasVer = "miscitas:ver";
    }
}