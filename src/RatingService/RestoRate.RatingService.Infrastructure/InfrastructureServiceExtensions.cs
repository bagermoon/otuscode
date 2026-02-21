using System.Reflection;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using RestoRate.BuildingBlocks.Messaging;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Infrastructure.Caching;
using RestoRate.RatingService.Infrastructure.Data;
using RestoRate.RatingService.Infrastructure.Extensions;
using RestoRate.RatingService.Infrastructure.Repositories;
using RestoRate.ServiceDefaults;

namespace RestoRate.RatingService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddRatingInfrastructure(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies
    )
    {
        AddMassTransitEventBus(builder, assemblies);
        AddMongoDbServices(builder);
        AddRedis(builder);

        builder.Services.TryAddScoped<IReviewReferenceRepository, ReviewReferenceRepository>();
        builder.Services.TryAddScoped<IRestaurantRatingCache, RedisRestaurantRatingCache>();

        return builder;
    }

    private static IHostApplicationBuilder AddMongoDbServices(
        this IHostApplicationBuilder builder)
    {
        builder.AddMongoDBClient(AppHostProjects.RatingDb);
        string databaseName = builder.Configuration.GetValue<string>("MONGODB_DATABASENAME") ?? "ratingdb";

        builder.Services.TryAddSingleton<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        builder.Services.AddMongoContext<MongoContext>();
        builder.Services.AddConfiguredMongoDbCollections(Assembly.GetExecutingAssembly());

        return builder;
    }

    private static IHostApplicationBuilder AddMassTransitEventBus(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        builder.AddMassTransitEventBus(
            AppHostProjects.RabbitMQ,
            configs =>
            {
                foreach (var assembly in assemblies.Distinct().ToArray())
                {
                    configs.AddConsumers(assembly);
                }

                configs.SetInMemorySagaOutbox();
            }
        );

        return builder;
    }

    private static IHostApplicationBuilder AddRedis(
        this IHostApplicationBuilder builder)
    {
        builder.AddRedisClient(AppHostProjects.RedisCache, configureOptions: options =>
        {
            options.DefaultDatabase = 1; // Move to configuration in the future
        });

        return builder;
    }
}
