namespace RestoRate.Contracts.Review.Events;

public sealed record ReviewAddedEvent(
    Guid ReviewId,
    Guid RestaurantId,
    Guid AuthorId,
    int Rating,
    string Text,
    string[] Tags);
