using Ardalis.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.Common;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.Services;
using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.Restaurant.Infrastructure.Repositories;
using RestoRate.BuildingBlocks.Migrations;

namespace RestoRate.Restaurant.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static TBuilder AddInfrastructureServices<TBuilder>(
        this TBuilder builder
    ) where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<RestaurantDbContext>(
            AppHostProjects.RestaurnatDb,
            null,
            optionsBuilder => {
                optionsBuilder.UseSnakeCaseNamingConvention();
            }
        );

        builder.Services.AddMigration<RestaurantDbContext>();

        builder.Services.AddScoped<IRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantRepository>();
        builder.Services.AddScoped<IReadRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantReadRepository>();

        builder.Services.AddScoped<ICreateRestaurantService, CreateRestaurantService>();
        builder.Services.AddScoped<IUpdateRestaurantService, UpdateRestaurantService>();
        builder.Services.AddScoped<IDeleteRestaurantService, DeleteRestaurantService>();

        return builder;
    }
}
