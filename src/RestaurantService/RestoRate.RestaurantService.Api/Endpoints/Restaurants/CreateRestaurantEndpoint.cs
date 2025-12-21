using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class CreateRestaurantEndpoint
{
    public static RouteGroupBuilder MapCreateRestaurant(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateRestaurantDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateRestaurantCommand(dto), ct);
            return result.Status switch
            {
                ResultStatus.Ok => Results.CreatedAtRoute("GetRestaurantById", new { id = result.Value.RestaurantId }, result.Value),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                ResultStatus.NotFound => Results.NotFound(),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("CreateRestaurant")
        .WithSummary("Создать ресторан")
        .WithDescription("Создаёт новый ресторан агрегат")
        .Produces<RestaurantDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}
