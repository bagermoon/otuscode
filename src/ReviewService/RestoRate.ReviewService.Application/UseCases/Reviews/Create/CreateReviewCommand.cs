using Ardalis.Result;

using Mediator;

using RestoRate.ReviewService.Application.DTOs;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Create;

public record CreateReviewCommand(CreateReviewDto Dto) : ICommand<Result<ReviewDto>>;
