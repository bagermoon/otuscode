using System.Reflection;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestoRate.ModerationService.Domain.Abstractions;
using RestoRate.ModerationService.Infrastructure.Configurations;

using RestoRate.BuildingBlocks.Messaging;
using RestoRate.ServiceDefaults;

namespace RestoRate.ModerationService.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IHostApplicationBuilder AddModerationInfrastructure(
        this IHostApplicationBuilder builder,
        Assembly? consumersAssembly = null)
    {
        builder.Services.Configure<ModerationSettingsOptions>(
            builder.Configuration.GetSection("ModerationSettings")
        );
        builder.Services.AddSingleton<IBadWordsDictionary, OptionsBadWordsDictionary>();
        builder.AddMassTransitEventBus(AppHostProjects.RabbitMQ, configs =>
        {
            configs.AddConsumers(consumersAssembly);
        });

        return builder;
    }
}
