using Ardalis.Result;
using Mediator;

using RestoRate.Review.Application.DTOs;

namespace RestoRate.Review.Application.UseCases.GetById;

public record GetReviewByIdQuery(Guid Id) : IQuery<Result<ReviewDto>>;
