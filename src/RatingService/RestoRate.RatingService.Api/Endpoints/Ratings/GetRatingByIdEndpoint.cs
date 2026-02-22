using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Rating.Dtos;
using RestoRate.RatingService.Application.UseCases.Ratings.GetById;

namespace RestoRate.RatingService.Api.Endpoints.Ratings;

internal static class GetRatingByIdEndpoint
{
    // Регистрирует endpoint для получения рейтинга по Id
    public static RouteGroupBuilder MapGetRatingById(this RouteGroupBuilder group)
    {
        group.MapGet("/{restaurantId:guid}", async (Guid restaurantId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRatingByIdQuery(restaurantId), ct);
            return result.Status switch
            {
                ResultStatus.Ok => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("GetRestaurantRating")
        .WithSummary("Получить рейтинг по ID")
        .Produces<RestaurantRatingDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
