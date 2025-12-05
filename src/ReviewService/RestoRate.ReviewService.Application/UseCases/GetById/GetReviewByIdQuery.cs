using Ardalis.Result;
using Mediator;

using RestoRate.ReviewService.Application.DTOs;

namespace RestoRate.ReviewService.Application.UseCases.GetById;

public record GetReviewByIdQuery(Guid Id) : IQuery<Result<ReviewDto>>;
