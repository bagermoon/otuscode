using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Mediation.Behaviors;

namespace RestoRate.ModerationService.Application.Configurations;

internal static class MediatorConfigs
{
    public static IServiceCollection AddMediatorConfigs(this IServiceCollection services)
    {
        // Root behaviors from registry; ModuleInitializer contributors have already populated it.
        services.AddMediator(options =>
        {
            options.Namespace = "RestoRate.ModerationService.Application";
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Assemblies = [
                typeof(ApplicationServiceExtensions)
            ];
            options.PipelineBehaviors = [
                typeof(RequestLoggingBehavior<,>)
            ];
        });

        return services;
    }
}
