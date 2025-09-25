using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MyUMCApp.API.DTOs;
using MyUMCApp.API.Models;
using MyUMCApp.API.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyUMCApp.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtSettings jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Account is disabled"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = await GenerateJwtTokenAsync(user);
            var userProfile = MapToUserProfile(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                User = userProfile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during login"
            };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User with this email already exists"
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Organization = request.Organization,
                ChurchRole = request.ChurchRole,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                EmailConfirmed = true // For simplicity, auto-confirm emails
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Registration failed: {errors}"
                };
            }

            // Add default role
            await _userManager.AddToRoleAsync(user, "Member");

            var userProfile = MapToUserProfile(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful",
                User = userProfile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during registration"
            };
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist
                return new AuthResponse
                {
                    Success = true,
                    Message = "If the email exists, a password reset link has been sent"
                };
            }

            // Generate reset token (simplified for demonstration)
            var resetToken = GenerateSecureToken();
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _userManager.UpdateAsync(user);

            // In a real application, you would send an email here
            _logger.LogInformation("Password reset token generated for user {Email}: {Token}", 
                request.Email, resetToken);

            return new AuthResponse
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password for user {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred while processing your request"
            };
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.PasswordResetToken != request.Token || 
                user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired reset token"
                };
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Password reset failed: {errors}"
                };
            }

            // Clear reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Password reset successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for user {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred while resetting your password"
            };
        }
    }

    public async Task<AuthResponse> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Password change failed: {errors}"
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Password changed successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change for user {UserId}", userId);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred while changing your password"
            };
        }
    }

    public async Task<AuthResponse> LogoutAsync(string userId)
    {
        try
        {
            // In a stateless JWT system, logout is typically handled client-side
            // by removing the token. For additional security, you could maintain
            // a blacklist of tokens or implement token revocation.
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                _logger.LogInformation("User {UserId} logged out", userId);
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Logout successful"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return new AuthResponse
            {
                Success = false,
                Message = "An error occurred during logout"
            };
        }
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? MapToUserProfile(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
            return null;
        }
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
        
        var roles = await _userManager.GetRolesAsync(user);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Email!),
            new(ClaimTypes.Email, user.Email!),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("organization", user.Organization),
            new("churchRole", user.ChurchRole)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static UserProfile MapToUserProfile(ApplicationUser user)
    {
        return new UserProfile
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Organization = user.Organization,
            ChurchRole = user.ChurchRole,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastLogin,
            IsActive = user.IsActive
        };
    }
}