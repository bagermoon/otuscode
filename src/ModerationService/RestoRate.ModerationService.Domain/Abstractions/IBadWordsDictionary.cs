namespace RestoRate.ModerationService.Domain.Abstractions;

public interface IBadWordsDictionary
{
    bool Contains(string word);
}
