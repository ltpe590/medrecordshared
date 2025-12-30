using Core.Interfaces.Repositories;
using Domain.Models;
using WPF.Models.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WPF.MainWindow;

namespace WPF.Services
{
    public class PatientService
    {
        private readonly IPatientRepository _repository;
        private readonly ApiService _apiService;

        public PatientService(IPatientRepository repository, ApiService apiService)
        {
            _repository = repository;
            _apiService = apiService;
        }

        public async Task<List<PatientViewModel>> LoadPatientsAsync()
        {
            var patients = await _repository.GetAllAsync();
            return patients.Select(ToViewModel).ToList();
        }

        public async Task<List<PatientViewModel>> LoadPatientsFromApiAsync(string baseUrl)
        {
            var patients = await _apiService.GetAsync<List<Patient>>($"{baseUrl}/api/Patients");
            return patients.Select(ToViewModel).ToList();
        }

        private static PatientViewModel ToViewModel(Patient p) => new()
        {
            PatientId = p.PatientId,
            Name = p.Name ?? "",
            DateOfBirth = p.DateOfBirth,
            Gender = p.Sex ?? "",
            ContactNumber = p.PhoneNumber ?? "",
            Address = p.Address ?? ""
        };
    }
}