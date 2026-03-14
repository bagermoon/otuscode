using System.Reflection;

using Ardalis.SharedKernel;

using MassTransit;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;

using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Messaging;
using RestoRate.BuildingBlocks.Serialization;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.ReviewService.Application.Configurations;
using RestoRate.ReviewService.Domain.Interfaces;
using RestoRate.ReviewService.Infrastructure.Data;
using RestoRate.ReviewService.Infrastructure.Repositories;
using RestoRate.ServiceDefaults;

namespace RestoRate.ReviewService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddReviewInfrastructure(
        this IHostApplicationBuilder builder,
        params Assembly[] assemblies
    )
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        builder.Services.AddOptions<RestaurantProjectionOptions>()
            .Bind(builder.Configuration.GetSection(RestaurantProjectionOptions.SectionName))
            .Validate(options => options.FreshnessTtl > TimeSpan.Zero, "Restaurant projection freshness TTL must be greater than zero.")
            .ValidateOnStart();

        builder.AddMongoDbContext<ReviewDbContext>(AppHostProjects.ReviewDb);

        var reviewDbConnectionString = builder.Configuration.GetConnectionString(AppHostProjects.ReviewDb);

        MongoUrl? reviewMongoUrl = null;
        if (!string.IsNullOrWhiteSpace(reviewDbConnectionString))
        {
            reviewMongoUrl = new MongoUrl(reviewDbConnectionString);
        }

        builder.AddMassTransitEventBus(
            AppHostProjects.RabbitMQ,
            configs =>
            {
                foreach (var assembly in assemblies.Distinct().ToArray())
                {
                    configs.AddConsumers(assembly);
                    configs.AddSagaStateMachines(assembly);
                }
                configs.AddRequestClient<GetRestaurantStatusRequest>();

                // В идеале перевести этот сервис MongoDb Driver вместо EF Core,
                // тогда можно будет включить настоящий Outbox, а не InMemory.
                // Так же это упростит серилизацию полей. Все в принципе готово в серивсе Rating,
                // но пока оставляем как есть.
                configs.SetInMemorySagaOutbox();
            }
        );

        builder.Services
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
            .AddScoped<IReviewRepository, ReviewRepository>()
        ;

        return builder;
    }
}
