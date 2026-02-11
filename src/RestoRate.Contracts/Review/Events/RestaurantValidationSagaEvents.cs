namespace RestoRate.Contracts.Review.Events;

public sealed record RestaurantReferenceValidationStatus(
    Guid RestaurantId,
    bool IsValid);
