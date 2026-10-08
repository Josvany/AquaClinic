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
        public const string EspecialidadesEditar = "especialidades:editar";
        public const string EspecialidadesEliminar = "especialidades:eliminar";

        // ===== Citas (fase de agendado) =====
        public const string CitasVer = "citas:ver";
        public const string CitasGestionar = "citas:gestionar";

        // ===== Bloqueos de la agenda (cierres de la clinica) =====
        public const string BloqueosVer = "bloqueos:ver";
        public const string BloqueosCrear = "bloqueos:crear";
        public const string BloqueosEditar = "bloqueos:editar";
        public const string BloqueosEliminar = "bloqueos:eliminar";

        // ===== Promociones del catalogo =====
        public const string PromocionesVer = "promociones:ver";
        public const string PromocionesCrear = "promociones:crear";
        public const string PromocionesEditar = "promociones:editar";
        public const string PromocionesEliminar = "promociones:eliminar";

        // ===== Citas propias del cliente =====
        public const string MisCitasVer = "miscitas:ver";

        /// <summary>Permite al cliente reprogramar una cita propia desde /miscitas.</summary>
        public const string MisCitasGestionar = "miscitas:gestionar";
    }
}