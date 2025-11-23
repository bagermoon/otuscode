using Ardalis.Result;

using Mediator;

using RestoRate.Restaurant.Application.UseCases.Delete;

namespace RestoRate.Restaurant.Api.Endpoints.Restaurants;

internal static class DeleteRestaurantEndpoint
{
    public static RouteGroupBuilder MapDeleteRestaurant(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:Guid}", async (Guid id, ISender sender, CancellationToken ct) =>
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

        return group;
    }
}
