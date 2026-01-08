using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class CreateRestaurantEndpoint
{
    public static RouteHandlerBuilder MapCreateRestaurant(this RouteGroupBuilder group)
    {
        return group.MapPost("/", async (CreateRestaurantDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateRestaurantCommand(dto), ct);
            return result;
        })
        .WithName("CreateRestaurant")
        .WithSummary("Создать ресторан")
        .WithDescription("Создаёт новый ресторан агрегат")
        .Produces<RestaurantDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
