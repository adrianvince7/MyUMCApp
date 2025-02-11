using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using MyUMCApp.Shared.Models.Auth;
using MyUMCApp.Shared.Services;

namespace MyUMCApp.Client.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAuthService _authService;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationState _anonymous;

    public CustomAuthStateProvider(IAuthService authService, ILocalStorageService localStorage)
    {
        _authService = authService;
        _localStorage = localStorage;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _localStorage.GetItemAsync<string>("access_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            return _anonymous;
        }

        try
        {
            var claims = ParseClaimsFromJwt(accessToken);
            var expiry = claims.FirstOrDefault(c => c.Type.Equals("exp"))?.Value;
            if (expiry != null)
            {
                var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expiry)).UtcDateTime;
                if (expiryDateTime <= DateTime.UtcNow)
                {
                    // Token has expired, try to refresh
                    var refreshToken = await _localStorage.GetItemAsync<string>("refresh_token");
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var response = await _authService.RefreshTokenAsync(new RefreshTokenRequest(refreshToken));
                        if (response.Successful && response.AccessToken != null)
                        {
                            await _localStorage.SetItemAsync("access_token", response.AccessToken);
                            if (response.RefreshToken != null)
                            {
                                await _localStorage.SetItemAsync("refresh_token", response.RefreshToken);
                            }
                            claims = ParseClaimsFromJwt(response.AccessToken);
                        }
                        else
                        {
                            await _localStorage.RemoveItemAsync("access_token");
                            await _localStorage.RemoveItemAsync("refresh_token");
                            return _anonymous;
                        }
                    }
                    else
                    {
                        await _localStorage.RemoveItemAsync("access_token");
                        return _anonymous;
                    }
                }
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            await _localStorage.RemoveItemAsync("access_token");
            await _localStorage.RemoveItemAsync("refresh_token");
            return _anonymous;
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
} 