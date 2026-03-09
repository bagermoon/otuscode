namespace RestoRate.ModerationService.Infrastructure.Configurations;

public class ModerationSettingsOptions
{
    public string[] BadWords { get; set; } = Array.Empty<string>();
}