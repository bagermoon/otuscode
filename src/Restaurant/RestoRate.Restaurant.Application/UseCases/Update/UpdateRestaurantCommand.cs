using Ardalis.Result;
using MediatR;
using RestoRate.Restaurant.Application.DTOs;

namespace RestoRate.Restaurant.Application.UseCases.Update;

public record UpdateRestaurantCommand(UpdateRestaurantDto Dto) : IRequest<Result<UpdateRestaurantDto>>, IRequest<Result>;
