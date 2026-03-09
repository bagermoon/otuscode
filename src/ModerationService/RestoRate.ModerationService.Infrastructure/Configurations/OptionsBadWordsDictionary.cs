using Microsoft.Extensions.Options;

using RestoRate.ModerationService.Domain.Abstractions;

namespace RestoRate.ModerationService.Infrastructure.Configurations;

public sealed class OptionsBadWordsDictionary : IBadWordsDictionary, IDisposable
{
    private HashSet<string> _badWords = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDisposable? _onChangeTracker;

    public OptionsBadWordsDictionary(IOptionsMonitor<ModerationSettingsOptions> options)
    {
        UpdateDictionary(options.CurrentValue);
        _onChangeTracker = options.OnChange(UpdateDictionary);
    }

    public bool Contains(string word)
    {
        return _badWords.Contains(word);
    }

    private void UpdateDictionary(ModerationSettingsOptions settings)
    {
        var words = settings.BadWords ?? Array.Empty<string>();
        _badWords = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _onChangeTracker?.Dispose();
        GC.SuppressFinalize(this);
    }
}