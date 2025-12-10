using System;
using Ardalis.SharedKernel;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Domain.Events;
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
        var review =  new Review(restaurantId, userId, rating, comment);
        
        review.RegisterDomainEvent(new ReviewAddedDomainEvent(review));
        return review;
    }

    public void Update(string comment, int rating)
    {
        Comment = comment;
        Rating = rating;
        UpdatedAt = DateTime.UtcNow;
    }
}
