using RestoRate.ModerationService.Application.Interfaces;

namespace RestoRate.ModerationService.Application.Rules;

public class SpamRule : IModerationRule
{
    public (bool IsValid, string? Reason) Check(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (true, null);

        if (text.Contains("http://") || text.Contains("https://") || text.Contains("www."))
        {
            return (false, "Ссылки в комментариях запрещены.");
        }

        return (true, null);
    }
}
