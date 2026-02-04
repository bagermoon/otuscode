using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

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

        builder.Services.AddTransient<IUserContextProvider, MassTransitUserContextProvider>();
        builder.Services.AddUserContext();

        builder.Services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(includeNamespace: true));

            configure?.Invoke(x);

            if (string.IsNullOrEmpty(connectionString))
            {
                // When generate OpenAPI docs locally
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseConsumeFilter(typeof(ConsumeUserContextFilter<>), context);
                    cfg.UsePublishFilter(typeof(PublishUserContextFilter<>), context);
                    cfg.UseSendFilter(typeof(SendUserContextFilter<>), context);
                    cfg.ConfigureEndpoints(context);
                });
                return;
            }

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.UseConsumeFilter(typeof(ConsumeUserContextFilter<>), context);
                cfg.UsePublishFilter(typeof(PublishUserContextFilter<>), context);
                cfg.UseSendFilter(typeof(SendUserContextFilter<>), context);
                cfg.ConfigureEndpoints(context);
            });
        });

        builder.Services.AddScoped<IIntegrationEventBus, MassTransitEventBus>();
        return builder;
    }

    public static IBusRegistrationConfigurator SetInMemorySagaOutbox(this IBusRegistrationConfigurator configurator)
    {
        configurator.SetInMemorySagaRepositoryProvider();
        configurator.AddConfigureEndpointsCallback((context, _, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Intervals(500, 1000));
            cfg.UseMessageScope(context);
            cfg.UseInMemoryOutbox(context);
        });
        return configurator;
    }

    public static IBusRegistrationConfigurator SetMongoDbSagaOutbox(
        this IBusRegistrationConfigurator configurator,
        MongoUrl mongoUrl)
    {
        ArgumentNullException.ThrowIfNull(mongoUrl, nameof(mongoUrl));

        var databaseName = mongoUrl.DatabaseName;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Mongo connection string must include a database name.", nameof(mongoUrl));
        }

        configurator.SetMongoDbSagaRepositoryProvider(mongoUrl.Url, cfg =>
        {
            cfg.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
            cfg.DatabaseFactory(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
        });

        configurator.AddMongoDbOutbox(cfg =>
        {
            cfg.QueryDelay = TimeSpan.FromSeconds(1);
            cfg.DuplicateDetectionWindow = TimeSpan.FromMinutes(10);

            cfg.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
            cfg.DatabaseFactory(provider => provider.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

            cfg.UseBusOutbox();
        });

        configurator.AddConfigureEndpointsCallback((context, _, cfg) =>
        {
            cfg.UseMessageRetry(r => r.Intervals(10, 50, 100, 1000, 1000, 1000, 1000, 1000));
            cfg.UseMessageScope(context);
            cfg.UseMongoDbOutbox(context);
        });

        return configurator;
    }
}
