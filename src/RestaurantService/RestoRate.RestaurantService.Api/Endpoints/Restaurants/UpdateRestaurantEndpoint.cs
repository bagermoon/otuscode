using Ardalis.Result;
using Mediator;
using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Update;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class UpdateRestaurantEndpoint
{
    public static RouteGroupBuilder MapUpdateRestaurant(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", async (Guid id, UpdateRestaurantDto body, ISender sender, CancellationToken ct) =>
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
