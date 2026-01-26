namespace RestoRate.ReviewService.Application.Sagas.Messages;
public sealed record UserReferenceValidationStatus(
    Guid UserId,
    bool IsValid);