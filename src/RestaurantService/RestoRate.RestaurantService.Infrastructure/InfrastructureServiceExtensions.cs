using Ardalis.SharedKernel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.ServiceDefaults;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.RestaurantService.Infrastructure.Repositories;
using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Messaging;

using MassTransit;

using System.Reflection;

namespace RestoRate.RestaurantService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantInfrastructure(
        this IHostApplicationBuilder builder,
        Assembly? consumersAssembly = null
    )
    {
        consumersAssembly ??= Assembly.GetCallingAssembly();
        builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);
        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ, configs =>
        {
            configs.AddConsumers(consumersAssembly);
        });

        builder.Services
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
        ;

        return builder;
    }
}
