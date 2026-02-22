using Microsoft.Extensions.DependencyInjection;

using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddRatingApplication(this IServiceCollection services)
    {
        services.AddMediatorConfigs();

        services.AddTransient<IReviewReferenceService, ReviewReferenceService>();
        services.AddTransient<IRatingCalculatorService, RatingCalculatorService>();
        services.AddTransient<IRatingProviderService, RatingProviderService>();
        services.AddTransient<IStatsCalculator, StatsCalculator>();

        return services;
    }
}
