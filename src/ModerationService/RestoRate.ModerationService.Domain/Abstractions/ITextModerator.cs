using Ardalis.Result;

namespace RestoRate.ModerationService.Domain.Abstractions;

public interface ITextModerator
{
    Result Moderate(string? text);
}