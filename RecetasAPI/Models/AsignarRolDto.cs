using System.ComponentModel.DataAnnotations;

namespace RecetasAPI.Models
{
    public class AsignarRolDto
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = string.Empty;
    }
}
