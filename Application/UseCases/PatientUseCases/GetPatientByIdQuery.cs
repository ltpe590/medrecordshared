using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Models;

namespace Application.UseCases.PatientUseCases
{
    public class GetPatientByIdQuery
    {
        public int PatientId { get; set; }
    }

    public class GetPatientByIdQueryHandler
    {
        private readonly IPatientRepository _patientRepository;

        public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<Patient> Handle(GetPatientByIdQuery query)
        {
            return await _patientRepository.GetByIdAsync(query.PatientId);
        }
    }
}