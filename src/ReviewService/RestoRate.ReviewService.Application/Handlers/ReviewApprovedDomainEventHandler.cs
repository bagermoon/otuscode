using Ardalis.SharedKernel;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewApprovedDomainEventHandler(
    IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<ReviewApprovedDomainEvent>
{
    public async ValueTask Handle(ReviewApprovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;

        var integrationEvent = new ReviewApprovedEvent(ReviewId: review.Id);
        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
