using System.Reflection;

using MassTransit;

using Microsoft.Extensions.Hosting;

using RestoRate.BuildingBlocks.Messaging;
using RestoRate.ServiceDefaults;

namespace RestoRate.ModerationService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddModerationInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddMassTransitEventBus(
            AppHostProjects.RabbitMQ,
            configs =>
            {
                configs.AddConsumers(Assembly.GetExecutingAssembly());
            }
        );

        return builder;
    }
}
