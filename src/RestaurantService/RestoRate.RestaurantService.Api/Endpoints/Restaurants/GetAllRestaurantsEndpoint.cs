using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class GetAllRestaurantsEndpoint
{
    public static RouteGroupBuilder MapGetAllRestaurants(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
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

            return result.Status switch
            {
                ResultStatus.Ok => Results.Ok(result.Value),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("GetAllRestaurants")
        .WithSummary("Получить список ресторанов")
        .WithDescription("Получает список ресторанов с пагинацией и фильтрацией")
        .Produces<Application.UseCases.Restaurants.GetAll.PagedResult<RestaurantDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
