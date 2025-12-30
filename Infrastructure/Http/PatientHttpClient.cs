using Core.DTOs;
using Core.Interfaces;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Http
{
    public sealed class PatientHttpClient : IPatientHttpClient
    {
        private readonly HttpClient _http;

        public PatientHttpClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<PatientDto?> GetPatientAsync(int id, CancellationToken ct = default)
        {
            // TODO: build correct URL, handle errors, logging, etc.
            var response = await _http.GetAsync($"/api/patients/{id}", ct);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<PatientDto>(cancellationToken: ct);
        }
    }
}