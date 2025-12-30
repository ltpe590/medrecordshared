using Core.DTOs;
using System.Threading.Tasks;

namespace Core.Interfaces.Services
{
    public interface ILoginService
    {
        Task<string> LoginAsync(string username, string password, string baseUrl);
    }
}