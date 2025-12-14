using Ardalis.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.ServiceDefaults;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.RestaurantService.Infrastructure.Repositories;
using RestoRate.BuildingBlocks.Data;
using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;
using RestoRate.BuildingBlocks.Messaging;
using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.RestaurantService.Domain.TagAggregate;

namespace RestoRate.RestaurantService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantInfrastructure(
        this IHostApplicationBuilder builder
    )
    {
        builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);
        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ);

        builder.Services
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>))
        ;

        return builder;
    }
}
