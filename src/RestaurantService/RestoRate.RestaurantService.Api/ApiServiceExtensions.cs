using RestoRate.RestaurantService.Infrastructure;
using RestoRate.RestaurantService.Application;
using RestoRate.RestaurantService.Api.Configurations;

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
