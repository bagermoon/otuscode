using RestoRate.ModerationService.Application.Interfaces;

namespace RestoRate.ModerationService.Application.Engine;

public class TextModerator : ITextModerator
{
    private readonly IEnumerable<IModerationRule> _rules;

    public TextModerator(IEnumerable<IModerationRule> rules)
    {
        _rules = rules;
    }

    public (bool IsApproved, string? RejectReason) Moderate(string? text)
    {
        foreach (var rule in _rules)
        {
            var (isValid, reason) = rule.Check(text);
            if (!isValid)
            {
                return (false, reason);
            }
        }

        return (true, null);
    }
}
