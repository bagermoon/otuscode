namespace RestoRate.ReviewService.Application.Sagas.UserValidationSaga.Messages;

public sealed record UserReferenceValidationStatus(
    Guid UserId,
    bool IsValid);
