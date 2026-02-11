using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Review.Dtos;

namespace RestoRate.ReviewService.Application.UseCases.Reviews.Create;

public record CreateReviewCommand(CreateReviewDto Dto) : ICommand<Result<ReviewDto>>;
