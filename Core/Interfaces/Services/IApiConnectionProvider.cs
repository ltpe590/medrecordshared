namespace Core.Interfaces.Services
{
    public interface IApiConnectionProvider
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
        void SetAuthToken(string token);
    }
}