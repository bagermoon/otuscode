using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class GetRestaurantByIdEndpoint
{
    public static RouteHandlerBuilder MapGetRestaurantById(this RouteGroupBuilder group)
    {
        return group.MapGet("/{id:Guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRestaurantByIdQuery(id), ct);
            return result;
        })
        .WithName("GetRestaurantById")
        .WithSummary("Получить ресторан по ID")
        .Produces<RestaurantDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
