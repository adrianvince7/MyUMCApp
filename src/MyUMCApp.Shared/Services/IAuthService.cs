using MyUMCApp.Shared.Models.Auth;

namespace MyUMCApp.Shared.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> ConfirmRegistrationAsync(ConfirmRegistrationRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task<bool> UpdateUserProfileAsync(string userId, UserProfile profile);
    Task<bool> SignOutAsync(string userId);
} 