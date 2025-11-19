using System;
using Ardalis.SharedKernel;

using RestoRate.Contracts.Review.Events;

namespace RestoRate.Review.Domain.ReviewAggregate;

public class Review : EntityBase<Guid>, IAggregateRoot
{
    public Guid RestaurantId { get; private set; } = default!;
    public Guid UserId { get; private set; } = default!;
    public int Rating { get; private set; } = default!;
    public string? Text { get; private set; }
    public DateTime CreatedAt { get; private set; } = default!;
    public DateTime? UpdatedAt { get; private set; }

    // Optionally, add moderation status, e.g.:
    // public ReviewStatus Status { get; private set; }

    private Review() { }

    public Review(Guid restaurantId, Guid userId, int rating, string text)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        UserId = userId;
        Rating = rating;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    public static Review Create(Guid restaurantId, Guid userId, int rating, string text)
    {
        var review =  new Review(restaurantId, userId, rating, text);

        return review;
    }

    public void Update(string text, int rating)
    {
        Text = text;
        UpdatedAt = DateTime.UtcNow;
    }
}
