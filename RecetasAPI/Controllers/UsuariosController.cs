using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecetasAPI.Models;
using System.Security.Claims;

namespace RecetasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager) : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        // POST: api/Usuarios/registro
        // Registra un usuario nuevo y le asigna automáticamente el rol "Usuario"
        [HttpPost("registro")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registro(RegistroDto registro)
        {
            var usuario = new Usuario { UserName = registro.Email, Email = registro.Email };
            var resultado = await _userManager.CreateAsync(usuario, registro.Password);

            if (!resultado.Succeeded)
            {
                return BadRequest(resultado.Errors);
            }

            await _userManager.AddToRoleAsync(usuario, Roles.Usuario);

            return Ok(new { mensaje = "Usuario registrado con el rol Usuario", usuario.Email });
        }

        // GET: api/Usuarios/actual
        // Muestra el usuario autenticado y sus roles (útil para comprobar la sesión)
        [HttpGet("actual")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult UsuarioActual()
        {
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);

            return Ok(new { usuario = User.Identity?.Name, roles });
        }

        // POST: api/Usuarios/cerrar-sesion
        // Elimina la cookie de autenticación
        [HttpPost("cerrar-sesion")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return NoContent();
        }

        // GET: api/Usuarios
        [HttpGet]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuarios()
        {
            var lista = new List<object>();
            foreach (var usuario in _userManager.Users.ToList())
            {
                lista.Add(new
                {
                    usuario.Id,
                    usuario.Email,
                    roles = await _userManager.GetRolesAsync(usuario)
                });
            }

            return Ok(lista);
        }

        // POST: api/Usuarios/asignar-rol
        // Solo el Administrador puede cambiar el rol de un usuario
        [HttpPost("asignar-rol")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AsignarRol(AsignarRolDto datos)
        {
            if (!await _roleManager.RoleExistsAsync(datos.Rol))
            {
                return BadRequest($"El rol '{datos.Rol}' no existe. Use '{Roles.Administrador}' o '{Roles.Usuario}'.");
            }

            var usuario = await _userManager.FindByEmailAsync(datos.Email);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Un usuario tiene un solo rol: se quitan los anteriores y se asigna el nuevo
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, datos.Rol);

            return Ok(new { mensaje = $"Rol '{datos.Rol}' asignado a {datos.Email}" });
        }
    }
}
