using Blazored.LocalStorage;
using Fluxor;
using MyUMCApp.Client.Store.State;

namespace MyUMCApp.Client.Services;

public interface IThemeService
{
    Task<string> GetThemeAsync();
    Task SetThemeAsync(string theme);
}

public class ThemeService : IThemeService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IDispatcher _dispatcher;
    private const string ThemeKey = "app_theme";

    public ThemeService(ILocalStorageService localStorage, IDispatcher dispatcher)
    {
        _localStorage = localStorage;
        _dispatcher = dispatcher;
    }

    public async Task<string> GetThemeAsync()
    {
        return await _localStorage.GetItemAsync<string>(ThemeKey) ?? "light";
    }

    public async Task SetThemeAsync(string theme)
    {
        await _localStorage.SetItemAsync(ThemeKey, theme);
        _dispatcher.Dispatch(new AppState(false, theme, string.Empty, string.Empty));
    }
} 