using Ardalis.SharedKernel;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Mediation.Behaviors;
using RestoRate.Restaurant.Domain.Interfaces;

namespace RestoRate.Restaurant.Application.Configurations;

internal static class MediatorConfigs
{
    public static IServiceCollection AddMediatorConfigs(this IServiceCollection services)
    {
        // Root behaviors from registry; ModuleInitializer contributors have already populated it.
        services.AddMediator(options =>
        {
            options.Namespace = "RestoRate.Restaurant.Application";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [
                typeof(IRestaurantService),
                typeof(ApplicationServiceExtensions)
            ];
            options.PipelineBehaviors = [
                typeof(RequestLoggingBehavior<,>),
                typeof(ValidationBehaviour<,>)
            ];
        });

        services
            .AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        return services;
    }
}