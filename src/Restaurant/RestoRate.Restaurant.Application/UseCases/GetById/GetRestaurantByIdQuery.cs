using Ardalis.Result;
using Mediator;
using RestoRate.Restaurant.Application.DTOs;

namespace RestoRate.Restaurant.Application.UseCases.GetById;

public record GetRestaurantByIdQuery(int RestaurantId) : IQuery<Result<RestaurantDto>>;
