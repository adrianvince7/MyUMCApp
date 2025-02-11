using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MyUMCApp.Shared.Data;
using MyUMCApp.Shared.Models;
using MyUMCApp.Shared.Models.Auth;
using MyUMCApp.Shared.Services;
using Xunit;

namespace MyUMCApp.Tests.Services;

public class CognitoAuthServiceTests
{
    private readonly Mock<IAmazonCognitoIdentityProvider> _cognitoProviderMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CognitoAuthService _authService;

    public CognitoAuthServiceTests()
    {
        _cognitoProviderMock = new Mock<IAmazonCognitoIdentityProvider>();
        _configurationMock = new Mock<IConfiguration>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _configurationMock.Setup(x => x["AWS:Cognito:ClientId"]).Returns("test-client-id");
        _configurationMock.Setup(x => x["AWS:Cognito:UserPoolId"]).Returns("test-pool-id");

        _authService = new CognitoAuthService(
            _cognitoProviderMock.Object,
            _configurationMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
        var authResult = new AuthenticationResultType
        {
            AccessToken = "test-access-token",
            RefreshToken = "test-refresh-token",
            ExpiresIn = 3600
        };

        _cognitoProviderMock
            .Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ReturnsAsync(new AdminInitiateAuthResponse { AuthenticationResult = authResult });

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.AccessToken.Should().Be(authResult.AccessToken);
        result.RefreshToken.Should().Be(authResult.RefreshToken);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+1234567890",
            Type = UserType.Member
        };

        var userRepositoryMock = new Mock<IRepository<User>>();
        _unitOfWorkMock
            .Setup(x => x.Repository<User>())
            .Returns(userRepositoryMock.Object);

        _cognitoProviderMock
            .Setup(x => x.SignUpAsync(It.IsAny<SignUpRequest>(), default))
            .ReturnsAsync(new SignUpResponse());

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmRegistrationAsync_WithValidCode_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ConfirmRegistrationRequest("test@example.com", "123456");
        var userRepositoryMock = new Mock<IRepository<User>>();
        var user = new User { Email = request.Email, IsActive = false };

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new[] { user });

        _unitOfWorkMock
            .Setup(x => x.Repository<User>())
            .Returns(userRepositoryMock.Object);

        _cognitoProviderMock
            .Setup(x => x.ConfirmSignUpAsync(It.IsAny<ConfirmSignUpRequest>(), default))
            .ReturnsAsync(new ConfirmSignUpResponse());

        // Act
        var result = await _authService.ConfirmRegistrationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.IsActive)), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        _cognitoProviderMock
            .Setup(x => x.ForgotPasswordAsync(It.IsAny<Amazon.CognitoIdentityProvider.Model.ForgotPasswordRequest>(), default))
            .ReturnsAsync(new ForgotPasswordResponse());

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "test@example.com",
            ConfirmationCode = "123456",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        _cognitoProviderMock
            .Setup(x => x.ConfirmForgotPasswordAsync(It.IsAny<ConfirmForgotPasswordRequest>(), default))
            .ReturnsAsync(new ConfirmForgotPasswordResponse());

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest("test-refresh-token");
        var authResult = new AuthenticationResultType
        {
            AccessToken = "new-access-token",
            ExpiresIn = 3600
        };

        _cognitoProviderMock
            .Setup(x => x.AdminInitiateAuthAsync(It.IsAny<AdminInitiateAuthRequest>(), default))
            .ReturnsAsync(new AdminInitiateAuthResponse { AuthenticationResult = authResult });

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Successful.Should().BeTrue();
        result.AccessToken.Should().Be(authResult.AccessToken);
    }
} 