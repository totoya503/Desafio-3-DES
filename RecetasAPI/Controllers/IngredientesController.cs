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
    public class IngredientesController(RecetasDbContext context) : ControllerBase
    {
        private readonly RecetasDbContext _context = context;

        // GET: api/Ingredientes  o  api/Ingredientes?recetaId=1
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Ingrediente>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Ingrediente>>> GetIngredientes(int? recetaId, CancellationToken cancellationToken)
        {
            var consulta = _context.Ingredientes.Include(i => i.Receta).AsQueryable();

            if (recetaId.HasValue)
            {
                consulta = consulta.Where(i => i.RecetaId == recetaId.Value);
            }

            return await consulta.ToListAsync(cancellationToken);
        }

        // GET: api/Ingredientes/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Ingrediente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Ingrediente>> GetIngrediente(int id, CancellationToken cancellationToken)
        {
            var ingrediente = await _context.Ingredientes
                .Include(i => i.Receta)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (ingrediente == null)
            {
                return NotFound();
            }

            return ingrediente;
        }

        // POST: api/Ingredientes
        [HttpPost]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(typeof(Ingrediente), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Ingrediente>> PostIngrediente(Ingrediente ingrediente, CancellationToken cancellationToken)
        {
            if (!await _context.Recetas.AnyAsync(r => r.Id == ingrediente.RecetaId, cancellationToken))
            {
                return BadRequest("La receta asociada no existe.");
            }

            _context.Ingredientes.Add(ingrediente);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetIngrediente), new { id = ingrediente.Id }, ingrediente);
        }

        // PUT: api/Ingredientes/5
        [HttpPut("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutIngrediente(int id, Ingrediente ingrediente, CancellationToken cancellationToken)
        {
            if (id != ingrediente.Id)
            {
                return BadRequest("El id de la URL no coincide con el del ingrediente.");
            }

            if (!await _context.Ingredientes.AnyAsync(i => i.Id == id, cancellationToken))
            {
                return NotFound();
            }

            if (!await _context.Recetas.AnyAsync(r => r.Id == ingrediente.RecetaId, cancellationToken))
            {
                return BadRequest("La receta asociada no existe.");
            }

            _context.Entry(ingrediente).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }

        // DELETE: api/Ingredientes/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Politicas.SoloAdministrador)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIngrediente(int id, CancellationToken cancellationToken)
        {
            var ingrediente = await _context.Ingredientes.FindAsync([id], cancellationToken);
            if (ingrediente == null)
            {
                return NotFound();
            }

            _context.Ingredientes.Remove(ingrediente);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
