namespace RestoRate.Contracts.Review.Events;

public sealed record UserReferenceValidationStatus(
    Guid UserId,
    bool IsValid);
