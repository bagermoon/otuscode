using RestoRate.Restaurant.Infrastructure;
using RestoRate.Restaurant.Application;
using RestoRate.Restaurant.Api.Configurations;

namespace Microsoft.Extensions.Hosting;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRestaurantApi(this IHostApplicationBuilder builder)
    {    
        builder.ConfigureAuthentication();
        builder.Services.AddRestaurantApplication();
        builder.AddRestaurantInfrastructure();

        return builder;
    }
}
