using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;

using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewRejectedDomainEventHandler(
    ILogger<ReviewRejectedDomainEventHandler> logger)
    : IDomainEventHandler<ReviewRejectedDomainEvent>
{
    public ValueTask Handle(ReviewRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;
        logger.LogReviewRejected(review.Id, review.RestaurantId, review.UserId);

        return ValueTask.CompletedTask;
    }
}
