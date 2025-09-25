using MyUMCApp.API.DTOs;

namespace MyUMCApp.API.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<AuthResponse> LogoutAsync(string userId);
    Task<UserProfile?> GetUserProfileAsync(string userId);
}