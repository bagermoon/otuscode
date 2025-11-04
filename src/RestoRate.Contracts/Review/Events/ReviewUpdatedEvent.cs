namespace RestoRate.Contracts.Review.Events;

public sealed record ReviewUpdatedEvent(
    Guid ReviewId,
    int Rating,
    string Text,
    string[] Tags);
