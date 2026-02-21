using Ardalis.SharedKernel;

using NodaMoney;

using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.Domain.ReviewReferenceAggregate;

public class ReviewReference : EntityBase<Guid>, IAggregateRoot
{
    public Guid RestaurantId { get; private set; }
    public decimal Rating { get; private set; }
    public Money? AverageCheck { get; private set; }
    public bool IsApproved { get; private set; }

    public static ReviewReference Create(
        Guid id,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck)
    {
        var review = new ReviewReference
        {
            Id = id,
            RestaurantId = restaurantId,
            Rating = rating,
            AverageCheck = averageCheck,
            IsApproved = false
        };

        review.RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(review));

        return review;
    }

    public ReviewReference Reject()
    {
        IsApproved = false;

        RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(this));
        return this;
    }

    public ReviewReference Approve()
    {
        if (IsApproved)
        {
            return this;
        }

        IsApproved = true;

        RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(this));
        return this;
    }
}
