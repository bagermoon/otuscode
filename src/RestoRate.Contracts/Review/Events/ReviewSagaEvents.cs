namespace RestoRate.Contracts.Review.Events;

public sealed record ReviewValidationRequested(
    Guid RestaurantId,
    Guid ReviewId,
    Guid UserId);

public sealed record RestaurantValidationCompleted(
    Guid RestaurantId,
    Guid ReviewId,
    DateTime ValidatedAt,
    bool IsValid);

public sealed record UserValidationCompleted(
    Guid UserId,
    Guid ReviewId,
    DateTime ValidatedAt,
    bool IsValid);
