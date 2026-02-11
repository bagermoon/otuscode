using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Review.Dtos;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetById;

public record GetReviewByIdQuery(Guid Id) : IQuery<Result<ReviewDto>>;
