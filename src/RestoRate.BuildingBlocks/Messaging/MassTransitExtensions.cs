using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.Abstractions.Messaging;

namespace RestoRate.BuildingBlocks.Messaging;

public static class MassTransitExtensions
{
    public static IHostApplicationBuilder AddMassTransitEventBus(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        builder.Services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: true));

            configure?.Invoke(x);
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(builder.Configuration.GetConnectionString(connectionName)!));
                cfg.UseMessageRetry(r => r.Intervals(500, 1000));
                cfg.UseInMemoryOutbox(context);

                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services.AddScoped<IIntegrationEventBus, MassTransitEventBus>();
        return builder;
    }
}
