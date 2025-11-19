using Ardalis.Result;

using Mediator;

using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.UseCases.Update;

namespace RestoRate.Restaurant.Api.Endpoints.Restaurants;

internal static class UpdateRestaurantEndpoint
{
    public static RouteGroupBuilder MapUpdateRestaurant(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:int}", async (int id, UpdateRestaurantDto body, ISender sender, CancellationToken ct) =>
        {
            // Ensure path ID is authoritative
            var dto = body with { RestaurantId = id };
            var result = await sender.Send(new UpdateRestaurantCommand(dto), ct);
            return result.Status switch
            {
                ResultStatus.Ok => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("UpdateRestaurant")
        .WithSummary("Обновить ресторан")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
