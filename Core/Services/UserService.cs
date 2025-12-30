using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public class UserService : IUserService
    {
        private readonly IApiConnectionProvider _api;
        private readonly IUserMappingService _mapper;

        public UserService(IApiConnectionProvider api, IUserMappingService mapper)
        {
            _api = api;
            _mapper = mapper;
        }

        public async Task<string> LoginAsync(string username, string password, string baseUrl)
        {
            var response = await _api.PostAsync<object, LoginResponse>(
                $"{baseUrl}/api/Auth/login",
                new { Username = username, Password = password });

            return response.Token;
        }

        public async Task<List<PatientDto>> GetPatientsAsync(string baseUrl, string token)
        {
            _api.SetAuthToken(token);
            var patients = await _api.GetAsync<List<PatientDto>>($"{baseUrl}/api/Patients");
            return patients;
        }

        public async Task SaveVisitAsync(VisitDto visit, string baseUrl, string token)
        {
            _api.SetAuthToken(token);
            await _api.PostAsync<VisitDto, object>($"{baseUrl}/api/Visits", visit);
        }
    }
}