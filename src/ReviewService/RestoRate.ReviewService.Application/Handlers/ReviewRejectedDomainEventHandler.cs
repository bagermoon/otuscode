using Ardalis.SharedKernel;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewRejectedDomainEventHandler(
    IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<ReviewRejectedDomainEvent>
{
    public async ValueTask Handle(ReviewRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;

        var integrationEvent = new ReviewRejectedEvent(
            ReviewId: review.Id,
            RestaurantId: review.RestaurantId,
            AuthorId: review.UserId,
            Rating: review.Rating,
            AverageCheck: review.AverageCheck?.ToDto(),
            Comment: review.Comment,
            Tags: Array.Empty<string>());
        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
