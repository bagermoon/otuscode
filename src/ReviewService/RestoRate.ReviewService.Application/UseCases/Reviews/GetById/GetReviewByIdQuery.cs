using Ardalis.Result;

using Mediator;

using RestoRate.ReviewService.Application.DTOs;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.GetById;

public record GetReviewByIdQuery(Guid Id) : IQuery<Result<ReviewDto>>;
