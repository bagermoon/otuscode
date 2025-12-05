using System.Reflection;

using Ardalis.SharedKernel;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Messaging;
using RestoRate.ReviewService.Infrastructure.Data;
using RestoRate.ReviewService.Infrastructure.Repositories;
using RestoRate.ServiceDefaults;

using ReviewEntity = RestoRate.ReviewService.Domain.ReviewAggregate.Review;

namespace RestoRate.ReviewService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddReviewInfrastructure(
        this IHostApplicationBuilder builder,
        Assembly? consumersAssembly = null
    )
    {
        consumersAssembly ??= Assembly.GetCallingAssembly();
        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ, configs =>
        {
            configs.AddConsumers(consumersAssembly);
        });

        builder.AddMongoDbContext<ReviewDbContext>(AppHostProjects.ReviewDb);

        builder.Services
            .AddScoped<IRepository<ReviewEntity>, ReviewRepository>()
        ;

        return builder;
    }
}
