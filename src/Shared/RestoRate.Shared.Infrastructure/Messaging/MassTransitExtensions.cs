using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RestoRate.Shared.Application.Messaging;
using RestoRate.Common;

namespace RestoRate.Shared.Infrastructure.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitEventBus(
        this IServiceCollection services,
        IConfiguration config,
        Action<IBusRegistrationConfigurator>? addConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            addConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(config.GetConnectionString(AppHostProjects.RabbitMQ)!));

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IIntegrationEventBus, MassTransitEventBus>();
        return services;
    }
}
