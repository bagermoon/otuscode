using Ardalis.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.ServiceDefaults;
using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.Restaurant.Infrastructure.Repositories;
using RestoRate.BuildingBlocks.Data;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;
using RestoRate.BuildingBlocks.Messaging;

namespace RestoRate.Restaurant.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantInfrastructure(
        this IHostApplicationBuilder builder
    )
    {
        builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);
        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ);

        builder.Services
            .AddScoped<IRepository<RestaurantEntity>, RestaurantRepository>()
            .AddScoped<IReadRepository<RestaurantEntity>, RestaurantReadRepository>()
        ;

        return builder;
    }
}
