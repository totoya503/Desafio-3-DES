using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RecetasAPI.Controllers
{
    // Devuelve las URLs del visor de reportes de SSRS (ReportViewer.aspx).
    // La configuración del servidor está en appsettings.json -> "ReportesSSRS".
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportesController(IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        // GET: api/Reportes/recetas-ingredientes?tiempoMinimo=0&tiempoMaximo=30
        // Reporte 1: recetas e ingredientes filtrados por tiempo de preparación
        [HttpGet("recetas-ingredientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult RecetasIngredientes(int tiempoMinimo = 0, int tiempoMaximo = 120)
        {
            if (tiempoMinimo < 0 || tiempoMaximo < tiempoMinimo)
            {
                return BadRequest("El rango de tiempo de preparación no es válido.");
            }

            var url = ConstruirUrl(_configuration["ReportesSSRS:ReporteRecetasIngredientes"]!)
                      + $"&TiempoMinimo={tiempoMinimo}&TiempoMaximo={tiempoMaximo}";

            return Ok(new { reporte = "Recetas e ingredientes por tiempo de preparación", url });
        }

        // GET: api/Reportes/pasos-preparacion
        // Reporte 2: cantidad de pasos por receta y su orden
        [HttpGet("pasos-preparacion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PasosPreparacion()
        {
            var url = ConstruirUrl(_configuration["ReportesSSRS:ReportePasosPreparacion"]!);

            return Ok(new { reporte = "Pasos de preparación por receta", url });
        }

        // Ejemplo: http://localhost/ReportServer/Pages/ReportViewer.aspx?%2fRecetas%2fReportePasosPreparacion&rs:Command=Render
        private string ConstruirUrl(string nombreReporte)
        {
            var servidor = _configuration["ReportesSSRS:UrlServidor"]!.TrimEnd('/');
            var carpeta = _configuration["ReportesSSRS:Carpeta"] ?? string.Empty;
            var ruta = Uri.EscapeDataString($"{carpeta.TrimEnd('/')}/{nombreReporte}");

            return $"{servidor}/Pages/ReportViewer.aspx?{ruta}&rs:Command=Render";
        }
    }
}
