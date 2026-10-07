using System.ComponentModel.DataAnnotations;

namespace RecetasAPI.Models
{
    public class PasoPreparacion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción del paso es obligatoria")]
        [MinLength(10, ErrorMessage = "La descripción del paso debe tener al menos 10 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "El orden del paso debe estar entre 1 y 100")]
        public int Orden { get; set; }

        // Receta asociada
        public int RecetaId { get; set; }
        public Receta? Receta { get; set; }
    }
}
