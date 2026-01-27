using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Common;
using RestoRate.Contracts.Restaurant.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

public sealed record GetAllRestaurantsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? CuisineType = null,
    string? Tag = null
) : IQuery<Result<Contracts.Common.PagedResult<RestaurantDto>>>;
