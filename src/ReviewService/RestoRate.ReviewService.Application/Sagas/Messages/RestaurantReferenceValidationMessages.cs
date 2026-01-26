namespace RestoRate.ReviewService.Application.Sagas.Messages;
public sealed record RestaurantReferenceValidationStatus(
    Guid RestaurantId,
    bool IsValid);
