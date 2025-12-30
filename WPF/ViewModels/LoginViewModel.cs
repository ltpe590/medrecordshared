using System.Net.Http;
using WPF.Configuration;
using Infrastructure.Services;

public class LoginViewModel
{
    private readonly LoginService _loginService;
    private readonly AppSettings _settings;

    public LoginViewModel(LoginService loginService, AppSettings settings)
    {
        _loginService = loginService;
        _settings = settings;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var token = await _loginService.LoginAsync(username, password, _settings.ApiBaseUrl);
            // Store token, navigate to main view, etc.
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}