using MudBlazor;

namespace RestoRate.BlazorDashboard.Helpers;

public static class StatusHelper
{
    public static string GetRussianName(string status)
    {
        return status switch
        {
            "Published" => "Опубликовано",
            "Draft" => "Черновик",
            "Archived" => "В архиве",
            "Deleted" => "Удалено",
            _ => status
        };
    }

    public static Color GetColor(string status)
    {
        return status switch
        {
            "Published" => Color.Success,
            "Draft" => Color.Warning,
            "Archived" => Color.Dark,
            "Deleted" => Color.Error,
            _ => Color.Default
        };
    }
}
