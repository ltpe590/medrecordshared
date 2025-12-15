using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedRecordWebApi.Data;
using MedRecordWebApi.Models;

namespace MedRecordWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrescriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Prescriptions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prescription>>> GetPrescriptions()
        {
            // Optional: Include visit data if needed for context
            return await _context.Prescriptions.ToListAsync();
        }

        // GET: api/Prescriptions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prescription>> GetPrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            return prescription;
        }

        // PUT: api/Prescriptions/5
        // Updates an existing prescription record.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrescription(int id, Prescription prescription)
        {
            if (id != prescription.PrescriptionId)
            {
                return BadRequest();
            }

            _context.Entry(prescription).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrescriptionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // HTTP 204 No Content
        }

        // POST: api/Prescriptions
        // Creates a new prescription record. Requires a valid VisitId in the request body.
        [HttpPost]
        public async Task<ActionResult<Prescription>> PostPrescription(Prescription prescription)
        {
            // Optional: Basic check to ensure the associated visit exists before saving
            var visitExists = await _context.Visits.AnyAsync(v => v.VisitId == prescription.VisitId);
            if (!visitExists && prescription.VisitId != 0)
            {
                return BadRequest("The specified VisitId does not exist.");
            }

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Returns HTTP 201 Created
            return CreatedAtAction("GetPrescription", new { id = prescription.PrescriptionId }, prescription);
        }

        // DELETE: api/Prescriptions/5
        // Deletes a specific prescription record.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            return NoContent(); // HTTP 204 No Content
        }

        // Helper method to check if a prescription exists
        private bool PrescriptionExists(int id)
        {
            return _context.Prescriptions.Any(e => e.PrescriptionId == id);
        }
    }
}
