using Fluxor;

namespace MyUMCApp.Client.Store.State;

[FeatureState]
public record AppState
{
    public bool IsLoading { get; init; }
    public string CurrentTheme { get; init; } = "light";
    public string CurrentLanguage { get; init; } = "en";
    public string ErrorMessage { get; init; } = string.Empty;

    private AppState() { } // Required for Fluxor

    public AppState(bool isLoading, string currentTheme, string currentLanguage, string errorMessage)
    {
        IsLoading = isLoading;
        CurrentTheme = currentTheme;
        CurrentLanguage = currentLanguage;
        ErrorMessage = errorMessage;
    }
} 