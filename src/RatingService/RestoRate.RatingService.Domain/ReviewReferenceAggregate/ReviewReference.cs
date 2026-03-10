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
    public bool IsRejected { get; private set; }

    public static ReviewReference Create(
        Guid id,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck)
        => CreateInternal(id, restaurantId, rating, averageCheck, isApproved: false, isRejected: false);

    public static ReviewReference CreateApproved(
        Guid id,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck)
        => CreateInternal(id, restaurantId, rating, averageCheck, isApproved: true, isRejected: false);

    public static ReviewReference CreateRejected(
        Guid id,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck)
        => CreateInternal(id, restaurantId, rating, averageCheck, isApproved: false, isRejected: true);

    private static ReviewReference CreateInternal(
        Guid id,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        bool isApproved,
        bool isRejected)
    {
        var review = new ReviewReference
        {
            Id = id,
            RestaurantId = restaurantId,
            Rating = rating,
            AverageCheck = averageCheck,
            IsApproved = isApproved,
            IsRejected = isRejected
        };

        review.RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(review));

        return review;
    }

    public ReviewReference Reject()
    {
        if (IsRejected)
        {
            return this;
        }

        IsApproved = false;
        IsRejected = true;

        RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(this));
        return this;
    }

    public ReviewReference Approve()
    {
        if (IsApproved || IsRejected)
        {
            return this;
        }

        IsApproved = true;
        IsRejected = false;

        RegisterDomainEvent(new ReviewReferenceChangedDomainEvent(this));
        return this;
    }
}
