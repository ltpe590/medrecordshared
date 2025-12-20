using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Medrecord.Data;

namespace Medrecord.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCatalogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestCatalogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TestCatalogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestCatalog>>> GetTestCatalogs()
        {
            return await _context.TestCatalogs.ToListAsync();
        }

        // GET: api/TestCatalogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestCatalog>> GetTestCatalog(int id)
        {
            var testCatalog = await _context.TestCatalogs.FindAsync(id);

            if (testCatalog == null)
            {
                return NotFound();
            }

            return testCatalog;
        }

        // PUT: api/TestCatalogs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTestCatalog(int id, TestCatalog testCatalog)
        {
            if (id != testCatalog.TestId)
            {
                return BadRequest();
            }

            _context.Entry(testCatalog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestCatalogExists(id))
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

        // POST: api/TestCatalogs
        [HttpPost]
        public async Task<ActionResult<TestCatalog>> PostTestCatalog(TestCatalog testCatalog)
        {
            _context.TestCatalogs.Add(testCatalog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTestCatalog", new { id = testCatalog.TestId }, testCatalog);
        }

        // DELETE: api/TestCatalogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestCatalog(int id)
        {
            var testCatalog = await _context.TestCatalogs.FindAsync(id);
            if (testCatalog == null)
            {
                return NotFound();
            }

            _context.TestCatalogs.Remove(testCatalog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TestCatalogExists(int id)
        {
            return _context.TestCatalogs.Any(e => e.TestId == id);
        }
    }
}
