namespace RecetasAPI.Models
{
    // Nombres de roles y políticas usados en toda la aplicación
    public static class Roles
    {
        public const string Administrador = "Administrador";
        public const string Usuario = "Usuario";
    }

    public static class Politicas
    {
        // Solo usuarios con rol Administrador (crear, modificar y eliminar)
        public const string SoloAdministrador = "SoloAdministrador";
    }
}
