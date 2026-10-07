using Microsoft.AspNetCore.Identity;
using RecetasAPI.Models;

namespace RecetasAPI.Data
{
    // Crea los roles "Administrador" y "Usuario" y dos usuarios de prueba
    public static class DbInitializer
    {
        public static async Task SembrarRolesYUsuariosAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

            // 1. Roles
            string[] roles = [Roles.Administrador, Roles.Usuario];
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Usuarios de prueba
            await CrearUsuarioAsync(userManager, "admin@recetas.com", "Admin123$", Roles.Administrador);
            await CrearUsuarioAsync(userManager, "usuario@recetas.com", "Usuario123$", Roles.Usuario);
        }

        private static async Task CrearUsuarioAsync(UserManager<Usuario> userManager, string email, string password, string rol)
        {
            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                usuario = new Usuario { UserName = email, Email = email, EmailConfirmed = true };
                var resultado = await userManager.CreateAsync(usuario, password);
                if (!resultado.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(usuario, rol))
            {
                await userManager.AddToRoleAsync(usuario, rol);
            }
        }
    }
}
