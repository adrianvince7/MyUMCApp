using MyUMCApp.Mobile.Models;

namespace MyUMCApp.Mobile.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginRequest request);
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<AuthResult> LogoutAsync();
    Task<AuthResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserProfile?> GetUserProfileAsync();
    Task<bool> IsAuthenticatedAsync();
    string? GetStoredToken();
}