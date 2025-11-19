using Ardalis.SharedKernel;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Mediation.Behaviors;

namespace RestoRate.Review.Application.Configurations;

internal static class MediatorConfigs
{
    public static IServiceCollection AddMediatorConfigs(this IServiceCollection services)
    {
        // Root behaviors from registry; ModuleInitializer contributors have already populated it.
        services.AddMediator(options =>
        {
            options.Namespace = "RestoRate.Review.Application";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [
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
