using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.UseCases.Restaurants.Delete;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class DeleteRestaurantEndpoint
{
    public static RouteHandlerBuilder MapDeleteRestaurant(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:Guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteRestaurantCommand(id), ct);
            return result.Status switch
            {
                ResultStatus.Ok => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("DeleteRestaurant")
        .WithSummary("Удалить ресторан")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
