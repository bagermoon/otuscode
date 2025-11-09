using Ardalis.SharedKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.Common;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.Services;
using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.Restaurant.Infrastructure.Repositories;
using RestoRate.BuildingBlocks.Data;

namespace RestoRate.Restaurant.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantInfrastructure(
        this IHostApplicationBuilder builder
    )
    {
        builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);

        builder.Services
            .AddScoped<IRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantRepository>()
            .AddScoped<IReadRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantReadRepository>()

            .AddScoped<ICreateRestaurantService, CreateRestaurantService>()
            .AddScoped<IUpdateRestaurantService, UpdateRestaurantService>()
            .AddScoped<IDeleteRestaurantService, DeleteRestaurantService>()
        ;

        return builder;
    }
}
