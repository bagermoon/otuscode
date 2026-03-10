using RestoRate.ModerationService.Infrastructure;
using RestoRate.ModerationService.Application;
using RestoRate.ModerationService.Api.Configurations;

namespace Microsoft.Extensions.Hosting;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddModerationApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();
        builder.Services.AddModerationApplication();
        builder.AddModerationInfrastructure(
            consumersAssembly: typeof(ApiServiceExtensions).Assembly
        );

        return builder;
    }
}
