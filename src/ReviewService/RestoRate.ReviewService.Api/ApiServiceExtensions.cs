using RestoRate.ReviewService.Api.Configurations;
using RestoRate.ReviewService.Infrastructure;
using RestoRate.ReviewService.Application;

namespace Microsoft.Extensions.Hosting;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddReviewApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();
        builder.Services.AddReviewApplication();
        builder.AddReviewInfrastructure(
            typeof(ApiServiceExtensions).Assembly,
            typeof(ApplicationServiceExtensions).Assembly
        );

        return builder;
    }
}
