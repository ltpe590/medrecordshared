using Core.Interfaces.Repositories;
using Core.UseCases.PatientUseCases;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;

        public PatientsController(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        // GET: api/Patients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Patient>>> GetPatients()
        {
            var patients = await _patientRepository.GetAllAsync();
            return Ok(patients);
        }

        // GET: api/Patients/5 - Using GetPatientByIdQuery pattern
        [HttpGet("{id}")]
        public async Task<ActionResult<Patient>> GetPatient(int id)
        {
            var query = new GetPatientByIdQuery { PatientId = id };
            var handler = new GetPatientByIdQueryHandler(_patientRepository);

            var patient = await handler.Handle(query);

            if (patient == null)
            {
                return NotFound();
            }

            return patient;
        }

        // PUT: api/Patients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPatient(int id, Patient patient)
        {
            if (id != patient.PatientId)
            {
                return BadRequest();
            }

            if (!await _patientRepository.ExistsAsync(id))
            {
                return NotFound();
            }

            await _patientRepository.UpdateAsync(patient);
            return NoContent();
        }

        // POST: api/Patients
        [HttpPost]
        public async Task<ActionResult<Patient>> PostPatient(Patient patient)
        {
            var createdPatient = await _patientRepository.AddAsync(patient);
            return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.PatientId }, createdPatient);
        }

        // DELETE: api/Patients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            if (!await _patientRepository.ExistsAsync(id))
            {
                return NotFound();
            }

            await _patientRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}