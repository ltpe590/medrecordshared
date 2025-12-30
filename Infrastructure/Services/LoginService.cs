using Core.DTOs;
using Core.Interfaces.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public sealed class LoginService : ILoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string username, string password, string baseUrl)
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/api/Auth/login",
                new { Username = username, Password = password });

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result.Token;
        }
    }
}