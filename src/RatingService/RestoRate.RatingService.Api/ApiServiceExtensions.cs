using RestoRate.RatingService.Api.Configurations;
using RestoRate.RatingService.Infrastructure;
using RestoRate.RatingService.Application;

using Microsoft.Extensions.Configuration;
namespace RestoRate.RatingService.Api;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRatingApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();

        var debounceWindow = GetStatsCalculatorDebounceWindow(builder.Configuration);
        builder.Services.AddRatingApplication(statsCalculatorDebounceWindow: debounceWindow);

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
}
