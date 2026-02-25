using Ardalis.SharedKernel;

using MassTransit;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewCreatedDomainEventHandler(
    IPublishEndpoint publishEndpoint)
    : IDomainEventHandler<ReviewCreatedDomainEvent>
{
    public async ValueTask Handle(ReviewCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var review = notification.Review;

        await publishEndpoint.Publish(
            new ReviewValidationRequested(
                RestaurantId: review.RestaurantId,
                ReviewId: review.Id,
                UserId: review.UserId),
            publishContext => publishContext.CorrelationId = review.Id,
            cancellationToken);
    }
}
