using FluentValidation;

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RestoRate.Restaurant.Application.UseCases.Create;
using RestoRate.Restaurant.Application.Validators;

namespace RestoRate.Restaurant.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateRestaurantHandler).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateRestaurantValidator>();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
