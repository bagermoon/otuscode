using Microsoft.Extensions.Options;

using RestoRate.ModerationService.Application.Interfaces;
using RestoRate.ModerationService.Application.Options;

namespace RestoRate.ModerationService.Application.Rules;

public class BadWordsRule : IModerationRule, IDisposable
{
    private static readonly char[] _separators =
        [' ', ',', '.', '!', '?', '\n', '\r', '\t', ';', ':', '-', '"', '\'', '(', ')', '[', ']'];

    private HashSet<string> _badWords = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDisposable? _onChangeTracker;

    public BadWordsRule(IOptionsMonitor<ModerationSettings> options)
    {
        UpdateDictionary(options.CurrentValue);

        _onChangeTracker = options.OnChange(newSettings =>
        {
            UpdateDictionary(newSettings);
        });
    }

    /// <summary>
    /// Перестраивает внутренний словарь запрещенных слов на основе новых настроек. <br/>
    /// Вызывается при старте приложения, а также автоматически (на лету) при каждом сохранении файла appsettings.json.
    /// Пересоздание HashSet обеспечивает поиск слов за O(1) и игнорирует регистр букв.
    /// </summary>
    /// <param name="settings">Новые настройки модерации, загруженные из конфигурации.</param>
    private void UpdateDictionary(ModerationSettings settings)
    {
        var words = settings.BadWords ?? Array.Empty<string>();
        _badWords = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
    }

    public (bool IsValid, string? Reason) Check(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (true, null);

        var words = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
            if (_badWords.Contains(word))
                return (false, "Комментарий содержит запрещенную лексику.");

        return (true, null);
    }

    public void Dispose()
    {
        _onChangeTracker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
