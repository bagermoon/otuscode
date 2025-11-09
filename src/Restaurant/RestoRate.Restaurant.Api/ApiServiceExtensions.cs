using RestoRate.Common;
using RestoRate.Restaurant.Infrastructure;
using RestoRate.Restaurant.Application;
using RestoRate.Restaurant.Api.Configurations;

namespace RestoRate.Restaurant.Api;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantModule(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
    
        builder.ConfigureAuthentication();
        builder.Services.AddApplicationServices();
        builder.AddRestaurantInfrastructure();
        builder.AddRestaurantApi();

        return builder;
    }

    public static IHostApplicationBuilder AddRestaurantApi(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatorConfigs();

        return builder;
    }
}
