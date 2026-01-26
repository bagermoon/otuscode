using Ardalis.SharedKernel;

using RestoRate.ReviewService.Domain.Events;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;
using RestoRate.ReviewService.Domain.UserReferenceAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.ReviewAggregate;

public class Review : EntityBase<Guid>, IAggregateRoot
{
    public Guid RestaurantId { get; private set; } = default!;
    public Guid UserId { get; private set; } = default!;
    public int Rating { get; private set; } = default!;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; } = default!;
    public DateTime? UpdatedAt { get; private set; }

    // Optionally, add moderation status, e.g.:
    public ReviewStatus Status { get; private set; }
    public RestaurantReference? Restaurant { get; private set; }
    public UserReference? User { get; private set; }

    private Review() { }

    public Review(Guid restaurantId, Guid userId, int rating, string comment)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
        Status = ReviewStatus.Pending;
    }

    public static Review Create(Guid restaurantId, Guid userId, int rating, string comment)
    {
        var review = new Review(restaurantId, userId, rating, comment);

        review.RegisterDomainEvent(new ReviewCreatedDomainEvent(review));
        return review;
    }

    public Review MoveToModerationPending()
    {
        if (Status == ReviewStatus.ModerationPending)
        {
            return this;
        }

        Status = ReviewStatus.ModerationPending;
        UpdatedAt = DateTime.UtcNow;
        RegisterDomainEvent(new ReviewModerationPendingDomainEvent(this));

        return this;
    }

    public Review Reject()
    {
        if (Status == ReviewStatus.Rejected)
        {
            return this;
        }

        Status = ReviewStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
        RegisterDomainEvent(new ReviewRejectedDomainEvent(this));

        return this;
    }

    public Review Approve()
    {
        if (Status == ReviewStatus.Approved)
        {
            return this;
        }

        Status = ReviewStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
        RegisterDomainEvent(new ReviewApprovedDomainEvent(this));

        return this;
    }

    public Review Update(string comment, int rating)
    {
        Comment = comment;
        Rating = rating;
        UpdatedAt = DateTime.UtcNow;
        return this;
    }
}
