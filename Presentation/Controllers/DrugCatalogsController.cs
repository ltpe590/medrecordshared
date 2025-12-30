using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrugCatalogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DrugCatalogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DrugCatalogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DrugCatalog>>> GetDrugCatalogs()
        {
            return await _context.DrugCatalogs.ToListAsync();
        }

        // GET: api/DrugCatalogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DrugCatalog>> GetDrugCatalog(int id)
        {
            var drugCatalog = await _context.DrugCatalogs.FindAsync(id);

            if (drugCatalog == null)
            {
                return NotFound();
            }

            return drugCatalog;
        }

        // PUT: api/DrugCatalogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDrugCatalog(int id, DrugCatalog drugCatalog)
        {
            if (id != drugCatalog.DrugId)
            {
                return BadRequest();
            }

            _context.Entry(drugCatalog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DrugCatalogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/DrugCatalogs
        [HttpPost]
        public async Task<ActionResult<DrugCatalog>> PostDrugCatalog(DrugCatalog drugCatalog)
        {
            _context.DrugCatalogs.Add(drugCatalog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDrugCatalog", new { id = drugCatalog.DrugId }, drugCatalog);
        }

        // DELETE: api/DrugCatalogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrugCatalog(int id)
        {
            var drugCatalog = await _context.DrugCatalogs.FindAsync(id);
            if (drugCatalog == null)
            {
                return NotFound();
            }

            _context.DrugCatalogs.Remove(drugCatalog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DrugCatalogExists(int id)
        {
            return _context.DrugCatalogs.Any(e => e.DrugId == id);
        }
    }
}
