using Ardalis.Result;
using MediatR;

namespace RestoRate.Restaurant.Application.UseCases.Delete;

public record DeleteRestaurantCommand(int RestaurantId) : IRequest<Result>;
