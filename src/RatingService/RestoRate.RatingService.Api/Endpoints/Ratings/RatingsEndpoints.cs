using RestoRate.Auth.Authorization;

namespace RestoRate.RatingService.Api.Endpoints.Ratings;

public static class RatingsEndpoints
{
    // Регистрирует группу endpoints для работы с рейтингами
    public static RouteGroupBuilder MapRatingsEndpoints(this RouteGroupBuilder group)
    {
        var adminGroup = group
            .MapGroup("/")
            .RequireAuthorization(PolicyNames.RequireAdminRole);

        adminGroup.MapGetRatingById();

        return group;
    }
}
