using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Rating.Dtos;

namespace RestoRate.RatingService.Application.UseCases.Ratings.GetById;

public record GetRatingByIdQuery(Guid Id) : IQuery<Result<RestaurantRatingDto>>;
