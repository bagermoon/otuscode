using Mediator;

using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Moderation;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class ModerateRestaurantEndpoint
{
    public static RouteHandlerBuilder MapModerateRestaurant(this RouteGroupBuilder group)
    {
        return group.MapPatch("/moderate/{id:Guid}", async (Guid id, ModerationRestaurantDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ModerationRestaurantCommand(
                RestaurantId: id,
                Status: dto.Status,
                Reason: dto.Reason ?? string.Empty
            ), ct);
            return result;
        })
        .WithName("ModerateRestaurant")
        .WithSummary("Модерировать ресторан")
        .WithDescription("Обновляет статус ресторана")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}
