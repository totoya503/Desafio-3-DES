using System.ComponentModel.DataAnnotations;

namespace RecetasAPI.Models
{
    public class Receta
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la receta es obligatorio")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Descripcion { get; set; }

        // Tiempo estimado de preparación expresado en minutos
        [Range(1, 1440, ErrorMessage = "El tiempo de preparación debe estar entre 1 y 1440 minutos")]
        public int TiempoPreparacion { get; set; }

        public ICollection<Ingrediente>? Ingredientes { get; set; }

        public ICollection<PasoPreparacion>? PasosPreparacion { get; set; }
    }
}
