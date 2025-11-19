using System.Reflection;

using Ardalis.SharedKernel;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Messaging;
using RestoRate.Review.Infrastructure.Data;
using RestoRate.Review.Infrastructure.Repositories;
using RestoRate.ServiceDefaults;
using ReviewEntity = RestoRate.Review.Domain.ReviewAggregate.Review;

namespace RestoRate.Review.Infrastructure;

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
