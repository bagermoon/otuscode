using System;
using System.Reflection;

using Ardalis.SharedKernel;

using Mediator;

using RestoRate.Restaurant.Application.Validators;
using RestoRate.SharedKernel.Mediator;

using RestaurantAggregate = RestoRate.Restaurant.Domain.RestaurantAggregate;

namespace RestoRate.Restaurant.Api.Configurations;

internal static class MediatorConfigs
{
    public static IServiceCollection AddMediatorConfigs(this IServiceCollection services)
    {
        AssemblyReference[] mediatorAssemblies = [
            Assembly.GetAssembly(typeof(RestaurantAggregate.Restaurant)), // Core
            Assembly.GetAssembly(typeof(CreateRestaurantValidator)) // UseCases
        ];

        // Root behaviors from registry; ModuleInitializer contributors have already populated it.
        services.AddMediator(options =>
        {
            options.Assemblies = [.. mediatorAssemblies];
            options.PipelineBehaviors = MediatorPipelineRegistry.GetAll();
        });

        services
            .AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        return services;
    }
}
