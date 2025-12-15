using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

public sealed record GetAllRestaurantsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? CuisineType = null,
    string? Tag = null
) : IQuery<Result<PagedResult<RestaurantDto>>>;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
