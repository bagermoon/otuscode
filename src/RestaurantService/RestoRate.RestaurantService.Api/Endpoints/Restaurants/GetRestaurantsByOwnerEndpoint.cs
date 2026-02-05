using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetByOwner;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class GetRestaurantsByOwnerEndpoint
{
    public static RouteHandlerBuilder MapGetRestaurantsByOwner(this RouteGroupBuilder group)
    {
        return group.MapGet("/owner/{ownerId:Guid}", async (Guid ownerId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRestaurantByOwnerQuery(ownerId), ct);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            return Results.NotFound();
        })
        .WithName("GetRestaurantByOwner")
        .WithSummary("Получить рестораны по OwnerId")
        .Produces<List<RestaurantDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
