using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewModerationPendingDomainEventHandler(
    IIntegrationEventBus integrationEventBus,
    ILogger<ReviewModerationPendingDomainEventHandler> logger)
    : IDomainEventHandler<ReviewModerationPendingDomainEvent>
{
    public async ValueTask Handle(ReviewModerationPendingDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;

        var integrationEvent = new ReviewAddedEvent(
            ReviewId: review.Id,
            RestaurantId: review.RestaurantId,
            AuthorId: review.UserId,
            Rating: review.Rating,
            AverageCheck: null,
            Comment: review.Comment,
            Tags: Array.Empty<string>());

        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
