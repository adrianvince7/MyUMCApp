using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Configuration;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;
using MyUMCApp.Shared.Models.Auth;
using AWSForgotPasswordRequest = Amazon.CognitoIdentityProvider.Model.ForgotPasswordRequest;
using AWSChangePasswordRequest = Amazon.CognitoIdentityProvider.Model.ChangePasswordRequest;

namespace MyUMCApp.Shared.Services;

public class CognitoAuthService : IAuthService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoProvider;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _clientId;
    private readonly string _userPoolId;

    public CognitoAuthService(
        IAmazonCognitoIdentityProvider cognitoProvider,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _cognitoProvider = cognitoProvider;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _clientId = _configuration["AWS:Cognito:ClientId"] ?? throw new InvalidOperationException("Cognito ClientId not configured");
        _userPoolId = _configuration["AWS:Cognito:UserPoolId"] ?? throw new InvalidOperationException("Cognito UserPoolId not configured");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var authRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _clientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    {"USERNAME", request.Email},
                    {"PASSWORD", request.Password}
                }
            };

            var authResponse = await _cognitoProvider.AdminInitiateAuthAsync(authRequest);

            if (authResponse.AuthenticationResult != null)
            {
                return new AuthResponse(
                    true,
                    "Login successful",
                    authResponse.AuthenticationResult.AccessToken,
                    authResponse.AuthenticationResult.RefreshToken,
                    DateTime.UtcNow.AddSeconds(authResponse.AuthenticationResult.ExpiresIn)
                );
            }

            return new AuthResponse(false, "Login failed");
        }
        catch (NotAuthorizedException)
        {
            return new AuthResponse(false, "Invalid credentials");
        }
        catch (UserNotFoundException)
        {
            return new AuthResponse(false, "User not found");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                Password = request.Password,
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "email", Value = request.Email },
                    new() { Name = "given_name", Value = request.FirstName },
                    new() { Name = "family_name", Value = request.LastName },
                    new() { Name = "phone_number", Value = request.PhoneNumber },
                    new() { Name = "custom:user_type", Value = request.Type.ToString() }
                }
            };

            await _cognitoProvider.SignUpAsync(signUpRequest);

            // Create local user record
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Type = request.Type,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<User>().AddAsync(user);

            return new AuthResponse(true, "Registration successful. Please check your email for confirmation code.");
        }
        catch (UsernameExistsException)
        {
            return new AuthResponse(false, "Email already registered");
        }
        catch (InvalidPasswordException)
        {
            return new AuthResponse(false, "Password does not meet requirements");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> ConfirmRegistrationAsync(ConfirmRegistrationRequest request)
    {
        try
        {
            var confirmRequest = new ConfirmSignUpRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                ConfirmationCode = request.Code
            };

            await _cognitoProvider.ConfirmSignUpAsync(confirmRequest);

            // Activate local user
            var userRepo = _unitOfWork.Repository<User>();
            var user = (await userRepo.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
            if (user != null)
            {
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await userRepo.UpdateAsync(user);
            }

            return new AuthResponse(true, "Registration confirmed successfully");
        }
        catch (CodeMismatchException)
        {
            return new AuthResponse(false, "Invalid confirmation code");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Confirmation failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(MyUMCApp.Shared.Models.Auth.ForgotPasswordRequest request)
    {
        try
        {
            var forgotRequest = new Amazon.CognitoIdentityProvider.Model.ForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = request.Email
            };

            await _cognitoProvider.ForgotPasswordAsync(forgotRequest);
            return new AuthResponse(true, "Password reset code sent to your email");
        }
        catch (UserNotFoundException)
        {
            return new AuthResponse(false, "User not found");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Password reset request failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            var resetRequest = new ConfirmForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                ConfirmationCode = request.Code,
                Password = request.NewPassword
            };

            await _cognitoProvider.ConfirmForgotPasswordAsync(resetRequest);
            return new AuthResponse(true, "Password reset successful");
        }
        catch (CodeMismatchException)
        {
            return new AuthResponse(false, "Invalid reset code");
        }
        catch (InvalidPasswordException)
        {
            return new AuthResponse(false, "Password does not meet requirements");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Password reset failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> ChangePasswordAsync(string userId, MyUMCApp.Shared.Models.Auth.ChangePasswordRequest request)
    {
        try
        {
            var changeRequest = new Amazon.CognitoIdentityProvider.Model.ChangePasswordRequest
            {
                PreviousPassword = request.CurrentPassword,
                ProposedPassword = request.NewPassword,
                AccessToken = userId // This should be the actual access token
            };

            await _cognitoProvider.ChangePasswordAsync(changeRequest);
            return new AuthResponse(true, "Password changed successfully");
        }
        catch (NotAuthorizedException)
        {
            return new AuthResponse(false, "Current password is incorrect");
        }
        catch (InvalidPasswordException)
        {
            return new AuthResponse(false, "New password does not meet requirements");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Password change failed: {ex.Message}");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            var refreshRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _clientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    {"REFRESH_TOKEN", request.RefreshToken}
                }
            };

            var authResponse = await _cognitoProvider.AdminInitiateAuthAsync(refreshRequest);

            if (authResponse.AuthenticationResult != null)
            {
                return new AuthResponse(
                    true,
                    "Token refreshed successfully",
                    authResponse.AuthenticationResult.AccessToken,
                    request.RefreshToken,
                    DateTime.UtcNow.AddSeconds(authResponse.AuthenticationResult.ExpiresIn)
                );
            }

            return new AuthResponse(false, "Token refresh failed");
        }
        catch (NotAuthorizedException)
        {
            return new AuthResponse(false, "Invalid refresh token");
        }
        catch (Exception ex)
        {
            return new AuthResponse(false, $"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = (await userRepo.FindAsync(u => u.Id.ToString() == userId)).FirstOrDefault();

            if (user == null)
                return null;

            return new UserProfile(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.Type,
                user.ProfilePictureUrl,
                user.IsActive,
                user.PreferredLanguage
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, UserProfile profile)
    {
        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = (await userRepo.FindAsync(u => u.Id.ToString() == userId)).FirstOrDefault();

            if (user == null)
                return false;

            // Update Cognito attributes
            var updateRequest = new AdminUpdateUserAttributesRequest
            {
                UserPoolId = _userPoolId,
                Username = user.Email,
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "given_name", Value = profile.FirstName },
                    new() { Name = "family_name", Value = profile.LastName },
                    new() { Name = "phone_number", Value = profile.PhoneNumber }
                }
            };

            await _cognitoProvider.AdminUpdateUserAttributesAsync(updateRequest);

            // Update local user record
            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.PhoneNumber = profile.PhoneNumber;
            user.ProfilePictureUrl = profile.ProfilePictureUrl;
            user.PreferredLanguage = profile.PreferredLanguage;
            user.UpdatedAt = DateTime.UtcNow;

            await userRepo.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SignOutAsync(string userId)
    {
        try
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = (await userRepo.FindAsync(u => u.Id.ToString() == userId)).FirstOrDefault();

            if (user == null)
                return false;

            var signOutRequest = new AdminUserGlobalSignOutRequest
            {
                UserPoolId = _userPoolId,
                Username = user.Email
            };

            await _cognitoProvider.AdminUserGlobalSignOutAsync(signOutRequest);

            // Clear refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepo.UpdateAsync(user);

            return true;
        }
        catch
        {
            return false;
        }
    }
} 