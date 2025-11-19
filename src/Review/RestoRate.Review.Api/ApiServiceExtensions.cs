using MassTransit;

using RestoRate.BuildingBlocks.Messaging;
using RestoRate.Review.Api.Configurations;
using RestoRate.Review.Infrastructure;
using RestoRate.Review.Application;
using RestoRate.ServiceDefaults;

namespace Microsoft.Extensions.Hosting;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddReviewApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();
        builder.Services.AddReviewApplication();
        builder.AddReviewInfrastructure(
            consumersAssembly: typeof(ApiServiceExtensions).Assembly
        );

        return builder;
    }
}
