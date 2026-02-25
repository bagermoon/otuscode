using Ardalis.SharedKernel;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewRejectedDomainEventHandler(
    IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<ReviewRejectedDomainEvent>
{
    public async ValueTask Handle(ReviewRejectedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;

        var integrationEvent = new ReviewRejectedEvent(ReviewId: review.Id);
        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
