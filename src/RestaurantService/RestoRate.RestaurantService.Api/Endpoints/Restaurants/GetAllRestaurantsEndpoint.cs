using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class GetAllRestaurantsEndpoint
{
    public static RouteHandlerBuilder MapGetAllRestaurants(this RouteGroupBuilder group)
    {
        return group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 20,
            string? searchTerm = null,
            string? cuisineType = null,
            string? tag = null) =>
        {
            var query = new GetAllRestaurantsQuery(
                pageNumber,
                pageSize,
                searchTerm,
                cuisineType,
                tag);

            var result = await sender.Send(query, ct);

            return result;
        })
        .WithName("GetAllRestaurants")
        .AllowAnonymous()
        .WithSummary("Получить список ресторанов")
        .WithDescription("Получает список ресторанов с пагинацией и фильтрацией")
        .Produces<Contracts.Common.PagedResult<RestaurantDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
