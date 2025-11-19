using Microsoft.Extensions.DependencyInjection;

using RestoRate.Review.Application.Configurations;

namespace RestoRate.Review.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddReviewApplication(this IServiceCollection services)
    {
        services.AddMediatorConfigs();

        return services;
    }
}
