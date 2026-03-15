namespace RestoRate.ReviewService.Application.Sagas.UserValidationSaga.Messages;

public sealed record UserValidationCompleted(
    Guid UserId,
    Guid ReviewId,
    DateTime ValidatedAt,
    bool IsValid);
