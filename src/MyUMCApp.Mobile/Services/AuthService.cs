using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyUMCApp.Mobile.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyUMCApp.Mobile.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private string? _storedToken;
    private UserProfile? _currentUser;

    public AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        // Set base address
        var baseAddress = _configuration["ApiSettings:BaseAddress"] ?? "http://localhost:5294/";
        _httpClient.BaseAddress = new Uri(baseAddress);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            var result = await response.Content.ReadFromJsonAsync<AuthResult>();

            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
            {
                _storedToken = result.Token;
                _currentUser = result.User;
                SetAuthorizationHeader(result.Token);
                
                _logger.LogInformation("Login successful for user: {Email}", request.Email);
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

            if (result?.Success == true)
            {
                _logger.LogInformation("Registration successful for user: {Email}", request.Email);
            }

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
            if (!string.IsNullOrEmpty(_storedToken))
            {
                SetAuthorizationHeader(_storedToken);
                await _httpClient.PostAsync("api/auth/logout", null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling logout API");
        }
        finally
        {
            // Always clear stored data
            _storedToken = null;
            _currentUser = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _logger.LogInformation("User logged out successfully");
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
            if (string.IsNullOrEmpty(_storedToken))
            {
                return null;
            }

            SetAuthorizationHeader(_storedToken);
            var response = await _httpClient.GetAsync("api/auth/profile");
            
            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
                _currentUser = profile;
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
            if (string.IsNullOrEmpty(_storedToken))
            {
                return false;
            }

            // Validate token with API
            SetAuthorizationHeader(_storedToken);
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

    public string? GetStoredToken()
    {
        return _storedToken;
    }

    private void SetAuthorizationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}