using Ardalis.SharedKernel;

namespace RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

public sealed class ReviewAddedDomainEvent(ReviewReference reviewReference) : DomainEventBase
{
    public ReviewReference ReviewReference { get; } = reviewReference;

    public Guid ReviewId => ReviewReference.Id;
    public Guid RestaurantId => ReviewReference.RestaurantId;
}
