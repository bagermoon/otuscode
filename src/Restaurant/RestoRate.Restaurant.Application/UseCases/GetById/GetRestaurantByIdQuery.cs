using Ardalis.Result;
using MediatR;
using RestoRate.Restaurant.Application.DTOs;

namespace RestoRate.Restaurant.Application.UseCases.GetById;

public record GetRestaurantByIdQuery(int RestaurantId) : IRequest<Result<RestaurantDto>>;
