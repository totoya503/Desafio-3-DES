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
    public class RecetasController(RecetasDbContext context) : ControllerBase
    {
        private readonly RecetasDbContext _context = context;

        // GET: api/Recetas
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Receta>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetas(CancellationToken cancellationToken)
        {
            return await _context.Recetas
                .Include(r => r.Ingredientes)
                .Include(r => r.PasosPreparacion!.OrderBy(p => p.Orden))
                .ToListAsync(cancellationToken);
        }

        // GET: api/Recetas/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Receta), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Receta>> GetReceta(int id, CancellationToken cancellationToken)
        {
            var receta = await _context.Recetas
                .Include(r => r.Ingredientes)
                .Include(r => r.PasosPreparacion!.OrderBy(p => p.Orden))
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (receta == null)
            {
                return NotFound();
            }

            return receta;
        }

        // POST: api/Recetas
        [HttpPost]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(typeof(Receta), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Receta>> PostReceta(Receta receta, CancellationToken cancellationToken)
        {
            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetReceta), new { id = receta.Id }, receta);
        }

        // PUT: api/Recetas/5
        [HttpPut("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutReceta(int id, Receta receta, CancellationToken cancellationToken)
        {
            if (id != receta.Id)
            {
                return BadRequest("El id de la URL no coincide con el de la receta.");
            }

            if (!await _context.Recetas.AnyAsync(r => r.Id == id, cancellationToken))
            {
                return NotFound();
            }

            _context.Entry(receta).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        // DELETE: api/Recetas/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReceta(int id, CancellationToken cancellationToken)
        {
            var receta = await _context.Recetas.FindAsync([id], cancellationToken);
            if (receta == null)
            {
                return NotFound();
            }

            // Los ingredientes y pasos se eliminan en cascada
            _context.Recetas.Remove(receta);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
