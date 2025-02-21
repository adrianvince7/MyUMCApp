using Blazored.LocalStorage;
using Fluxor;
using MyUMCApp.Client.Store.State;

namespace MyUMCApp.Client.Services;

public interface ILanguageService
{
    Task<string> GetLanguageAsync();
    Task SetLanguageAsync(string language);
    Dictionary<string, string> GetAvailableLanguages();
}

public class LanguageService : ILanguageService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IDispatcher _dispatcher;
    private const string LanguageKey = "app_language";

    private readonly Dictionary<string, string> _availableLanguages = new()
    {
        { "en", "English" },
        { "sn", "Shona" },
        { "nd", "Ndebele" }
    };

    public LanguageService(ILocalStorageService localStorage, IDispatcher dispatcher)
    {
        _localStorage = localStorage;
        _dispatcher = dispatcher;
    }

    public async Task<string> GetLanguageAsync()
    {
        return await _localStorage.GetItemAsync<string>(LanguageKey) ?? "en";
    }

    public async Task SetLanguageAsync(string language)
    {
        if (!_availableLanguages.ContainsKey(language))
            throw new ArgumentException("Invalid language code", nameof(language));

        await _localStorage.SetItemAsync(LanguageKey, language);
        _dispatcher.Dispatch(new AppState(false, string.Empty, language, string.Empty));
    }

    public Dictionary<string, string> GetAvailableLanguages()
    {
        return _availableLanguages;
    }
} 