using Ardalis.SharedKernel;

using MassTransit;

using Microsoft.Extensions.Logging;

using RestoRate.ReviewService.Application.Sagas.Messages;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewCreatedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<ReviewCreatedDomainEventHandler> logger)
    : IDomainEventHandler<ReviewCreatedDomainEvent>
{
    public async ValueTask Handle(ReviewCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;
        logger.LogReviewAdded(notification.Review.Id, notification.Review.RestaurantId);
        await publishEndpoint.Publish(
            new ReviewValidationRequested(
                RestaurantId: review.RestaurantId,
                ReviewId: review.Id,
                UserId: review.UserId),
            publishContext => publishContext.CorrelationId = review.Id,
            cancellationToken);
    }
}
