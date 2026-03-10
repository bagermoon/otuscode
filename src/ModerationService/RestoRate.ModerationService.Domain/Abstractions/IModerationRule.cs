using Ardalis.Result;

namespace RestoRate.ModerationService.Domain.Abstractions;

public interface IModerationRule
{
    Result Check(string? text);
}
