using Microsoft.Extensions.DependencyInjection;

using RestoRate.ReviewService.Application.Configurations;

namespace RestoRate.ReviewService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddReviewApplication(this IServiceCollection services)
    {
        services.AddMediatorConfigs();

        return services;
    }
}
