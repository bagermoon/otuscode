namespace RestoRate.ModerationService.Application.Interfaces
{
    public interface IModerationRule
    {
        (bool IsValid, string? Reason) Check(string? text);
    }
}
