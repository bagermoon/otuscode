namespace RestoRate.ReviewService.Application.Sagas.RestaurantValidationSaga.Messages;

public sealed record RestaurantValidationCompleted(
    Guid RestaurantId,
    Guid ReviewId,
    DateTime ValidatedAt,
    bool IsValid);
