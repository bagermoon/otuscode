namespace RestoRate.ReviewService.Application.Sagas.RestaurantValidationSaga.Messages;

public sealed record RestaurantReferenceValidationStatus(
    Guid RestaurantId,
    bool IsValid);