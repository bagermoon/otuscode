using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Restaurant.Application.Configurations;
using RestoRate.Restaurant.Application.UseCases.Create;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.Services;

namespace RestoRate.Restaurant.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddRestaurantApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateRestaurantValidator>();
        services.AddMediatorConfigs();

        services.AddScoped<IRestaurantService, RestaurantService>();
        return services;
    }
}
