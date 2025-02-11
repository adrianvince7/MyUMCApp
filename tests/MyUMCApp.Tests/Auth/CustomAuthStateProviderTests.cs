using System.Security.Claims;
using Blazored.LocalStorage;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using MyUMCApp.Client.Auth;
using MyUMCApp.Shared.Models.Auth;
using MyUMCApp.Shared.Services;
using Xunit;

namespace MyUMCApp.Tests.Auth;

public class CustomAuthStateProviderTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILocalStorageService> _localStorageMock;
    private readonly CustomAuthStateProvider _authStateProvider;

    public CustomAuthStateProviderTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _localStorageMock = new Mock<ILocalStorageService>();
        _authStateProvider = new CustomAuthStateProvider(_authServiceMock.Object, _localStorageMock.Object);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithNoToken_ReturnsAnonymousState()
    {
        // Arrange
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("access_token", default))
            .ReturnsAsync((string?)null);

        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithValidToken_ReturnsAuthenticatedState()
    {
        // Arrange
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                   "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoyNTE2MjM5MDIyfQ." +
                   "4Adcj3UFYzPUVaVF4uq_QL7SmFJp0F3vTJQpEFNwpHg";

        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("access_token", default))
            .ReturnsAsync(token);

        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        authState.User.FindFirst("name")?.Value.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithExpiredToken_AttemptsRefresh()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                          "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNTE2MjM5MDIyfQ." +
                          "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var refreshToken = "refresh-token";
        var newToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                      "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoyNTE2MjM5MDIyfQ." +
                      "4Adcj3UFYzPUVaVF4uq_QL7SmFJp0F3vTJQpEFNwpHg";

        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("access_token", default))
            .ReturnsAsync(expiredToken);
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refresh_token", default))
            .ReturnsAsync(refreshToken);

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>()))
            .ReturnsAsync(new AuthResponse { Successful = true, AccessToken = newToken });

        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeTrue();
        _authServiceMock.Verify(x => x.RefreshTokenAsync(It.Is<RefreshTokenRequest>(r => r.RefreshToken == refreshToken)), Times.Once);
        _localStorageMock.Verify(x => x.SetItemAsync("access_token", newToken, default), Times.Once);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WithExpiredTokenAndFailedRefresh_ReturnsAnonymousState()
    {
        // Arrange
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
                          "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiZXhwIjoxNTE2MjM5MDIyfQ." +
                          "SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var refreshToken = "refresh-token";

        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("access_token", default))
            .ReturnsAsync(expiredToken);
        _localStorageMock
            .Setup(x => x.GetItemAsync<string>("refresh_token", default))
            .ReturnsAsync(refreshToken);

        _authServiceMock
            .Setup(x => x.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>()))
            .ReturnsAsync(new AuthResponse { Successful = false });

        // Act
        var authState = await _authStateProvider.GetAuthenticationStateAsync();

        // Assert
        authState.User.Identity.Should().NotBeNull();
        authState.User.Identity!.IsAuthenticated.Should().BeFalse();
        _localStorageMock.Verify(x => x.RemoveItemAsync("access_token", default), Times.Once);
        _localStorageMock.Verify(x => x.RemoveItemAsync("refresh_token", default), Times.Once);
    }

    [Fact]
    public void NotifyAuthenticationStateChanged_NotifiesStateChange()
    {
        // Arrange
        AuthenticationState? newState = null;
        _authStateProvider.AuthenticationStateChanged += state => newState = state.Result;

        // Act
        _authStateProvider.NotifyAuthenticationStateChanged();

        // Assert
        newState.Should().NotBeNull();
    }
} 