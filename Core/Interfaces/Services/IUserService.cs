using Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<string> LoginAsync(string username, string password, string baseUrl);
        Task<List<PatientDto>> GetPatientsAsync(string baseUrl, string token);
        Task CreatePatientAsync(PatientCreateDto patient, string baseUrl, string token);
        Task<List<VisitDto>> GetVisitsByPatientAsync(int patientId, string baseUrl, string token);
        Task SaveVisitAsync(VisitDto visit, string baseUrl, string token);
    }
}