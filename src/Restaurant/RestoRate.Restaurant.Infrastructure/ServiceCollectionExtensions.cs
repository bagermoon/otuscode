using Ardalis.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.Services;
using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.Restaurant.Infrastructure.Repositories;

namespace RestoRate.Restaurant.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantRepository>();
        services.AddScoped<IReadRepository<Domain.RestaurantAggregate.Restaurant>, RestaurantReadRepository>();

        services.AddScoped<ICreateRestaurantService, CreateRestaurantService>();
        services.AddScoped<IUpdateRestaurantService, UpdateRestaurantService>();
        services.AddScoped<IDeleteRestaurantService, DeleteRestaurantService>();

        return services;
    }
}
