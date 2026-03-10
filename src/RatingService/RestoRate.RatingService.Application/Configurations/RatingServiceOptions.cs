namespace RestoRate.RatingService.Application.Configurations;

public sealed class RatingServiceOptions
{
    public const string SectionName = "RatingService";
    public const int DefaultDebounceWindowMs = 1000;

    public int DebounceWindowMs { get; set; } = DefaultDebounceWindowMs;

    public TimeSpan DebounceWindow => NormalizeDebounceWindow(DebounceWindowMs);

    public static TimeSpan NormalizeDebounceWindow(int debounceWindowMs)
        => TimeSpan.FromMilliseconds(NormalizeDebounceWindowMs(debounceWindowMs));

    public static int NormalizeDebounceWindowMs(int debounceWindowMs)
        => debounceWindowMs > 0 ? debounceWindowMs : DefaultDebounceWindowMs;
}
