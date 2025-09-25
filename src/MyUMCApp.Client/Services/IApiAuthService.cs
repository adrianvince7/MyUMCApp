using MyUMCApp.Client.Models;

namespace MyUMCApp.Client.Services;

public interface IApiAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LogoutAsync();
    Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserProfile?> GetUserProfileAsync();
    Task<bool> IsAuthenticatedAsync();
    event Action<bool> AuthenticationStateChanged;
}