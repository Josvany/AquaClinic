using AquaClinic.Identity;

namespace AquaClinic.Api.Authorization
{
    /// <summary>
    /// Mapeo estatico rol &#8594; conjunto de permisos. Los roles viajan como claims
    /// en la cookie de Identity; el <see cref="PermissionProvider"/> consulta este
    /// mapeo para decidir si el usuario tiene un permiso.
    /// </summary>
    public static class PermisosPorRol
    {
        private static readonly Dictionary<string, HashSet<string>> Mapa = new(StringComparer.OrdinalIgnoreCase)
        {
            [RolesClinica.Administrador] =
            [
                Permisos.ServiciosVer,
                Permisos.ServiciosCrear,
                Permisos.ServiciosEditar,
                Permisos.ServiciosEliminar,
                Permisos.EspecialidadesVer,
                Permisos.EspecialidadesCrear,
                Permisos.EspecialidadesEditar,
                Permisos.EspecialidadesEliminar,
                Permisos.CitasVer,
                Permisos.CitasGestionar,
                Permisos.BloqueosVer,
                Permisos.BloqueosCrear,
                Permisos.BloqueosEditar,
                Permisos.BloqueosEliminar,
                Permisos.PromocionesVer,
                Permisos.PromocionesCrear,
                Permisos.PromocionesEditar,
                Permisos.PromocionesEliminar
            ],
            [RolesClinica.Recepcion] =
            [
                Permisos.ServiciosVer,
                Permisos.EspecialidadesVer,
                Permisos.CitasVer,
                Permisos.CitasGestionar
            ],
            [RolesClinica.Esteticista] =
            [
                Permisos.ServiciosVer,
                Permisos.EspecialidadesVer,
                Permisos.CitasVer
            ],
            [RolesClinica.Cliente] =
            [
                Permisos.ServiciosVer,
                Permisos.EspecialidadesVer,
                Permisos.MisCitasVer
            ]
        };

        /// <summary>Devuelve la union de permisos de todos los roles indicados.</summary>
        public static HashSet<string> Obtener(IEnumerable<string> roles)
        {
            var permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rol in roles)
            {
                if (Mapa.TryGetValue(rol, out var permisosDelRol))
                {
                    permisos.UnionWith(permisosDelRol);
                }
            }
            return permisos;
        }
    }
}
