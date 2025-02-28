using Blazored.LocalStorage;
using MyUMCApp.Client.Resources;

namespace MyUMCApp.Client.Services;

public interface ITranslationService
{
    string GetText(string key);
    string GetText(string key, params object[] args);
    Task<string> GetCurrentLanguageAsync();
    Task SetLanguageAsync(string language);
    event Action? LanguageChanged;
}

public class TranslationService : ITranslationService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILanguageService _languageService;
    private string _currentLanguage = "en";

    public event Action? LanguageChanged;

    public TranslationService(ILocalStorageService localStorage, ILanguageService languageService)
    {
        _localStorage = localStorage;
        _languageService = languageService;
        InitializeLanguageAsync().ConfigureAwait(false);
    }

    private async Task InitializeLanguageAsync()
    {
        _currentLanguage = await _languageService.GetLanguageAsync();
    }

    public string GetText(string key)
    {
        if (Translations.Resources.TryGetValue(_currentLanguage, out var translations) &&
            translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback to English if translation not found
        if (_currentLanguage != "en" &&
            Translations.Resources.TryGetValue("en", out var englishTranslations) &&
            englishTranslations.TryGetValue(key, out var englishTranslation))
        {
            return englishTranslation;
        }

        return key; // Return the key if no translation found
    }

    public string GetText(string key, params object[] args)
    {
        var text = GetText(key);
        return string.Format(text, args);
    }

    public async Task<string> GetCurrentLanguageAsync()
    {
        return await _languageService.GetLanguageAsync();
    }

    public async Task SetLanguageAsync(string language)
    {
        if (_currentLanguage != language)
        {
            _currentLanguage = language;
            await _languageService.SetLanguageAsync(language);
            LanguageChanged?.Invoke();
        }
    }
} 