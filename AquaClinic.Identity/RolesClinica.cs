namespace AquaClinic.Identity
{
    /// <summary>
    /// Roles del sistema. Los permisos granulares se derivan de estos roles
    /// (ver AquaClinic.Api/Authorization/PermisosPorRol.cs).
    /// </summary>
    public static class RolesClinica
    {
        public const string Administrador = "Administrador";
        public const string Recepcion = "Recepcion";
        public const string Esteticista = "Esteticista";
        public const string Cliente = "Cliente";

        /// <summary>Todos los roles del sistema, en orden de creacion.</summary>
        public static readonly string[] Todos =
        [
            Administrador,
            Recepcion,
            Esteticista,
            Cliente
        ];
    }
}
