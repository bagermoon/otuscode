using FluentValidation;

using Microsoft.Extensions.DependencyInjection;
using RestoRate.Restaurant.Application.Validators;

namespace RestoRate.Restaurant.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateRestaurantValidator>();

        return services;
    }
}
