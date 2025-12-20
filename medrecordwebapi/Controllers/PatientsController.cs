using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Medrecord.Data;

namespace Medrecord.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            return await _context.Patients.ToListAsync();
        }

        // GET: api/Patients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
            {
                return NotFound();
            }

            return patient;
        }

        // PUT: api/Patients/5
        // Updates an existing patient record.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatient(int id, Patient patient)
        {
            if (id != patient.PatientId)
            {
                // Returns HTTP 400 Bad Request if the ID in the URL does not match the ID in the body
                return BadRequest();
            }

            // Tells EF Core that the entity is already being tracked and should be marked as modified
            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(id))
                {
                    return NotFound(); // Returns HTTP 404 Not Found
                }
                else
                {
                    throw;
                }
            }

            // Returns HTTP 204 No Content, which is standard for a successful PUT request
            return NoContent();
        }

        // POST: api/Patients
        [HttpPost]
        public async Task<ActionResult<Patient>> PostPatient(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Returns a standard HTTP 201 Created response, including the new patient's ID
            return CreatedAtAction("GetPatient", new { id = patient.PatientId }, patient);
        }

        // DELETE: api/Patients/5
        // Deletes a specific patient record.
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound(); // Returns HTTP 404 Not Found
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            // Returns HTTP 204 No Content, standard for a successful DELETE request
            return NoContent();
        }

        // Helper method to check if a patient exists
        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }
    }
}
