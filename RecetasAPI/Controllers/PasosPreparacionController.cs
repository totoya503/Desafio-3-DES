using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecetasAPI.Data;
using RecetasAPI.Models;

namespace RecetasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Cualquier usuario autenticado puede consultar
    public class PasosPreparacionController(RecetasDbContext context) : ControllerBase
    {
        private readonly RecetasDbContext _context = context;

        // GET: api/PasosPreparacion  o  api/PasosPreparacion?recetaId=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PasoPreparacion>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PasoPreparacion>>> GetPasosPreparacion(int? recetaId, CancellationToken cancellationToken)
        {
            var consulta = _context.PasosPreparacion.Include(p => p.Receta).AsQueryable();

            if (recetaId.HasValue)
            {
                consulta = consulta.Where(p => p.RecetaId == recetaId.Value);
            }

            // Se devuelven en el orden en que deben realizarse
            return await consulta
                .OrderBy(p => p.RecetaId)
                .ThenBy(p => p.Orden)
                .ToListAsync(cancellationToken);
        }

        // GET: api/PasosPreparacion/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PasoPreparacion), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PasoPreparacion>> GetPasoPreparacion(int id, CancellationToken cancellationToken)
        {
            var paso = await _context.PasosPreparacion
                .Include(p => p.Receta)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (paso == null)
            {
                return NotFound();
            }

            return paso;
        }

        // POST: api/PasosPreparacion
        [HttpPost]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(typeof(PasoPreparacion), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PasoPreparacion>> PostPasoPreparacion(PasoPreparacion paso, CancellationToken cancellationToken)
        {
            if (!await _context.Recetas.AnyAsync(r => r.Id == paso.RecetaId, cancellationToken))
            {
                return BadRequest("La receta asociada no existe.");
            }

            _context.PasosPreparacion.Add(paso);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetPasoPreparacion), new { id = paso.Id }, paso);
        }

        // PUT: api/PasosPreparacion/5
        [HttpPut("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutPasoPreparacion(int id, PasoPreparacion paso, CancellationToken cancellationToken)
        {
            if (id != paso.Id)
            {
                return BadRequest("El id de la URL no coincide con el del paso.");
            }

            if (!await _context.PasosPreparacion.AnyAsync(p => p.Id == id, cancellationToken))
            {
                return NotFound();
            }

            if (!await _context.Recetas.AnyAsync(r => r.Id == paso.RecetaId, cancellationToken))
            {
                return BadRequest("La receta asociada no existe.");
            }

            _context.Entry(paso).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        // DELETE: api/PasosPreparacion/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePasoPreparacion(int id, CancellationToken cancellationToken)
        {
            var paso = await _context.PasosPreparacion.FindAsync([id], cancellationToken);
            if (paso == null)
            {
                return NotFound();
            }

            _context.PasosPreparacion.Remove(paso);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
