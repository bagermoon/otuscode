using Microsoft.Extensions.DependencyInjection;

using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddRatingApplication(
        this IServiceCollection services,
        TimeSpan? statsCalculatorDebounceWindow = null)
    {
        services.AddMediatorConfigs();

        services.AddTransient<IReviewReferenceService, ReviewReferenceService>();
        services.AddTransient<IRatingCalculatorService, RatingCalculatorService>();
        services.AddTransient<IRatingProviderService, RatingProviderService>();

        var debounceWindow = GetDebounceWindow(statsCalculatorDebounceWindow);
        services.AddTransient<IStatsCalculator>(sp =>
            ActivatorUtilities.CreateInstance<StatsCalculator>(sp, debounceWindow));

        return services;
    }

    private static TimeSpan GetDebounceWindow(TimeSpan? debounceWindow = null)
    {
        return debounceWindow switch
        {
            null => TimeSpan.FromSeconds(1),
            var x when x.Value <= TimeSpan.Zero => TimeSpan.FromSeconds(1),
            _ => debounceWindow.Value
        };
    }
}
