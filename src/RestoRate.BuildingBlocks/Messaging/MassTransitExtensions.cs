using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.Abstractions.Identity;
using RestoRate.Abstractions.Messaging;
using RestoRate.BuildingBlocks.Messaging.Identity;

namespace RestoRate.BuildingBlocks.Messaging;

public static class MassTransitExtensions
{
    public static IHostApplicationBuilder AddMassTransitEventBus(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionName);

        builder.Services.AddScoped<IUserContextProvider, MassTransitUserContextProvider>();
        builder.Services.AddUserContext();

        builder.Services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: true));

            configure?.Invoke(x);

            if (string.IsNullOrEmpty(connectionString))
            {
                // // When generate OpenAPI docs locally
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                return;
            }
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.UseMessageRetry(r => r.Intervals(500, 1000));
                cfg.UseInMemoryOutbox(context);
                cfg.UseConsumeFilter(typeof(ConsumeUserContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services.AddScoped<IIntegrationEventBus, MassTransitEventBus>();
        return builder;
    }
}
