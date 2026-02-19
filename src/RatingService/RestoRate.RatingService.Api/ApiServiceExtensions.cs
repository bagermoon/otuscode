using System;

using RestoRate.RatingService.Api.Configurations;
using RestoRate.RatingService.Infrastructure;
using RestoRate.RatingService.Application;
namespace RestoRate.RatingService.Api;

internal static class ApiServiceExtensions
{
    public static IHostApplicationBuilder AddRatingApi(this IHostApplicationBuilder builder)
    {
        builder.ConfigureAuthentication();
        builder.Services.AddRatingApplication();
        builder.AddRatingInfrastructure(
            typeof(ApiServiceExtensions).Assembly
        );

        return builder;
    }
}
