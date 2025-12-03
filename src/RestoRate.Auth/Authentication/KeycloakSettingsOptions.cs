namespace RestoRate.Auth.Authentication;

public class KeycloakSettingsOptions
{
    public const string SectionName = "KeycloakSettings";

    public string Realm { get; set; } = "restorate";
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
