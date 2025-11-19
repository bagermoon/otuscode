using Ardalis.Result;
using Mediator;
using RestoRate.Review.Application.DTOs;

namespace RestoRate.Review.Application.UseCases.Create;

public record CreateReviewCommand(CreateReviewDto Dto) : ICommand<Result<ReviewDto>>;
