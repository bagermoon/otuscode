using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Domain.Services;

public sealed class ReviewReferenceService(IReviewReferenceRepository repository) : IReviewReferenceService
{
    public async Task AddAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        await repository.TryAddAsync(
            reviewId,
            restaurantId,
            rating,
            averageCheck,
            cancellationToken);
    }

    public async Task ApproveAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        await repository.TryApproveAsync(
            reviewId,
            restaurantId,
            rating,
            averageCheck,
            cancellationToken);
    }

    public async Task RejectAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        await repository.TryRejectAsync(
            reviewId,
            restaurantId,
            rating,
            averageCheck,
            cancellationToken);
    }
}
