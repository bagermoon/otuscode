using Ardalis.Result;

using RestoRate.ModerationService.Domain.Abstractions;

namespace RestoRate.ModerationService.Domain.Rules;

public class SpamRule : IModerationRule
{
    public Result Check(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Success();

        if (text.Contains("http://") || text.Contains("https://") || text.Contains("www."))
        {
            return Result.Invalid(new ValidationError("Ссылки в комментариях запрещены."));
        }

        return Result.Success();
    }
}