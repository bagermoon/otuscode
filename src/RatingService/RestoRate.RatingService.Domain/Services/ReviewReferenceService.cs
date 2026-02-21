using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;

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
        var existing = await repository.GetReviewReferenceByIdAsync(reviewId, cancellationToken);
        if (existing is not null)
        {
            // Idempotency for message retries: if the reference already exists, do not insert again.
            return;
        }

        var reviewReference = ReviewReference.Create(
            reviewId,
            restaurantId,
            rating,
            averageCheck);

        await repository.AddReviewReferenceAsync(reviewReference, cancellationToken);
    }

    public async Task ApproveAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetReviewReferenceByIdAsync(reviewId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Approve();
        await repository.UpdateReviewReferenceAsync(existing, cancellationToken);
    }

    public async Task RejectAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var existing = await repository.GetReviewReferenceByIdAsync(reviewId, cancellationToken);
        if (existing is null)
        {
            return;
        }

        existing.Reject();
        await repository.DeleteReviewReferenceAsync(reviewId, cancellationToken);
    }
}
