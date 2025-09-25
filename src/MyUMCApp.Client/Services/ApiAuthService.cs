using Blazored.LocalStorage;
using MyUMCApp.Client.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;

namespace MyUMCApp.Client.Services;

public class ApiAuthService : IApiAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ApiAuthService> _logger;
    private const string TokenKey = "auth_token";
    private const string UserKey = "user_profile";

    public event Action<bool>? AuthenticationStateChanged;

    public ApiAuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ILogger<ApiAuthService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();

            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync(TokenKey, result.Token);
                if (result.User != null)
                {
                    await _localStorage.SetItemAsync(UserKey, result.User);
                }
                
                SetAuthorizationHeader(result.Token);
                AuthenticationStateChanged?.Invoke(true);
            }

            return result ?? new AuthResult { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResult { Success = false, Message = "An error occurred during login" };
        }
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();

            return result ?? new AuthResult { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthResult { Success = false, Message = "An error occurred during registration" };
        }
    }

    public async Task<AuthResult> LogoutAsync()
    {
        try
        {
            // Only call API if we have a token
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (!string.IsNullOrEmpty(token))
            {
                SetAuthorizationHeader(token);
                await _httpClient.PostAsync("api/auth/logout", null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling logout API");
        }
        finally
        {
            // Always clear local storage
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            AuthenticationStateChanged?.Invoke(false);
        }

        return new AuthResult { Success = true, Message = "Logged out successfully" };
    }

    public async Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();

            return result ?? new AuthResult { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return new AuthResult { Success = false, Message = "An error occurred while processing your request" };
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();

            return result ?? new AuthResult { Success = false, Message = "Invalid response" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return new AuthResult { Success = false, Message = "An error occurred while resetting your password" };
        }
    }

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync("api/auth/profile");
            
            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
                if (profile != null)
                {
                    await _localStorage.SetItemAsync(UserKey, profile);
                }
                return profile;
            }

            // Token might be expired, clear it
            await LogoutAsync();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Validate token with API
            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync("api/auth/validate-token");
            
            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            await LogoutAsync();
            return false;
        }
    }

    private void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}