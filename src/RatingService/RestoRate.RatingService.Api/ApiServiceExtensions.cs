using RestoRate.RatingService.Api.Configurations;
using RestoRate.RatingService.Api.Workers;
using RestoRate.RatingService.Application;
using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Infrastructure;
using RestoRate.ServiceDefaults;
namespace RestoRate.RatingService.Api;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRatingApi(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureRatingOptions(builder.Configuration);

        builder.ConfigureAuthentication();
        builder.Services.AddRatingApplication();

        builder.Services.AddRecalculationHostedServiceIfNeeded(builder.Configuration);

        builder.AddRatingInfrastructure(
            typeof(ApiServiceExtensions).Assembly
        );

        return builder;
    }

    private static void ConfigureRatingOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RatingServiceOptions>()
            .Bind(configuration.GetSection(RatingServiceOptions.SectionName));
        services.PostConfigure<RatingServiceOptions>(options =>
            options.DebounceWindowMs = RatingServiceOptions.NormalizeDebounceWindowMs(options.DebounceWindowMs));
    }

    private static void AddRecalculationHostedServiceIfNeeded(this IServiceCollection services, IConfiguration configuration)
    {
        if (HasRedisConnection(configuration))
        {
            services.AddHostedService<RecalculationHostedService>();
        }
    }

    private static bool HasRedisConnection(IConfiguration configuration)
        => !string.IsNullOrWhiteSpace(configuration.GetConnectionString(AppHostProjects.RedisCache));
}
