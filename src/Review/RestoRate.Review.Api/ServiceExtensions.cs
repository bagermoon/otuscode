using MassTransit;

using RestoRate.BuildingBlocks.Messaging;
using RestoRate.Review.Api.Configurations;
using RestoRate.ServiceDefaults;

namespace Microsoft.Extensions.Hosting;

internal static class HostBuilderExtensions
{
    public static IHostApplicationBuilder AddRestaurantApi(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
    
        builder.ConfigureAuthentication();

        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ, configs =>
        {
            configs.AddConsumers(typeof(HostBuilderExtensions).Assembly);
        });

        return builder;
    }
}
