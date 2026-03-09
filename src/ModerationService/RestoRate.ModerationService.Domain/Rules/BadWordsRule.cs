using Ardalis.Result;

using RestoRate.ModerationService.Domain.Abstractions;

namespace RestoRate.ModerationService.Domain.Rules;

public class BadWordsRule : IModerationRule
{
    private static readonly char[] _separators =
        [' ', ',', '.', '!', '?', '\n', '\r', '\t', ';', ':', '-', '"', '\'', '(', ')', '[', ']'];

    private readonly IBadWordsDictionary _badWords;

    public BadWordsRule(IBadWordsDictionary badWords)
    {
        _badWords = badWords;
    }

    public Result Check(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Success();

        var words = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
            if (_badWords.Contains(word))
                return Result.Invalid(new ValidationError("Комментарий содержит запрещенную лексику."));

        return Result.Success();
    }
}