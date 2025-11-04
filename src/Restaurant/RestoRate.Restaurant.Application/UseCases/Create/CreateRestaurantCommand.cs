using Ardalis.Result;
using MediatR;
using RestoRate.Restaurant.Application.DTOs;

namespace RestoRate.Restaurant.Application.UseCases.Create;

public record CreateRestaurantCommand(CreateRestaurantDto Dto) : IRequest<Result>, Mediator.IRequest<Result<RestaurantDto>>;
