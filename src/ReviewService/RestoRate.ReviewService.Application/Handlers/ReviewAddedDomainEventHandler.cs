using System;

using Ardalis.SharedKernel;

using Microsoft.Extensions.Logging;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;
using RestoRate.Abstractions.Identity;

namespace RestoRate.ReviewService.Application.Handlers;

public class ReviewAddedDomainEventHandler(
    IIntegrationEventBus integrationEventBus,
    IUserContext userContext,
    ILogger<ReviewAddedDomainEventHandler> logger) : IDomainEventHandler<ReviewAddedDomainEvent>
{
    public async ValueTask Handle(ReviewAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogReviewAdded(notification.Review.Id, notification.Review.RestaurantId);
        var integrationEvent = new ReviewAddedEvent(
            ReviewId: notification.Review.Id,
            RestaurantId: notification.Review.RestaurantId,
            AuthorId: userContext.UserId,
            Rating: notification.Review.Rating,
            Text: notification.Review.Comment ?? string.Empty,
            Tags: Array.Empty<string>());

        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
