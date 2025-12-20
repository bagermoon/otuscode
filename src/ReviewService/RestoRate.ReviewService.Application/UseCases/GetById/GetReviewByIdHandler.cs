using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.ReviewService.Application.DTOs;

using ReviewEntity = RestoRate.ReviewService.Domain.ReviewAggregate.Review;

namespace RestoRate.ReviewService.Application.UseCases.GetById;

public sealed class GetReviewByIdHandler(
    IRepository<ReviewEntity> repository,
    ILogger<GetReviewByIdHandler> logger)
    : IQueryHandler<GetReviewByIdQuery, Result<ReviewDto>>
{
    public async ValueTask<Result<ReviewDto>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Получение отзыва по Id: {ReviewId}", request.Id);
        var result = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (result is null)
        {
            logger.LogWarning("Отзыв не найден: {ReviewId}", request.Id);
            return Result<ReviewDto>.NotFound();
        }
        var dto = new ReviewDto(
            result.Id,
            result.RestaurantId,
            result.UserId,
            result.Rating,
            result.Comment ?? string.Empty,
            result.CreatedAt,
            result.UpdatedAt);
        return Result<ReviewDto>.Success(dto);
    }
}
