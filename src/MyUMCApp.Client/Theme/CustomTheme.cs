using MudBlazor;

namespace MyUMCApp.Client.Theme;

public static class CustomTheme
{
    public static MudTheme UMCTheme => new()
    {
        Palette = new PaletteLight
        {
            Primary = "#CC1030", // UMC Red
            Secondary = "#000000", // Black
            AppbarBackground = "#CC1030",
            Background = "#FFFFFF",
            DrawerBackground = "#F5F5F5",
            DrawerText = "rgba(0,0,0,0.7)",
            Success = "#4CAF50",
            Error = "#FF5252",
            Info = "#2196F3",
            Warning = "#FFC107",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#CC1030", // UMC Red
            Secondary = "#FFFFFF", // White
            AppbarBackground = "#CC1030",
            Background = "#1A1A1A",
            DrawerBackground = "#262626",
            DrawerText = "rgba(255,255,255,0.7)",
            Surface = "#262626",
            Success = "#4CAF50",
            Error = "#FF5252",
            Info = "#2196F3",
            Warning = "#FFC107",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "Roboto", "Helvetica", "Arial", "sans-serif" },
                FontSize = "1rem",
                FontWeight = 400,
                LineHeight = 1.5,
                LetterSpacing = ".00938em"
            },
            H1 = new H1
            {
                FontSize = "2.5rem",
                FontWeight = 300,
                LineHeight = 1.167,
                LetterSpacing = "-.01562em"
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "4px",
            DrawerWidthLeft = "260px",
            DrawerMiniWidthLeft = "80px"
        }
    };
} 