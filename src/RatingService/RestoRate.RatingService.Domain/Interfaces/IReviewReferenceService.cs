using NodaMoney;

namespace RestoRate.RatingService.Domain.Interfaces;

public interface IReviewReferenceService
{
    Task AddAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default);

    Task ApproveAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default);

    Task RejectAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default);
}
