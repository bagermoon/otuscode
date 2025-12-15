using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.RestaurantService.Application.Configurations;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;
using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.Services;

namespace RestoRate.RestaurantService.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddRestaurantApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateRestaurantValidator>();
        services.AddMediatorConfigs();

        services.AddScoped<IRestaurantService, RestaurantSvc>();
        return services;
    }
}
