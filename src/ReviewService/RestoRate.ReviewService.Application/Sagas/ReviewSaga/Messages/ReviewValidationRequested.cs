namespace RestoRate.ReviewService.Application.Sagas.ReviewSaga.Messages;

public sealed record ReviewValidationRequested(
    Guid RestaurantId,
    Guid ReviewId,
    Guid UserId);
