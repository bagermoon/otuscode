using MudBlazor;

namespace RestoRate.BlazorDashboard.Themes;

public static class AppTheme
{
    public static MudTheme Default
    {
        get
        {
            var theme = new MudTheme();

            theme.PaletteLight = new PaletteLight
            {
                Primary = "#77BEF0",
                Secondary = "#FFCB61",
                Background = "#f8fbff",
                Surface = "#ffffff",
                AppbarBackground = "rgba(255,255,255,0.8)",
                TextPrimary = "rgba(15, 23, 42, 0.92)",
                TextSecondary = "rgba(15, 23, 42, 0.70)",
            };

            theme.Typography.Default.FontFamily = new[] { "Montserrat", "Helvetica", "Arial", "sans-serif" };

            return theme;
        }
    }
}
