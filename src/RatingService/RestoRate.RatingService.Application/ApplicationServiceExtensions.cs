using Microsoft.Extensions.DependencyInjection;

using RestoRate.RatingService.Application.Configurations;

namespace RestoRate.RatingService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddRatingApplication(this IServiceCollection services)
    {
        services.AddMediatorConfigs();

        return services;
    }
}
