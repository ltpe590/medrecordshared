using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Http
{
    public interface IApiConnectionProvider
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
        void SetAuthToken(string token);
    }
}