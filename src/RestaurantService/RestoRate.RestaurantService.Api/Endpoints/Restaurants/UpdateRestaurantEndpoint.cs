using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Update;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class UpdateRestaurantEndpoint
{
    public static RouteGroupBuilder MapUpdateRestaurant(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:Guid}", async (Guid id, UpdateRestaurantDto body, ISender sender, CancellationToken ct) =>
        {
            var dto = body with { RestaurantId = id };
            var result = await sender.Send(new UpdateRestaurantCommand(dto), ct);

            if (result.IsSuccess)
                return Results.NoContent();

            if (result.Status == ResultStatus.Invalid)
                return Results.BadRequest(result.ValidationErrors);

            if (result.Status == ResultStatus.NotFound)
                return Results.NotFound(result.Errors);

            return Results.BadRequest(result.Errors);
        })
        .WithName("UpdateRestaurant")
        .WithSummary("Обновить ресторан")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
