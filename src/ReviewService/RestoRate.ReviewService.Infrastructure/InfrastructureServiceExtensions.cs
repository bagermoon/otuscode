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

        MoneyBsonSerializer.EnsureRegistered();

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

                // Для работы MongoDbSagaOutbox требуется MongoDb в режиме ReplicaSet. 
                // Этот режим не поддерживается в Aspire на момент разработки.
                var useMongoDbSagaOutbox = builder.Configuration.GetValue("MassTransit:UseMongoDbSagaOutbox", false);

                if (useMongoDbSagaOutbox
                    && reviewMongoUrl is not null
                    && !string.IsNullOrWhiteSpace(reviewMongoUrl.DatabaseName))
                {
                    configs.SetMongoDbSagaOutbox(reviewMongoUrl);
                }
                else
                {
                    configs.SetInMemorySagaOutbox();
                }
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
