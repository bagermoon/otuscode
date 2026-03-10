using RestoRate.RatingService.Api.Configurations;
using RestoRate.RatingService.Api.Services;
using RestoRate.RatingService.Infrastructure;
using RestoRate.RatingService.Application;
using RestoRate.ServiceDefaults;

using Microsoft.Extensions.Configuration;
namespace RestoRate.RatingService.Api;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRatingApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();

        var debounceWindow = GetStatsCalculatorDebounceWindow(builder.Configuration);
        builder.Services.AddRatingApplication(statsCalculatorDebounceWindow: debounceWindow);

        if (HasRedisConnection(builder.Configuration))
        {
            builder.Services.AddSingleton<IHostedService>(sp =>
                ActivatorUtilities.CreateInstance<RatingRecalculationHostedService>(sp, debounceWindow));
        }

        builder.AddRatingInfrastructure(
            typeof(ApiServiceExtensions).Assembly
        );

        return builder;
    }

    private static TimeSpan GetStatsCalculatorDebounceWindow(IConfiguration configuration)
    {
        var ms = configuration.GetValue<int?>("RatingService:StatsCalculator:DebounceWindowMs");

        return ms switch
        {
            null or <= 0 => TimeSpan.Zero,
            _ => TimeSpan.FromMilliseconds(ms.Value)
        };
    }

    private static bool HasRedisConnection(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration.GetConnectionString(AppHostProjects.RedisCache));
}
