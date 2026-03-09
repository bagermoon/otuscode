using System.Collections.Generic;

using Ardalis.Result;
using System.Linq;
using RestoRate.ModerationService.Domain.Abstractions;

namespace RestoRate.ModerationService.Domain.Services;

public class TextModerator : ITextModerator
{
    private readonly IEnumerable<IModerationRule> _rules;

    public TextModerator(IEnumerable<IModerationRule> rules)
    {
        _rules = rules;
    }

    public Result Moderate(string? text)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Check(text);
            if (result.Status != ResultStatus.Ok)
            {
                if (result.Status == ResultStatus.Invalid)
                    return Result.Invalid(result.ValidationErrors);

                return Result.Error(string.Join(';', result.Errors));
            }
        }

        return Result.Success();
    }
}
